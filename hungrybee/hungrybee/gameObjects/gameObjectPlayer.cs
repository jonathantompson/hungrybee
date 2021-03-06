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

        bool playerDead;
        protected float playerDeathSequenceTime;
        protected float playerDeathSequenceScale;
        protected Vector3 playerDeathStartPos;
        protected Quaternion playerDeathStartOrient;

        protected bool playerCollided;
        protected float playerCollidedTime;

        public force forcePlayerInput;

        #endregion

        #region Constructor - gameObjectPlayer(game game, string modelfile, float scale)
        /// Constructor - gameObjectPlayer(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectPlayer(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled,
                                float _scale, Vector3 _pos, Quaternion startingOrient)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled)
        {
            playerHealth = 100.0f;
            base.movable = true;
            base.collidable = true;
            jumping = false;

            // Set starting RBO state
            state.scale = prevState.scale = new Vector3(_scale, _scale, _scale);
            state.pos = prevState.pos = _pos;
            state.orient = prevState.orient = startingOrient;

            // Setup the force structures to describe movement
            forcePlayerInput = new forcePlayerInput(Vector3.Zero,                                  // Starting Velocity
                                                    Quaternion.Identity,                           // Starting Orientation
                                                    game.h_GameSettings.playerTimeToAccelerate, // Time to reach velocity
                                                    game.h_GameSettings.playerMaxAcceleration,  // maximum acceleration
                                                    game.h_GameSettings.playerTimeToOrient);    // time to reach orientation
            ((forcePlayerInput)forcePlayerInput).SetDesiredOrientationFromForwardVector(new Vector3(1,0,0)); // Player starts facing right

            // Add the force structures to the forceList for enumeration at runtime
            base.forceList.Add(forcePlayerInput);
            // Get the forward vector from the input startingOrient
            Vector3 startForwardVector = Vector3.Transform(Vector3.Forward, startingOrient);
            ((forcePlayerInput)forcePlayerInput).SetDesiredOrientationFromForwardVector(startForwardVector);
            // Add gravity
            base.forceList.Add(new forceGravity(new Vector3(0.0f, -h_game.h_GameSettings.gravity, 0.0f)));
            playerDead = false;

            playerDeathStartPos = new Vector3();
            playerDeathStartOrient = new Quaternion();
        }
        #endregion

        #region Update()
        /// Update() - Implement player controls and update orientation
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            
            if(playerDead && !h_game.h_PhysicsManager.gamePaused)
            {
                playerDeathSequenceTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (playerDeathSequenceTime > base.h_game.h_GameSettings.playerDeathSequenceDuration)
                    base.h_game.h_GameObjectManager.h_GameObjectsRemoveList.Add(this);
                else
                {
                    float deltaT = playerDeathSequenceTime;
                    // Shrink the model over time
                    base.modelScaleToNormalizeSize = playerDeathSequenceScale / (1.0f + base.h_game.h_GameSettings.playerDeathSequenceScaleRateIncrease * deltaT);

                    Vector3 offset = new Vector3();
                    // Add a linear velocity in the Z direction
                    offset.Z = h_game.h_GameSettings.playerDeathZVelocity * deltaT;
                    // Constant position in the X direction
                    offset.X = 0.0f;
                    // Position in y direction follows a polynomial function: see "playerDeathSequence.xls" for more info
                    // Offset = (k1*T^3 + k2*T) * yAmp
                    // Offset = (T * (k2 + T^2*k1)) * yAmp             --> Few multiplications
                    deltaT = deltaT * h_game.h_GameSettings.playerDeathYFuncTScale; // Speeds up the sequence a little
                    offset.Y = deltaT * (1.5f - 0.5f * deltaT * deltaT) * h_game.h_GameSettings.playerDeathYAmplitude;

                    base.state.pos = playerDeathStartPos + offset;
                    base.prevState.pos = base.state.pos;
                }
            }

            if (playerCollided && playerCollidedTime < h_game.h_GameSettings.playerCollisionPause)
            {
                playerCollidedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                // If the timer is up, add back the forces to our fource list
                if (playerCollided)
                {
                    playerCollided = false;
                    base.forceList.Add(forcePlayerInput);
                }
                // Get player input only if we haven't collided within h_game.h_GameSettings.playerCollisionPause
                GetPlayerInput(gameTime);
            }

            // Update the base
            base.Update(gameTime);
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

        #region StopPlayerInput()
        /// GetPlayerInput() - Turn off all acceleration due to user input for a set period of time
        /// ***********************************************************************
        public void StopPlayerInput()
        {
            if (!playerCollided)
            {
                playerCollidedTime = 0.0f; // Reset the clock
                playerCollided = true;
                base.forceList.Remove(forcePlayerInput); // O(n) - but n is small so probably ok
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
            if (playerHealth <= 0.0f)
            {
                h_game.h_AudioManager.CueSound(soundType.PLAYER_FALLING);
                base.movable = false;
                base.collidable = false;
                playerDead = true;
                playerDeathSequenceTime = 0.0f;
                playerDeathSequenceScale = base.modelScaleToNormalizeSize;
                playerDeathStartPos = state.pos;
                playerDeathStartOrient = state.orient;
            }
            else
            {
                h_game.h_AudioManager.CueSound(soundType.PLAYER_HURT);
            }
        }
        #endregion
    }


}
