using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Xml;
using CloudLibrary;
using HtmlAgilityPack;
using System.Linq;
using System;

namespace CrawlerWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public static CloudTableClient tableClient = AccountManager.storageAccount.CreateCloudTableClient();
        public static CloudQueueClient queueClient = AccountManager.storageAccount.CreateCloudQueueClient();

        private static ConcurrentSet<string> visitedUrls = new ConcurrentSet<string>();
        private List<string> forbiddenUrls = new List<string>(); 

        private CloudQueue htmlQueue;
        private CloudQueue xmlQueue;
        private CloudQueue forbiddenQueue;
        private CloudQueue errorQueue;
        private CloudTable urlTable;

        public override void Run()
        {
            Trace.TraceInformation("CrawlerWorkerRole is running");

            try
            {
                //this.RunAsync(this.cancellationTokenSource.Token).Wait(1000);
                ThreadPool.SetMaxThreads(4, 4);
                while (true)
                {
                    Trace.TraceInformation("Working");

                    // if there are new blacklist URLs (say someone starts crawling cnn
                    // mid crawl for bleacherreport)
                    CloudQueueMessage forbiddenMessage = forbiddenQueue.GetMessage();
                    if (forbiddenMessage != null)
                    {
                        forbiddenQueue.DeleteMessage(forbiddenMessage);
                        ParseForbiddenUrl(forbiddenMessage.AsString);
                    }

                    CloudQueueMessage htmlMessage = htmlQueue.GetMessage();
                    if (htmlMessage != null)
                    {
                        htmlQueue.DeleteMessage(htmlMessage);
                        ThreadPool.QueueUserWorkItem(state => ParseHtmlUrl(htmlMessage.AsString));
                    }
                    // parse xml and html from queue
                    CloudQueueMessage xmlMessage = xmlQueue.GetMessage();
                    if (xmlMessage != null)
                    {
                        xmlQueue.DeleteMessage(xmlMessage);
                        ThreadPool.QueueUserWorkItem(state => ParseXmlUrl(xmlMessage.AsString));
                        //ParseXmlUrl(xmlMessage.AsString);
                    }
                    Thread.Sleep(100);
                }
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        // good place to start running diagnostics
        // returns true if worker is running
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("CrawlerWorkerRole has been started");
            
            htmlQueue = queueClient.GetQueueReference("htmlqueue");
            htmlQueue.CreateIfNotExists();

            xmlQueue = queueClient.GetQueueReference("xmlqueue");
            xmlQueue.CreateIfNotExists();

            forbiddenQueue = queueClient.GetQueueReference("forbiddenqueue");
            forbiddenQueue.CreateIfNotExists();

            errorQueue = queueClient.GetQueueReference("errorqueue");
            errorQueue.CreateIfNotExists();

            urlTable = tableClient.GetTableReference("urltable");
            urlTable.CreateIfNotExists();

            return result;
        }

        // On stop method
        // called when role is to be shut down
        // good place to clean up, only have 30 sec tho
        public override void OnStop()
        {
            Trace.TraceInformation("CrawlerWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("CrawlerWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            //ThreadPool.SetMaxThreads(3, 3);
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                await Task.Delay(100);
            }
        }

        private void InsertToTable(string url, DateTime date, string title)
        {
            WebPageEntity webPage = new WebPageEntity(url, date, title);
            TableOperation insert = TableOperation.Insert(webPage);
            urlTable.Execute(insert);
        }

        //private void ParseXmlUrl(string xmlUrl)
        private void ParseXmlUrl(string xmlUrl)
        {
            //string xmlUrl = (string)state; 
            // and is not a 'disallow link'

            if (visitedUrls.Contains(xmlUrl))
            {
                return;
            }
            visitedUrls.Add(xmlUrl);
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlTextReader tr = new XmlTextReader(xmlUrl))
            {
                tr.Namespaces = false;
                xmlDoc.Load(tr);
            }
            XmlNodeList urls = xmlDoc.SelectNodes("//loc");
            foreach (XmlNode urlNode in urls)
            {
                string url = urlNode.InnerText;
                if (!IsForbidden(url) && !visitedUrls.Contains(url))
                {
                    CloudQueueMessage message = new CloudQueueMessage(url);
                    if (UriValidator.IsValidXml(url))
                    {
                        xmlQueue.AddMessage(message);
                    }
                    else if (UriValidator.IsValidHtml(url))
                    {
                        htmlQueue.AddMessage(message);
                    }
                }
            }
        }

        private void ParseHtmlUrl(string htmlUrl)
        {
            if (visitedUrls.Contains(htmlUrl))
            {
                return;
            }
            // grab title and date, throw into table (start 3rd concurrent thread?)
            visitedUrls.Add(htmlUrl);
            HtmlDocument htmlDoc = new HtmlDocument();

            try
            {
                //htmlDoc.Load(htmlUrl);
                using (var client = new WebClient())
                {
                    htmlDoc.LoadHtml(client.DownloadString(htmlUrl));
                }
                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                {
                    throw new WebException();
                }
            }
            catch (Exception e)
            {
                CloudQueueMessage errorMessage = new CloudQueueMessage(htmlUrl);
                errorQueue.AddMessage(errorMessage);
            }


            DateTime date;
            var pageDate = htmlDoc.DocumentNode.Descendants("meta").Where(m => m.Attributes["content"] != null
            && m.GetAttributeValue("name", "").Equals("pubdate", StringComparison.InvariantCultureIgnoreCase)
            || m.GetAttributeValue("name", "").Equals("og:pubdate", StringComparison.InvariantCultureIgnoreCase)).Select(m => m.Attributes["content"].Value).ToList();
            if (pageDate.Count == 0 || pageDate == null)
            {
                date = DateTime.Today;
            }
            else
            {
                date = DateTime.Parse(pageDate[0]);
            }

            string title;
            var pageTitle = htmlDoc.DocumentNode.Descendants("title").Where(t => t != null).Select(t => t.InnerHtml).ToList();
            if (pageTitle.Count == 0 || pageTitle == null)
            {
                title = "Page Title Not Found";
            }
            else
            {
                title = pageTitle[0];
            }
            ThreadPool.QueueUserWorkItem(state => InsertToTable(htmlUrl, date, title));

            var links = htmlDoc.DocumentNode.Descendants("a").ToList().Where(a => a.Attributes["href"] != null && a.Attributes["href"].Value != "/").Select(a => a.Attributes["href"]).ToList();
            if (links != null && links.Count > 0)
            {

                foreach (HtmlAttribute pageLinkAttribute in links)
                {
                    string pageLink = pageLinkAttribute.Value;

                    string foundUrl;
                    if (UriValidator.IsAbsoluteUrl(pageLink))
                    {
                        foundUrl = pageLink;
                    }
                    else
                    {
                        var baseUrl = new Uri(htmlUrl);
                        var url = new Uri(baseUrl, pageLink);
                        foundUrl = url.AbsoluteUri;
                    }
                    if (!visitedUrls.Contains(foundUrl) && UriValidator.IsValidHtml(foundUrl))
                    {
                        CloudQueueMessage newHtmlPage = new CloudQueueMessage(foundUrl);
                        htmlQueue.AddMessage(newHtmlPage);
                    }
                }
            }

        }

        private void ParseForbiddenUrl(string forbiddenUrl)
        {
            if (!IsForbidden(forbiddenUrl))
                forbiddenUrls.Add(forbiddenUrl);
        }

        private bool IsForbidden(string url)
        {
            foreach (string forbidden in forbiddenUrls)
            {
                if (url.Contains(forbidden))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
