using System;

public static class AccountManager
{
    public static CloudStorageAccount storageAccount;
    public static CloudQueueClient queueClient;

    public AccountManager()
    {
        storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        queueClient = storageAccount.CreateCloudQueueClient();
    }
}
