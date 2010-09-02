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
    /// **                              game                                 **
    /// ** Main singleton game class --> Initialized first on startup.       **
    /// ** --> Handles main game loop, initialization of devices and         **
    /// **     sub-systems and game logic.                                   **
    /// ** USEFULL: http://www.apress.com/book/view/143021855x BOOK SOURCE   **
    /// ***********************************************************************
    /// </summary>
    public class game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager   h_GraphicsDeviceManager;
        GraphicsDevice          h_GraphicsDevice;           // Handler to the graphics device
        BasicEffect             h_Effect;           // Handler to the main effect
        GameComponent           h_GameSettings;     // Handler to the Settings (both user and code customizable)
        GameComponent           h_Camera;           // Handler to the Camera
        DrawableGameComponent   h_SkyPlane;         // Handler to the SkyPlane (like a skybox but with one side to save rendering calls)

        coordCross              h_coordCross;       // Handler to the temporary debug class     

        public game() // Default constructor
        {
            h_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Create the GameComponent and DrawableGameComponent instances
            h_GameSettings = new gameSettings(this);
            h_Camera = new camera(this);
            h_SkyPlane = new skyPlane(this);

            // Manually specify the update order for interdependancies
            h_GameSettings.UpdateOrder = 0;
            h_Camera.UpdateOrder = 1;
            h_SkyPlane.UpdateOrder = 2;

            // Manually specify the draw order for DrawableGameComponent
            h_SkyPlane.DrawOrder = 0;

            // Add the new game components
            Components.Add(h_GameSettings);
            Components.Add(h_Camera);
            Components.Add(h_SkyPlane);
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
            h_GraphicsDevice = h_GraphicsDeviceManager.GraphicsDevice;
            base.Initialize(); // Call .Initialize() for all added components.
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// ***********************************************************************
        /// </summary>
        protected override void LoadContent()
        {
            h_Effect = new BasicEffect(h_GraphicsDevice, null);
            h_coordCross = new coordCross(h_GraphicsDevice);
            base.LoadContent(); // Call .LoadContent() for all added components.
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
            // Allows the game to exit --> Later move to keyboard class
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape) || 
                Keyboard.GetState().IsKeyDown(Keys.Q) )
                this.Exit();

            base.Update(gameTime); // Call .Update() for all added components.
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// ***********************************************************************
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Get a pointer to the camera interface
            cameraInterface camera = (cameraInterface)Services.GetService(typeof(cameraInterface));

            h_GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            base.Draw(gameTime); // RENDER DRAWABLEGAMECOMPONENTs FIRST --> NEED TO RENDER SKYPLANE FIRST

            h_Effect.World = Matrix.Identity;
            h_Effect.View = camera.ViewMatrix;
            h_Effect.Projection = camera.ProjectionMatrix;
            h_Effect.Begin();
            foreach (EffectPass pass in h_Effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                h_coordCross.DrawUsingPresetEffect();
                pass.End();
            }
            h_Effect.End();

            base.Draw(gameTime);

        }

        /// <summary>
        /// A bunch of "inline" functions (though not supported in C#)
        /// ***********************************************************************
        /// </summary>
        public GraphicsDeviceManager GetGraphicsDeviceManager() { return h_GraphicsDeviceManager; }
        public GraphicsDevice GetGraphicsDevice() { return h_GraphicsDevice; }
        public GameComponent GetCamera() { return h_Camera; }
        public gameSettings GetGameSettings() { return (gameSettings)h_GameSettings; }
    }
}
