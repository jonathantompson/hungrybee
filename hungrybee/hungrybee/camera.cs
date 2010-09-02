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
    /// **                             camera                                **
    /// ** Main Singleton camera class (written for HB but extendable to     **
    /// ** other projects)                                                   **
    /// ***********************************************************************
    /// </summary>
    class camera : GameComponent, cameraInterface
    {
        private game h_game;

        /// <summary>
        /// Local Variables
        /// ***********************************************************************
        /// </summary>        
        private Matrix h_viewMatrix;
        private Matrix h_projectionMatrix;
        private Viewport h_viewPort;

        private Vector3 h_cameraPosition;
        private Vector3 h_cameraUp;
        private Vector3 h_cameraForward;


        private float h_viewAngle;
        private float h_nearPlane;
        private float h_farPlane;

        /// <summary>
        /// Getter and Setter functions to extend cameraInterface
        /// ***********************************************************************
        /// </summary>
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

        /// <summary>
        /// Constructor - Just register the cameraInterface services
        /// ***********************************************************************
        /// </summary>
        public camera(game game) : base(game)  
        {
            h_game = (game)game;
            game.Services.AddService(typeof(cameraInterface), this);
        }

        /// <summary>
        /// Initializes to default values 
        /// ***********************************************************************
        /// </summary>
        public override void Initialize()
        {
            h_cameraPosition = new Vector3(0, 0, 20);
            h_cameraForward = Vector3.Forward;          // XNA standard (0,0,-1)
            h_cameraUp = Vector3.Up;                    // XNA standard (0,1,0)
            this.Resize();
            base.Initialize();
        }

        /// <summary>
        /// Reset the Perspective Matrix --> Required only on window resize 
        /// ***********************************************************************
        /// </summary>
        public void Resize()
        {
            h_viewAngle = MathHelper.PiOver4;
            h_viewPort = h_game.GraphicsDevice.Viewport;
            h_nearPlane = 0.5f;
            h_farPlane = 100.0f;
            h_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(h_viewAngle, h_viewPort.AspectRatio, h_nearPlane, h_farPlane);
        }

        /// <summary>
        /// Update the camera
        /// ***********************************************************************
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            h_viewMatrix = Matrix.CreateLookAt(h_cameraPosition, h_cameraPosition + h_cameraForward, h_cameraUp);
            base.Update(gameTime);
        }
    }
}
