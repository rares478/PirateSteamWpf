using IWshRuntimeLibrary;
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
using System.Text.RegularExpressions;


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
        private double size;

        public Library()
        {
            InitializeComponent();

            Directory.CreateDirectory(path);
            games.Clear();
            games = UpdateSteamCMD.Items;
            games = games.OrderBy(g => g.Title, StringComparer.OrdinalIgnoreCase).ToList();
            lbLibrary.Items.Clear();

            List<Game> remove = new List<Game>();
            foreach (Game game in games)
            {
                if (game.Type == "App")
                    apps.Add(game);
                else
                {
                    if (game.Title != null)
                        lbLibrary.Items.Add(game);
                    else
                    {
                        remove.Add(game);
                    }
                }

            }
            foreach (Game game in apps)
            {
                games.Remove(game);
            }
            foreach (Game game in remove)
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

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            game.Background = "https://cdn.cloudflare.steamstatic.com/steam/apps/" + game.SteamAppid.ToString() + "/library_hero.jpg?t=1624181121";
            bitmapImage.UriSource = new Uri(game.Background, UriKind.Absolute);
            bitmapImage.EndInit();
            img_Background.Source = bitmapImage;

            BitmapImage bitmapImage2 = new BitmapImage();
            bitmapImage2.BeginInit();
            game.Logo = "https://cdn.cloudflare.steamstatic.com/steam/apps/" + game.SteamAppid.ToString() + "/logo.png?t=1624181121";
            bitmapImage2.UriSource = new Uri(game.Logo, UriKind.Absolute);
            bitmapImage2.EndInit();
            img_Logo.Source = bitmapImage2;

            if (game.Last_Played != null)
            {
                double epochTime1 = game.Last_Played;
                DateTime dateTime1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epochTime1);
                tb_LastPlayed.Text = dateTime1.ToString("MMM dd");
            }



            switch (game.Installed)
            {
                case 0:
                    {
                        bt_Play.Content = "Install";
                        bt_Play.Background = new SolidColorBrush(Color.FromArgb(100, 49, 132, 226));
                        break;
                    }
                case 1:
                    {
                        bt_Play.Content = "Play";
                        bt_Play.Background = new SolidColorBrush(Color.FromRgb(112, 214, 30));
                        break;
                    }
                case 2:
                    {
                        bt_Play.Content = "Pause";
                        bt_Play.Background = new SolidColorBrush(Color.FromArgb(100, 49, 132, 226));
                        break;
                    }
                case 3:
                    {
                        bt_Play.Content = "Resume";
                        bt_Play.Background = new SolidColorBrush(Color.FromArgb(100, 49, 132, 226));
                        break;
                    }
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
            switch (game.Installed)
            {
                case 0:
                    {
                        Install install = new Install(game, this);
                        install.ShowDialog();
                        break;
                    }
                case 1:
                    {
                        string dir = Directory.GetCurrentDirectory();
                        Directory.SetCurrentDirectory(Directory.GetParent(game.Path).ToString());
                        Process.Start(game.Path);
                        Directory.SetCurrentDirectory(dir);
                        break;
                    }
                case 2:
                    {
                        DownloadProcess.Kill();
                        DownloadProcess.Dispose();

                        Process[] processes = Process.GetProcessesByName("steamcmd");
                        foreach (Process process in processes)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                        game.Installed = 3;
                        bt_Play.Content = "Resume";
                        break;
                    }
                case 3:
                    {
                        downloadGame(game, "", game.Size);
                        break;
                    }
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


        private Process DownloadProcess;
        private int downloading;
        private string DownloadPath = null;

        public async Task<bool> downloadGame(Game game, string path, double size)
        {

            if (DownloadPath == null)
                DownloadPath = path;

            LoadingBar.Visibility = Visibility.Visible;
            tb_Downloading.Visibility = Visibility.Visible;
            tb_down_of_total.Visibility = Visibility.Visible;
            tb_space_req.Visibility = Visibility.Hidden;
            tb_Size.Visibility = Visibility.Hidden;
            bt_Play.Content = "Pause";
            game.Installed = 2;
            this.size = size;

            downloading = lbLibrary.Items.IndexOf(game);


            string steamcmdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "steamcmd.exe");
            string da = "D:\\Codeblocks\\coduri\\Visual Studio\\WpfApp3\\bin\\Debug\\net7.0-windows\\Steam\\RTconsole.exe";
            ProcessStartInfo DownloadProcessInfo = new ProcessStartInfo
            {
                FileName = da,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                Arguments = steamcmdPath + $" +force_install_dir {DownloadPath} +login rares478 +app_update {game.SteamAppid} validate +quit"
            };

            DownloadProcess = new Process();
            DownloadProcess.StartInfo = DownloadProcessInfo;
            DownloadProcess.OutputDataReceived += Process_OutputDataReceived;
            DownloadProcess.Start();
            DownloadProcess.BeginOutputReadLine();
            await DownloadProcess.WaitForExitAsync();
            return true;
        }

        private void UpdateLoadingBar(string percentageString)
        {
            float percentage;

            if (float.TryParse(percentageString.TrimEnd('%'), out percentage))
            {
                Dispatcher.Invoke(() =>
                {
                    tb_down_of_total.Text = Util.FormatBytes(Convert.ToInt64(percentage * size / 100)) + " of " + Util.FormatBytes(size);
                    LoadingBar.Value = percentage;
                });
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            if (!string.IsNullOrEmpty(e.Data))
            {
                string outputText = e.Data;

                Dispatcher.Invoke(() =>
                {
                    tb_scrie_aici.AppendText(outputText + Environment.NewLine);
                });

                if(outputText.Contains("Success! App"))
                {
                    
                    games[downloading].Installed = 1;
                    games[downloading].Path_Directory = DownloadPath;

                    string temp = games[downloading].Path;
                    games[downloading].Path = Path.Combine(DownloadPath, games[downloading].Path);

                    this.Dispatcher.Invoke(() =>
                    {
                        tb_Downloading.Visibility = Visibility.Collapsed;
                        tb_down_of_total.Visibility = Visibility.Collapsed;
                        tb_Size.Visibility = Visibility.Collapsed;
                        tb_space_req.Visibility = Visibility.Collapsed;
                        LoadingBar.Visibility = Visibility.Collapsed;
                        bt_Play.Content = "Play";
                        bt_Play.Background = new SolidColorBrush(Color.FromRgb(112, 214, 30));
                    });


                }
                else
                {

                    if (outputText.Contains("verifying"))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            tb_Downloading.Text = "Validating";
                        });
                    }

                    int percentageIndex = outputText.LastIndexOf('%');
                    Match match = Regex.Match(outputText, @"progress: (\d+\.\d+)");
                    if (match.Success)
                    {
                        {
                            string progressstr = match.Groups[1].Value;
                            double progress = double.Parse(progressstr);

                            UpdateLoadingBar(progressstr);
                        }
                    }
                }
            }
        }


        private void bt_Play_Down_Click(object sender, RoutedEventArgs e)
        {
            ppPlay.IsOpen = true;
        }

        private void bt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (games[lbLibrary.SelectedIndex].Installed == 2 || games[lbLibrary.SelectedIndex].Installed == 3)
            {
                DownloadProcess.Kill();
                DownloadProcess.Dispose();

                Process[] processes = Process.GetProcessesByName("steamcmd");
                foreach (Process process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }
                Directory.Delete(DownloadPath, true);
                games[lbLibrary.SelectedIndex].Installed = 0;
                bt_Play.Content = "Install";
            }
        }

        private void bt_Open_Folder_Click(object sender, RoutedEventArgs e)
        {
            Game game = games[lbLibrary.SelectedIndex];
            Process.Start("explorer.exe", DownloadPath);
        }

    }
}
