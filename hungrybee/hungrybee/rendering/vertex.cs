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
    /// **                        VertexPosition                             **
    /// ** Vertex class with only position info                              **
    /// ***********************************************************************
    public struct VertexPosition
    {
        #region Local and Static Variables
        public Vector3 Position;
        public static readonly int SizeInBytes = sizeof(float) * 3;
        public static readonly VertexElement[] VertexElements = 
            {
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 )
            };
        #endregion

        #region Constructor - VertexPosition(Vector3 position)
        public VertexPosition(Vector3 position)
        {
            this.Position = position;
        }
        #endregion

    }

    /// <summary>
    /// ***********************************************************************
    /// **                      VertexPositionNormal                         **
    /// ** Vertex class with Position and Normal info                        **
    /// ***********************************************************************
    public struct VertexPositionNormal
    {
        #region Local and Static Variables
        public Vector3 Position;
        public Vector3 Normal;
        public static readonly int SizeInBytes = sizeof(float) * 6;
        public static readonly VertexElement[] VertexElements = 
            {
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 )
            };
        #endregion

        #region Constructor - VertexPosition(Vector3 position)
        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            this.Position = position;
            this.Normal = normal;
        }
        #endregion

    }
    /// <summary>
    /// ***********************************************************************
    /// **                      VertexPositionNormal                         **
    /// ** Vertex class with Position and Normal info                        **
    /// ***********************************************************************
    public struct VertexPositionNormalTextureColor
    {
        #region Local and Static Variables
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public Color   Color;
        public static readonly int SizeInBytes = sizeof(float) * 9;
        public static readonly VertexElement[] VertexElements = 
            {
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
                 new VertexElement( 0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),
                 new VertexElement( 0, 0, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 )
            };
        #endregion

        #region Constructor - VertexPosition(Vector3 position)
        public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 texture, Color color)
        {
            this.Position = position;
            this.Normal = normal;
            this.Texture = texture;
            this.Color = color;
        }
        #endregion

    }
}
