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
    /// **                         gameObjectFriend                          **
    /// ** This is a class to store the data for each friend game object     **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = TRUE                                                    **
    /// ***********************************************************************
    /// </summary>
    class gameObjectFriend : gameObjectNPC
    {
        #region Local Variables


        #endregion

        #region Constructor - gameObjectFriend(game game, string modelfile, float scale)
        /// Constructor - gameObjectFriend(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectFriend(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, float _scale, Vector3 startingPos, Vector3 startingMom)
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
