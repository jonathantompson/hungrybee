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

namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                          cameraInterface                          **
    /// ** Interface class to switch between camera types if we want.        **
    /// ***********************************************************************
    /// </summary>
    interface cameraInterface
    {
        Vector3 Position { get; }
        Vector3 Forward { get; }
        Vector3 upVector { get; }

        Matrix ViewMatrix { get; }
        Matrix ProjectionMatrix { get; }
    }
}
