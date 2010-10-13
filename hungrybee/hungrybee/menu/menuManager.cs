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
                h_game.h_AudioManager.PlayMenuMusic();
                // Check if the menu music is running and if not then run it
                if (!h_game.h_AudioManager.menuMusicPlaying)
                {
                    MediaPlayer.Play(h_game.h_AudioManager.menuMusic);
                    h_game.h_AudioManager.menuMusicPlaying = true;
                }

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
                h_game.h_GameObjectManager.StartLevel(h_game.h_GameSettings.startingLevel);
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
                h_game.h_AudioManager.CueSound(soundType.MENU_ENTER);
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
        /// LoadContent()
        /// ***********************************************************************
        public void LoadContent()
        {
            lastKeybState = Keyboard.GetState();
            menuList = new List<menuWindow>();
            menusRunning = true;

            SpriteFont menuFont = h_game.Content.Load<SpriteFont>(h_game.h_GameSettings.menuFont);
            Texture2D mainMenuImage = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.menuBG);
            Texture2D optionsMenuImage = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.menuBGOptions);
            spriteBatch = new SpriteBatch(h_game.GraphicsDevice);

            // Dummy menuWindows to run external commands
            gameStart = new menuWindow(h_game, null, null, null, null);
            gameExit = new menuWindow(h_game, null, null, null, null);

            // Create the main menu
            menuWindowPlacement mainMenuPlacement = new menuWindowPlacement();
            mainMenuPlacement.titleFontScale = 0.94f;
            mainMenuPlacement.itemFontScale = 0.5f;
            mainMenuPlacement.itemFontOffsetFromLeft = 0.1f;
            mainMenuPlacement.titleFontOffsetFromLeft = 0.1f;
            mainMenuPlacement.itemFontOffsetFromTop = 0.5f;
            mainMenuPlacement.titleFontOffsetFromTop = 0.4f;
            mainMenuPlacement.itemFontSpacing = 0.1f;
            mainMenu = new menuWindow(h_game, menuFont, "HuNGrY BeE!", mainMenuImage, mainMenuPlacement);

            // Create the Options menu
            menuWindowPlacement optionsMenuPlacement = new menuWindowPlacement();
            optionsMenuPlacement.titleFontScale = 0.94f;
            optionsMenuPlacement.itemFontScale = 0.4f;
            optionsMenuPlacement.itemFontOffsetFromLeft = 0.1f;
            optionsMenuPlacement.titleFontOffsetFromLeft = 0.1f;
            optionsMenuPlacement.itemFontOffsetFromTop = 0.5f;
            optionsMenuPlacement.titleFontOffsetFromTop = 0.4f;
            optionsMenuPlacement.itemFontSpacing = 0.08f;
            menuWindow optionsMenu = new menuWindow(h_game, menuFont, "oPtiOns", optionsMenuImage, optionsMenuPlacement);

            //  Add main menu items
            mainMenu.AddMenuItem("nEw GAmE", gameStart);
            mainMenu.AddMenuItem("oPtiOns", optionsMenu);
            mainMenu.AddMenuItem("eXit gaMe", gameExit);

            // Add options menu items
            optionsMenu.AddMenuItem("Change controls", mainMenu);
            optionsMenu.AddMenuItem("Change graphics setting", mainMenu);
            optionsMenu.AddMenuItem("Change sound setting", mainMenu);
            optionsMenu.AddMenuItem("Back to Main menu", mainMenu);

            // Add menus to the menu list
            menuList.Add(mainMenu);
            menuList.Add(optionsMenu);

            if (h_game.h_GameSettings.skipMenu)
            {
                activeMenu = gameStart;
            }
            else
            {
                activeMenu = mainMenu;
                mainMenu.WakeUp();
            }

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
