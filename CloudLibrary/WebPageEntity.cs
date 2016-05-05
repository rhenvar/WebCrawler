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
        private string Url;
        private string Date;
        private string Title;

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
