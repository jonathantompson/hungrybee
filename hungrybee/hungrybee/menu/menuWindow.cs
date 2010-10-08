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

    #region class menuWindowPlacement
    public class menuWindowPlacement
    {
        public float titleFontScale;          // FontScales are defined in 800x600 window and then scaled up/down if window size is larger / smaller
        public float itemFontScale;
        public float itemFontOffsetFromLeft;  // Percentage of window width
        public float titleFontOffsetFromLeft; // Percentage of window width
        public float itemFontOffsetFromTop;   // Percentage of window height
        public float titleFontOffsetFromTop;  // Percentage of window height
        public float itemFontSpacing;         // Percentage of window height
    }
    #endregion

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

        // Placement Settings - calculated values
        Rectangle backgroundImageSource;
        float backgroundImageScale;
        float itemSpacing;
        Vector2 itemPosition; // of the first item
        Vector2 titlePosition;
        float titleFontScaleWindow;
        float itemFontScaleWindow;

        #endregion

        #region Constructor - MenuWindow(SpriteFont spriteFont, string menuTitle, Texture2D backgroundImage)
        public menuWindow(game game, SpriteFont spriteFont, string menuTitle, Texture2D backgroundImage, menuWindowPlacement placement)
        {
            h_game = game;

            itemList = new List<MenuItem>();
            changeSpan = TimeSpan.FromSeconds(h_game.h_GameSettings.menuTransitionTime);
            selectedItem = 0;
            changeProgress = 0;
            windowState = windowState.Inactive;

            this.spriteFont = spriteFont;
            this.menuTitle = menuTitle;
            this.backgroundImage = backgroundImage;

            if (backgroundImage != null) // it's null for dummy menus
            {
                // Calculate the source background image so that the aspect ratio isn't messed up
                float imageAspectRatio = (float)backgroundImage.Width / (float)backgroundImage.Height;
                float windowAspectRatio = (float)h_game.h_GameSettings.xWindowSize / (float)h_game.h_GameSettings.yWindowSize;

                if (imageAspectRatio > windowAspectRatio) // image needs to be cropped in the x direction
                    backgroundImageSource = new Rectangle(0, 0, (int)Math.Floor(windowAspectRatio * backgroundImage.Height), backgroundImage.Height);
                else
                    backgroundImageSource = new Rectangle(0, 0, backgroundImage.Width, (int)Math.Floor(backgroundImage.Width / windowAspectRatio));
            
                // Calculate the scale to fill the window
                backgroundImageScale = (float)h_game.h_GameSettings.xWindowSize / backgroundImageSource.Width + 0.005f; // Add 5% scale to make sure there are no black boarders

                itemPosition = new Vector2(placement.itemFontOffsetFromLeft * ((float)h_game.h_GameSettings.xWindowSize),
                                           placement.itemFontOffsetFromTop * ((float)h_game.h_GameSettings.yWindowSize));
                titlePosition = new Vector2(placement.titleFontOffsetFromLeft * ((float)h_game.h_GameSettings.xWindowSize),
                                            placement.titleFontOffsetFromTop * ((float)h_game.h_GameSettings.yWindowSize));
                itemSpacing = placement.itemFontSpacing * ((float)h_game.h_GameSettings.yWindowSize);

                float xScale = ((float)h_game.h_GameSettings.xWindowSize) / 800.0f;
                float yScale = ((float)h_game.h_GameSettings.yWindowSize) / 600.0f;
                titleFontScaleWindow = placement.titleFontScale * Math.Min(xScale, yScale) ;
                itemFontScaleWindow = placement.itemFontScale * Math.Min(xScale, yScale);
            }
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
            int startSelectedItem = selectedItem;
            if (lastKeybState.IsKeyUp(Keys.Down) && currentKeybState.IsKeyDown(Keys.Down))
                selectedItem++;

            if (lastKeybState.IsKeyUp(Keys.Up) && currentKeybState.IsKeyDown(Keys.Up))
                selectedItem--;

            if (selectedItem < 0)
                selectedItem = 0;

            if (selectedItem >= itemList.Count)
                selectedItem = itemList.Count - 1;

            if (selectedItem != startSelectedItem)
                h_game.h_AudioManager.CueSound(soundType.MENU_UPDOWN);

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

            float horPositionOffset = 0;
            float alphaValue;
            float bgLayerDepth;
            Color bgColor;

            switch (windowState)
            {
                case windowState.Starting:
                    horPositionOffset -= 200 * (1.0f - (float)smoothedProgress);
                    alphaValue = smoothedProgress;
                    bgLayerDepth = 0.5f;
                    bgColor = new Color(new Vector4(1, 1, 1, alphaValue));
                    break;
                case windowState.Ending:
                    horPositionOffset += 200 * (float)smoothedProgress;
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

            // Draw the background
            spriteBatch.Draw(backgroundImage,               // Texture2D texturE
                             Vector2.Zero,                  // Vector2 position (from top-left)
                             backgroundImageSource,         // Rectangle sourceRectangle
                             bgColor,                       // Color color
                             0,                             // float rotation
                             Vector2.Zero,                  // Vector2 origin
                             backgroundImageScale,          // float scale
                             SpriteEffects.None,            // SpriteEffects effects
                             bgLayerDepth);                 // float layerDepth
            
            // Draw the menu title
            Vector2 curTitlePostition = titlePosition + new Vector2(horPositionOffset, 0.0f);
            spriteBatch.DrawString(spriteFont,                          // SpriteFont spriteFont
                                   menuTitle,                           // string text
                                   curTitlePostition,                   // Vector2 position
                                   titleColor,                          // Color color
                                   0,                                   // float rotation
                                   Vector2.Zero,                        // Vector2 origin
                                   titleFontScaleWindow,                // float scale
                                   SpriteEffects.None,                  // SpriteEffects effects
                                   0);                                  // float layerDepth

            Vector2 curItemPostition = itemPosition + new Vector2(horPositionOffset, 0.0f);
            for (int itemID = 0; itemID < itemList.Count; itemID++)
            {
                curItemPostition += new Vector2(0.0f, itemSpacing);
                Color itemColor = Color.White;

                if (itemID == selectedItem)
                    itemColor = new Color(new Vector4(1, 0, 0, alphaValue));
                else
                    itemColor = new Color(new Vector4(1, 1, 1, alphaValue));

                spriteBatch.DrawString(spriteFont,                      // SpriteFont spriteFont
                                       itemList[itemID].itemText,       // string text
                                       curItemPostition,                // Vector2 position
                                       itemColor,                       // Color color
                                       0,                               // float rotation
                                       Vector2.Zero,                    // Vector2 origin
                                       itemFontScaleWindow,             // float scale
                                       SpriteEffects.None,              // SpriteEffects effects
                                       0);                              // float layerDepth
            }
        }
        #endregion
    }
}
