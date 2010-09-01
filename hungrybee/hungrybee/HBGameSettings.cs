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
    /// **                         HBGameSettings                            **
    /// ** This is a singleton class to store the application's setup state. **
    /// ** --> Also responsible for reading and writing settings to file.    **
    /// ***********************************************************************
    /// </summary>
    class HBGameSettings : GameComponent
    {
        // Local variables
        public int xWindowSize, yWindowSize;
        private HBGame h_HBGame;

        /// <summary>
        /// Initializes to default values 
        /// --> Likely these are overwritten later when loading from disk in Initialize() function.
        /// ***********************************************************************
        /// </summary>
        public HBGameSettings(Game game) : base(game)  
        {
            xWindowSize = 640; yWindowSize = 480;
            h_HBGame = (HBGame)game;
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
            h_HBGame.getGraphics().PreferredBackBufferWidth = xWindowSize;
            h_HBGame.getGraphics().PreferredBackBufferHeight = yWindowSize;
            h_HBGame.getGraphics().ApplyChanges();

            // Now write the settings back to disk 
            // (applicable if config.ini doesn't exist and we're building it for the first time)
            this.WriteSettings();
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
                        throw new Exception("HBGameSettings::ReadSettings(): Corrupt settings.csv, expecting 2 to elements per token");
                    switch (curToken[0])
                    {
                        case "xWindowSize":
                            this.xWindowSize = Convert.ToInt32(curToken[1]);
                            break;
                        case "yWindowSize":
                            this.yWindowSize = Convert.ToInt32(curToken[1]);
                            break;

                        //// *********************************
                        //// *** INSERT MORE SETTINGS HERE ***
                        //// *********************************

                        default:
                            throw new Exception("HBGameSettings::ReadSettings(): Corrupt settings.csv, setting " + curToken[0] + " is not recognised");
                    }
                }
                reader.Close(); // Destructor would do this anyway once out of scope, but just to be safe.
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("HBGameSettings::ReadSettings(): The file " + reader.GetFileName() + " could not be read, possibly it doesn't exist"); System.Diagnostics.Debug.Flush();
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

            //// *********************************
            //// *** INSERT MORE SETTINGS HERE ***
            //// *********************************

            writer.Close(); // Destructor would do this anyway once out of scope, but just to be safe.
        }
    }
}
