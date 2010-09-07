﻿#region using statements
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

        private Matrix h_cameraRotation;
        private float h_cameraRotationLeft;
        private float h_cameraRotationForward;

        private float h_viewAngle;
        private float h_nearPlane;
        private float h_farPlane;

        private bool h_running;
        private float h_cameraSpeed;
        private float h_cameraRunningMult;
        private float h_cameraRotationSpeed;
        private MouseState currentMouseState;
        private MouseState oldMouseState;

        private Vector3 direction;
        private Vector3 normForward;
        
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
            h_cameraPosition = new Vector3(0, 0, 10);
            h_cameraForward = Vector3.Forward;
            h_cameraUp = Vector3.Up;
            h_cameraRotation = Matrix.Identity;
            h_running = false;
            h_cameraSpeed = h_game.GetGameSettings().cameraSpeed;
            h_cameraRunningMult = h_game.GetGameSettings().cameraRunningMult;
            h_cameraRotationSpeed = h_game.GetGameSettings().cameraRotationSpeed;
            Mouse.SetPosition(h_game.Window.ClientBounds.Width / 2, h_game.Window.ClientBounds.Height / 2);
            oldMouseState = Mouse.GetState();
            h_cameraRotationLeft = 0.0f;
            h_cameraRotationForward = 0.0f;

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
            h_nearPlane = 0.5f;
            h_farPlane = 10000.0f;
            h_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(h_viewAngle, h_viewPort.AspectRatio, h_nearPlane, h_farPlane);
        }
        #endregion

        #region Update()
        /// Update the camera
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            Rotate();
            Move();

            h_viewMatrix = Matrix.CreateLookAt(h_cameraPosition, h_cameraPosition + h_cameraForward, h_cameraUp);
            base.Update(gameTime);
        }
        #endregion

        #region Rotate()
        /// Rotate the up and forward vectors
        /// ***********************************************************************
        public void Rotate()
        {
            currentMouseState = Mouse.GetState();
            if (currentMouseState != oldMouseState)
            {
                // Need to rotate about axis along the screen
                direction = Vector3.Cross(h_cameraForward, h_cameraUp);
                direction = Vector3.Normalize(direction);
                h_cameraRotation = Matrix.CreateFromAxisAngle(direction, -1.0f * h_cameraRotationSpeed*(currentMouseState.Y - oldMouseState.Y));

                // Need to rotate about up axis
                direction = Vector3.Normalize(h_cameraUp);
                h_cameraRotation = h_cameraRotation * Matrix.CreateFromAxisAngle(direction, -1.0f * h_cameraRotationSpeed * (currentMouseState.X - oldMouseState.X));

                // Now rotate the up and forward directions
                h_cameraForward = Vector3.Transform(h_cameraForward, h_cameraRotation);
                h_cameraUp = Vector3.Transform(h_cameraUp, h_cameraRotation);

                // Reset the mouse state for next frame and move the mouse back to the center
                Mouse.SetPosition(h_game.Window.ClientBounds.Width / 2, h_game.Window.ClientBounds.Height / 2);
            }
            
        }
        #endregion

        #region Move()
        /// Move down the forward direction
        /// ***********************************************************************
        public void Move()
        {
            normForward = Vector3.Normalize(h_cameraForward);

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.LeftShift))
                h_running = true;
            else
                h_running = false;

            float mov_scale = (h_running ? h_cameraRunningMult : 1.0f) * h_cameraSpeed;

            // Move front or back
            if (keyState.IsKeyDown(Keys.W))
            {
                h_cameraPosition += normForward * mov_scale;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                h_cameraPosition -= normForward * mov_scale;
            }

            // Move left or right
            if (keyState.IsKeyDown(Keys.A))
            {
                direction = Vector3.Cross(normForward, h_cameraUp);
                direction = Vector3.Normalize(direction); 
                h_cameraPosition -= direction * mov_scale;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                direction = Vector3.Cross(normForward, h_cameraUp);
                direction = Vector3.Normalize(direction); 
                h_cameraPosition += direction * mov_scale;
            }

            // Move up or down
            direction = Vector3.Normalize(h_cameraUp);
            if (keyState.IsKeyDown(Keys.Q))
            {
                direction = Vector3.Normalize(h_cameraUp); 
                h_cameraPosition += direction * mov_scale;
            }
            if (keyState.IsKeyDown(Keys.Z))
            {
                direction = Vector3.Normalize(h_cameraUp);
                h_cameraPosition -= direction * mov_scale;
            }
        }
        #endregion
    }
}
