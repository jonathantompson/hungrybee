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

        // Make these public to avoid large access function penalties (no inline in C#)
        // THESE ALL DERIVE FROM GameComponent CLASS
        public GraphicsDeviceManager   h_GraphicsDeviceManager;
        public GraphicsDevice h_GraphicsDevice;
        public gameSettings h_GameSettings;
        public camera h_Camera;
        public renderManager h_RenderManager;
        public skyPlane h_SkyPlane;             // The background SkyPlane (like a skybox but with one side to save rendering calls)  
        public gameObjectManager h_GameObjectManager;
        public physicsManager h_PhysicsManager;
        public hud h_Hud;
        public menuManager h_Menu;
        public audioManager h_AudioManager;

        KeyboardState lastKeyboardState;

        #endregion

        #region Default Constructor - game()
        public game() // Default constructor
        {
            h_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            h_GraphicsDeviceManager.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            Content.RootDirectory = "Content";

            base.IsFixedTimeStep = false;
            h_GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;

            // Create the GameComponent instances
            h_GameSettings = new gameSettings(this);
            h_Camera = new camera(this);
            h_SkyPlane = new skyPlane(this);
            h_RenderManager = new renderManager(this);
            h_GameObjectManager = new gameObjectManager(this);
            h_PhysicsManager = new physicsManager(this);
            h_Hud = new hud(this);
            h_Menu = new menuManager(this);
            h_AudioManager = new audioManager(this);

            // Manually specify the update order for interdependancies
            h_GameSettings.UpdateOrder      = 0;
            h_Menu.UpdateOrder              = 1;
            h_GameObjectManager.UpdateOrder = 2;
            h_Camera.UpdateOrder            = 3;
            h_RenderManager.UpdateOrder     = 4;
            h_SkyPlane.UpdateOrder          = 5;
            h_PhysicsManager.UpdateOrder    = 6;
            h_Hud.UpdateOrder               = 7;
            h_AudioManager.UpdateOrder      = 8;

            // Add the new game components
            Components.Add(h_GameSettings);
            Components.Add(h_Camera);
            Components.Add(h_SkyPlane);
            Components.Add(h_RenderManager);
            Components.Add(h_GameObjectManager);
            Components.Add(h_PhysicsManager);
            Components.Add(h_Hud);
            Components.Add(h_Menu);
            Components.Add(h_AudioManager);
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
            lastKeyboardState = Keyboard.GetState();
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
            h_SkyPlane.LoadContent();
            h_Menu.LoadContent();
            h_GameObjectManager.LoadContent(); // Will call physicsManager.LoadContent() itself internally
            h_RenderManager.LoadContent();
            h_Hud.LoadContent();
            h_AudioManager.LoadContent();
        }
        #endregion

        #region UnloadContent()
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// ***********************************************************************
        protected override void UnloadContent()
        {
            // Call UnloadContent() for GameDevices that need it done manually
            h_RenderManager.UnloadContent();
            h_GameObjectManager.UnloadContent();
        }
        #endregion

        #region Update()
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// ***********************************************************************
        protected override void Update(GameTime gameTime)
        {
            if (!h_Menu.menusRunning)
            {
                KeyboardState curKeyboardState = Keyboard.GetState();

                if (lastKeyboardState.IsKeyUp(Keys.P) && curKeyboardState.IsKeyDown(Keys.P))
                    h_PhysicsManager.PauseUnpauseGame(); // Toggle the physics engine to change pause state

                // Allows the game to enter the main menu --> Later move to keyboard class
                if (lastKeyboardState.IsKeyUp(Keys.Escape) && curKeyboardState.IsKeyDown(Keys.Escape))
                {
                    h_PhysicsManager.PauseGame();
                    h_Menu.EnterMainMenu();
                }

                lastKeyboardState = curKeyboardState;
            }

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
            h_RenderManager.Draw(gameTime);
        }
        #endregion
    }
}
