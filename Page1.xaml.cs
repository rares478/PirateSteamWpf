using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
            appid = 526870;
            listBox.SelectedIndex = 0;

            /*List<JArray> notes = new List<JArray>();
            notes.Add(new JArray(newsItems));
            NotesList.ItemsSource = notes;*/
            

        }

        private void notes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (frame.Visibility == Visibility.Visible)
            {
                frame.Visibility = Visibility.Collapsed;
                gridLibrary.Effect = null;
            }
            else
            {
                PatchNote clickedNote;
                if (sender is StackPanel)
                {
                    clickedNote = (sender as StackPanel)?.DataContext as PatchNote;
                }
                else if (sender is TextBlock)
                {
                    clickedNote = (sender as TextBlock)?.DataContext as PatchNote;
                }
                else
                {
                    clickedNote = (sender as Image)?.DataContext as PatchNote;
                }


                frame.Content = new Page2(clickedNote);
                gridLibrary.Effect = new BlurEffect();

                frame.Width = 850;


                frame.HorizontalAlignment = HorizontalAlignment.Center;
                frame.VerticalAlignment = VerticalAlignment.Center;
                frame.Margin = new Thickness(listBox.Width + 10, 70, 0, 1);
                frame.Visibility = Visibility.Visible;
            }
        }


        private void GridLibrary_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(frame.Visibility == Visibility.Visible) 
            {
                frame.Visibility = Visibility.Collapsed;
                gridLibrary.Effect = null;
            }
        }

        public static List<PatchNote> notes = new List<PatchNote>();
        int appid;

        public class PatchNote
        {
            public string Title {  get; set; }
            public string Content { get; set; }
            public string Date { get; set; }
            public bool IsNews { get; set; }
            public int Id { get; set; }
        }


        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string url = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=" + appid + "&format=json";
            HttpWebRequest request = WebRequest.CreateHttp(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            JObject data = JObject.Parse(json);
            JArray newsItems = (JArray)data["appnews"]["newsitems"];
            int j = 0;
            
            List<PatchNote> notes = new List<PatchNote>();
            foreach (JObject newsItem in newsItems)
            {

                string title = (string)newsItem["title"];
                string feedname = (string)newsItem["feedname"];
                if (feedname == "steam_community_announcements")
                {
                    string[] images = new string[30];

                    double epochTime = (double)newsItem["date"];
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epochTime);
                    string formattedDate = dateTime.ToString("ddd, MMMM dd");

                    JToken contentToken = newsItem["contents"];
                    string mid = contentToken.ToString(Newtonsoft.Json.Formatting.None);
                    mid = mid.Replace("\\n", "^br^");

                    string[] tagstoken = { "" };
                    if (newsItem.ContainsKey("tags"))
                    {
                        tagstoken = ((JArray)newsItem["tags"]).Select(t => (string)t).ToArray();
                    }
                    if (tagstoken[0] == "patchnotes")
                    {
                        notes.Add(new PatchNote() { Title = title, Content = mid, Date = formattedDate, IsNews = false });
                    }
                    else
                    {
                        notes.Add(new PatchNote() { Title = title, Content = mid, Date = formattedDate, IsNews = true });
                    }



                    /*Regex regex = new Regex(@"(?<=\[img\])https?://[^\[\]]+(?=\[/img\])");
                    MatchCollection matches = regex.Matches(contents);
                    int i = 0;
                    foreach (Match match in matches)
                    {
                        images[i] = match.Value;
                        i++;
                    }*/
                }
            }
            NotesList.ItemsSource = notes;
        }
    }
}
