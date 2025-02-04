using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AutoRerollFullPatch.AutoReroll.Gadgets
{
    public static class GadgetMethods
    {
        public static int ReforgePrice(this Item item, Player player = null)
        {
            if (item == null || item.IsAir)
            {
                return 0;
            }
            if (player == null)
            {
                player = Main.LocalPlayer;
            }
            int reforgePrice = item.value;
            bool canApplyDiscount = true;
            if (ItemLoader.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount))
            {
                if (canApplyDiscount && player.discountAvailable)
                {
                    reforgePrice = (int)(reforgePrice * 0.8f);
                }
                reforgePrice /= 3;
            }
            return reforgePrice;
        }

        public static void PrefixItem(ref Item item, bool silent = false, bool reset = false)
        {
            ItemLoader.PreReforge(item);
            item.ResetPrefix();
            bool favorited = item.favorited;
            int stack = item.stack;
            Item tempItem = new Item();
            tempItem.netDefaults(item.netID);
            tempItem = item.Clone()/* tModPorter Note: Removed. Use Clone, ResetPrefix or Refresh */;
            if (!reset)
            {
                tempItem.Prefix(-2);
            }
            item = tempItem.Clone();
            item.Center = Main.LocalPlayer.Center;
            item.favorited = favorited;
            item.stack = stack;
            if (!reset)
            {
                ItemLoader.PostReforge(item);
            }
            PopupText.NewText(PopupTextContext.ItemReforge, item, item.stack, true, false);
            if (silent)
            {
                return;
            }
            SoundEngine.PlaySound(reset ? SoundID.Grab : SoundID.Item37);
        }

        public static long GetTotalMoney(this Player player)
        {
            long num = Utils.CoinsCount(out bool overflowing, player.inventory, new int[5]
            {
                58,
                57,
                56,
                55,
                54
            });
            long num2 = Utils.CoinsCount(out overflowing, player.bank.item, new int[0]);
            long num3 = Utils.CoinsCount(out overflowing, player.bank2.item, new int[0]);
            long num4 = Utils.CoinsCount(out overflowing, player.bank3.item, new int[0]);
            long num5 = Utils.CoinsCount(out overflowing, player.bank4.item, new int[0]);
            long totalMoney = Utils.CoinsCombineStacks(out overflowing, new long[5]
            {
                num,
                num2,
                num3,
                num4,
                num5
            });
            return totalMoney;
        }
    }
}