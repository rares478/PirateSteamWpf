using CodeKicker.BBCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfApp3.Classes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public Page2(PatchNote patchNote)
        {
            InitializeComponent();
            GetNotes(patchNote);
        }

        private void GetNotes(PatchNote patchNote)
        {
            string input = "";
            string patch_name = "";
            string patch_title = "";


            input = patchNote.Content;

            patch_title = patchNote.Title;

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

                
                //new BBTag("previewyoutube","<iframe width=\"760\"height=\"470\" src=\"https://www.youtube.com/embed/${videocode}\" >","</iframe>",attrsyoutube),
                //idk how to embed youtube videos in the stupid wpf WebBrowser

                new BBTag("previewyoutube","<a href=\"https://www.youtube.com/embed/${url}\">https://www.youtube.com/embed/${url}","</a>",attrs),

                new BBTag("code", "<pre class=\"prettyprint\">", "</pre>"),

                new BBTag("color","<color=\"${color}\">","</color>",new BBAttribute("color","")),
                new BBTag("img", "<img src=\"${content}\" class=\"resized-image\"/>", "", false, true),

                };
            var parser = new BBCodeParser(bbTags);


                    
            input = ReplaceInvalidTags(input, bbTags);

            // Convert BBCode to HTML
            var output = parser.ToHtml(input);


            // Replace the placeholder text with the original invalid tags
            output = Regex.Replace(output, @"\|(.+?)\|", "[$1]");
            output = Regex.Replace(output, @"\^(.+?)\^", "<$1>");
            output = output.Replace("&quot;", "");
            output = output.Replace("{STEAM_CLAN_IMAGE}", "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/clans/");

            // Check if you should have a picture or not
            if (patchNote.IsNews == false)
            {
                patch_name = "SMALL UPDATE / PATCH NOTES";
                output = "<!DOCTYPE html><html><head><meta charset='utf-8'><title>My Page</title><style>body {margin: 0;padding: 0;}.resized-image{ max-width: 820px;max-height: 450px; width:auto; height:auto; }.header {height: 149px;background-image: url(\"path/to/image.jpg\");background-size: cover;background-position: center;}.panel {height: 50px;background-color: rgb(64, 68, 74);align-items: left;padding: 0;}.panel h2 {color: #fff;font-size: 24px;margin: 0;margin-left: 16px;font-family: Tahoma, sans-serif;}.news {color: rgb(41, 152, 247);font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.posted {color: grey;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;margin-left: 16px;}.date {color: grey;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.notes{margin: 16px;margin-right: 16px;}.news-panel {align-items: left;background-color: rgb(64, 68, 74);padding: 5px;}.news-panel span{margin-right: 10px;}</style></head><body style=\"background-color:rgb(27, 40, 56); font-family:Tahoma; font-size: 15.25px; color:rgb(183, 185, 186);\"><div class=\"news-panel\"><p class=\"news\">" + patch_name + "<span class=\"posted\">POSTED</span><span class=\"date\">" + patchNote.Date + "</span></p></div><div class=\"panel\"><h2>" + patch_title + "</h2></div><div class=\"notes\">" + output;
                output = output + "</div></body></html>";
            }
            else
            {
                patch_name = "NEWS";
                output = "<!DOCTYPE html><html><head><meta charset='utf-8'><title>My Page</title><style>body {margin: 0;padding: 0;}.resized-image{ max-width: 820px;max-height: 450px; width:auto; height:auto; }.header {height: 149px;background-image: url(\"path/to/image.jpg\");background-size: cover;background-position: center;}.panel {height: 50px;background-color: rgb(64, 68, 74);align-items: left;padding: 0;}.panel h2 {color: #fff;font-size: 24px;margin: 0;margin-left: 16px;font-family: Tahoma, sans-serif;}.news {color: rgb(41, 152, 247);font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.posted {color: grey;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;margin-left: 16px;}.date {color: grey;font-family: Tahoma, sans-serif;font-size: 10pt;margin: 0;margin-right: 16px;}.notes{margin: 16px;margin-right: 16px;}.news-panel {align-items: left;background-color: rgb(64, 68, 74);padding: 5px;}.news-panel span{margin-right: 10px;}</style></head><body style=\"background-color:rgb(27, 40, 56); font-family:Tahoma; font-size: 15.25px; color:rgb(183, 185, 186);\"><div class=\"header\"></div><div class=\"news-panel\"><p class=\"news\">" + patch_name + "<span class=\"posted\">POSTED</span><span class=\"date\">" + patchNote.Date + "</span></p></div><div class=\"panel\"><h2>" + patch_title + "</h2></div><div class=\"notes\">" + output;
                output = output + "</div></body></html>";
            }

            webBrowser1.NavigateToString(output);
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            // Check if the URI is a link and not a local file
            if (e.Uri != null && !e.Uri.IsFile)
            {
                // Prevent the WebBrowser from navigating to the link
                e.Cancel = true;

                // Open the link in the user's default browser
                Process.Start("explorer", e.Uri.AbsoluteUri);
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
