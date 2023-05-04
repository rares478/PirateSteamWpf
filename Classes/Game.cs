using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp3.Classes
{
    public class Game
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string Path_Directory { get; set; }
        public string Type { get; set; }
        public string Background { get; set; }
        public long Last_Played { get; set; }
        public int SteamAppid { get; set; }
        public string Logo { get; set; }

        public int Installed = 0; // 0 not installed 1 installed 2 installing

        public List<DLC> DLCs = new List<DLC>() { };
        public double Size { get; set; }

    }

    public class DLC
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
