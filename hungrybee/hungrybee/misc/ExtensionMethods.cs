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
        static float PRECISION = 0.000001f;

        public static bool testFloatEquality(float f1, float f2)
        {
            // Note: if f1 = a and f2 = -a --> (f1 - f2) = (a - (-a)) = 0 --> So need to check both cases
            if ((float)Math.Abs(f1 - f2) < PRECISION && (float)Math.Abs(f2 - f1) < PRECISION)
                return true;
            else
                return false;
        }

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
        public static void Vector3SetAt(ref Vector3 vec, int index, float val)
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
                    throw new Exception("Vector3SetAt: index out of bounds");
            }
        }

        // Add a modifier so we can use index referencing to Vector3
        public static void Vector2SetAt(ref Vector2 vec, int index, float val)
        {
            switch (index)
            {
                case 0:
                    vec.X = val;
                    break;
                case 1:
                    vec.Y = val;
                    break;
                default:
                    throw new Exception("Vector2SetAt: index out of bounds");
            }
        }

        // Add a squared length function like David Eberly's
        public static float SquaredLength(this Vector3 vec)
        {
            return (vec.X * vec.X) + (vec.Y * vec.Y) + (vec.Z * vec.Z);
        }

        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm
        static float q_w = 0.0f;
        static float EPSILON = 0.00001f;
        public static void GetAxisAngleFromQuaternion(ref Quaternion quat, ref Vector3 axis, ref float angle)
        {
            q_w = (float)Math.Sqrt(quat.X * quat.X + quat.Y * quat.Y + quat.Z * quat.Z + quat.W * quat.W);
            if (Math.Abs(q_w - 1.0f) > EPSILON)
                throw new Exception("forcePlayerInput::GetTorque() - rotError quaternion is not unit length!");
            angle = 2.0f * (float)Math.Acos(quat.W);

            float s = (float)Math.Sqrt(1 - quat.W * quat.W);
            if (Math.Abs(s) < EPSILON) // Test so that we don't divide by zero
            { axis.X = quat.X; axis.Y = quat.Y; axis.Z = quat.Z; }
            else
            { s = 1.0f / s; axis.X = quat.X * s; axis.Y = quat.Y * s; axis.Z = quat.Z * s; }

            axis = Vector3.Normalize(axis);
        }
    }
}