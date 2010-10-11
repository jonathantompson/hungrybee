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
    public class hud : GameComponent
    {
        #region Local Variables
        game h_game;
        SpriteFont fontFPS;
        StringBuilder stringFPS;
        SpriteFont fontOverlay;
        StringBuilder stringOverlay;
        Vector2 fpsPosition;
        Vector2 overlayPosition;
        float fpsScale;
        float overlayScale;
        Rectangle healthPosition;
        Rectangle happyFace;
        Rectangle okFace;
        Rectangle crazyFace;
        Rectangle currentSourceRectangle;
        Texture2D beeTexture;
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime;
        bool drawOverlay;

        static float OVERLAY_OFFSET_FROM_WINDOW = 0.2f;

        #endregion

        #region Constructor - hud(game game)
        /// Initializes to default values 
        /// ***********************************************************************
        public hud(game game) : base(game)  
        {
            h_game = game;
            fontFPS = null;
            fontOverlay = null;
            stringFPS = null;
            stringOverlay = null;
            frameCounter = 0;
            frameRate = 0;
            elapsedTime = TimeSpan.Zero;
            fpsPosition = new Vector2();
            fpsScale = 0.5f;
            overlayPosition = new Vector2();
            overlayScale = 1.0f;
            healthPosition = new Rectangle();
            drawOverlay = false;
        }
        #endregion

        #region Draw()
        public void Draw()
        {
            frameCounter++;

            h_game.h_RenderManager.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            if(h_game.h_GameSettings.renderFPS)
                h_game.h_RenderManager.spriteBatch.DrawString(fontFPS, stringFPS, fpsPosition, Color.White, 0, new Vector2(0, 0), fpsScale, SpriteEffects.None, 0);

            if(drawOverlay)
                h_game.h_RenderManager.spriteBatch.DrawString(fontOverlay, stringOverlay, overlayPosition, Color.White, 0, new Vector2(0, 0), overlayScale, SpriteEffects.None, 0);

            h_game.h_RenderManager.spriteBatch.Draw(beeTexture, healthPosition, currentSourceRectangle, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
            h_game.h_RenderManager.spriteBatch.End();
        }
        #endregion

        #region Update()
        /// Update - Update the string to draw and the health icon.
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            if(!h_game.h_Menu.menusRunning)
            {
                if (h_game.h_GameSettings.renderFPS)
                {
                    elapsedTime += gameTime.ElapsedGameTime;

                    if (elapsedTime > TimeSpan.FromSeconds(1))
                    {
                        elapsedTime -= TimeSpan.FromSeconds(1);
                        frameRate = frameCounter;
                        frameCounter = 0;
                    }

                    stringFPS.Length = 0;
                    stringFPS.Append(string.Format("fps: {0}", frameRate));
                }

                // Pick the correct texture to rend
                if (((gameObjectPlayer)h_game.h_GameObjectManager.player).playerHealth > 75.0f)
                    currentSourceRectangle = happyFace;
                else if (((gameObjectPlayer)h_game.h_GameObjectManager.player).playerHealth > 50.0f)
                    currentSourceRectangle = okFace;
                else
                    currentSourceRectangle = crazyFace;
            }
        }
        #endregion

        #region Initialize()
        /// Perform initialization - Nothing to initialize
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Load in the textures and effects files
        /// ***********************************************************************
        public void LoadContent()
        {
            fontFPS = h_game.Content.Load<SpriteFont>(h_game.h_GameSettings.fontFPSFile);
            fontOverlay = h_game.Content.Load<SpriteFont>(h_game.h_GameSettings.menuFont);
            stringFPS = new StringBuilder();
            stringOverlay = new StringBuilder();

            float fpsOffset = 0.05f; // Offset % from the top-left of the window
            fpsPosition.X = (float)h_game.h_GameSettings.xWindowSize * fpsOffset;
            fpsPosition.Y = (float)h_game.h_GameSettings.yWindowSize * fpsOffset;

            beeTexture = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.beeFaceTextureFile);
            int width = h_game.h_GameSettings.beeFaceTextureWidth;
            int height = h_game.h_GameSettings.beeFaceTextureHeight;
            happyFace = new Rectangle(0, 0, width/3, height);
            okFace = new Rectangle(width / 3, 0, width / 3, height);
            crazyFace = new Rectangle(2 * width / 3, 0, width / 3, height);

            float aspectRatio = (width / 3.0f) / height;
            float healthSize = 0.15f; // percentage of window size (y dimension)
            float healthOffset = 0.05f; // // Offset % from the bottom-left of the window
            healthPosition.X = (int)((float)h_game.h_GameSettings.xWindowSize * healthOffset);
            healthPosition.Y = (int)((float)h_game.h_GameSettings.yWindowSize * (1.0f - healthSize - healthOffset));
            healthPosition.Height = (int)((float)h_game.h_GameSettings.yWindowSize * healthSize);
            healthPosition.Width = (int)((float)h_game.h_GameSettings.yWindowSize * healthSize * aspectRatio);
        }
        #endregion

        #region SetOverlay()
        public void SetOverlay(String str)
        {
            stringOverlay.Length = 0;
            stringOverlay.Append(str);

            // Get the string length and scale the font so it fits in the window with a certain padding from the sides
            Vector2 strSize = fontOverlay.MeasureString(stringOverlay);

            float xScale = ((float)h_game.h_GameSettings.xWindowSize * (1.0f-2.0f*OVERLAY_OFFSET_FROM_WINDOW)) / strSize.X;
            if (xScale > 1)
                xScale = 1.0f; // Don't make it bigger than the font size
            float yScale = ((float)h_game.h_GameSettings.yWindowSize * (1.0f-2.0f*OVERLAY_OFFSET_FROM_WINDOW)) / strSize.Y;
            if (yScale > 1)
                yScale = 1.0f; // Don't make it bigger than the font size

            overlayScale = Math.Min(xScale, yScale);

            // Now center the overlay in the middle
            overlayPosition.Y = ((float)h_game.h_GameSettings.yWindowSize * 0.5f) - ((strSize.Y * overlayScale) * 0.5f);

            if (xScale < 1.0f)
                overlayPosition.X = (float)h_game.h_GameSettings.xWindowSize * OVERLAY_OFFSET_FROM_WINDOW;
            else
                overlayPosition.X = ((float)h_game.h_GameSettings.xWindowSize - (strSize.X * overlayScale)) * 0.5f + OVERLAY_OFFSET_FROM_WINDOW;

        }
        #endregion

        #region ShowOverlay()
        public void ShowOverlay()
        {
            drawOverlay = true;
        }
        #endregion

        #region HideOverlay()
        public void HideOverlay()
        {
            drawOverlay = false;
        }
        #endregion
    }
}
