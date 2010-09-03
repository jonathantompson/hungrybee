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
    /// **                             camera                                **
    /// ** Main Singleton camera class (written for HB but extendable to     **
    /// ** other projects)                                                   **
    /// ***********************************************************************
    /// </summary>
    class camera : GameComponent, cameraInterface
    {
        #region Local Variables
        /// Local Variables
        /// ***********************************************************************
        private game h_game;

        private Matrix h_viewMatrix;
        private Matrix h_projectionMatrix;
        private Viewport h_viewPort;

        private Vector3 h_cameraPosition;
        private Vector3 h_cameraUp;
        private Vector3 h_cameraForward;


        private float h_viewAngle;
        private float h_nearPlane;
        private float h_farPlane;
        #endregion

        #region Access and Modifier functions
        /// Getter and Setter functions to extend cameraInterface
        /// ***********************************************************************
        public Vector3 Position
        {
            get { return h_cameraPosition; }
        }
        public Vector3 Forward
        {
            get { return h_cameraForward; }
        }
        public Vector3 upVector
        {
            get { return h_cameraUp; }
        }
        public Matrix ViewMatrix
        {
            get { return h_viewMatrix; }
        }
        public Matrix ProjectionMatrix
        {
            get { return h_projectionMatrix; }
        }
        #endregion

        #region Constructor - camera(game game)
        /// Constructor - Just register the cameraInterface services
        /// ***********************************************************************
        public camera(game game) : base(game)  
        {
            h_game = (game)game;
            game.Services.AddService(typeof(cameraInterface), this);
        }
        #endregion

        #region Initialize()
        /// Initializes to default values 
        /// ***********************************************************************
        public override void Initialize()
        {
            // ORIGIONAL DEFAULTS
            //h_cameraPosition = new Vector3(0, 0, 20);
            //h_cameraForward = Vector3.Forward;          // XNA standard (0,0,-1)
            //h_cameraUp = Vector3.Up;                    // XNA standard (0,1,0)


            h_cameraPosition = new Vector3(3000, 1500, 0);
            h_cameraForward = new Vector3(-3000, -1500, 0);
            h_cameraUp = Vector3.Up;

            this.Resize();
            base.Initialize();
        }
        #endregion

        #region Resize()
        /// Reset the Perspective Matrix --> Required only on window resize 
        /// ***********************************************************************
        public void Resize()
        {
            h_viewAngle = MathHelper.PiOver4;
            h_viewPort = h_game.GraphicsDevice.Viewport;
            h_nearPlane = 1000.0f;
            h_farPlane = 10000.0f;
            h_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(h_viewAngle, h_viewPort.AspectRatio, h_nearPlane, h_farPlane);
        }
        #endregion

        #region Update()
        /// Update the camera
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            h_viewMatrix = Matrix.CreateLookAt(h_cameraPosition, h_cameraPosition + h_cameraForward, h_cameraUp);
            base.Update(gameTime);
        }
        #endregion

        //Matrix view = Matrix.CreateLookAt(new Vector3(3000, 1500, 0),
        //                          Vector3.Zero,
        //                          Vector3.Up);

        //Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
        //                                                        aspectRatio,
        //                                                        1000, 10000);
    }
}
