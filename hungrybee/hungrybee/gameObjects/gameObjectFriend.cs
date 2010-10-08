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
        protected float friendCapturedSequenceStart;
        protected float friendCapturedSequenceEnd;
        protected float friendCapturedSequenceScale;
        protected gameObject capturingPlayer;
        Vector3 displacementToPlayer;
        Vector3 rotAxis;

        #endregion

        #region Constructor - gameObjectFriend(game game, string modelfile, float scale)
        /// Constructor - gameObjectFriend(game game, string modelfile, float _scale)
        /// ***********************************************************************
        public gameObjectFriend(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, float _scale, Vector3 startingPos, Vector3 startingMom)
            : base(game, modelfile, _objType, textureEnabled, vertexColorEnabled, _scale, startingPos, startingMom)
        {
            // Nothing to do yet
            friendCaptured = false;
            displacementToPlayer = new Vector3();
            rotAxis = new Vector3();
            capturingPlayer = null;
        }
        #endregion

        #region Update()
        /// Update() - TO DO: update friend movements
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Update the base
            base.Update(gameTime);

            if (friendCaptured)
            {
                if ((float)gameTime.TotalGameTime.TotalSeconds > (friendCapturedSequenceEnd))
                    base.h_game.h_GameObjectManager.h_GameObjectsRemoveList.Add(this);
                else
                {
                    // Shrink the model over time
                    base.modelScaleToNormalizeSize = friendCapturedSequenceScale / (1.0f + base.h_game.h_GameSettings.friendSequenceScaleRateIncrease * ((float)gameTime.TotalGameTime.TotalSeconds - friendCapturedSequenceStart));
                    
                    // Also spin the model about the y-axis around the player
                    float angle = (base.h_game.h_GameSettings.friendSequenceAngularVelocity * ((float)gameTime.TotalGameTime.TotalSeconds - friendCapturedSequenceStart)) % (2.0f * (float)Math.PI);
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
            friendCapturedSequenceStart = state.time;
            friendCapturedSequenceEnd = friendCapturedSequenceStart + base.h_game.h_GameSettings.friendSequenceDuration;
            friendCapturedSequenceScale = base.modelScaleToNormalizeSize;
            displacementToPlayer = state.pos - capturingPlayer.state.pos;
            rotAxis = Vector3.Cross(displacementToPlayer, Vector3.Backward);
            h_game.h_AudioManager.CueSound(soundType.FRIEND_COLLECTED);
        }
        #endregion

    }
}
