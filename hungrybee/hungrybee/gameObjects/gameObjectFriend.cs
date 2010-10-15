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

        bool friendCaptured;
        protected float friendCapturedSequenceTime;
        protected float friendCapturedSequenceScale;
        protected gameObject capturingPlayer;
        Vector3 displacementToPlayer;
        Vector3 rotAxis;

        #endregion

        #region Constructor - gameObjectFriend(game game, string modelfile, float scale)
        /// Constructor - gameObjectFriend(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectFriend(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled,
                                float _scale, Vector3 startingPos, Vector3 startingMom, Quaternion startingOrient)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled, startingOrient)
        {
            // Nothing to do yet
            friendCaptured = false;
            displacementToPlayer = new Vector3();
            rotAxis = new Vector3();
            capturingPlayer = null;

            // Set starting RBO state
            state.scale = prevState.scale = new Vector3(_scale, _scale, _scale);
            state.pos = prevState.pos = startingPos;
            state.linearMom = prevState.linearMom = startingMom;
            state.orient = prevState.orient = startingOrient;
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update friend movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            if (friendCaptured && !h_game.h_PhysicsManager.gamePaused)
            {
                friendCapturedSequenceTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (friendCapturedSequenceTime > base.h_game.h_GameSettings.friendSequenceDuration)
                    base.h_game.h_GameObjectManager.h_GameObjectsRemoveList.Add(this);
                else
                {
                    // Shrink the model over time
                    base.modelScaleToNormalizeSize = friendCapturedSequenceScale / (1.0f + base.h_game.h_GameSettings.friendSequenceScaleRateIncrease * friendCapturedSequenceTime);
                    
                    // Also spin the model about the y-axis around the player
                    float angle = (base.h_game.h_GameSettings.friendSequenceAngularVelocity * friendCapturedSequenceTime) % (2.0f * (float)Math.PI);
                    state.pos = prevState.pos = capturingPlayer.prevState.pos + Vector3.Transform(displacementToPlayer, Matrix.CreateFromAxisAngle(rotAxis, angle));
                 }
            } // if (friendCaptured)
        }
        #endregion
         
        #region CaptureFriend()
        /// Update() - TO DO: update friend movements
        /// ***********************************************************************
        public void CaptureFriend(gameObject player)
        {
            friendCaptured = true;
            capturingPlayer = player;
            friendCapturedSequenceTime = 0.0f;
            friendCapturedSequenceScale = base.modelScaleToNormalizeSize;
            displacementToPlayer = state.pos - capturingPlayer.state.pos;
            rotAxis = Vector3.Cross(displacementToPlayer, Vector3.Backward);
            h_game.h_AudioManager.CueSound(soundType.FRIEND_COLLECTED);
        }
        #endregion




    }
}
