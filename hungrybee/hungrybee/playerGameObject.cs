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
    class playerGameObject : gameObject
    {
        #region Local Variables

        int health;

        #endregion

        #region Constructor
        public playerGameObject(game game, string modelfile) : base(game, modelfile)
        {
            health = 0;
        }
        #endregion
    }
}
