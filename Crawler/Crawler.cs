using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ReadSharp;

namespace Crawler
{
    public class Crawler
    {
        public void GetPagesFromUrl(string mainUrl)
        {
            string path = Directory.GetCurrentDirectory() + "/" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            DirectoryInfo di = Directory.CreateDirectory(path);
            DirectoryInfo diindex = Directory.CreateDirectory(path+"/index");
            var urls = new List<string>();
            Chilkat.Spider spider = new Chilkat.Spider();
            Chilkat.StringArray seenDomains = new Chilkat.StringArray();
            Chilkat.StringArray seedUrls = new Chilkat.StringArray();

            seenDomains.Unique = false;
            seedUrls.Unique = true;
            
            seedUrls.Append(mainUrl);
            
            spider.CacheDir = "c:/spiderCache/";
            spider.FetchFromCache = true;
            spider.UpdateCache = true;
            List<string> resultUrls = new List<string>();
            string url = seedUrls.Pop();
            spider.Initialize(url);
            
            string domain = spider.GetUrlDomain(url);
            seenDomains.Append(spider.GetBaseDomain(domain));

            //int i;
            bool success;
            while (resultUrls.Count < 200)
            {
                success = spider.CrawlNext();
                if (success == true)
                {
                    var plainText = HtmlUtilities.ConvertToPlainText(spider.LastHtml);
                    var words = GetWords(plainText);
                    if (words.Length > 1000)
                    {
                        resultUrls.Add(spider.LastUrl);
                        Console.WriteLine(spider.LastUrl);
                        WriteToFile(plainText, path + "/" + resultUrls.Count + ".txt");
                    }
                }
            }

            WriteIndex(resultUrls, path + "/index/index.txt");
        }

        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\w(?<!\d)[\w'-]*");

            var words = from m in matches.Cast<Match>()
                where !string.IsNullOrEmpty(m.Value)
                select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }

        public void WriteToFile(string text, string path)
        {
            using (FileStream fs = File.Create(path, 1024))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(text);
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }

        public void WriteIndex(List<string> urls, string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Console.WriteLine("File deleted.");
            }

            File.Create(path).Dispose();
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var url in urls)
                {
                    var fileline = (urls.IndexOf(url) + 1).ToString() + ". Ссылка: " + url;
                    sw.WriteLine(fileline);
                }
            }
        }
    }
}
