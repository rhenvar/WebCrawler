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

namespace CrawlerWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public static CloudTableClient tableClient = AccountManager.storageAccount.CreateCloudTableClient();
        public static CloudQueueClient queueClient = AccountManager.storageAccount.CreateCloudQueueClient();

        private HashSet<string> visitedUrls = new HashSet<string>();
        private HashSet<string> forbiddenUrls = new HashSet<string>();

        private CloudQueue htmlQueue;
        private CloudQueue xmlQueue;
        private CloudTable urlTable;

        public override void Run()
        {
            Trace.TraceInformation("CrawlerWorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait(1000);
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

            urlTable = tableClient.GetTableReference("urltable");
            urlTable.CreateIfNotExists();

            PopulateForbidden();

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
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                // parse xml and html from queue
                CloudQueueMessage xmlMessage = xmlQueue.GetMessage();
                if (xmlMessage != null)
                {
                    ParseXmlUrl(xmlMessage.AsString);
                    xmlQueue.DeleteMessage(xmlMessage);
                }

                CloudQueueMessage htmlMessage = htmlQueue.GetMessage();
                if (htmlMessage != null)
                {
                    ParseHtmlUrl(htmlMessage.AsString);
                    htmlQueue.DeleteMessage(htmlMessage);
                }
                await Task.Delay(1000);
            }
        }

        private bool InsertToTable(string url, string date, string title)
        {
            return true;
        }

        private void ParseXmlUrl(string xmlUrl)
        {
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
                CloudQueueMessage message = new CloudQueueMessage(url);
                if (!IsForbidden(url))
                {
                    if (url.EndsWith(".xml"))
                    {
                        xmlQueue.AddMessage(message);
                    }
                    else if (url.EndsWith(".html"))
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
            visitedUrls.Add(htmlUrl);
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlTextReader tr = new XmlTextReader(htmlUrl))
            {
                tr.Namespaces = false;
                xmlDoc.Load(tr);
            }
            XmlNode date = xmlDoc.SelectSingleNode("//");
        }

        private void PopulateForbidden()
        {
            CloudQueue forbiddenQueue = queueClient.GetQueueReference("forbiddenqueue");
            forbiddenQueue.CreateIfNotExists();

            CloudQueueMessage forbiddenMessage = forbiddenQueue.GetMessage();

            while (forbiddenMessage != null)
            {
                string url = forbiddenMessage.AsString;
                forbiddenUrls.Add(url);
                forbiddenQueue.DeleteMessage(forbiddenMessage);
                forbiddenMessage = forbiddenQueue.GetMessage();
            }
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
