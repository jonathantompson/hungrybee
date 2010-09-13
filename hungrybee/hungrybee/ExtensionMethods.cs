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

namespace ExtensionMethods
{    /// <summary>
    /// ***********************************************************************
    /// **                            MyExtensions                           **
    /// ** Some extention functions to make ports from C/C++ functions       **
    /// ** easier.                                                           **
    /// ***********************************************************************
    /// </summary>
    public static class MyExtensions
    {
        // Add a selector so we can use index referencing to Vector3
        public static float Mag(this Vector3 vec)
        {
            return (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
        }

        // Add a selector so we can use index referencing to Vector3
        public static float GetAt(this Vector3 vec, int index)
        {
            switch (index)
            {
                case 0:
                    return vec.X;
                case 1:
                    return vec.Y;
                case 2:
                    return vec.Z;
                default:
                    throw new Exception("Vector3.GetAt(): index out of bounds");
            }
        }

        // Add a modifier so we can use index referencing to Vector3
        public static void SetAt(this Vector3 vec, int index, float val)
        {
            switch (index)
            {
                case 0:
                    vec.X = val;
                    break;
                case 1:
                    vec.Y = val;
                    break;
                case 2:
                    vec.Z = val;
                    break;
                default:
                    throw new Exception("Vector3.GetAt(): index out of bounds");
            }
        }

        // Add a squared length function like David Eberly's
        public static float SquaredLength(this Vector3 vec)
        {
            return (vec.X * vec.X) + (vec.Y * vec.Y) + (vec.Z * vec.Z);
        }
    }
}