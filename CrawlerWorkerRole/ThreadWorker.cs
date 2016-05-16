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
        internal void RunInternal()
        {
            try
            {
                WorkerRole.htmlQueue.CreateIfNotExists();
                WorkerRole.forbiddenQueue.CreateIfNotExists();
                WorkerRole.errorQueue.CreateIfNotExists();
                WorkerRole.urlTable.CreateIfNotExists();
                Run();
            }
            catch (SystemException)
            {
                throw;
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
                WorkerRole.forbiddenUrls.Add(forbiddenUrl);
        }

        protected bool IsForbidden(string url)
        {
            foreach (string forbidden in WorkerRole.forbiddenUrls)
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
