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
    /// **                       gameObjectPlayer                            **
    /// ** This is a class to store the data for each player game object     **
    /// ** ie, the bee to be controlled by the human                         **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = TRUE                                                    **
    /// ***********************************************************************
    /// </summary>
    class gameObjectPlayer : gameObject
    {
        #region Local Variables

        public float acceleration;
        public float stoppingTime;
        public float playerHealth;

        public force forceSlowDown;
        public force forcePlayerInput;

        #endregion

        #region Constructor - gameObjectPlayer(game game, string modelfile, float scale)
        /// Constructor - gameObjectPlayer(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectPlayer(game game, string modelfile, boundingObjType _objType, float _scale, float _acceleration, float _stoppingTime, float _maxVel)
            : base(game, modelfile, _objType)
        {
            state.scale = new Vector3(_scale, _scale, _scale);
            acceleration = _acceleration;
            stoppingTime = _stoppingTime;
            playerHealth = 100.0f;
            base.movable = true;
            base.collidable = true;
            base.maxVel = _maxVel;

            // Setup the force structures to describe movement
            forceSlowDown = new forceSlowDown();
            forcePlayerInput = new forcePlayerInput(Vector3.Zero);

            // Add the force structures to the forceList for enumeration at runtime
            base.forceList.Add(forceSlowDown);
            base.forceList.Add(forcePlayerInput);
        }
        #endregion

        #region Update()
        /// Update() - Implement player controls and update orientation
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            // Get player input
            GetPlayerInput(gameTime);
        }
        #endregion

        #region GetPlayerInput()
        /// GetPlayerInput() - Move the player and set the desired orientation
        /// ***********************************************************************
        public void GetPlayerInput(GameTime gameTime)
        {
            // Zero out the acceleration
            bool keyPressed = false;
            Vector3 acc;
            acc.X = 0.0f; acc.Y = 0.0f; acc.Z = 0.0f;

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) && !keyState.IsKeyDown(Keys.Down)) // Ignore case when both keys are pressed
            {
                acc.Y += acceleration;
                keyPressed = true;
            }

            if (keyState.IsKeyDown(Keys.Down) && !keyState.IsKeyDown(Keys.Up)) // Ignore case when both keys are pressed
            {
                acc.Y -= acceleration;
                keyPressed = true;
            }

            if (keyState.IsKeyDown(Keys.Left) && !keyState.IsKeyDown(Keys.Right)) // Ignore case when both keys are pressed
            {
                acc.X -= acceleration;
                keyPressed = true;
            }

            if (keyState.IsKeyDown(Keys.Right) && !keyState.IsKeyDown(Keys.Left)) // Ignore case when both keys are pressed
            {
                acc.X += acceleration;
                keyPressed = true;
            }

            // If there's no player input, slow the player down to a stop in the desired amount of time
            if (!keyPressed)
            {
                ((forceSlowDown)forceSlowDown).SetStopTime((float)gameTime.TotalGameTime.TotalSeconds + stoppingTime);
                ((forcePlayerInput)forcePlayerInput).SetAcceleration(Vector3.Zero);
            }
            // Otherwise add the acceleration and remove the decelleration
            else
            {
                ((forceSlowDown)forceSlowDown).SetStopTime(0.0f);
                ((forcePlayerInput)forcePlayerInput).SetAcceleration(acc);
            }
        }
        #endregion
    }
}
