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

namespace hungrybee
{
    #region enum types
    public enum collisionType { COL_UNDEFINED, EDGE_EDGE, EDGE_FACE };
    #endregion

    public class collision
    {
        public collisionType colType;
        public Object obj1;
        public Object obj2;
        public Vector3 colPoint;  // In world coordinates
        public Vector3 colNorm;   // For obj1 --> for obj2 use Negative(colNorm)
        public float colTime;

        public collision(collisionType _colType, gameObject _obj1, gameObject _obj2, float _Tcollision, Vector3 _colPoint, Vector3 _colNorm)
        {
            colType = _colType;
            obj1 = _obj1;
            obj2 = _obj2;
            colPoint = _colPoint;
            colNorm = _colNorm;
            colTime = _Tcollision;
        }

    }
}
