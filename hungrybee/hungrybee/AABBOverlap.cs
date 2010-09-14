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
    public class AABBOverlap
    {
        public bool xAxisOverlap,   // xAxisOverlaps
                    yAxisOverlap,
                    zAxisOverlap;
        public AABBOverlap(bool _xAxisOverlap, bool _yAxisOverlap, bool _zAxisOverlap)
        {
            xAxisOverlap = _xAxisOverlap; yAxisOverlap = _yAxisOverlap; zAxisOverlap = _zAxisOverlap;
        }

        public static AABBOverlap GetOverlapStatus(ref List<AABBOverlap> AABBOverlapStatus, int index1, int index2, int arraySize)
        {
            if (index1 < index2)
            {
                return AABBOverlapStatus[index1 * arraySize + index2];
            }
            else
            {
                return AABBOverlapStatus[index2 * arraySize + index1];
            }
        }
    };
}
