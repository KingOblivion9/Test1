using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;

namespace AutoRerollFullPatch.AutoReroll
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class AutoRerollFix : Mod
    {
        public const string modName = "Auto Reforge Fix";
        public static AutoRerollFix Instance;
        public static UnifiedRandom Rng = new UnifiedRandom(Environment.TickCount);
        public static int ForgePerSec = 10;
        public static bool UseDefaultReforgeMenu = false;
        public bool isInReforgeMenu;
        public bool ReforgeMenu;
        public UserInterface userInterface;


        public override void Load()
        {
            Instance = this;
            if (!Main.dedServ)
            {
                userInterface = new UserInterface();
            }
        }
        public override void Unload()
        {
            Instance = null;
        }
    }
}
