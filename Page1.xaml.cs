using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml;
using IWshRuntimeLibrary;
using System.ComponentModel;

namespace WpfApp3
{
    public partial class Page1 : Page
    {
        
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        private static string xml = path + "\\Games.xml";
        public static List<PatchNote> notes = new List<PatchNote>();
        public static List<Game> games = new List<Game>();

        public Page1()
        {
            InitializeComponent();

            Directory.CreateDirectory(path);

            if (System.IO.File.Exists(xml))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml);

                XmlNodeList gameNodes = doc.SelectNodes("/games/game");

                foreach (XmlNode gameNode in gameNodes)
                {
                    XmlNode pathNode = gameNode.SelectSingleNode("title");
                    string pathValue = pathNode.InnerText.Trim();
                    

                    Game game = new Game();
                    game.Title = pathValue;
                    game.Path = gameNode.SelectSingleNode("path").InnerText;
                    game.Path_Directory = gameNode.SelectSingleNode("path_directory").InnerText;
                    game.Type = gameNode.SelectSingleNode("type").InnerText;
                    game.Background = gameNode.SelectSingleNode("background").InnerText;
                    game.Logo = gameNode.SelectSingleNode("logo").InnerText;
                    game.Date_Added = double.Parse(gameNode.SelectSingleNode("date").InnerText);
                    if(gameNode.SelectSingleNode("last_played").InnerText == "")
                    {
                        game.Last_Played = 0;
                    }
                    else 
                        game.Last_Played = double.Parse(gameNode.SelectSingleNode("last_played").InnerText);
                    game.SteamAppid = int.Parse(gameNode.SelectSingleNode("steamappid").InnerText);
                    game.Playtime = float.Parse(gameNode.SelectSingleNode("playtime").InnerText);
                    games.Add(game);
                    lbLibrary.Items.Add(pathValue);
                }
            }
            lbLibrary.Items.SortDescriptions.Add(new SortDescription("",ListSortDirection.Ascending));
            games = games.OrderBy(g => g.Title).ToList();
            lbLibrary.SelectedIndex = 0;
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

                frame.Width = 875;


                frame.HorizontalAlignment = HorizontalAlignment.Center;
                frame.VerticalAlignment = VerticalAlignment.Center;
                frame.Margin = new Thickness(lbLibrary.Width + 10, 70, 0, 1);
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

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (frame.Visibility == Visibility.Visible)
            {
                frame.Visibility = Visibility.Collapsed;
                gridLibrary.Effect = null;
            }

            Game game = games[lbLibrary.SelectedIndex];

            //setting up the page
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(game.Background, UriKind.Absolute);
            bitmapImage.EndInit();
            img_Background.Source = bitmapImage;

            BitmapImage bitmapImage2 = new BitmapImage();
            bitmapImage2.BeginInit();
            bitmapImage2.UriSource = new Uri(game.Logo, UriKind.Absolute);
            bitmapImage2.EndInit();
            img_Logo.Source = bitmapImage2;

            double epochTime1 = game.Last_Played;
            DateTime dateTime1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epochTime1);
            tb_LastPlayed.Text = dateTime1.ToString("MMM dd");

            tb_PlayTime.Text = game.Playtime.ToString() + " hours";

            string url = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=" + game.SteamAppid + "&format=json";
            HttpWebRequest request = WebRequest.CreateHttp(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            JObject data = JObject.Parse(json);
            JArray newsItems = (JArray)data["appnews"]["newsitems"];
            
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
                        notes.Add(new PatchNote() { Title = title, Content = mid, Date = formattedDate, IsNews = true});
                    }
                }
            }
            NotesList.ItemsSource = notes;
            
        }

        private void bt_Play_Click(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            Directory.SetCurrentDirectory(game.Path_Directory);
            Process.Start(game.Path);
        }

        private void bt_ShortcutMaker(object sender, RoutedEventArgs e) 
        {
            Game game = games[lbLibrary.SelectedIndex];
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + game.Title + ".lnk";
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = game.Path;
            shortcut.WorkingDirectory = game.Path_Directory;
            shortcut.Save();
        }

        private void bt_BrowseFiles(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            Process.Start("explorer.exe", game.Path_Directory);
        }

        private void bt_Unistall(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            if (Directory.Exists(game.Path_Directory))
            {

                MessageBoxResult result = MessageBox.Show("Are you sure?", "haoleo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Directory.Delete(game.Path_Directory, true);
                    MessageBox.Show("Deleted");
                }
            }
        }
        private void bt_Properties(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            PropertiesYeah newProperties = new PropertiesYeah(game);
            newProperties.Show();
            
        }

        private void ListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (e.Source is ListBoxItem listBoxItem)
            {
                ContextMenu contextMenu = listBoxItem.ContextMenu;
                if (contextMenu != null)
                {
                    contextMenu.PlacementTarget = listBoxItem;
                    contextMenu.IsOpen = true;
                }
            }
            e.Handled = true;
        }
    }
}
