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
    /// **                           gameSettings                            **
    /// ** This is a singleton class to store the application's setup state. **
    /// ** --> Also responsible for reading and writing settings to file.    **
    /// ***********************************************************************
    /// </summary>
    public class gameSettings : GameComponent
    {
        #region Local Variables
        // VARIABLES SAVED TO DISK
        public int      xWindowSize, yWindowSize;
        public string   skyPlaneTextureFile;
        public float    skyPlaneScale;
        public int      startingGameObjectCapacity;
        private int     renderSettingsIndex;
        public string   cartoonEffectFile;
        public string   postprocessEffectFile;
        public string   fontFile;
        public string   sketchTextureFile;
        public float    cameraSpeed;
        public float    cameraRunningMult;
        public float    cameraRotationSpeed;
        public float    EPSILON;
        public int      forceListCapacity;
        public float    physicsMinVel;
        public int      physicsObjectsStartingCapacity;
        public float    gravity;
        public bool     renderBoundingObjects;
        public bool     pauseOnCollision;
        public bool     renderCollisions;

        // VARIABLES NOT SAVED TO DISK
        private game h_game;
        renderSettings[] PresetRenderSettings = {new renderSettings("Cartoon", true, true, 1, 1, false, false, 0, 0, 0),
                                                 new renderSettings("Pencil", false, true, 0.5f, 0.5f, true, false, 0.1f, 0.3f, 0.05f),
                                                 new renderSettings("Chunky Monochrome", true, true, 1.5f, 0.5f, true, false, 0, 0.35f, 0),
                                                 new renderSettings("Colored Hatching", false, true, 0.5f, 0.333f, true, true, 0.2f, 0.5f, 0.075f),
                                                 new renderSettings("Cartoon and Shading", true, true, 1, 1, true, true, 0.1f, 0.3f, 0),
                                                 new renderSettings("Nothing", false, false, 0, 0, false, false, 0, 0, 0)};  

        //// ************************************
        //// *** 1. INSERT MORE SETTINGS HERE ***
        //// ************************************

        #endregion

        #region Constructor - gameSettings(game game)
        /// Initializes to default values 
        /// --> Likely these are overwritten later when loading from disk in Initialize() function.
        /// ***********************************************************************
        public gameSettings(game game) : base(game)  
        {
            xWindowSize = 800; yWindowSize = 600;
            skyPlaneTextureFile = "clouds"; //skyPlaneTextureFile = "clouds_resize";
            skyPlaneScale = 1.75f;
            renderSettingsIndex = 4;
            startingGameObjectCapacity = 64;
            cartoonEffectFile = "CartoonEffect";
            postprocessEffectFile = "PostprocessEffect";
            fontFile = "arial";
            sketchTextureFile = "SketchTexture";
            cameraSpeed = 0.05f;
            cameraRunningMult = 4.0f;
            cameraRotationSpeed = 0.002f;
            EPSILON = 0.00000001f;
            forceListCapacity = 4;
            physicsMinVel = 0.0001f;
            physicsObjectsStartingCapacity = 64;
            gravity = 2f;
            renderBoundingObjects = false;
            pauseOnCollision = false;
            renderCollisions = true;

            //// ************************************
            //// *** 2. INSERT MORE SETTINGS HERE ***
            //// ************************************

            h_game = (game)game;
        }
        #endregion

        #region Initialize()
        /// Perform full initialization and load values from config.csv on disk (if it can be found)
        /// ***********************************************************************
        public override void Initialize()
        {
            // See if we can open the config file and get the info
            this.ReadSettings();

            // Set the window size and call reset of graphics device
            h_game.GetGraphicsDeviceManager().PreferredBackBufferWidth = xWindowSize;
            h_game.GetGraphicsDeviceManager().PreferredBackBufferHeight = yWindowSize;
            h_game.GetGraphicsDeviceManager().ApplyChanges();
            ((cameraInterface)h_game.GetCamera()).Resize();

            // Now write the settings back to disk 
            // (applicable if config.ini doesn't exist and we're building it for the first time)
            this.WriteSettings();

            base.Initialize();
        }
        #endregion

        #region Update()
        /// Update - Nothing to update
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion

        #region ReadSettings()
        /// Try reading the config.ini file from disk.  
        /// It may not exist if we're writing it for the first time.
        /// ***********************************************************************
        private void ReadSettings()
        {
            csvHandleRead reader = new csvHandleRead(".\\gameSettings\\settings.csv");

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
                        case "skyPlaneScale":
                            this.skyPlaneScale = float.Parse(curToken[1]);
                            break;
                        case "renderSettingsIndex":
                            this.renderSettingsIndex = Convert.ToInt32(curToken[1]);
                            break;
                        case "startingGameObjectCapacity":
                            this.startingGameObjectCapacity = Convert.ToInt32(curToken[1]);
                            break;
                        case "cartoonEffectFile":
                            this.cartoonEffectFile = curToken[1];
                            break;
                        case "postprocessEffectFile":
                            this.postprocessEffectFile = curToken[1];
                            break;
                        case "fontFile":
                            this.fontFile = curToken[1];
                            break;
                        case "sketchTextureFile":
                            this.sketchTextureFile = curToken[1];
                            break;
                        case "cameraSpeed":
                            this.cameraSpeed =float.Parse(curToken[1]);
                            break;
                        case "cameraRunningMult":
                            this.cameraRunningMult = float.Parse(curToken[1]);
                            break;
                        case "cameraRotationSpeed":
                            this.cameraRotationSpeed = float.Parse(curToken[1]);
                            break;
                        case "EPSILON":
                            this.EPSILON = float.Parse(curToken[1]);
                            break;
                        case "forceListCapacity":
                            this.forceListCapacity = Convert.ToInt32(curToken[1]);
                            break;
                        case "physicsMinVel":
                            this.physicsMinVel = float.Parse(curToken[1]);
                            break;
                        case "gravity":
                            this.gravity = float.Parse(curToken[1]);
                            break;
                        case "physicsObjectsStartingCapacity":
                            this.physicsObjectsStartingCapacity = Convert.ToInt32(curToken[1]);
                            break;
                        case "renderBoundingObjects":
                            this.renderBoundingObjects = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "pauseOnCollision":
                            this.pauseOnCollision = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "renderCollisions":
                            this.renderCollisions = Convert.ToInt32(curToken[1]) == 1;
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
        #endregion

        #region WriteSettings()
        /// Write the config.ini file to disk.  This method should always work
        /// ***********************************************************************
        private void WriteSettings()
        {
            csvHandleWrite writer = new csvHandleWrite(".\\gameSettings\\settings.csv");

            writer.WriteNextToken("xWindowSize", this.xWindowSize);
            writer.WriteNextToken("yWindowSize", this.yWindowSize);
            writer.WriteNextToken("skyPlaneTextureFile", this.skyPlaneTextureFile);
            writer.WriteNextToken("skyPlaneScale", this.skyPlaneScale);
            writer.WriteNextToken("renderSettingsIndex", this.renderSettingsIndex);
            writer.WriteNextToken("startingGameObjectCapacity", this.startingGameObjectCapacity);
            writer.WriteNextToken("cartoonEffectFile", this.cartoonEffectFile);
            writer.WriteNextToken("postprocessEffectFile", this.postprocessEffectFile);
            writer.WriteNextToken("fontFile", this.fontFile);
            writer.WriteNextToken("sketchTextureFile", this.sketchTextureFile);
            writer.WriteNextToken("cameraSpeed", this.cameraSpeed);
            writer.WriteNextToken("cameraRunningMult", this.cameraRunningMult);
            writer.WriteNextToken("cameraRotationSpeed", this.cameraRotationSpeed);
            writer.WriteNextToken("EPSILON", this.EPSILON);
            writer.WriteNextToken("forceListCapacity", this.forceListCapacity);
            writer.WriteNextToken("physicsMinVel", this.physicsMinVel);
            writer.WriteNextToken("physicsObjectsStartingCapacity", this.physicsObjectsStartingCapacity);
            writer.WriteNextToken("gravity", this.gravity);
            writer.WriteNextToken("renderBoundingObjects", this.renderBoundingObjects ? (int)1 : (int)0 );
            writer.WriteNextToken("pauseOnCollision", this.pauseOnCollision ? (int)1 : (int)0);
            writer.WriteNextToken("renderCollisions", this.renderCollisions ? (int)1 : (int)0);

            //// ************************************
            //// *** 4. INSERT MORE SETTINGS HERE ***
            //// ************************************

            writer.Close(); // Destructor would do this anyway once out of scope, but just to be safe.
        }
        #endregion

        #region Access and Modifier functions
        public renderSettings RenderSettings { get { return PresetRenderSettings[renderSettingsIndex]; } }
        #endregion
    }
}
