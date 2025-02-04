using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AutoRerollFullPatch.AutoReroll;

namespace AutoReroll
{
    class AutoReforgeGoblin : GlobalNPC
    {


        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == NPCID.GoblinTinkerer;
        }

        public override bool PreChatButtonClicked(NPC npc, bool firstButton)
        {
            if (firstButton == false && !AutoRerollFix.UseDefaultReforgeMenu)
            {
                Main.npcChatText = "";
                AutoRerollFix.Instance.ReforgeMenu = true;
            }
            return true;
        }


    }
}
