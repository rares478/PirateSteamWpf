﻿using CodeKicker.BBCode;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public Page2(int appid)
        {
            InitializeComponent();
            GetNotes();
        }

        private void GetNotes()
        {
            string input = "";

            foreach (JObject newsItem in Page1.newsItems)
            {
                string title = (string)newsItem["title"];

                JToken contentToken = newsItem["contents"];
                string mid = contentToken.ToString(Newtonsoft.Json.Formatting.None);
                mid = mid.Replace("\\n", "^br^");
                string feedname = (string)newsItem["feedname"];

                if (feedname == "steam_community_announcements")
                {
                    string[] tagstoken = { "" };
                    if (newsItem.ContainsKey("tags"))
                    {
                        tagstoken = ((JArray)newsItem["tags"]).Select(t => (string)t).ToArray();
                        // Do something with the tagstoken array
                    }
                    if (tagstoken[0] == "patchnotes")
                    {
                        //tbNewsName.Text = "SMALL UPDATE / PATCH NOTES";
                        //pbNewsInside.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //tbNewsName.Text = "NEWS";
                        //pbNewsInside.Image = Library.picyeah();
                    }


                    input = mid;

                    // tbNameNews.Text = title;


                    var attrs = new BBAttribute[]
                {
                    new BBAttribute("url",""),
                };
                    var attrsyoutube = new BBAttribute[]
                    {
                    new BBAttribute("videocode",""),
                    new BBAttribute("full","")
                    };

                    var bbTags = new List<BBTag>()
                {
                new BBTag("h1","<h1>","</h1>"),
                new BBTag("h2","<h2>","</h2>"),
                new BBTag("h3","<h3>","</h3>"),
                new BBTag("h4","<h4>","</h4>"),

                new BBTag("b", "<strong>", "</strong>"),
                new BBTag("i", "<em>", "</em>"),
                new BBTag("u", "<span style=\"text-decoration: line-through\">", "</span>"),
                new BBTag("list", "<ul>", "</ul>"),
                new BBTag("spoiler","",""),
                new BBTag("strike","<s>","</s>"),
                new BBTag("*", "<li>", "</li>", true, false),
                new BBTag("url","<a href=\"${url}\">","</a>",attrs),
                new BBTag("previewyoutube","<iframe width=\"760\"height=\"470\" src=\"https://www.youtube.com/embed/${videocode}\" >","</iframe>",attrsyoutube),



                new BBTag("code", "<pre class=\"prettyprint\">", "</pre>"),

                new BBTag("color","<color=\"${color}\">","</color>",new BBAttribute("color","")),
                new BBTag("img", "<img src=\"${content}\" width=\"800\"height=\"450\"/>", "", false, true),

                };

                    var parser = new BBCodeParser(bbTags);



                    input = ReplaceInvalidTags(input, bbTags);



                    // Convert BBCode to HTML
                    var output = parser.ToHtml(input);


                    // Replace the placeholder text with the original invalid tags
                    output = Regex.Replace(output, @"\|(.+?)\|", "[$1]");
                    output = Regex.Replace(output, @"\^(.+?)\^", "<$1>");
                    output = output.Replace("&quot;", "");



                    output = "<!DOCTYPE html><html><head><title>My Page</title><style>body {margin: 0;padding: 0;}.header {height: 149px;background-image: url(\"path/to/image.jpg\");background-size: cover;background-position: center;}.panel {height: 74px;background-color: rgb(64, 68, 74);display: flex;align-items: center;padding: 0;}.panel h2 {color: #fff;font-size: 24px;margin: 0;margin-left: 16px;font-family: Tahoma, sans-serif;}.news {color: blue;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.posted {color: grey;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.date {color: grey;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.notes{margin: 16px;margin-right: 16px;}</style></head><body style=\"background-color:rgb(27, 40, 56); font-family:Tahoma; font-size: 15.25px; color:rgb(183, 185, 186);\"><div class=\"header\"></div><div class=\"panel\"><h2>Patch Notes</h2><p class=\"news\">NEWS</p><p class=\"posted\">POSTED</p><p class=\"date\">Sat, February 25</p></div><div class=\"notes\">" + output;
                    output = output + "</div></body></html>";

                    

                    webBrowser1.NavigateToString(output);


                }
            }
        }
        public static string ReplaceInvalidTags(string input, List<BBTag> validTags)
        {
            var regex = new Regex(@"\[(?<tag>[^\[\]/]*)\]");
            var matches = regex.Matches(input);
            var stack = new Stack<string>();

            foreach (Match match in matches)
            {
                var tag = match.Groups["tag"].Value;

                if (validTags.Any(x => x.Name == tag))
                {

                }
                else if (tag != "" && !validTags.Any(x => x.Name == tag))
                {
                    var tag2 = match.Value;
                    if (tag2.Contains("[previewyoutube="))
                    {
                        if (tag2.Contains(";full"))
                        {
                            tag2.Replace(";full", "");
                            input = input.Replace(tag2, match.Value.Replace(";full", ""));
                        }
                    }
                    else
                    {
                        input = input.Replace(tag2, $"|{match.Value.Trim('[', ']')}|");
                    }

                }
                else if (tag == "" && stack.Count > 0)
                {
                    stack.Pop();
                }
            }

            while (stack.Count > 0)
            {
                input += "[/" + stack.Pop() + "]";
            }

            return input;
        }
    }
}
