using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerWorkerRole
{
    class WebPage : TableEntity
    {
        // URL, title, and date
        public string URL { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }

        public WebPage(string url, string title, string date)
        {
            URL = url;
            Title = title;
            Date = date;
        }
    }
}
