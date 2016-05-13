using CloudLibrary;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> urls = new HashSet<string>();
            CloudQueue htmlqueue = AccountManager.queueClient.GetQueueReference("htmlqueue");

            CloudQueueMessage message = htmlqueue.GetMessage();
            while (message != null)
            {
                string url = message.AsString;
                if (urls.Contains(url))
                {
                    Console.WriteLine("FAILED FAGGOT AHAHAHA");
                    Console.Read();
                }
                else
                {
                    urls.Add(url);
                }
                Console.WriteLine(url);

                htmlqueue.DeleteMessage(message);
                message = htmlqueue.GetMessage();
            }
        }
    }
}
