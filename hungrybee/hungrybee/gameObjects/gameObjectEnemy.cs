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
using ExtensionMethods;
#endregion

namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                         gameObjectEnemy                           **
    /// ** This is a class to store the data for each enemy game object      **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = TRUE                                                    **
    /// ***********************************************************************
    /// </summary>
    class gameObjectEnemy : gameObjectNPC
    {
        #region Local Variables

        protected bool deathSequence;
        protected float deathSequenceTime;
        protected float deathSequenceScale;

        #endregion

        #region Constructor - gameObjectEnemy()
        /// Constructor - gameObjectEnemy()
        /// ***********************************************************************
        public gameObjectEnemy(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, 
                               float _scale, Vector3 startingPos, Vector3 startingMom, Quaternion startingOrient)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled, startingOrient)
        {
            // Nothing to do yet
            deathSequence = false;

            // Set starting RBO state
            state.scale = prevState.scale = new Vector3(_scale, _scale, _scale);
            state.pos = prevState.pos = startingPos;
            state.linearMom = prevState.linearMom = startingMom;
            state.orient = prevState.orient = startingOrient;
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update enemy movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            if (deathSequence && !h_game.h_PhysicsManager.gamePaused)
            {
                deathSequenceTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (deathSequenceTime > base.h_game.h_GameSettings.enemySequenceDuration)
                    base.h_game.h_GameObjectManager.h_GameObjectsRemoveList.Add(this);
                else
                {
                    base.modelScaleToNormalizeSize = deathSequenceScale / (1.0f + base.h_game.h_GameSettings.enemySequenceScaleRateIncrease * deathSequenceTime);
                }
            }// if (deathSequence)
        }
        #endregion

        #region KillEnemy()
        public void KillEnemy()
        {
            base.collidable = false;
            deathSequence = true;
            deathSequenceTime = 0.0f;
            deathSequenceScale = base.modelScaleToNormalizeSize;
            h_game.h_AudioManager.CueSound(soundType.ENEMY_KILLED);
        }
        #endregion

    }
}
