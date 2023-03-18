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
            appid = 990080;
        }

        private void notes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            frame.Content = new Page2(990080);
            gridLibrary.Effect = new BlurEffect();

            frame.Width = 850;


            frame.HorizontalAlignment = HorizontalAlignment.Center;
            frame.VerticalAlignment = VerticalAlignment.Center;
            frame.Margin = new Thickness(listBox.Width + 10, 70 , 0,1);
            frame.Visibility = Visibility.Visible;
        }

        int oppenednotes = 0;


        private void GridLibrary_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (oppenednotes == 0)
                oppenednotes++;
            else
            {
                // hide frame and remove blur effect
                frame.Visibility = Visibility.Collapsed;
                gridLibrary.Effect = null;
                oppenednotes = 0;
            }
        }

        public static JArray newsItems = new JArray();
        int appid;

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string url = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=" + appid + "&count=10&format=json";
            HttpWebRequest request = WebRequest.CreateHttp(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            JObject data = JObject.Parse(json);
            newsItems = (JArray)data["appnews"]["newsitems"];
            int j = 0;
            foreach (JObject newsItem in newsItems)
            {
                string title = (string)newsItem["title"];
                string contents = (string)newsItem["contents"];
                string feedname = (string)newsItem["feedname"];
                if (feedname == "steam_community_announcements")
                {
                    string[] images = new string[30];

                    Regex regex = new Regex(@"(?<=\[img\])https?://[^\[\]]+(?=\[/img\])");
                    MatchCollection matches = regex.Matches(contents);
                    int i = 0;
                    foreach (Match match in matches)
                    {
                        images[i] = match.Value;
                        i++;
                    }
                }
            }
        }
    }
}
