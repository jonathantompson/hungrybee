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

        //    ***** RENDERING *****
        public int      xWindowSize;
        public int      yWindowSize;
        public string   skyPlaneTextureFile;
        public float    skyPlaneScale;
        private int     renderSettingsIndex;
        public string   cartoonEffectFile;
        public string   postprocessEffectFile;
        public string   fontFPSFile;
        public string   sketchTextureFile;
        public string   beeFaceTextureFile;
        public int      beeFaceTextureWidth;
        public int      beeFaceTextureHeight;
        public string   heightMapTextureFile;
        public bool     renderFPS;
        public bool     renderBoundingObjects;
        public bool     renderCollisions;

        //    **** GAME OBJECTS ***
        public int      startingGameObjectCapacity;

        //    ******* CAMERA ******
        public float    cameraSpeed;
        public float    cameraRunningMult;
        public float    cameraRotationSpeed;

        //    ******** MISC *******
        public float    EPSILON;

        //    ***** GAME PLAY *****
        public float    enemyCollisionAngleTollerence;
        public float    enemySequenceDuration;
        public float    enemyHealthImpact;
        public float    enemyPlayerCollisionVelocity;
        public float    enemySequenceScaleRateIncrease;
        public float    friendSequenceScaleRateIncrease;
        public float    friendSequenceDuration;
        public float    friendSequenceAngularVelocity;
        public float    playerDeathSequenceDuration;
        public float    playerDeathSequenceScaleRateIncrease;
        public float    playerDeathZVelocity;
        public float    playerDeathYAmplitude;
        public float    playerDeathYFuncTScale;
        public float    floatingAngularVel;
        public float    floatingAmplitude;
        public float    levelTransitionTime;
        public int      startingLevel;
        public bool     skipMenu;

        //    ****** PHYSICS ******
        public int      forceListCapacity;
        public int      physicsObjectsStartingCapacity;
        public float    gravity;
        public bool     pauseOnCollision;
        public bool     limitXYCollisionResponce;
        public float    coeffRestitution;

        //    ****** MOVEMENT *****
        public float    NPCTimeToOrient;
        public float    playerTimeToOrient;
        public float    playerTimeToAccelerate;
        public float    playerMaxAcceleration;
        public float    playerVelocity;
        public float    playerJumpMomentum;
        public float    playerCollisionPause;

        //    ******* MENU ********
        public string   menuFont;
        public string   menuBG;
        public string   menuBGOptions;
        public float    menuTransitionTime;

        //    ******* AUDIO *******
        public float    musicVolume;

        public static Vector3 collisionMask = new Vector3();

        // VARIABLES NOT SAVED TO DISK
        private game h_game;
        renderSettings[] PresetRenderSettings = {new renderSettings("Cartoon and Shading", true, true, 0.5f, 0.5f, false, false, 0.01f, 0.3f, 0.1f),
                                                 new renderSettings("Cartoon and Shading", true, true, 0.5f, 0.5f, true, true, 0.01f, 0.3f, 0.1f),
                                                 new renderSettings("Nothing", false, false, 0, 0, false, false, 0, 0, 0)};  

        #endregion

        #region Constructor - gameSettings(game game)
        /// Initializes to default values 
        /// --> Likely these are overwritten later when loading from disk in Initialize() function.
        /// ***********************************************************************
        public gameSettings(game game) : base(game)  
        {
            //   ***** RENDERING *****
            xWindowSize = 1280;
            yWindowSize = 1024;
            skyPlaneTextureFile = ".\\images\\clouds";
            skyPlaneScale = 1.75f;
            renderSettingsIndex = 0;
            cartoonEffectFile = ".\\effects\\CartoonEffect";
            postprocessEffectFile = ".\\effects\\PostprocessEffect";
            fontFPSFile = ".\\fonts\\hudArialFont";
            sketchTextureFile = ".\\images\\SketchTexture";
            beeFaceTextureFile = ".\\images\\bee-cartoon_COMBINDED_transparent";
            beeFaceTextureWidth = 564;
            beeFaceTextureHeight = 180;
            heightMapTextureFile = ".\\images\\Grass";
            renderFPS = true;
            renderBoundingObjects = false;
            renderCollisions = false;

            //   **** GAME OBJECTS ***
            startingGameObjectCapacity = 64;

            //   ******* CAMERA ******
            cameraSpeed = 0.1f;
            cameraRunningMult = 4;
            cameraRotationSpeed = 0.002f;

            //   ******** MISC *******
            EPSILON = 0.00000001f;

            //   ***** GAME PLAY *****
            enemyCollisionAngleTollerence = 0.5235988f;
            enemySequenceDuration = 0.2f;
            enemyHealthImpact = 33.34f;
            enemyPlayerCollisionVelocity = 2;
            enemySequenceScaleRateIncrease = 10;
            friendSequenceScaleRateIncrease = 30;
            friendSequenceDuration = 0.75f;
            friendSequenceAngularVelocity = 9.4247780f;
            playerDeathSequenceDuration = 1.0f;
            playerDeathSequenceScaleRateIncrease = 2;
            playerDeathZVelocity = 5;
            playerDeathYAmplitude = 1;
            playerDeathYFuncTScale = 3f;
            floatingAngularVel = 6.283185f; // 2PI per sec
            floatingAmplitude = 0.05f;
            levelTransitionTime = 1.5f;
            startingLevel = 1;
            skipMenu = false;

            //   ****** PHYSICS ******
            forceListCapacity = 4;
            physicsObjectsStartingCapacity = 64;
            gravity = 9.81f;
            pauseOnCollision = false;
            limitXYCollisionResponce = true;
            coeffRestitution = 0.2f;

            //   ****** MOVEMENT *****
            NPCTimeToOrient = 0.2f;
            playerTimeToOrient = 0.2f;
            playerTimeToAccelerate = 0.2f;
            playerMaxAcceleration = 10;
            playerVelocity = 5;
            playerJumpMomentum = 6;
            playerCollisionPause = 0.25f;

            //   ******* MENU ********
            menuFont = ".\\fonts\\Graffiti";
            menuBG = ".\\images\\menuBG";
            menuBGOptions = ".\\images\\menuBG2";
            menuTransitionTime = 0.3f;

            //   ******* AUDIO *******
            musicVolume = 0.2f;

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
                    if (curToken.Count != 2 && curToken.Count != 1)
                        throw new Exception("gameSettings::ReadSettings(): Corrupt settings.csv, expecting 2 to elements per token");
                    switch (curToken[0])
                    {
                        //    ***** RENDERING *****
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
                        case "cartoonEffectFile":
                            this.cartoonEffectFile = curToken[1];
                            break;
                        case "postprocessEffectFile":
                            this.postprocessEffectFile = curToken[1];
                            break;
                        case "fontFPSFile":
                            this.fontFPSFile = curToken[1];
                            break;
                        case "sketchTextureFile":
                            this.sketchTextureFile = curToken[1];
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
                        case "heightMapTextureFile":
                            this.heightMapTextureFile = curToken[1];
                            break;
                        case "renderFPS":
                            this.renderFPS = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "renderBoundingObjects":
                            this.renderBoundingObjects = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "renderCollisions":
                            this.renderCollisions = Convert.ToInt32(curToken[1]) == 1;
                            break;

                        //   **** GAME OBJECTS ***
                        case "startingGameObjectCapacity":
                            this.startingGameObjectCapacity = Convert.ToInt32(curToken[1]);
                            break;

                        //   ******* CAMERA ******
                        case "cameraSpeed":
                            this.cameraSpeed =float.Parse(curToken[1]);
                            break;
                        case "cameraRunningMult":
                            this.cameraRunningMult = float.Parse(curToken[1]);
                            break;
                        case "cameraRotationSpeed":
                            this.cameraRotationSpeed = float.Parse(curToken[1]);
                            break;

                        //   ******** MISC *******
                        case "EPSILON":
                            this.EPSILON = float.Parse(curToken[1]);
                            break;

                        //   ***** GAME PLAY *****
                        case "enemyCollisionAngleTollerence":
                            this.enemyCollisionAngleTollerence = float.Parse(curToken[1]);
                            break;
                        case "enemySequenceDuration":
                            this.enemySequenceDuration = float.Parse(curToken[1]);
                            break;
                        case "enemyHealthImpact":
                            this.enemyHealthImpact = float.Parse(curToken[1]);
                            break;
                        case "enemyPlayerCollisionVelocity":
                            this.enemyPlayerCollisionVelocity = float.Parse(curToken[1]);
                            break;
                        case "enemySequenceScaleRateIncrease":
                            this.enemySequenceScaleRateIncrease = float.Parse(curToken[1]);
                            break;
                        case "friendSequenceScaleRateIncrease":
                            this.friendSequenceScaleRateIncrease = float.Parse(curToken[1]);
                            break;
                        case "friendSequenceDuration":
                            this.friendSequenceDuration = float.Parse(curToken[1]);
                            break;
                        case "friendSequenceAngularVelocity":
                            this.friendSequenceAngularVelocity = float.Parse(curToken[1]);
                            break;
                        case "playerDeathSequenceDuration":
                            this.playerDeathSequenceDuration = float.Parse(curToken[1]);
                            break;   
                        case "playerDeathSequenceScaleRateIncrease":
                            this.playerDeathSequenceScaleRateIncrease = float.Parse(curToken[1]);
                            break;
                        case "playerDeathZVelocity":
                            this.playerDeathZVelocity = float.Parse(curToken[1]);
                            break;
                        case "playerDeathYAmplitude":
                            this.playerDeathYAmplitude = float.Parse(curToken[1]);
                            break;
                        case "playerDeathYFuncTScale":
                            this.playerDeathYFuncTScale = float.Parse(curToken[1]);
                            break;
                        case "floatingAngularVel":
                            this.floatingAngularVel = float.Parse(curToken[1]);
                            break;
                        case "floatingAmplitude":
                            this.floatingAmplitude = float.Parse(curToken[1]);
                            break;
                        case "levelTransitionTime":
                            this.levelTransitionTime = float.Parse(curToken[1]);
                            break;
                        case "startingLevel":
                            this.startingLevel = Convert.ToInt32(curToken[1]);
                            break;
                        case "skipMenu":
                            this.skipMenu = Convert.ToInt32(curToken[1]) == 1;
                            break;

                        //   ****** PHYSICS ******
                        case "forceListCapacity":
                            this.forceListCapacity = Convert.ToInt32(curToken[1]);
                            break;
                        case "physicsObjectsStartingCapacity":
                            this.physicsObjectsStartingCapacity = Convert.ToInt32(curToken[1]);
                            break;
                        case "gravity":
                            this.gravity = float.Parse(curToken[1]);
                            break;
                        case "pauseOnCollision":
                            this.pauseOnCollision = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "limitXYCollisionResponce":
                            this.limitXYCollisionResponce = Convert.ToInt32(curToken[1]) == 1;
                            break;
                        case "coeffRestitution":
                            this.coeffRestitution = float.Parse(curToken[1]);
                            break;

                        //   ****** MOVEMENT *****
                        case "NPCTimeToOrient":
                            this.NPCTimeToOrient = float.Parse(curToken[1]);
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
                        case "playerCollisionPause":
                            this.playerCollisionPause = float.Parse(curToken[1]);
                            break;

                        //   ******* MENU ********
                       case "menuFont":
                            this.menuFont = curToken[1];
                            break;
                       case "menuBG":
                            this.menuBG = curToken[1];
                            break;
                       case "menuBGOptions":
                            this.menuBGOptions = curToken[1];
                            break;
                       case "menuTransitionTime":
                            this.menuTransitionTime = float.Parse(curToken[1]);
                            break;

                       //   ******* AUDIO *******
                       case "musicVolume":
                            this.musicVolume = float.Parse(curToken[1]);
                            break;

                       //   ******* FORMATTING *******
                       case "//": // Comment
                            break;
                       case "": // Empty line
                            break;
                           
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

            writer.WriteNextToken("//", "   ***** RENDERING *****");
            writer.WriteNextToken("xWindowSize", this.xWindowSize);
            writer.WriteNextToken("yWindowSize", this.yWindowSize);
            writer.WriteNextToken("skyPlaneTextureFile", this.skyPlaneTextureFile);
            writer.WriteNextToken("skyPlaneScale", this.skyPlaneScale);
            writer.WriteNextToken("renderSettingsIndex", this.renderSettingsIndex);
            writer.WriteNextToken("cartoonEffectFile", this.cartoonEffectFile);
            writer.WriteNextToken("postprocessEffectFile", this.postprocessEffectFile);
            writer.WriteNextToken("fontFPSFile", this.fontFPSFile);
            writer.WriteNextToken("sketchTextureFile", this.sketchTextureFile);
            writer.WriteNextToken("beeFaceTextureFile", this.beeFaceTextureFile);
            writer.WriteNextToken("beeFaceTextureWidth", this.beeFaceTextureWidth);
            writer.WriteNextToken("beeFaceTextureHeight", this.beeFaceTextureHeight);
            writer.WriteNextToken("heightMapTextureFile", this.heightMapTextureFile);
            writer.WriteNextToken("renderFPS", this.renderFPS ? (int)1 : (int)0);
            writer.WriteNextToken("renderBoundingObjects", this.renderBoundingObjects ? (int)1 : (int)0);
            writer.WriteNextToken("renderCollisions", this.renderCollisions ? (int)1 : (int)0);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   **** GAME OBJECTS ***");
            writer.WriteNextToken("startingGameObjectCapacity", this.startingGameObjectCapacity);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   ******* CAMERA ******");
            writer.WriteNextToken("cameraSpeed", this.cameraSpeed);
            writer.WriteNextToken("cameraRunningMult", this.cameraRunningMult);
            writer.WriteNextToken("cameraRotationSpeed", this.cameraRotationSpeed);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   ******** MISC *******");
            writer.WriteNextToken("EPSILON", this.EPSILON);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   ***** GAME PLAY *****");
            writer.WriteNextToken("enemyCollisionAngleTollerence", this.enemyCollisionAngleTollerence);
            writer.WriteNextToken("enemySequenceDuration", this.enemySequenceDuration);
            writer.WriteNextToken("enemyHealthImpact", this.enemyHealthImpact);
            writer.WriteNextToken("enemyPlayerCollisionVelocity", this.enemyPlayerCollisionVelocity);
            writer.WriteNextToken("enemySequenceScaleRateIncrease", this.enemySequenceScaleRateIncrease);
            writer.WriteNextToken("friendSequenceScaleRateIncrease", this.friendSequenceScaleRateIncrease);
            writer.WriteNextToken("friendSequenceDuration", this.friendSequenceDuration);
            writer.WriteNextToken("friendSequenceAngularVelocity", this.friendSequenceAngularVelocity);
            writer.WriteNextToken("playerDeathSequenceDuration", this.playerDeathSequenceDuration);
            writer.WriteNextToken("playerDeathSequenceScaleRateIncrease", this.playerDeathSequenceScaleRateIncrease);
            writer.WriteNextToken("playerDeathZVelocity", this.playerDeathZVelocity);
            writer.WriteNextToken("playerDeathYAmplitude", this.playerDeathYAmplitude);
            writer.WriteNextToken("playerDeathYFuncTScale", this.playerDeathYFuncTScale);
            writer.WriteNextToken("floatingAngularVel", this.floatingAngularVel);
            writer.WriteNextToken("floatingAmplitude", this.floatingAmplitude);
            writer.WriteNextToken("levelTransitionTime", this.levelTransitionTime);
            writer.WriteNextToken("startingLevel", this.startingLevel);
            writer.WriteNextToken("skipMenu", this.skipMenu ? (int)1 : (int)0 );
            writer.WriteNewLine();
            
            writer.WriteNextToken("//", "   ****** PHYSICS ******");
            writer.WriteNextToken("forceListCapacity", this.forceListCapacity);
            writer.WriteNextToken("physicsObjectsStartingCapacity", this.physicsObjectsStartingCapacity);
            writer.WriteNextToken("gravity", this.gravity);
            writer.WriteNextToken("pauseOnCollision", this.pauseOnCollision ? (int)1 : (int)0);
            writer.WriteNextToken("limitXYCollisionResponce", this.limitXYCollisionResponce ? (int)1 : (int)0);
            writer.WriteNextToken("coeffRestitution", this.coeffRestitution);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   ****** MOVEMENT *****");
            writer.WriteNextToken("NPCTimeToOrient", this.NPCTimeToOrient);
            writer.WriteNextToken("playerTimeToOrient", this.playerTimeToOrient);
            writer.WriteNextToken("playerTimeToAccelerate", this.playerTimeToAccelerate);
            writer.WriteNextToken("playerMaxAcceleration", this.playerMaxAcceleration);
            writer.WriteNextToken("playerVelocity", this.playerVelocity);
            writer.WriteNextToken("playerJumpMomentum", this.playerJumpMomentum);
            writer.WriteNextToken("playerCollisionPause", this.playerCollisionPause);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   ******* MENU ********");
            writer.WriteNextToken("menuFont", this.menuFont);
            writer.WriteNextToken("menuBG", this.menuBG);
            writer.WriteNextToken("menuBGOptions", this.menuBGOptions);
            writer.WriteNextToken("menuTransitionTime", this.menuTransitionTime);
            writer.WriteNewLine();

            writer.WriteNextToken("//", "   ******* AUDIO *******");
            writer.WriteNextToken("musicVolume", this.musicVolume);
            writer.WriteNewLine();

            writer.Close(); // Destructor would do this anyway once out of scope, but just to be safe.
        }
        #endregion

        #region Access and Modifier functions
        public renderSettings RenderSettings { get { return PresetRenderSettings[renderSettingsIndex]; } }
        #endregion
    }
}
