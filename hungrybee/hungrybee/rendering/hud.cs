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
        Vector2 healthPosition;
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
            healthPosition = new Vector2();
        }
        #endregion

        #region Draw()
        public void Draw()
        {
            frameCounter++;

            h_game.h_RenderManager.spriteBatch.Begin();
            h_game.h_RenderManager.spriteBatch.DrawString(font, stringBuilder, fpsPosition, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
            h_game.h_RenderManager.spriteBatch.End();
        }
        #endregion

        #region Update()
        /// Update - Update the string to draw and the health icon.
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
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

            fpsPosition.X = h_game.h_GameSettings.xWindowSize * 0.05f; // Offset 5% from the top-left of the window
            fpsPosition.Y= h_game.h_GameSettings.yWindowSize * 0.05f;
        }
        #endregion
    }
}
