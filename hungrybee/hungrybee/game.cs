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
        #region Local Variables
        GraphicsDeviceManager   h_GraphicsDeviceManager;
        GraphicsDevice          h_GraphicsDevice;       
        GameComponent           h_GameSettings;        
        GameComponent           h_Camera;               
        GameComponent           h_RenderManager;        
        GameComponent           h_SkyPlane;             // The background SkyPlane (like a skybox but with one side to save rendering calls)  
        GameComponent           h_GameObjectManager;

        #endregion

        #region Default Constructor - game()
        public game() // Default constructor
        {
            h_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            h_GraphicsDeviceManager.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            Content.RootDirectory = "Content";

            // Create the GameComponent instances
            h_GameSettings = new gameSettings(this);
            h_Camera = new camera(this);
            h_SkyPlane = new skyPlane(this);
            h_RenderManager = new renderManager(this);
            h_GameObjectManager = new gameObjectManager(this);

            // Manually specify the update order for interdependancies
            h_GameSettings.UpdateOrder      = 0;
            h_GameObjectManager.UpdateOrder = 1;
            h_Camera.UpdateOrder            = 2;
            h_RenderManager.UpdateOrder     = 3;
            h_SkyPlane.UpdateOrder          = 4;

            // Add the new game components
            Components.Add(h_GameSettings);
            Components.Add(h_Camera);
            Components.Add(h_SkyPlane);
            Components.Add(h_RenderManager);
            Components.Add(h_GameObjectManager);
        }
        #endregion

        #region Initialize()
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// ***********************************************************************
        protected override void Initialize()
        {
            h_GraphicsDevice = h_GraphicsDeviceManager.GraphicsDevice;
            base.Initialize(); // Call .Initialize() for all added components.
        }
        #endregion

        #region LoadContent()
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// ***********************************************************************
        protected override void LoadContent()
        {
            base.LoadContent(); // Call .LoadContent() for all added components.

            // Call LoadContent() for GameDevices that need it done manually
            ((skyPlane)h_SkyPlane).LoadContent();
            ((gameObjectManager)h_GameObjectManager).LoadContent(); // MUST BE CALLED BEFORE renderManager.LoadContent()!!
            ((renderManager)h_RenderManager).LoadContent();
            
        }
        #endregion

        #region UnloadContent()
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// ***********************************************************************
        protected override void UnloadContent()
        {
            // Call UnloadContent() for GameDevices that need it done manually
            ((renderManager)h_RenderManager).UnloadContent();
            ((gameObjectManager)h_GameObjectManager).UnloadContent();
        }
        #endregion

        #region Update()
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// ***********************************************************************
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit --> Later move to keyboard class
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            base.Update(gameTime); // Call .Update() for all added components.
        }
        #endregion
        
        #region Draw()
        /// This is called when the game should draw itself.
        /// I don't use ANY DrawableGameComponent instances...  I like to manage them myself
        /// ***********************************************************************
        protected override void Draw(GameTime gameTime)
        {
            // Simply send a draw request off to the render manager...
            ((renderManager)h_RenderManager).Draw(gameTime);
        }
        #endregion

        #region Access and Modifier functions
        /// A bunch of "inline" functions (though not supported in C#)
        /// ***********************************************************************
        public GraphicsDeviceManager GetGraphicsDeviceManager() { return h_GraphicsDeviceManager; }
        public GraphicsDevice GetGraphicsDevice() { return h_GraphicsDevice; }
        public GameComponent GetCamera() { return h_Camera; }
        public gameSettings GetGameSettings() { return (gameSettings)h_GameSettings; }
        public skyPlane GetSkyPlane() { return (skyPlane)h_SkyPlane; }
        public gameObjectManager GetGameObjectManager() { return (gameObjectManager)h_GameObjectManager; }
        #endregion
    }
}
