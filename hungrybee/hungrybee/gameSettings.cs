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
    /// **                           gameSettings                            **
    /// ** This is a singleton class to store the application's setup state. **
    /// ** --> Also responsible for reading and writing settings to file.    **
    /// ***********************************************************************
    /// </summary>
    public class gameSettings : GameComponent
    {
        // Local variables
        public int xWindowSize, yWindowSize;
        private game h_game;
        public string skyPlaneTextureFile;
        public string skyPlaneEffectsFile;

        //// ************************************
        //// *** 1. INSERT MORE SETTINGS HERE ***
        //// ************************************

        /// <summary>
        /// Initializes to default values 
        /// --> Likely these are overwritten later when loading from disk in Initialize() function.
        /// ***********************************************************************
        /// </summary>
        public gameSettings(game game) : base(game)  
        {
            xWindowSize = 640; yWindowSize = 480;
            skyPlaneTextureFile = "skyPlaneTexture";
            skyPlaneEffectsFile = "skyPlane";

            //// ************************************
            //// *** 2. INSERT MORE SETTINGS HERE ***
            //// ************************************

            h_game = (game)game;
        } 

        /// <summary>
        /// Perform full initialization and load values from config.csv on disk (if it can be found)
        /// ***********************************************************************
        /// </summary>
        public override void Initialize()
        {
            // See if we can open the config file and get the info
            this.ReadSettings();

            // Set the window size and call reset of graphics device
            h_game.GetGraphicsDeviceManager().PreferredBackBufferWidth = xWindowSize;
            h_game.GetGraphicsDeviceManager().PreferredBackBufferHeight = yWindowSize;
            h_game.GetGraphicsDeviceManager().ApplyChanges();

            // Now write the settings back to disk 
            // (applicable if config.ini doesn't exist and we're building it for the first time)
            this.WriteSettings();

            base.Initialize();
        }

        /// <summary>
        /// Update - Nothing to update
        /// ***********************************************************************
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Try reading the config.ini file from disk.  
        /// It may not exist if we're writing it for the first time.
        /// ***********************************************************************
        /// </summary>
        private void ReadSettings()
        {
            csvHandleRead reader = new csvHandleRead(".\\settings.csv");

            if (reader.IsOpen())
            {
                List<string> curToken = new List<string>();
                while (reader.ReadNextToken(ref curToken)) // ReadNextToken returns false when nothing to read
                {
                    // Expecting token to contain 2 strings
                    if (curToken.Count != 2)
                        throw new Exception("gameSettings::ReadSettings(): Corrupt settings.csv, expecting 2 to elements per token");
                    switch (curToken[0])
                    {
                        case "xWindowSize":
                            this.xWindowSize = Convert.ToInt32(curToken[1]);
                            break;
                        case "yWindowSize":
                            this.yWindowSize = Convert.ToInt32(curToken[1]);
                            break;
                        case "skyPlaneTextureFile":
                            this.skyPlaneTextureFile = curToken[1];
                            break;
                        case "skyPlaneEffectsFile":
                            this.skyPlaneEffectsFile = curToken[1];
                            break;

                        //// ************************************
                        //// *** 3. INSERT MORE SETTINGS HERE ***
                        //// ************************************

                        default:
                            throw new Exception("gameSettings::ReadSettings(): Corrupt settings.csv, setting " + curToken[0] + " is not recognised");
                    }
                }
                reader.Close(); // Destructor would do this anyway once out of scope, but just to be safe.
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("gameSettings::ReadSettings(): The file " + reader.GetFileName() + " could not be read, possibly it doesn't exist"); System.Diagnostics.Debug.Flush();
#endif
            }
        }

        /// <summary>
        /// Write the config.ini file to disk.  This method should always work
        /// ***********************************************************************
        /// </summary>
        private void WriteSettings()
        {
            csvHandleWrite writer = new csvHandleWrite(".\\settings.csv");

            writer.WriteNextToken("xWindowSize", this.xWindowSize);
            writer.WriteNextToken("yWindowSize", this.yWindowSize);
            writer.WriteNextToken("skyPlaneTextureFile", this.skyPlaneTextureFile);
            writer.WriteNextToken("skyPlaneEffectsFile", this.skyPlaneEffectsFile);

            //// ************************************
            //// *** 4. INSERT MORE SETTINGS HERE ***
            //// ************************************

            writer.Close(); // Destructor would do this anyway once out of scope, but just to be safe.
        }
    }
}
