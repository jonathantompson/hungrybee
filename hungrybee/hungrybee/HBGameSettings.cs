using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hungrybee
{
    /// <summary>
    /// This is a singleton class to store the application's setup state
    /// </summary>
    class HBGameSettings
    {
        // Local variables
        public int xWindowSize, yWindowSize;

        /// <summary>
        /// Initializes to default values 
        /// --> Likely these are overwritten later when loading from disk in Initialize() function.
        /// </summary>
        public HBGameSettings()
        {
            xWindowSize = 640; yWindowSize = 480;
        }

        /// <summary>
        /// Perform full initialization and load values from config.csv on disk (if it can be found)
        /// </summary>
        public void Initialize()
        {
            // Set the window size and call reset of graphics device
            //HBGame.graphics.PreferredBackBufferWidth = xWindowSize;
            //HBGame.graphics.PreferredBackBufferHeight = yWindowSize;
            //HBGame.graphics.ApplyChanges();
        }        
    }
}
