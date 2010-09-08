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
    /// **                             player                                **
    /// ** This is a class to store the data for each player game object     **
    /// ** ie, the bee to be controlled by the human                         **
    /// ***********************************************************************
    /// </summary>
    class gameObjectPlayer : gameObject
    {
        #region Local Variables

        game h_game;
        // int playerHealth;

        #endregion

        #region Constructor - gameObjectPlayer(game game, string modelfile, float scale) : base(game, modelfile)
        public gameObjectPlayer(game game, string modelfile, float _scale)
            : base(game, modelfile)
        {
            h_game = game;
            scale = Matrix.CreateScale(_scale);
            dirtyWorldMatrix = true;
        }
        #endregion
    }
}
