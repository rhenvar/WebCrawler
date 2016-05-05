using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLibrary
{
    public static class AccountManager
    {
        public static CloudStorageAccount storageAccount { get; private set; }
        public static CloudQueueClient queueClient { get; private set; }
        private static string connString = "DefaultEndpointsProtocol = https; AccountName=krolazurestorage;AccountKey=M6JxbA+o55darToYsonq2T2aTgbn0ZZZc9NgArCiht7V32cOa+yze/zMIcoofu2oYFM4QQLHU3aaMhEMXfhraw==";

        static AccountManager()
        {
            try
            {
                storageAccount = CloudStorageAccount.Parse(connString);
                queueClient = storageAccount.CreateCloudQueueClient();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
