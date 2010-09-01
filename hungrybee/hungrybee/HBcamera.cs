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
    /// ** Main Singleton camera class (written for HB but extendable to)    **
    /// ** other projects                                                    **
    /// ***********************************************************************
    /// </summary>
    class HBcamera : GameComponent, cameraInterface
    {
        private HBGame h_HBGame;

        /// <summary>
        /// Local Variables
        /// ***********************************************************************
        /// </summary>        
        private Matrix viewMatrix;
        private Matrix projectionMatrix;
        private Viewport viewPort;

        private Vector3 cameraPosition;
        private Vector3 cameraUp;
        private Vector3 cameraTarget;

        private float viewAngle;
        private float nearPlane;
        private float farPlane;

        /// <summary>
        /// Getter and Setter functions to extend cameraInterface
        /// ***********************************************************************
        /// </summary>
        public Vector3 Position
        {
            get { return cameraPosition; }
        }
        public Vector3 Forward
        {
            get { return (cameraPosition - cameraTarget); }
        }
        public Vector3 upVector
        {
            get { return cameraUp; }
        }
        public Matrix ViewMatrix
        {
            get { return ViewMatrix; }
        }
        public Matrix ProjectionMatrix
        {
            get { return ProjectionMatrix; }
        }

        /// <summary>
        /// Constructor - Just register the cameraInterface services
        /// ***********************************************************************
        /// </summary>
        public HBcamera(Game game) : base(game)  
        {
            h_HBGame = (HBGame)game;
            game.Services.AddService(typeof(cameraInterface), this);
        }

        /// <summary>
        /// Initializes to default values 
        /// ***********************************************************************
        /// </summary>
        public override void Initialize()
        {
            cameraPosition = new Vector3(10, 0, 0);
            cameraTarget = new Vector3(0, 0, 0);
            cameraUp = new Vector3(0, 1, 0);
            this.Resize();
        }

        /// <summary>
        /// Reset the Perspective Matrix --> Required only on window resize 
        /// ***********************************************************************
        /// </summary>
        public void Resize()
        {
            viewAngle = MathHelper.PiOver4;
            viewPort = h_HBGame.GraphicsDevice.Viewport;
            nearPlane = 0.5f;
            farPlane = 100.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, viewPort.AspectRatio, nearPlane, farPlane);
        }

        /// <summary>
        /// Update the camera
        /// ***********************************************************************
        /// </summary>
        public override void Update(GameTime gameTime)
        {

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
        }
    }
}
