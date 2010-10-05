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

        // Rendering
        public string   skyPlaneTextureFile;
        public float    skyPlaneScale;
        private int     renderSettingsIndex;
        public string   cartoonEffectFile;
        public string   postprocessEffectFile;
        public string   fontFile;
        public string   sketchTextureFile;
        public string   beeFaceTextureFile;
        public int      beeFaceTextureWidth;
        public int      beeFaceTextureHeight;

        // Game Objects
        public int      startingGameObjectCapacity;

        // Camera
        public float    cameraSpeed;
        public float    cameraRunningMult;
        public float    cameraRotationSpeed;

        // Misc
        public float    EPSILON;

        // Game Play Settings
        public float    enemyCollisionAngleTollerence;
        public float    deathSequenceDuration;
        public float    enemyHealthImpact;
        public float    enemyPlayerCollisionVelocity;
        public float    deathSequenceScaleRateIncrease;

        // Physics
        public int      forceListCapacity;
        public int      physicsObjectsStartingCapacity;
        public float    gravity;
        public bool     renderBoundingObjects;
        public bool     pauseOnCollision;
        public bool     renderCollisions;
        public bool     limitXYCollisionResponce;
        public float    coeffRestitution;

        // Movement
        public float    enemyTimeToOrient;
        public float    playerTimeToOrient;
        public float    playerTimeToAccelerate;
        public float    playerMaxAcceleration;
        public float    playerVelocity;
        public float    playerJumpMomentum;

        public static Vector3 collisionMask = new Vector3();

        // VARIABLES NOT SAVED TO DISK
        private game h_game;
        renderSettings[] PresetRenderSettings = {new renderSettings("Cartoon", true, true, 1, 1, false, false, 0, 0, 0),
                                                 new renderSettings("Pencil", false, true, 0.5f, 0.5f, true, false, 0.1f, 0.3f, 0.05f),
                                                 new renderSettings("Chunky Monochrome", true, true, 1.5f, 0.5f, true, false, 0, 0.35f, 0),
                                                 new renderSettings("Colored Hatching", false, true, 0.5f, 0.333f, true, true, 0.2f, 0.5f, 0.075f),
                                                 new renderSettings("Cartoon and Shading", true, true, 0.5f, 0.5f, true, true, 0.01f, 0.3f, 0.1f),
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
            fontFile = "hudArialFont";
            sketchTextureFile = "SketchTexture";
            cameraSpeed = 0.1f;
            cameraRunningMult = 4.0f;
            cameraRotationSpeed = 0.002f;
            EPSILON = 0.00000001f;
            forceListCapacity = 4;
            physicsObjectsStartingCapacity = 64;
            gravity = 9.81f;
            renderBoundingObjects = true;
            pauseOnCollision = false;
            renderCollisions = false;
            limitXYCollisionResponce = true;
            coeffRestitution = 0.2f;
            enemyTimeToOrient = 0.2f;
            playerTimeToOrient = 0.2f;
            playerTimeToAccelerate = 0.2f;
            playerMaxAcceleration = 10.0f;
            playerVelocity = 2.5f;
            playerJumpMomentum = 6.0f;
            enemyCollisionAngleTollerence = 0.52359877f; // 30deg
            deathSequenceDuration = 0.8f;
            enemyHealthImpact = 25.0f;
            enemyPlayerCollisionVelocity = 5.0f;
            deathSequenceScaleRateIncrease = 2.0f;
            beeFaceTextureFile = "bee-cartoon_COMBINDED_transparent";
            beeFaceTextureWidth = 564;
            beeFaceTextureHeight = 180;

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
            h_game.h_GraphicsDeviceManager.PreferredBackBufferWidth = xWindowSize;
            h_game.h_GraphicsDeviceManager.PreferredBackBufferHeight = yWindowSize;
            h_game.h_GraphicsDeviceManager.ApplyChanges();
            ((cameraInterface)h_game.h_Camera).ResizeProjectionMatrix();

            // Now write the settings back to disk 
            // (applicable if config.ini doesn't exist and we're building it for the first time)
            this.WriteSettings();

            base.Initialize();

            if (limitXYCollisionResponce)
            { collisionMask.X = 1.0f; collisionMask.Y = 1.0f; collisionMask.Z = 0.0f; }
            else
            { collisionMask.X = 1.0f; collisionMask.Y = 1.0f; collisionMask.Z = 1.0f; }
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
                        case "limitXYCollisionResponce":
                            this.limitXYCollisionResponce = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "coeffRestitution":
                            this.coeffRestitution = float.Parse(curToken[1]);
                            break;
                        case "enemyTimeToOrient":
                            this.enemyTimeToOrient = float.Parse(curToken[1]);
                            break;
                        case "playerTimeToOrient":
                            this.playerTimeToOrient = float.Parse(curToken[1]);
                            break;
                        case "playerTimeToAccelerate":
                            this.playerTimeToAccelerate = float.Parse(curToken[1]);
                            break;
                        case "playerMaxAcceleration":
                            this.playerMaxAcceleration = float.Parse(curToken[1]);
                            break;
                        case "playerVelocity":
                            this.playerVelocity = float.Parse(curToken[1]);
                            break;
                        case "playerJumpMomentum":
                            this.playerJumpMomentum = float.Parse(curToken[1]);
                            break;
                        case "enemyCollisionAngleTollerence":
                            this.enemyCollisionAngleTollerence = float.Parse(curToken[1]);
                            break;
                        case "deathSequenceDuration":
                            this.deathSequenceDuration = float.Parse(curToken[1]);
                            break;
                       case "enemyHealthImpact":
                            this.enemyHealthImpact = float.Parse(curToken[1]);
                            break;
                       case "enemyPlayerCollisionVelocity":
                            this.enemyPlayerCollisionVelocity = float.Parse(curToken[1]);
                            break;
                       case "deathSequenceScaleRateIncrease":
                            this.deathSequenceScaleRateIncrease = float.Parse(curToken[1]);
                            break;
                       case "beeFaceTextureFile":
                            this.beeFaceTextureFile = curToken[1];
                            break;
                       case "beeFaceTextureWidth":
                            this.beeFaceTextureWidth = Convert.ToInt32(curToken[1]);
                            break;
                       case "beeFaceTextureHeight":
                            this.beeFaceTextureHeight = Convert.ToInt32(curToken[1]);
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
            writer.WriteNextToken("physicsObjectsStartingCapacity", this.physicsObjectsStartingCapacity);
            writer.WriteNextToken("gravity", this.gravity);
            writer.WriteNextToken("renderBoundingObjects", this.renderBoundingObjects ? (int)1 : (int)0 );
            writer.WriteNextToken("pauseOnCollision", this.pauseOnCollision ? (int)1 : (int)0);
            writer.WriteNextToken("renderCollisions", this.renderCollisions ? (int)1 : (int)0);
            writer.WriteNextToken("limitXYCollisionResponce", this.limitXYCollisionResponce ? (int)1 : (int)0);
            writer.WriteNextToken("coeffRestitution", this.coeffRestitution);
            writer.WriteNextToken("enemyTimeToOrient", this.enemyTimeToOrient);
            writer.WriteNextToken("playerTimeToOrient", this.playerTimeToOrient);
            writer.WriteNextToken("playerTimeToAccelerate", this.playerTimeToAccelerate);
            writer.WriteNextToken("playerMaxAcceleration", this.playerMaxAcceleration);
            writer.WriteNextToken("playerVelocity", this.playerVelocity);
            writer.WriteNextToken("playerJumpMomentum", this.playerJumpMomentum);
            writer.WriteNextToken("enemyCollisionAngleTollerence", this.enemyCollisionAngleTollerence);
            writer.WriteNextToken("deathSequenceDuration", this.deathSequenceDuration);
            writer.WriteNextToken("enemyHealthImpact", this.enemyHealthImpact);
            writer.WriteNextToken("enemyPlayerCollisionVelocity", this.enemyPlayerCollisionVelocity);
            writer.WriteNextToken("deathSequenceScaleRateIncrease", this.deathSequenceScaleRateIncrease);
            writer.WriteNextToken("beeFaceTextureFile", this.beeFaceTextureFile);
            writer.WriteNextToken("beeFaceTextureWidth", this.beeFaceTextureWidth);
            writer.WriteNextToken("beeFaceTextureHeight", this.beeFaceTextureHeight);

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
