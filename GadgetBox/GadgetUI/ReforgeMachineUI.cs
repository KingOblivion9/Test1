using AutoReroll;
using AutoRerollFullPatch.AutoReroll;
using AutoRerollFullPatch.AutoReroll.Gadgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutoRerollFullPatch.GadgetBox.GadgetUI
{
    internal class ReforgeMachineUI : UIState
    {
        internal UIPanel reforgePanel;
        internal UIItemSlot reforgeSlot;
        internal UIFancyButton reforgeButton;
        internal UIPanel reforgeListPanel;
        internal UIList reforgeList;
        internal UIMoneyPanel moneyPanel;

        private List<int> selectedPrefixes = new List<int>();
        private int reforgePrice;
        private bool autoReforge;
        private double tickCounter;
        private double silenceCounter;
        private int lastItem = ItemID.None;
        private readonly int PrefixCountX2 = PrefixLoader.PrefixCount * 2;
        private readonly Item air = new Item();

        private bool isDragging;
        private Vector2 dragOffset;

        public override void OnInitialize()
        {
            Main.recBigList = false;
            Main.playerInventory = true;
            Main.hidePlayerCraftingMenu = true;

            reforgePanel = new UIReforgePanel(() => reforgeSlot.item, () => reforgePrice);
            reforgePanel.SetPadding(4);
            reforgePanel.Top.Pixels = Main.instance.invBottom + 10;
            reforgePanel.Left.Pixels = 65;
            reforgePanel.MinHeight.Pixels = 300;
            reforgePanel.OnLeftMouseDown += delegate
            {
                if (reforgePanel.ContainsPoint(Main.MouseScreen))
                {
                    isDragging = true;
                    dragOffset = Main.MouseScreen - reforgePanel.GetDimensions().Position();
                }
            };
            reforgePanel.OnLeftMouseUp += delegate
            {
                isDragging = false;
            };

            reforgeSlot = new UIItemSlot(0.85f);
            reforgeSlot.Top.Pixels = reforgeSlot.Left.Pixels = 12;
            reforgeSlot.CanClick += () => Main.mouseItem.type == ItemID.None || Main.mouseItem.Prefix(-3);
            reforgeSlot.OnLeftMouseDown += (a, b) => { selectedPrefixes.Clear(); OnItemChanged(); };
            reforgePanel.Append(reforgeSlot);

            moneyPanel = new UIMoneyPanel();
            moneyPanel.Left.Pixels = 170;
            moneyPanel.Top.Pixels = 45;
            moneyPanel.BackgroundColor = Color.Transparent;
            moneyPanel.BorderColor = Color.Transparent;
            moneyPanel.Visible = false;
            reforgePanel.Append(moneyPanel);

            reforgeButton = new UIFancyButton(TextureAssets.Reforge[0].Value, TextureAssets.Reforge[1].Value);
            reforgeButton.Top.Pixels = 20;
            reforgeButton.Left.Pixels = 64;
            reforgeButton.CanClick += CanReforgeItem;
            reforgeButton.OnLeftMouseDown += OnReforgeButtonClick;
            reforgeButton.HoverText = Language.GetTextValue("LegacyInterface.19");
            reforgePanel.Append(reforgeButton);

            reforgeListPanel = new UIPanel();
            reforgeListPanel.Top.Pixels = 80;
            reforgeListPanel.Left.Pixels = 12;
            reforgeListPanel.Width.Set(-24, 1);
            reforgeListPanel.Height.Set(-82, 1);
            reforgeListPanel.SetPadding(6);
            reforgeListPanel.BackgroundColor = Color.CadetBlue;
            reforgePanel.Append(reforgeListPanel);

            reforgeList = new UIList();
            reforgeList.Width.Precent = reforgeList.Height.Precent = 1f;
            reforgeList.Width.Pixels = -24;
            reforgeList.ListPadding = 2;
            reforgeListPanel.Append(reforgeList);

            var reforgeListScrollbar = new FixedUIScrollbar(AutoRerollFix.Instance.userInterface);
            reforgeListScrollbar.SetView(100f, 1000f);
            reforgeListScrollbar.Top.Pixels = 4;
            reforgeListScrollbar.Height.Set(-8, 1f);
            reforgeListScrollbar.Left.Set(-20, 1f);
            reforgeListPanel.Append(reforgeListScrollbar);
            reforgeList.SetScrollbar(reforgeListScrollbar);


            Append(reforgePanel);
        }

        public override void OnDeactivate()
        {
            if (!reforgeSlot.item.IsAir)
            {
                var source = reforgeSlot.item.GetSource_DropAsItem();
                Main.LocalPlayer.QuickSpawnItem(source, reforgeSlot.item, reforgeSlot.item.stack);
                reforgeSlot.item.TurnToAir();
            }

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isDragging)
            {
                Vector2 mousePosition = Main.MouseScreen - dragOffset;
                reforgePanel.Left.Set(mousePosition.X, 0f);
                reforgePanel.Top.Set(mousePosition.Y, 0f);
                reforgePanel.Recalculate();
            }

            bool closeUI = false;
            bool silent = false;
            if (Main.LocalPlayer.talkNPC == -1)
            {
                AutoRerollFix.Instance.ReforgeMenu = false;
            }
            if (!AutoRerollFix.Instance.ReforgeMenu)
            {
                closeUI = true;
            }

            if (closeUI)
            {
                if (!silent)
                {
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                AutoRerollFix.Instance.userInterface.SetState(null);
                AutoRerollFix.Instance.isInReforgeMenu = false;
                AutoRerollFix.Instance.ReforgeMenu = false;
                return;
            }

            reforgePrice = reforgeSlot.item.ReforgePrice();
            if (autoReforge)
            {

                tickCounter += gameTime.ElapsedGameTime.TotalMilliseconds;

                silenceCounter += .001f * gameTime.ElapsedGameTime.TotalMilliseconds;

                reforgeButton.Rotation += 10 * .001f * gameTime.ElapsedGameTime.Milliseconds;
                if (selectedPrefixes.Count == 0 || selectedPrefixes.Contains(reforgeSlot.item.prefix) || !CanReforgeItem())
                {
                    autoReforge = false;
                    tickCounter = 0;
                }
                else if (tickCounter > 1000 / AutoRerollFix.ForgePerSec)
                {

                    tickCounter = 0;
                    ReforgeItem(silenceCounter < .2f ? true : false);
                    if (silenceCounter > .2f) silenceCounter = 0;
                    if (selectedPrefixes.Contains(reforgeSlot.item.prefix))
                    {
                        autoReforge = false;
                        tickCounter = 0;
                    }
                }
            }
            else if (reforgeButton.Rotation != 0)
            {
                if (reforgeButton.Rotation > MathHelper.TwoPi)
                {
                    reforgeButton.Rotation %= MathHelper.TwoPi;
                }
                reforgeButton.Rotation = MathHelper.TwoPi - reforgeButton.Rotation <= 0.2f ? 0 : reforgeButton.Rotation + 0.2f;

            }
            else
            {
                silenceCounter = .2f;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Main.hidePlayerCraftingMenu = true;
            if (reforgePanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.HoverItem.TurnToAir();
                Main.hoverItemName = "";
            }
            reforgeButton.Visible = !reforgeSlot.item.IsAir;
            moneyPanel.Visible = !reforgeSlot.item.IsAir;
        }

        private void OnReforgeButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (autoReforge)
            {
                autoReforge = false;
                tickCounter = 0;
            }
            else if (selectedPrefixes.Count > 0)
            {
                autoReforge = true;
            }
            else
            {
                ReforgeItem(false);
            }
        }

        private bool CanReforgeItem() => !reforgeSlot.item.IsAir && !selectedPrefixes.Contains(reforgeSlot.item.prefix) &&
            Main.LocalPlayer.CanAfford(reforgePrice + moneyPanel.GetMoneyValue(), -1);

        private void OnItemChanged()
        {
            reforgeList.Clear();
            if (reforgeSlot.item.IsAir)
            {
                return;
            }
            if (lastItem != reforgeSlot.item.type)
            {
                UpdateValidPrefixes();
                lastItem = reforgeSlot.item.type;
            }

            UpdateReforgeList();
        }


        private List<Item> validPrefixes;
        private void UpdateValidPrefixes()
        {

            validPrefixes = new List<Item>();

            if (reforgeSlot.item.IsAir)
                return;
            Item item = reforgeSlot.item.Clone();

            var validPrefixValues = new HashSet<int>();
            int remainingAttempts = 100;
            while (remainingAttempts > 0)
            {
                item.SetDefaults(item.type);
                item.Prefix(-2);
                remainingAttempts--;
                if (item.prefix != 0 && validPrefixValues.Add(item.prefix))
                {
                    remainingAttempts = 100;
                    validPrefixes.Add(item.Clone());
                }
            }

            validPrefixes = validPrefixes.OrderBy(x => x.rare).ToList();
        }

        private void UpdateReforgeList()
        {
            Item controlItem = reforgeSlot.item.Clone();

            controlItem.netDefaults(reforgeSlot.item.netID);
            controlItem = reforgeSlot.item.Clone();

            UIReforgeLabel reforgeLabel;
            List<int> tempSelected = new List<int>();
            foreach (Item item in validPrefixes)
            {
                Item tempItem = controlItem.Clone();
                tempItem.ResetPrefix();
                tempItem.Prefix(item.prefix);
                reforgeLabel = new UIReforgeLabel(tempItem);
                reforgeLabel.OnLeftMouseDown += ChoseReforge;
                reforgeLabel.SetPadding(10);
                if (selectedPrefixes.Contains(item.prefix))
                {
                    reforgeLabel.selected = true;
                    tempSelected.Add(item.prefix);
                }
                reforgeList.Add(reforgeLabel);
            }
            selectedPrefixes = tempSelected;
        }

        private void bReforge_onLeftClick(object sender, EventArgs e)
        {
            reforgeSlot.item.Prefix(-2);
        }

        private void ChoseReforge(UIMouseEvent evt, UIElement listeningElement)
        {
            UIReforgeLabel element = (UIReforgeLabel)listeningElement;
            element.selected = !element.selected;
            if (!selectedPrefixes.Remove(element.shownItem.prefix))
            {
                selectedPrefixes.Add(element.shownItem.prefix);
            }
            reforgeList.UpdateOrder();
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void ReforgeItem(bool silent)
        {
            Main.LocalPlayer.BuyItem(reforgePrice, -1);
            GadgetMethods.PrefixItem(ref reforgeSlot.item, silent);
        }
    }
}