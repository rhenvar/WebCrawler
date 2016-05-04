using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace CrawlerWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public static CloudTableClient tableClient = AccountManager.storageAccount.CreateCloudTableClient();
        public static CloudQueueClient queueClient = AccountManager.storageAccount.CreateCloudQueueClient();

        private HashSet<string> visitedUrls = new HashSet<string>();

        private CloudQueue queue;
        private CloudTable table;

        // typically infinite loop?
        // role will shut down if loop is exited
        public override void Run()
        {
            Trace.TraceInformation("CrawlerWorkerRole is running");
            try
            {
                queue.FetchAttributes();
                if (queue.ApproximateMessageCount == 0)
                {
                    return;
                }

                CloudQueueMessage message = queue.GetMessage();
                queue.DeleteMessage(message);

                string url = message.AsString;

                // check if the url has a robots.txt (nested sitemap)
                bool containsSitemaps= true;
                using (var client = new WebClient())
                {
                    try
                    {
                        byte[] testResource = client.DownloadData(url + "/robots.txt");
                        ParseRobots(testResource);
                    }
                    catch (ArgumentNullException a)
                    {
                        // no nested robots
                        containsSitemaps = false;
                    }
                }

                if (!containsSitemaps)
                {
                    if (url.EndsWith(".xml"))
                    {
                        ParseXML(url);
                    }
                    else
                    {
                        ParseHTML(url);
                    }
                }

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
            queue = queueClient.GetQueueReference("myurls");
            queue.CreateIfNotExists();

            table = tableClient.GetTableReference("urltable");
            table.CreateIfNotExists();

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
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }

        // need to handle for non xml sitemaps (imdb)
        private void ParseRobots(byte[] data)
        {
            using (StreamReader reader = new StreamReader(new MemoryStream(data)))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("Sitemap"))
                    {
                        string[] lineArray = line.Split(' ');
                        if (!visitedUrls.Contains(lineArray[1]))
                        {
                            visitedUrls.Add(lineArray[1]);
                            queue.AddMessage(new CloudQueueMessage(lineArray[1]));
                        }
                    }
                }
            }
        }

        private void ParseXML(string url)
        {

        }

        private void ParseHTML(string url)
        {

        }
    }
}
