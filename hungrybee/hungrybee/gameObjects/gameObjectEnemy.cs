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


        #endregion

        #region Constructor - gameObjectEnemy()
        /// Constructor - gameObjectEnemy()
        /// ***********************************************************************
        public gameObjectEnemy(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, 
                               float _scale, Vector3 startingPos, Vector3 startingMom)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled, _scale, startingPos, startingMom)
        {
            // Nothing to do yet
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update enemy movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);
        }
        #endregion

    }
}
