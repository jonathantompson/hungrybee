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
        protected float deathSequenceStart;
        protected float deathSequenceEnd;
        protected float deathSequenceScale;

        #endregion

        #region Constructor - gameObjectEnemy()
        /// Constructor - gameObjectEnemy()
        /// ***********************************************************************
        public gameObjectEnemy(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, 
                               float _scale, Vector3 startingPos, Vector3 startingMom)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled, _scale, startingPos, startingMom)
        {
            // Nothing to do yet
            deathSequence = false;
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update enemy movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            if (deathSequence)
            {
                if ((float)gameTime.TotalGameTime.TotalSeconds > (deathSequenceEnd))
                    base.h_game.h_GameObjectManager.h_GameObjectsRemoveList.Add(this);
                else
                {
                    base.modelScaleToNormalizeSize = deathSequenceScale / (1.0f + base.h_game.h_GameSettings.enemySequenceScaleRateIncrease * ((float)gameTime.TotalGameTime.TotalSeconds - deathSequenceStart));
                }
            }// if (deathSequence)
        }
        #endregion

        #region KillEnemy()
        public void KillEnemy()
        {
            base.collidable = false;
            deathSequence = true;
            deathSequenceStart = state.time;
            deathSequenceEnd = deathSequenceStart + base.h_game.h_GameSettings.enemySequenceDuration;
            deathSequenceScale = base.modelScaleToNormalizeSize;
        }
        #endregion

    }
}
