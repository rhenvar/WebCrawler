using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CrawlerWorkerRole
{
    public class HtmlUrlNode
    {
        public HtmlNode documentNode { get; set; }
        public string rootUrl { get; set; }

        public HtmlUrlNode(string url, HtmlNode node)
        {
            rootUrl = url;
            documentNode = node;
        }
    }
}
