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
    /// **                             player                                **
    /// ** This is a class to store the data for each player game object     **
    /// ** ie, the bee to be controlled by the human                         **
    /// ***********************************************************************
    /// </summary>
    class gameObjectPlayer : gameObject
    {
        #region Local Variables

        game h_game;
        float playerAcceleration;
        float playerDecceleration;
        float playerHealth;

        #endregion

        #region Constructor - gameObjectPlayer(game game, string modelfile, float scale)
        /// Constructor - gameObjectPlayer(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectPlayer(game game, string modelfile, float _scale, float _playerAcceleration, float _playerDecelleration)
            : base(game, modelfile)
        {
            h_game = game;
            state.scale = new Vector3(_scale, _scale, _scale);
            playerAcceleration = _playerAcceleration;
            playerDecceleration = _playerDecelleration;
            playerHealth = 100.0f;
            movable = true;
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
            GetPlayerInput();
        }
        #endregion

        #region GetPlayerInput()
        /// GetPlayerInput() - Move the player and set the desired orientation
        /// ***********************************************************************
        public void GetPlayerInput()
        {
            // Zero out the acceleration
            Vector3 acc;
            acc.X = 0.0f; acc.Y = 0.0f; acc.Z = 0.0f;

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up))
                acc.Y += playerAcceleration;

            if (keyState.IsKeyDown(Keys.Down))
                acc.Y -= playerAcceleration;

            if (keyState.IsKeyDown(Keys.Left))
                acc.X -= playerAcceleration;

            if (keyState.IsKeyDown(Keys.Right))
                acc.X += playerAcceleration;

            // If there's no player input, slow the player down to a stop
            if (acc.X < h_game.GetGameSettings().EPSILON && acc.Y < h_game.GetGameSettings().EPSILON)
                acc = state.linearVel * (-1.0f) * playerDecceleration;

            // Calculate force

        }
        #endregion
    }
}
