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

        public float playerHealth;
        public bool  jumping;

        public force forcePlayerInput;

        #endregion

        #region Constructor - gameObjectPlayer(game game, string modelfile, float scale)
        /// Constructor - gameObjectPlayer(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectPlayer(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, 
                                float _scale, Vector3 _pos)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled)
        {
            state.scale = new Vector3(_scale, _scale, _scale);
            playerHealth = 100.0f;
            base.movable = true;
            base.collidable = true;
            base.state.pos = _pos;
            base.prevState.pos = _pos;
            jumping = false;

            // Setup the force structures to describe movement
            forcePlayerInput = new forcePlayerInput(Vector3.Zero,                                  // Starting Velocity
                                                    Quaternion.Identity,                           // Starting Orientation
                                                    game.h_GameSettings.playerTimeToAccelerate, // Time to reach velocity
                                                    game.h_GameSettings.playerMaxAcceleration,  // maximum acceleration
                                                    game.h_GameSettings.playerTimeToOrient);    // time to reach orientation
            ((forcePlayerInput)forcePlayerInput).SetDesiredOrientationFromForwardVector(new Vector3(1,0,0)); // Player starts facing right

            // Add the force structures to the forceList for enumeration at runtime
            base.forceList.Add(forcePlayerInput);
            // Add gravity
            base.forceList.Add(new forceGravity(new Vector3(0.0f, -h_game.h_GameSettings.gravity, 0.0f)));
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
        /// GetPlayerInput() - Move the player and set the desired velocity (effectively an impulse
        /// ***********************************************************************
        public void GetPlayerInput(GameTime gameTime)
        {
            // Zero out the acceleration
            bool keyPressed = false;
            Vector3 desiredVelValue = new Vector3();

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Left) && !keyState.IsKeyDown(Keys.Right)) // Ignore case when both keys are pressed
            {
                desiredVelValue += new Vector3(-1.0f, 0.0f, 0.0f);
                keyPressed = true;
            }

            if (keyState.IsKeyDown(Keys.Right) && !keyState.IsKeyDown(Keys.Left))
            {
                desiredVelValue += new Vector3(+1.0f, 0.0f, 0.0f);
                keyPressed = true;
            }
            if (keyPressed)
            {
                desiredVelValue = Vector3.Normalize(desiredVelValue);
                desiredVelValue = base.h_game.h_GameSettings.playerVelocity * desiredVelValue;
                ((forcePlayerInput)forcePlayerInput).SetVelocity(desiredVelValue);
                ((forcePlayerInput)forcePlayerInput).SetDesiredOrientationFromForwardVector(desiredVelValue);
            }
            else
            {
                ((forcePlayerInput)forcePlayerInput).SetVelocity(Vector3.Zero);
            }

            // Add a vertical impulse to trigger a jump, only if we're in resting contact with the ground
            if (keyState.IsKeyDown(Keys.Space) && base.resting && !jumping )
            {
                prevState.linearMom += new Vector3(0.0f, base.h_game.h_GameSettings.playerJumpMomentum, 0.0f);
                prevState.RecalculateDerivedQuantities();
                jumping = true;
                h_game.h_AudioManager.CueSound(soundType.JUMP);
            }
            if (keyState.IsKeyUp(Keys.Space))
            {
                jumping = false;
            }
        }
        #endregion

        #region StopPlayerControls()
        public void StopPlayerControls()
        {
            ((forcePlayerInput)forcePlayerInput).SetVelocity(Vector3.Zero);
            state.linearMom = Vector3.Zero;
            state.linearVel = Vector3.Zero;
            state.angularMom = Vector3.Zero;
            state.angularVel = Vector3.Zero;
        }
        #endregion

        #region HurtPlayer()
        public void HurtPlayer()
        {
            playerHealth = playerHealth - base.h_game.h_GameSettings.enemyHealthImpact;
            h_game.h_AudioManager.CueSound(soundType.PLAYER_HURT);
        }
        #endregion
    }


}
