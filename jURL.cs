using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace jURL{
    static class URLprocessor{
        public struct URL_result {
            public bool found_all_data = false;
            public string? title = null;
            public string? company = null;
            public URL_result(){}
        }
        private static HttpClient? client = null;
        private static HttpClient GetHTTPClient(){
            if (client != null) return client;
            //HttpClientHandler handler = new HttpClientHandler()
            //{ AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd"); // gives us a compressed html thing
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,en-AU;q=0.8");
            client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            client.DefaultRequestHeaders.Add("Priority", "u=0, i");
            client.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Chromium\";v=\"124\", \"Microsoft Edge\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
            client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
            client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0");
            return client;
        }
        public static async Task<URL_result?> Process(string link){
            if(!(Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
                return null;
            if (link.Substring(0, 8) != "https://")
                return null;

            URL_result result = new URL_result();
            string HTML_data;
            try{HTML_data = await GetHTTPClient().GetStringAsync(link);
            } catch (Exception ex){ 
                return result;}

            // switch so we only have to search for specific things for specific sites
            switch (link.Substring(8).Split("/")[0]){
                case "au.indeed.com":
                    ScrapeString(HTML_data, "data-testid=\"jobsearch-JobInfoHeader-title\"><span>", 0, ref result.title);
                    ScrapeString(HTML_data, "class=\"css-1ioi40n e19afand0\">", 0, ref result.company);
                    break;
                case "www.seek.com.au":
                    ScrapeString(HTML_data, "data-automation=\"job-detail-title\">", 0, ref result.title);
                    ScrapeString(HTML_data, "data-automation=\"advertiser-name\">", 0, ref result.company);
                    break;
                case "www.linkedin.com": // requires compression to accept our GET??

                    break;
                default:


                    break;
            }
            //result.title = "test title";
            //result.company = "test company";

            // return true if all data was found
            result.found_all_data = !(string.IsNullOrWhiteSpace(result.title) | string.IsNullOrWhiteSpace(result.company));
            return result;
        }

        private static bool ScrapeString(string src, string pattern, int offset_from_pattern, ref string? output){
            int pattern_offset = src.IndexOf(pattern);
            if (pattern_offset == -1) return false;

            // TODO: instead of having an optional offset, just find the next '>' which enters whatever parent node we're scraping

            string result = "";
            int value_offset = pattern_offset + pattern.Length + offset_from_pattern;
            for (;; value_offset++){
                char curr_char = src[value_offset];
                switch (curr_char){ // escape on delimiter
                    case '>':
                    case '<': goto escape;
                }
                result += curr_char;
            }
        escape:
            output = result;
            return true;
        }
    }
}
