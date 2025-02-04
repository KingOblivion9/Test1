using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using AutoRerollFullPatch.AutoReroll;
using AutoRerollFullPatch.GadgetBox.GadgetUI;

namespace AutoReroll;

internal class AutoRerollSystems : ModSystem
{
    private int lastSeenScreenWidth;
    private int lastSeenScreenHeight;
    private UserInterface userInterface;
    public override void OnModLoad()
    {
        userInterface = AutoRerollFix.Instance.userInterface;
    }
    public override void UpdateUI(GameTime gameTime)
    {
        if (AutoRerollFix.Instance.ReforgeMenu && AutoRerollFix.Instance.isInReforgeMenu == false)
        {
            userInterface.SetState(new ReforgeMachineUI());
            AutoRerollFix.Instance.isInReforgeMenu = true;
        }
        if (userInterface != null)
        {
            userInterface.Update(gameTime);
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "BestModifierRoll: MyInterface",
                delegate
                {
                    if (Main.playerInventory && !Main.recBigList)
                    {
                        if (lastSeenScreenWidth != Main.screenWidth || lastSeenScreenHeight != Main.screenHeight || Main.hasFocus)
                        {
                            userInterface.Recalculate();
                            lastSeenScreenWidth = Main.screenWidth;
                            lastSeenScreenHeight = Main.screenHeight;
                        }

                        userInterface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
                    }

                    return true;
                },
       InterfaceScaleType.UI));
        }
    }
}
