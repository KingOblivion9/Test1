using System.ComponentModel;
using Terraria.ModLoader.Config;
using AutoRerollFullPatch.AutoReroll;
namespace AutoReroll
{
    class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		[Range(1, 100)]
		[Label("AutoReforge Speed")]
		[DefaultValue(10)]
		[Increment(1)]
		[Tooltip("Reforge Per Sec")]
		public int ReforgePerSec;
		[DefaultValue(false)]
		public bool UseDefaultReforgeMenu;
		public override void OnChanged()
		{
			AutoRerollFix.ForgePerSec = ReforgePerSec;
			AutoRerollFix.UseDefaultReforgeMenu = UseDefaultReforgeMenu;
		}
	}
}
