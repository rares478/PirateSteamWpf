using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        private string xml = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam\\settings.xml");
        public Settings()
        {
            InitializeComponent();
            if(System.IO.File.Exists(xml) == false)
            {
                System.IO.File.Create(xml).Close();
                XmlDocument doc = new XmlDocument();

                XmlElement root = doc.CreateElement("Application");
                doc.AppendChild(root);

                XmlElement crackElement = doc.CreateElement("Crack");
                crackElement.InnerText = "Goldberg";
                root.AppendChild(crackElement);

                XmlElement startupElement = doc.CreateElement("Startup");
                startupElement.InnerText = "No";
                root.AppendChild(startupElement);

                doc.Save(xml);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml);
                XmlNodeList startupElements = doc.GetElementsByTagName("Startup");
                foreach (XmlNode startupElement in startupElements)
                {
                    if (startupElement.InnerText == "Yes")
                        rb_StartAtStartup.IsChecked = true;
                    else 
                        rb_NoStartAtStartup.IsChecked = true;
                }
                XmlNodeList crackElements = doc.GetElementsByTagName("Crack");
                foreach (XmlNode crackElement in crackElements)
                {
                    switch (crackElement.InnerText)
                    {
                        case "CreamAPI":
                            rb_CreamAPI.IsChecked = true;
                            break;
                        case "Goldberg":
                            rb_Goldberg.IsChecked = true;
                            break;
                        case "Goldberg Experimental":
                            rb_Goldberg_Experimental.IsChecked = true;
                            break;
                    }
                }
            }
        }

        private void rb_CreamAPI_Checked(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList startupElements = doc.GetElementsByTagName("Crack");
            foreach (XmlNode startupElement in startupElements)
            {
                startupElement.InnerText = "CreamAPI";
            }
            doc.Save(xml);
        }
        private void rb_Goldberg_Checked(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList startupElements = doc.GetElementsByTagName("Crack");
            foreach (XmlNode startupElement in startupElements)
            {
                startupElement.InnerText = "Goldberg";
            }
            doc.Save(xml);
        }

        private void rb_Goldberg_Experimental_Checked(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList startupElements = doc.GetElementsByTagName("Crack");
            foreach (XmlNode startupElement in startupElements)
            {
                startupElement.InnerText = "Goldberg Experimental";
            }
            doc.Save(xml);
        }


        private void rb_StartAtStartup_Checked(object sender, RoutedEventArgs e)
        {
            WshShell wshShell = new WshShell();
            IWshShortcut shortcut;
            string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            shortcut = (IWshShortcut)wshShell.CreateShortcut(startUpFolderPath + "\\" + "PirateSteam.lnk");
            MessageBox.Show(startUpFolderPath + "\\" + "PirateSteam.lnk");

            shortcut.TargetPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\WpfApp3.exe";
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            shortcut.Description = "Launch Pirate Steam";
            shortcut.Save();

            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList startupElements = doc.GetElementsByTagName("Startup");
            foreach (XmlNode startupElement in startupElements)
            {
                startupElement.InnerText = "Yes";
            }
            doc.Save(xml);
        }

        private void rb_NoStartAtStartup_Checked(object sender, RoutedEventArgs e)
        {
            string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if(System.IO.Path.Exists(startUpFolderPath+ "\\" + "PirateSteam.lnk"))
            {
                System.IO.File.Delete(startUpFolderPath + "\\" + "PirateSteam.lnk");

                XmlDocument doc = new XmlDocument();
                doc.Load(xml);
                XmlNodeList startupElements = doc.GetElementsByTagName("Startup");
                foreach (XmlNode startupElement in startupElements)
                {
                    startupElement.InnerText = "No";
                }
                doc.Save(xml);
            }
        }
    }
}
