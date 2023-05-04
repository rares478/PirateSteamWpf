using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp3.Classes
{
    internal class Util
    {

        public static string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static string FormatBits(double bits)
        {
            string[] sizes = { "B", "KiB", "MiB", "GiB", "TiB" };
            double len = bits;
            int order = 0;
            while(len>= 1000 && order <sizes.Length - 1)
            {
                order++;
                len /= 1000;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
