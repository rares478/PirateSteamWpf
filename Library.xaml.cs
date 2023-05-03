﻿using IWshRuntimeLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
using System.Xml.Linq;
using System.Xml;
using WpfApp3.Classes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page
    {
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        private static string xml = path + "\\Games.xml";
        public static List<PatchNote> notes = new List<PatchNote>();
        public static List<Game> games = new List<Game>();
        public static List<Game> apps = new List<Game>();

        public Library()
        {
            InitializeComponent();

            Directory.CreateDirectory(path);
            games.Clear();
            games = UpdateSteamCMD.Items;
            games = games.OrderBy(g => g.Title, StringComparer.OrdinalIgnoreCase).ToList();
            lbLibrary.Items.Clear();

            List<Game> remove = new List<Game>();
            foreach(Game  game in games)
            {
                if (game.Type == "App")
                    apps.Add(game);
                else
                {
                    if(game.Title != null)
                        lbLibrary.Items.Add(game);
                    else
                    {
                        remove.Add(game);
                    }
                }
                    
            }
            foreach(Game game in apps)
            {
                games.Remove(game);
            }
            foreach(Game game in remove)
            {
                games.Remove(game);
            }
            remove.Clear();
            lbLibrary.SelectedIndex = 0;
            lbLibrary.DisplayMemberPath = "Title";
        }



        private void notes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (frame.Visibility == Visibility.Visible)
            {
                frame.Visibility = Visibility.Collapsed;
                scroll.Effect = null;
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
                scroll.Effect = new BlurEffect();

                frame.Width = 875;


                frame.HorizontalAlignment = HorizontalAlignment.Center;
                frame.VerticalAlignment = VerticalAlignment.Center;
                frame.Margin = new Thickness(lbLibrary.Width + 10, 70, 0, 1);
                frame.Visibility = Visibility.Visible;
            }
        }


        private void GridLibrary_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (frame.Visibility == Visibility.Visible)
            {
                frame.Visibility = Visibility.Collapsed;
                scroll.Effect = null;
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (frame.Visibility == Visibility.Visible)
            {
                frame.Visibility = Visibility.Collapsed;
                scroll.Effect = null;
            }

            LoadingBar.Visibility = Visibility.Collapsed;
            tb_Downloading.Visibility = Visibility.Collapsed;
            tb_down_of_total.Visibility = Visibility.Collapsed;

            Game game = games[lbLibrary.SelectedIndex];

            //setting up the page

            tb_dlc.Text = game.DLCs.Count.ToString();

            if (game.Background != null)
            {
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
            }

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
                        notes.Add(new PatchNote() { Title = title, Content = mid, Date = formattedDate, IsNews = true });
                    }
                }
            }
            NotesList.ItemsSource = notes;

        }

        private async void bt_Play_Click(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            if (game.Installed == true)
            {

                string dir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(Directory.GetParent(game.Path).ToString());
                Process.Start(game.Path);
                Directory.SetCurrentDirectory(dir);
            }
            else
            {
                Install install = new Install(game,this);
                install.ShowDialog();

            }
        }

        private void bt_ShortcutMaker(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + game.Title + ".lnk";
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = game.Path;
            shortcut.WorkingDirectory = Directory.GetParent(game.Path).ToString();
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
                    XDocument xmlDoc = XDocument.Load(xml);
                    var gameNode = xmlDoc.Descendants("game").FirstOrDefault(node => Convert.ToInt32(node.Element("steamappid")?.Value) == game.SteamAppid);
                    if (gameNode != null)
                    {
                        gameNode.Remove();
                        xmlDoc.Save(xml);
                    }
                    games.Remove(game);
                    lbLibrary.Items.Remove(lbLibrary.SelectedIndex);
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

        public void downloadGame(int appid, string Path, long size)
        {
            LoadingBar.Visibility = Visibility.Visible;
            tb_Downloading.Visibility = Visibility.Visible;
            tb_down_of_total.Visibility = Visibility.Visible;
            this.size = size;
            Path = "D:\\Downloads\\tulip\\descarca aici";


            var processInfo = new ProcessStartInfo
            {
                FileName = "D:\\Downloads\\tulip\\downloader\\DepotDownloader.exe",
                Arguments = $"-app 1326470 -username {MainWindow.User.Username} -password {MainWindow.User.Password}{(Path != null ? $" -dir \"{Path}\"" : $" -dir \"{AppContext.BaseDirectory}\\manifest\"")}",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = processInfo };
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            process.StandardInput.WriteLine("rpkck");
        }

        private void UpdateLoadingBar(string percentageString)
        {
            float percentage;
            
            if (float.TryParse(percentageString.TrimEnd('%'), out percentage))
            {
                tb_down_of_total.Text = Util.FormatBytes(Convert.ToInt64(percentage * size / 100)) + " of " + Util.FormatBytes(size);
                Dispatcher.Invoke(() =>
                {
                    LoadingBar.Value = percentage;
                });
            }
        }

        private long size;

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                string outputText = e.Data;

                Dispatcher.Invoke(() =>
                {
                    tb_scrie_aici.AppendText(outputText + Environment.NewLine);
                });

                int percentageIndex = outputText.LastIndexOf('%');

                if (percentageIndex != -1)
                {
                    int startIndex = outputText.LastIndexOf(' ', percentageIndex) + 1;
                    int length = percentageIndex - startIndex + 1;

                    string percentage = outputText.Substring(startIndex, length);

                    // Update the loading bar with the extracted percentage
                    UpdateLoadingBar(percentage);
                }
            }
        }
    }
}
