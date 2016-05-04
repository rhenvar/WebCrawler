using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Configuration;

public static class AccountManager
{
    public static CloudStorageAccount storageAccount;
    public static CloudQueueClient queueClient;

    static AccountManager()
    {
        try
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            queueClient = storageAccount.CreateCloudQueueClient();
        }
        catch (Exception e)
        {
            throw;
        }
    }
}
