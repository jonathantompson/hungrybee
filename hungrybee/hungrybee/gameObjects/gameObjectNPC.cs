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
    /// **                         gameObjectNPC                             **
    /// ** This is a class to store the data for each non-player game object **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = TRUE                                                    **
    /// ***********************************************************************
    /// </summary>
    class gameObjectNPC : gameObject
    {
        #region Local Variables

        public force forceGravity;
        public force forceSetOrientation;

        public static Vector3 forward = new Vector3();
        public static float velocity = 0.0f;

        #endregion

        #region Constructor - gameObjectEnemy(game game, string modelfile, float scale)
        /// Constructor - gameObjectEnemy(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectNPC(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled,
                            float _scale, Vector3 startingPos, Vector3 startingMom)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled)
        {
            state.scale = new Vector3(_scale, _scale, _scale);
            state.pos = startingPos;
            state.linearMom = startingMom;
            base.movable = true;
            base.collidable = true;

            // Setup the force structures to describe movement
            forceGravity = new forceGravity(new Vector3(0.0f, -1.0f * game.h_GameSettings.gravity, 0.0f));

            // Add the force structures to the forceList for enumeration at runtime
            base.forceList.Add(forceGravity);

            // Setup the force structures to describe movement
            forceSetOrientation = new forceSetOrientation(Quaternion.Identity, game.h_GameSettings.enemyTimeToOrient);
            ((forceSetOrientation)forceSetOrientation).SetDesiredOrientationFromForwardVector(new Vector3(1, 0, 0)); // Player starts facing right

            // Add the force structures to the forceList for enumeration at runtime
            base.forceList.Add(forceSetOrientation);
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update enemy movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            // Set the desired orientation in the x direction we're moving
            if (prevState.linearVel.X > 0.0f)
                ((forceSetOrientation)forceSetOrientation).SetDesiredOrientationFromForwardVector(Vector3.Right);
            else if (prevState.linearVel.X < 0.0f)
                ((forceSetOrientation)forceSetOrientation).SetDesiredOrientationFromForwardVector(Vector3.Left);
        }
        #endregion



    }
}
