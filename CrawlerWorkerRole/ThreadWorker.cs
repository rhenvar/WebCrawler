using CloudLibrary;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerWorkerRole
{
    public class ThreadWorker
    {
        protected static ConcurrentSet<string> visitedUrls = new ConcurrentSet<string>();
        protected List<string> forbiddenUrls = new List<string>();

        protected static CloudTableClient tableClient = AccountManager.storageAccount.CreateCloudTableClient();
        protected static CloudQueueClient queueClient = AccountManager.storageAccount.CreateCloudQueueClient();

        protected static CloudQueue htmlQueue = queueClient.GetQueueReference("htmlqueue");
        protected static CloudQueue xmlQueue = queueClient.GetQueueReference("xmlqueue");
        protected static CloudQueue forbiddenQueue = queueClient.GetQueueReference("forbiddenqueue");
        protected static CloudQueue errorQueue = queueClient.GetQueueReference("errorqueue");
        protected static CloudTable urlTable = tableClient.GetTableReference("urltable");

        protected static Queue urlInsertions = Queue.Synchronized(new Queue());
        protected static Queue htmlDocuments = Queue.Synchronized(new Queue());

        internal void RunInternal()
        {
            try
            {
                htmlQueue.CreateIfNotExists();
                xmlQueue.CreateIfNotExists();
                forbiddenQueue.CreateIfNotExists();
                errorQueue.CreateIfNotExists();
                urlTable.CreateIfNotExists();
                Run();
            }
            catch (SystemException)
            {
                //throw;
            }
            catch (Exception)
            {
            }
        }
        public virtual void Run()
        {
        }

        public virtual void OnStop()
        {
        }

        protected void ParseForbiddenUrl(string forbiddenUrl)
        {
            if (!IsForbidden(forbiddenUrl))
                forbiddenUrls.Add(forbiddenUrl);
        }

        protected bool IsForbidden(string url)
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
