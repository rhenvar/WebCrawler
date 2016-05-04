using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;

namespace CrawlerWebRole
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class admin : System.Web.Services.WebService
    {

        // should only be reading data from azure table here
        // don't invoke worker or insert urls

        [WebMethod]
        public bool StartCrawling()
        {
            using (var client = new WebClient())
            {
                byte[] data = client.DownloadData("http://www.cnn.com/robots.txt");
                using (StreamReader reader = new StreamReader(new MemoryStream(data)))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] testLine = line.Split(' ');
                        CloudQueue queue = AccountManager.queueClient.GetQueueReference("myurls");
                        queue.CreateIfNotExists();
                        if (testLine[1].Contains(".xml"))
                        {
                            queue.AddMessage(new CloudQueueMessage(testLine[1]));
                        }
                    }
                }
            }
            return true;
        }

        [WebMethod]
        public bool StopCrawling()
        {
            return true;
        }

        [WebMethod]
        public bool ClearIndex()
        {
            CloudQueue queue = AccountManager.queueClient.GetQueueReference("myurls");
            queue.FetchAttributes();
            while (queue.ApproximateMessageCount > 0)
            {
                CloudQueueMessage removeMessage = queue.GetMessage();
                queue.DeleteMessage(removeMessage);
                queue.FetchAttributes();
            }
            return true;
        }

        [WebMethod]
        public string GetPageTitle()
        {
            CloudQueue queue = AccountManager.queueClient.GetQueueReference("myurls");
            CloudQueueMessage message = queue.GetMessage(TimeSpan.FromSeconds(5));
            return message.AsString;
        }
    }
}
