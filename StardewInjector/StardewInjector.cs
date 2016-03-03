using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewInjector
{
    public class StardewInjector : Mod
    {
        public override string Name
        {
            get { return "Stardew Injector"; }
        }

        public override string Authour
        {
            get { return "Zoryn Aaron"; }
        }

        public override string Version
        {
            get { return "1.0"; }
        }

        public override string Description
        {
            get { return "Pulled from https://github.com/kevinmurphy678/Stardew_Injector and converted to a mod."; }
        }

        public static Stardew_Hooker hooker { get; set; }
        public override void Entry(params object[] objects)
        {
            if (objects.Length <= 0 || (objects.Length > 0 && objects[0].AsBool() == false))
            {
                hooker = new Stardew_Hooker();
                hooker.Initialize();
                hooker.ApplyHooks();
                hooker.Finalize();

                Program.LogInfo("INJECTOR ENTERED");
            }
            else if (objects.Length > 0 && objects[0].AsBool() == true)
            {
                Program.LogInfo("INJECTOR LAUNCHING");
                hooker.Run();
            }
            else
            {
                Program.LogError("INVALID PARAMETERS FOR INJECTOR");
            }
        }
    }
}
