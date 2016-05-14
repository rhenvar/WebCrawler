using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudLibrary
{
    public static class UriValidator
    {
        private static CultureInfo ci = new CultureInfo("en-US");

        public static bool IsValidHtml(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) || !IsAbsoluteUrl(url))
            {
                return false;
            }
            //if (url.EndsWith(".html"))
            //{
            //    return true;
            //}
            return url.EndsWith(".html");
            //using (var testClient = new WebClient())
            //{
            //    try
            //    {
            //        var downloadHtml = testClient.DownloadString(url);
            //        bool isHTML = downloadHtml.Contains("<!DOCTYPE html>");
            //        return isHTML;
            //    }
            //    catch
            //    {
            //        return false;
            //    }
            //}
        }

        public static bool IsValidXml(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return false;
            }
            return url.EndsWith(".xml", true, ci);
        }

        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }
    }
}
