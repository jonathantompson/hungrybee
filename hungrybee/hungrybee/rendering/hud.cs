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
        SpriteFont font;
        StringBuilder stringBuilder;
        Vector2 fpsPosition;
        Rectangle healthPosition;
        Rectangle happyFace;
        Rectangle okFace;
        Rectangle crazyFace;
        Rectangle currentSourceRectangle;
        Texture2D beeTexture;
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime;
        #endregion

        #region Constructor - hud(game game)
        /// Initializes to default values 
        /// ***********************************************************************
        public hud(game game) : base(game)  
        {
            h_game = game;
            font = null;
            stringBuilder = null;
            frameCounter = 0;
            frameRate = 0;
            elapsedTime = TimeSpan.Zero;
            fpsPosition = new Vector2();
            healthPosition = new Rectangle();
        }
        #endregion

        #region Draw()
        public void Draw()
        {
            frameCounter++;

            h_game.h_RenderManager.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            h_game.h_RenderManager.spriteBatch.DrawString(font, stringBuilder, fpsPosition, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
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
                elapsedTime += gameTime.ElapsedGameTime;

                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    frameRate = frameCounter;
                    frameCounter = 0;
                }

                stringBuilder.Length = 0;
                stringBuilder.Append(string.Format("fps: {0}", frameRate));

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
            font = h_game.Content.Load<SpriteFont>(h_game.h_GameSettings.fontFile);
            stringBuilder = new StringBuilder();

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
    }
}
