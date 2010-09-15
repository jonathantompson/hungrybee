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
    /// **                         gameObjectEnemy                           **
    /// ** This is a class to store the data for each player game object     **
    /// ** ie, the bee to be controlled by the human                         **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = TRUE                                                    **
    /// ***********************************************************************
    /// </summary>
    class gameObjectEnemy : gameObject
    {
        #region Local Variables

        public float enemyHealth;

        public force forceGravity;

        #endregion

        #region Constructor - gameObjectPlayer(game game, string modelfile, float scale)
        /// Constructor - gameObjectPlayer(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectEnemy(game game, string modelfile, boundingObjType _objType, float _scale, float _maxVel, Vector3 startingPos)
            : base(game, modelfile, _objType)
        {
            state.scale = new Vector3(_scale, _scale, _scale);
            state.pos = startingPos;
            enemyHealth = 100.0f;
            base.movable = true;
            base.collidable = true;
            base.maxVel = _maxVel;

            // Setup the force structures to describe movement
            forceGravity = new forceGravity(new Vector3(0.0f, -1.0f * game.GetGameSettings().gravity,0.0f));

            // Add the force structures to the forceList for enumeration at runtime
            base.forceList.Add(forceGravity);
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update enemy movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            // Nothing to do yet
        }
        #endregion

    }
}
