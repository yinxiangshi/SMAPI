using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Menus;

namespace StardewModdingAPI.Inheritance.Menus
{
    public class SBobberBar : BobberBar
    {
        public static FieldInfo[] PrivateFields { get { return GetPrivateFields(); } }

        /// <summary>
        /// DO NOT CONSTRUCT THIS CLASS
        /// This class ONLY provides functionality to access the base BobberBar class fields.
        /// </summary>
        /// <param name="whichFish"></param>
        /// <param name="fishSize"></param>
        /// <param name="treasure"></param>
        /// <param name="bobber"></param>
        public SBobberBar(int whichFish, float fishSize, bool treasure, int bobber) : base(whichFish, fishSize, treasure, bobber)
        {
            
        }

        public static FieldInfo[] GetPrivateFields()
        {
            return typeof (BobberBar).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
