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

namespace hungrybee
{
    
    /// <summary>
    /// ***********************************************************************
    /// **                             HBGame                                **
    /// ** Main singleton game class --> Initialized first on startup.       **
    /// ** --> Handles main game loop, initialization of devices and         **
    /// **     sub-systems and game logic.                                   **
    /// ** USEFULL: http://www.apress.com/book/view/143021855x BOOK SOURCE   **
    /// ***********************************************************************
    /// </summary>
    public class HBGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Effect HBEffect;                    // Handler to the main HB effect file
        GameComponent h_GameSettings;     // Handler to the HB Settings (both user and code customizable)
        GameComponent h_Camera;           // Handler to the HB Camera

        public HBGame() // Default constructor
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Create the GameComponent instances
            h_GameSettings = new HBGameSettings(this);
            h_Camera = new HBcamera(this);

            // Manually specify the update order for interdependancies
            h_GameSettings.UpdateOrder = 0;
            h_Camera.UpdateOrder = 1;

            // Add the new game components
            Components.Add(h_GameSettings);
            Components.Add(h_Camera);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// ***********************************************************************
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize(); // Call HBGame.Initilize() then .Initialize for all added components.
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// ***********************************************************************
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            HBEffect = Content.Load<Effect>("HBEffect");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// ***********************************************************************
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// ***********************************************************************
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// ***********************************************************************
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            cameraInterface camera = (cameraInterface)Services.GetService(typeof(cameraInterface));

            base.Draw(gameTime);
        }

        /// <summary>
        /// A bunch of "inline" functions (though not supported in C#)
        /// ***********************************************************************
        /// </summary>
        public GraphicsDeviceManager getGraphics() { return graphics; }
        public GameComponent getCamera() { return h_Camera; }
    }
}
