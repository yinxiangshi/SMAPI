using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewModdingAPI
{
    public static class Version
    {
        public const int MajorVersion = 0;
        public const int MinorVersion = 37;
        public const int PatchVersion = 1;
        public const string Build = "Alpha";

        public static string VersionString { 
            get
            {
                return string.Format("{0}.{1}.{2} {3}", MajorVersion, MinorVersion, PatchVersion, Build);
            }
         }
    }
}
