using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace GetTruyen
{
    public class Novel
    {
        public string Name;
        public string Author;
        public string Url;
        public string folder;
        public int Downloading;
        public ChapterCollection Chapters;
        public Exception Error;
        public HtmlAgilityPack.HtmlDocument HtmlDoc;
        public Novel()
        {
            Chapters = new ChapterCollection();
            HtmlDoc = new HtmlAgilityPack.HtmlDocument();
            Downloading = 0;
        }

        public bool LoadUrl(string url)
        {
            if (CheckURL(url))
            {
                Url = url;
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.Proxy = null;
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                        string htmlString = client.DownloadString(url);

                        HtmlDoc = new HtmlAgilityPack.HtmlDocument();
                        HtmlDoc.LoadHtml(htmlString);

                        Name = GetName(HtmlDoc.DocumentNode);
                        Author = GetAuthor(HtmlDoc.DocumentNode);
                    }
                    catch(Exception e)
                    {
                        Error = e;
                        return false;
                    }
                }
                return true;
            }
            else return false;
        }

        public void LoadChaptersList(HtmlNode docNode)
        {                  
            using (WebClient client = new WebClient())
            {
                HtmlNode scriptNode = docNode.SelectSingleNode("//a[@href='#truyencv-detail-chap']");
                string script = scriptNode.Attributes["onclick"].Value;

                Regex showChapter_patt = new Regex(@"([0-9]+),([0-9]+),([0-9]+),'([a-z\s]+)'");
                Match match = showChapter_patt.Match(script);
                folder = match.Groups[4].Value;
                string uploadString = "showChapter=1" +
                    "&media_id=" + match.Groups[1].Value +
                    "&number=" + match.Groups[2].Value +
                    "&page=" + match.Groups[3].Value +
                    "&type=" + match.Groups[4].Value;
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string chaptersHtml = client.UploadString("http://truyencv.com/index.php", uploadString);

                HtmlAgilityPack.HtmlDocument chaptersHtmlDoc = new HtmlAgilityPack.HtmlDocument();
                chaptersHtmlDoc.LoadHtml(chaptersHtml);

                HtmlNodeCollection nodes = chaptersHtmlDoc.DocumentNode.SelectNodes("//div[@class='item']//a");
                for(int i = nodes.Count-1; i>=0; i--)
                {
                
                    HtmlNode node = nodes[i];
                    node.ChildNodes[1].Remove();
                    Chapter newChapter = new Chapter(nodes.Count - i - 1, node.InnerText, node.Attributes["href"].Value);
                    Chapters.Add(newChapter);
                }
            }
        }

        public Chapter DownloadChapter(int id)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                string html=client.DownloadString(Chapters[id].Url);
                HtmlAgilityPack.HtmlDocument chapterDoc = new HtmlDocument();
                chapterDoc.LoadHtml(html);
                HtmlNodeCollection divNodes = chapterDoc.DocumentNode.SelectNodes("//div[@id='js-truyencv-content']//div");
                foreach(HtmlNode divNode in divNodes)
                {
                    divNode.Remove();
                }
                HtmlNode titleNode=chapterDoc.DocumentNode.SelectSingleNode("//div[@class='header']//h2[@class='title']");
                HtmlNode contentsNode = chapterDoc.GetElementbyId("js-truyencv-content");
                Chapters[id].Contents = contentsNode.InnerHtml;
                return Chapters[id];
            }
        }
        public bool CheckURL(string url)
        {
            Regex url_patt = new Regex("^(http://)*truyencv.com/([a-z])([a-z\\-]*)([a-z\\/])$");
            Match m = url_patt.Match(url);
            if (m.Success) return true;
            else return false;
        }
        public string GetName(HtmlNode docNode)
        {
            HtmlNodeCollection nodes = docNode.SelectNodes("//h1[@class='title']//a");
            return nodes[0].InnerHtml;
        }
        public string GetAuthor(HtmlNode docNode)
        {
            HtmlNodeCollection nodes = docNode.SelectNodes("//a[@class='author']");
            return nodes[0].InnerText;

        }
    }
}
