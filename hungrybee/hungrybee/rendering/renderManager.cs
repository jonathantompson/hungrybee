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
    /// **                          renderManager                            **
    /// ** This is a singleton class to store and manage the application's   **
    /// ** renderer related variables.  Effects, etc.                        **
    /// ** LOTS OF CODE HERE TAKEN FROM XNA CREATORS CLUB:                   **
    /// ** Non Photo Realistic Rendering example (I like the effects)        **
    /// http://digierr.spaces.live.com/blog/cns!2B7007E9EC2AE37B!1792.entry
    /// ***********************************************************************
    /// </summary>
    public class renderManager : GameComponent
    {
        #region Local Variables

        // Local variables
        private game            h_game;
        Random                  random;
        public SpriteBatch      spriteBatch;
        Effect                  postprocessEffect;      // Effect used to apply the edge detection and pencil sketch postprocessing.
        Texture2D               sketchTexture;          // Overlay texture containing the pencil sketch stroke pattern.
        Vector2                 sketchJitter;           // Randomly offsets the sketch pattern to create a hand-drawn animation effect.
        TimeSpan                timeToNextJitter;
        RenderTarget2D          sceneRenderTarget;      // Custom rendertargets.
        RenderTarget2D          normalDepthRenderTarget;
        Effect                  cartoonEffect;

        public bool             changeModelsEffects;    // Before the next render, change the effects in the models (happens on each level load)

        #endregion

        #region Constructor - renderManager(game game)
        /// Initializes to default values
        /// ***********************************************************************
        public renderManager(game game) : base(game)  
        {
            h_game = (game)game;
            random = new Random();
        }
        #endregion

        #region ClearLevel()
        /// ClearLevel()
        /// ***********************************************************************
        public void ClearLevel()
        {
            changeModelsEffects = true; // Since the models are new we'll need to change the effects again
        }
        #endregion

        #region Initialize()
        /// Perform initialization - Nothing to initialize
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Load in the textures and effects files
        /// ***********************************************************************
        public void LoadContent()
        {

            spriteBatch = new SpriteBatch(h_game.h_GraphicsDevice);
            postprocessEffect = h_game.Content.Load<Effect>(h_game.h_GameSettings.postprocessEffectFile);
            sketchTexture = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.sketchTextureFile);

            // Change the model to use our custom cartoon shading effect.
            cartoonEffect = h_game.Content.Load<Effect>(h_game.h_GameSettings.cartoonEffectFile);

            // Since we've just loaded the models we'll need to call for a effects change
            changeModelsEffects = true;

            // Create two custom rendertargets.
            PresentationParameters pp = h_game.h_GraphicsDevice.PresentationParameters;

            sceneRenderTarget = new RenderTarget2D(h_game.h_GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            normalDepthRenderTarget = new RenderTarget2D(h_game.h_GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1,
                pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);
        }
        #endregion

        #region UnloadContent()
        /// UnloadContent - Unload the custom render targets
        /// ***********************************************************************
        public void UnloadContent()
        {
            if (sceneRenderTarget != null)
            {
                sceneRenderTarget.Dispose();
                sceneRenderTarget = null;
            }
            if (normalDepthRenderTarget != null)
            {
                normalDepthRenderTarget.Dispose();
                normalDepthRenderTarget = null;
            }
        }
        #endregion

        #region Update()
        /// Update - Update jitter Effect
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the sketch overlay texture jitter animation.
            if (h_game.h_GameSettings.RenderSettings.SketchJitterSpeed > 0)
            {
                timeToNextJitter -= gameTime.ElapsedGameTime;

                if (timeToNextJitter <= TimeSpan.Zero)
                {
                    sketchJitter.X = (float)random.NextDouble();
                    sketchJitter.Y = (float)random.NextDouble();

                    timeToNextJitter += TimeSpan.FromSeconds(h_game.h_GameSettings.RenderSettings.SketchJitterSpeed);
                }
            }
            base.Update(gameTime);
        }
        #endregion

        #region Draw()
        /// Update - Update jitter Effect
        /// ***********************************************************************
        public void Draw(GameTime gameTime)
        {
            if (h_game.h_Menu.menusRunning)
            {
                h_game.h_GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

                h_game.h_Menu.Draw(gameTime);
            }
            else
            {
                if (changeModelsEffects)
                {
                    h_game.h_GameObjectManager.ChangeEffectUsedByModels(cartoonEffect);
                    changeModelsEffects = false;
                }

                // Get a pointer to the camera interface
                cameraInterface camera = (cameraInterface)h_game.Services.GetService(typeof(cameraInterface));

                h_game.h_GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

                // Calculate the camera matrices.
                float time = (float)gameTime.TotalGameTime.TotalSeconds;

                // If we are doing edge detection, first off we need to render the
                // normals and depth of our model into a special rendertarget.
                if (h_game.h_GameSettings.RenderSettings.EnableEdgeDetect)
                {
                    h_game.h_GraphicsDevice.SetRenderTarget(0, normalDepthRenderTarget);
                    h_game.h_GraphicsDevice.Clear(Color.Black);

                    // Draw the models
                    h_game.h_GameObjectManager.DrawModels(gameTime, h_game.h_GraphicsDevice, camera.ViewMatrix, camera.ProjectionMatrix, "NormalDepth");
                }

                // If we are doing edge detection and/or pencil sketch processing, we
                // need to draw the model into a special rendertarget which can then be
                // fed into the postprocessing shader. Otherwise can just draw it
                // directly onto the backbuffer.
                if (h_game.h_GameSettings.RenderSettings.EnableEdgeDetect || h_game.h_GameSettings.RenderSettings.EnableSketch)
                    h_game.h_GraphicsDevice.SetRenderTarget(0, sceneRenderTarget);
                else
                    h_game.h_GraphicsDevice.SetRenderTarget(0, null);

                h_game.h_GraphicsDevice.Clear(Color.CornflowerBlue);
                // Draw the model, using either the cartoon or lambert shading technique.
                string effectTechniqueName;

                if (h_game.h_GameSettings.RenderSettings.EnableToonShading)
                    effectTechniqueName = "Toon";
                else
                    effectTechniqueName = "Lambert";



                // Draw the SkyPlane
                h_game.h_SkyPlane.Draw(h_game.h_GraphicsDevice, camera.ViewMatrix, camera.ProjectionMatrix);

                // Draw the models
                h_game.h_GameObjectManager.DrawModels(gameTime, h_game.h_GraphicsDevice, camera.ViewMatrix, camera.ProjectionMatrix, effectTechniqueName);

                // Run the postprocessing filter over the scene that we just rendered.
                if (h_game.h_GameSettings.RenderSettings.EnableEdgeDetect || h_game.h_GameSettings.RenderSettings.EnableSketch)
                {
                    h_game.h_GraphicsDevice.SetRenderTarget(0, null);

                    ApplyPostprocess();
                }

                // Display some text over the top. Note how we draw this after the
                // postprocessing, because we don't want the text to be affected by it.
                h_game.h_Hud.Draw();
            }

        }
        #endregion

        #region ApplyPostprocess()
        /// Applies the edge detection and pencil sketch postprocess effect.
        /// ***********************************************************************
        void ApplyPostprocess()
        {
            EffectParameterCollection parameters = postprocessEffect.Parameters;
            string effectTechniqueName;

            // Set effect parameters controlling the pencil sketch effect.
            if (h_game.h_GameSettings.RenderSettings.EnableSketch)
            {
                parameters["SketchThreshold"].SetValue(h_game.h_GameSettings.RenderSettings.SketchThreshold);
                parameters["SketchBrightness"].SetValue(h_game.h_GameSettings.RenderSettings.SketchBrightness);
                parameters["SketchJitter"].SetValue(sketchJitter);
                parameters["SketchTexture"].SetValue(sketchTexture);
            }

            // Set effect parameters controlling the edge detection effect.
            if (h_game.h_GameSettings.RenderSettings.EnableEdgeDetect)
            {
                Vector2 resolution = new Vector2(sceneRenderTarget.Width,
                                                 sceneRenderTarget.Height);

                Texture2D normalDepthTexture = normalDepthRenderTarget.GetTexture();

                parameters["EdgeWidth"].SetValue(h_game.h_GameSettings.RenderSettings.EdgeWidth);
                parameters["EdgeIntensity"].SetValue(h_game.h_GameSettings.RenderSettings.EdgeIntensity);
                parameters["ScreenResolution"].SetValue(resolution);
                parameters["NormalDepthTexture"].SetValue(normalDepthTexture);

                // Choose which effect technique to use.
                if (h_game.h_GameSettings.RenderSettings.EnableSketch)
                {
                    if (h_game.h_GameSettings.RenderSettings.SketchInColor)
                        effectTechniqueName = "EdgeDetectColorSketch";
                    else
                        effectTechniqueName = "EdgeDetectMonoSketch";
                }
                else
                    effectTechniqueName = "EdgeDetect";
            }
            else
            {
                // If edge detection is off, just pick one of the sketch techniques.
                if (h_game.h_GameSettings.RenderSettings.SketchInColor)
                    effectTechniqueName = "ColorSketch";
                else
                    effectTechniqueName = "MonoSketch";
            }

            // Activate the appropriate effect technique.
            postprocessEffect.CurrentTechnique =
                                    postprocessEffect.Techniques[effectTechniqueName];

            // Draw a fullscreen sprite to apply the postprocessing effect.
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            postprocessEffect.Begin();
            postprocessEffect.CurrentTechnique.Passes[0].Begin();

            spriteBatch.Draw(sceneRenderTarget.GetTexture(), Vector2.Zero, Color.White);

            spriteBatch.End();

            postprocessEffect.CurrentTechnique.Passes[0].End();
            postprocessEffect.End();
        }
        #endregion
    }

    /// <summary>
    /// ***********************************************************************
    /// **                         renderSettings                            **
    /// ** Structure to hold a render setting state kept in gameSettings     **
    /// ** singleton class                                                   **
    /// ** LOTS OF CODE HERE TAKEN FROM XNA CREATORS CLUB:                   **
    /// ** Non Photo Realistic Rendering example (I like the effects)        **
    /// ***********************************************************************
    /// </summary>
    public struct renderSettings
    {
        #region Local Variables

        // Name of a preset setting, for display to the user.
        public readonly string Name;

        // Is the cartoon lighting shader enabled?
        public readonly bool EnableToonShading;

        // Settings for the edge detect filter.
        public readonly bool EnableEdgeDetect;
        public readonly float EdgeWidth;
        public readonly float EdgeIntensity;

        // Settings for the pencil sketch effect.
        public readonly bool EnableSketch;
        public readonly bool SketchInColor;
        public readonly float SketchThreshold;
        public readonly float SketchBrightness;
        public readonly float SketchJitterSpeed;

        #endregion

        #region Constructor - renderSettings(...)
        /// Constructor
        /// ***********************************************************************
        public renderSettings(string name, bool enableToonShading,
                                         bool enableEdgeDetect,
                                         float edgeWidth, float edgeIntensity,
                                         bool enableSketch, bool sketchInColor,
                                         float sketchThreshold, float sketchBrightness,
                                         float sketchJitterSpeed)
        {
            Name = name;
            EnableToonShading = enableToonShading;
            EnableEdgeDetect = enableEdgeDetect;
            EdgeWidth = edgeWidth;
            EdgeIntensity = edgeIntensity;
            EnableSketch = enableSketch;
            SketchInColor = sketchInColor;
            SketchThreshold = sketchThreshold;
            SketchBrightness = sketchBrightness;
            SketchJitterSpeed = sketchJitterSpeed;
        }
        #endregion

    }
}
