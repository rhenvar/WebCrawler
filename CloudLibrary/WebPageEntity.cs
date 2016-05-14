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
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }

        public WebPageEntity(string url, DateTime date, string title)
        {
            this.PartitionKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url));
            this.RowKey = Guid.NewGuid().ToString();

            this.Url = url;
            this.Date = date;
            this.Title = title;
        }
        public WebPageEntity() { }
    }
}
