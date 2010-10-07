#region using statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace hungrybee
{
    public enum windowState { Starting, Active, Ending, Inactive }

    /// <summary>
    /// ***********************************************************************
    /// **                           MenuWindow                              **
    /// ** This is a class to store menu state for a single window           **
    /// ** Thiw was taken directly from XNA 3.0 Game Programming Recipes     **
    /// ***********************************************************************
    /// </summary>
    class menuWindow
    {

        #region struct MenuItem
        private struct MenuItem
        {
            public string itemText;
            public menuWindow itemLink;

            public MenuItem(string itemText, menuWindow itemLink)
            {
                this.itemText = itemText;
                this.itemLink = itemLink;
            }
        }
        #endregion

        #region Local Variables

        game h_game;

        private TimeSpan changeSpan;
        public windowState windowState;
        private List<MenuItem> itemList;
        private int selectedItem;
        private double changeProgress;

        private SpriteFont spriteFont;
        private string menuTitle;
        private Texture2D backgroundImage;

        // Placement Settings
        float titleFontScale;
        float itemFontScale;

        #endregion

        #region Constructor - MenuWindow(SpriteFont spriteFont, string menuTitle, Texture2D backgroundImage)
        public menuWindow(game game, SpriteFont spriteFont, string menuTitle, Texture2D backgroundImage)
        {
            h_game = game;

            itemList = new List<MenuItem>();
            changeSpan = TimeSpan.FromMilliseconds(800);
            selectedItem = 0;
            changeProgress = 0;
            windowState = windowState.Inactive;

            titleFontScale = 1.0f;
            itemFontScale = 0.5f;

            this.spriteFont = spriteFont;
            this.menuTitle = menuTitle;
            this.backgroundImage = backgroundImage;
        }
        #endregion

        #region AddMenuItem()
        public void AddMenuItem(string itemText, menuWindow itemLink)
        {
            MenuItem newItem = new MenuItem(itemText, itemLink);
            itemList.Add(newItem);
        }
        #endregion

        #region WakeUp()
        public void WakeUp()
        {
            windowState = windowState.Starting;
        }
        #endregion

        #region Update(double timePassedSinceLastFrame)
        public void Update(double timePassedSinceLastFrame)
        {
            if ((windowState == windowState.Starting) || (windowState == windowState.Ending))
                changeProgress += timePassedSinceLastFrame / changeSpan.TotalMilliseconds;

            if (changeProgress >= 1.0f)
            {
                changeProgress = 0.0f;
                if (windowState == windowState.Starting)
                    windowState = windowState.Active;
                else if (windowState == windowState.Ending)
                    windowState = windowState.Inactive;
            }
        }
        #endregion

        #region ProcessInput()
        public menuWindow ProcessInput(KeyboardState lastKeybState, KeyboardState currentKeybState)
        {
            if (lastKeybState.IsKeyUp(Keys.Down) && currentKeybState.IsKeyDown(Keys.Down))
                selectedItem++;

            if (lastKeybState.IsKeyUp(Keys.Up) && currentKeybState.IsKeyDown(Keys.Up))
                selectedItem--;

            if (selectedItem < 0)
                selectedItem = 0;

            if (selectedItem >= itemList.Count)
                selectedItem = itemList.Count - 1;

            if ((lastKeybState.IsKeyUp(Keys.Enter) && currentKeybState.IsKeyDown(Keys.Enter)))
            {
                windowState = windowState.Ending;
                return itemList[selectedItem].itemLink;
            }
            else if (lastKeybState.IsKeyUp(Keys.Escape) && currentKeybState.IsKeyDown(Keys.Escape))
            {
                return null;
            }
            else
                return this;
        }
        #endregion

        #region Draw()
        public void Draw(SpriteBatch spriteBatch)
        {
            if (windowState == windowState.Inactive)
                return;

            float smoothedProgress = MathHelper.SmoothStep(0, 1, (float)changeProgress);

            int verPosition = 300;
            float horPosition = 300;
            float alphaValue;
            float bgLayerDepth;
            Color bgColor;

            switch (windowState)
            {
                case windowState.Starting:
                    horPosition -= 200 * (1.0f - (float)smoothedProgress);
                    alphaValue = smoothedProgress;
                    bgLayerDepth = 0.5f;
                    bgColor = new Color(new Vector4(1, 1, 1, alphaValue));
                    break;
                case windowState.Ending:
                    horPosition += 200 * (float)smoothedProgress;
                    alphaValue = 1.0f - smoothedProgress;
                    bgLayerDepth = 1;
                    bgColor = Color.White;
                    break;
                default:
                    alphaValue = 1;
                    bgLayerDepth = 1;
                    bgColor = Color.White;
                    break;
            }

            Color titleColor = new Color(new Vector4(1, 1, 1, alphaValue));
            spriteBatch.Draw(backgroundImage, new Vector2(), null, bgColor, 0, Vector2.Zero, 1, SpriteEffects.None, bgLayerDepth);
            spriteBatch.DrawString(spriteFont, menuTitle, new Vector2(horPosition, 200), titleColor, 0, Vector2.Zero, titleFontScale, SpriteEffects.None, 0);

            for (int itemID = 0; itemID < itemList.Count; itemID++)
            {
                Vector2 itemPostition = new Vector2(horPosition, verPosition);
                Color itemColor = Color.White;

                if (itemID == selectedItem)
                    itemColor = new Color(new Vector4(1, 0, 0, alphaValue));
                else
                    itemColor = new Color(new Vector4(1, 1, 1, alphaValue));

                spriteBatch.DrawString(spriteFont, itemList[itemID].itemText, itemPostition, itemColor, 0, Vector2.Zero, itemFontScale, SpriteEffects.None, 0);
                verPosition += 30;
            }
        }
        #endregion
    }
}
