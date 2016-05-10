using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLibrary
{
    public class WebPageEntity : TableEntity
    {
        public string Url { get; private set; }
        public string Date { get; private set; }
        public string Title { get; private set; }

        public WebPageEntity(string url, string date, string title)
        {
            this.PartitionKey = url;
            this.RowKey = Guid.NewGuid().ToString();

            this.Url = url;
            this.Date = date;
            this.Title = title;
        }
        public WebPageEntity() { }
    }
}
