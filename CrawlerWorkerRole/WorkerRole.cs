using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Xml;
using CloudLibrary;
using HtmlAgilityPack;
using System.Linq;
using System;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CrawlerWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private EventWaitHandle EventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private readonly List<Thread> threads = new List<Thread>();
        private readonly List<ThreadWorker> workers = new List<ThreadWorker>();



        public override void Run()
        {
            Trace.TraceInformation("CrawlerWorkerRole is running");
            try
            {
                foreach (var worker in workers)
                {
                    threads.Add(new Thread(worker.RunInternal));
                }
                foreach (var thread in threads)
                {
                    thread.Start();
                }
                while (!EventWaitHandle.WaitOne(500))
                {
                    for (var i = 0; i < threads.Count; i++)
                    {
                        if (threads[i].IsAlive)
                        {
                            continue;
                        }
                        threads[i] = new Thread(workers[i].RunInternal);
                        threads[i].Start();
                    }
                    EventWaitHandle.WaitOne(2000);
                }
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            workers.Add(new XmlParser());
            workers.Add(new HtmlTitleDateParser());
            workers.Add(new UrlFinder());
            workers.Add(new TableManager());
            bool result = base.OnStart();
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

        internal class HtmlTitleDateParser : ThreadWorker 
        {
            public override void Run()
            {
                while (true)
                {
                    CloudQueueMessage message = htmlQueue.GetMessage();
                    if (message != null)
                    {
                        htmlQueue.DeleteMessage(message);
                        ParseTitleDateHtmlUrl(message.AsString);
                    }
                    Thread.Sleep(10);
                }
            }

            private void ParseTitleDateHtmlUrl(string htmlUrl)
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
                //InsertToTable(htmlUrl, date, title);
                urlInsertions.Enqueue(new WebPageEntity(htmlUrl, date, title));
                htmlDocuments.Enqueue(new HtmlUrlNode(htmlUrl, htmlDoc.DocumentNode));
            }
        }
    }

    internal class UrlFinder : ThreadWorker
    {
        public override void Run()
        {
            while (true)
            {
                if (htmlDocuments.Count > 0)
                {
                    HtmlUrlNode foundPage = htmlDocuments.Dequeue() as HtmlUrlNode;
                    ParsePageLinks(foundPage.rootUrl, foundPage.documentNode);
                }
                Thread.Sleep(10);
            }
        }
        private void ParsePageLinks(string htmlUrl, HtmlNode rootNode)
        {
            var links = rootNode.Descendants("a").ToList().Where(a => a.Attributes["href"] != null && a.Attributes["href"].Value != "/").Select(a => a.Attributes["href"]).ToList();
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
                    if (UriValidator.IsValidHtml(foundUrl))
                    {
                        CloudQueueMessage newHtmlPage = new CloudQueueMessage(foundUrl);
                        htmlQueue.AddMessage(newHtmlPage);
                    }
                }
            }
        }
    }

    internal class XmlParser : ThreadWorker
    {
        public override void Run()
        {
            while (true)
            {
                CloudQueueMessage xmlMessage = xmlQueue.GetMessage();
                if (xmlMessage != null)
                {
                    xmlQueue.DeleteMessage(xmlMessage);
                    ParseXmlUrl(xmlMessage.AsString);
                }
                Thread.Sleep(100);
            }
        }
        private void ParseXmlUrl(string xmlUrl)
        {
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
    }

    internal class TableManager : ThreadWorker
    {
        public override void Run()
        {
            while (true)
            {
                if (urlInsertions.Count > 0)
                {
                    WebPageEntity toInsert = urlInsertions.Dequeue() as WebPageEntity;
                    InsertToTable(toInsert);
                }
                Thread.Sleep(10);
            }
        }
        private void InsertToTable(WebPageEntity webPage)
        {
            TableOperation insert = TableOperation.Insert(webPage);
            urlTable.Execute(insert);
        }
    }
}
