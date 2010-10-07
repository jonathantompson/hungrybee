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
    /// **                          menuManager                              **
    /// ** Singleton class to hold and store all the menu states.            **
    /// ***********************************************************************
    /// </summary>
    public class menuManager : GameComponent
    {
        #region Local Variables

        game h_game;

        List<menuWindow> menuList;
        menuWindow activeMenu;
        menuWindow mainMenu;
        KeyboardState lastKeybState;

        // Dummy menus
        menuWindow gameStart;
        menuWindow gameExit;

        public bool menusRunning;
        SpriteBatch spriteBatch;

        #endregion

        #region Constructor - gameObjectManager(game game)
        /// Initializes to default values
        /// ***********************************************************************
        public menuManager(game game)
            : base(game)  
        {
            h_game = (game)game;
        }
        #endregion

        #region Update()
        /// Update()
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            if(menusRunning)
            {
                KeyboardState keybState = Keyboard.GetState();

                foreach (menuWindow currentMenu in menuList)
                    currentMenu.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
                MenuInput(keybState);

                lastKeybState = keybState;
            }

            base.Update(gameTime);
        }
        #endregion

        #region MenuInput()
        private void MenuInput(KeyboardState currentKeybState)
        {
            menuWindow newActive = activeMenu.ProcessInput(lastKeybState, currentKeybState);

            if (newActive == gameStart)
            {
                //set level to easy
                h_game.h_GameObjectManager.StartLevel(1);
                menusRunning = false;
                activeMenu = newActive;
            }
            else if (newActive == gameExit)
            {
                h_game.Exit();
            }
            else if (newActive == null && activeMenu == mainMenu)
            {
                // if a level is loaded then return
                if (h_game.h_GameObjectManager.loadedLevel != -1)
                {
                    menusRunning = false;
                    h_game.h_PhysicsManager.UnpauseGame();
                }
            }
            else if (newActive == null)
            {
                activeMenu.windowState = windowState.Ending;
                mainMenu.WakeUp();
                activeMenu = mainMenu;
            }
            else if (newActive != activeMenu)
            {
                newActive.WakeUp();
                activeMenu = newActive;
            }
        }
        #endregion

        #region Initialize()
        /// Initialize - Nothing to Initialize --> All done in LoadContent()
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region LoadContent()
        /// Update()
        /// ***********************************************************************
        public void LoadContent()
        {
            lastKeybState = Keyboard.GetState();
            menuList = new List<menuWindow>();
            menusRunning = true;

            SpriteFont menuFont = h_game.Content.Load<SpriteFont>(h_game.h_GameSettings.menuFont);
            Texture2D backgroundImage = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.menuBG);
            Texture2D bg = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.menuBG2);
            spriteBatch = new SpriteBatch(h_game.GraphicsDevice);

            // Dummy menuWindows to run external commands
            gameStart = new menuWindow(h_game, null, null, null);
            gameExit = new menuWindow(h_game, null, null, null);

            mainMenu = new menuWindow(h_game, menuFont, "Main Menu", backgroundImage);
            menuWindow optionsMenu = new menuWindow(h_game, menuFont, "Options Menu", backgroundImage);
            menuList.Add(mainMenu);
            menuList.Add(optionsMenu);

            mainMenu.AddMenuItem("New Game", gameStart);
            mainMenu.AddMenuItem("Options", optionsMenu);
            mainMenu.AddMenuItem("Exit Game", gameExit);

            optionsMenu.AddMenuItem("Change controls", mainMenu);
            optionsMenu.AddMenuItem("Change graphics setting", mainMenu);
            optionsMenu.AddMenuItem("Change sound setting", mainMenu);
            optionsMenu.AddMenuItem("Back to Main menu", mainMenu);

            activeMenu = mainMenu;
            mainMenu.WakeUp();

        }
        #endregion

        #region Draw()
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            foreach (menuWindow currentMenu in menuList)
                currentMenu.Draw(spriteBatch);
            spriteBatch.End();
        }
        #endregion

        #region EnterMainMenu()
        public void EnterMainMenu()
        {
            activeMenu = mainMenu;
            mainMenu.WakeUp();
            menusRunning = true;
            lastKeybState = Keyboard.GetState(); // This will cause us not to exit prematrualy
        }
        #endregion
    }
}
