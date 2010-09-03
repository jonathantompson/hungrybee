#region using statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                           coordCross                              **
    /// ** Debug class for rendering: From XNA 3.0 Game Programming Recipies **
    /// ***********************************************************************
    /// </summary>
    class coordCross
    {
        #region Local Variables

        private VertexPositionColor[] vertices;
        private VertexDeclaration vertDeclaration;

        #endregion 

        #region Constructor - coordCross(GraphicsDevice device)
        /// Constructor
        /// ***********************************************************************
        public coordCross(GraphicsDevice device)
        {
            InitVertices();
            vertDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
        }
        #endregion

        #region InitVerticies()
        /// InitVertices - Form the verticies by hand
        /// ***********************************************************************
        private void InitVertices()
        {
            vertices = new VertexPositionColor[30];

            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[1] = new VertexPositionColor(Vector3.Right * 5, Color.White);
            vertices[2] = new VertexPositionColor(new Vector3(5, 0, 0), Color.White);
            vertices[3] = new VertexPositionColor(new Vector3(4.5f, 0.5f, 0), Color.White);
            vertices[4] = new VertexPositionColor(new Vector3(5, 0, 0), Color.White);
            vertices[5] = new VertexPositionColor(new Vector3(4.5f, -0.5f, 0), Color.White);

            vertices[6] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[7] = new VertexPositionColor(Vector3.Up * 5, Color.White);
            vertices[8] = new VertexPositionColor(new Vector3(0, 5, 0), Color.White);
            vertices[9] = new VertexPositionColor(new Vector3(0.5f, 4.5f, 0), Color.White);
            vertices[10] = new VertexPositionColor(new Vector3(0, 5, 0), Color.White);
            vertices[11] = new VertexPositionColor(new Vector3(-0.5f, 4.5f, 0), Color.White);

            vertices[12] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[13] = new VertexPositionColor(Vector3.Forward * 5, Color.White);
            vertices[14] = new VertexPositionColor(new Vector3(0, 0, -5), Color.White);
            vertices[15] = new VertexPositionColor(new Vector3(0, 0.5f, -4.5f), Color.White);
            vertices[16] = new VertexPositionColor(new Vector3(0, 0, -5), Color.White);
            vertices[17] = new VertexPositionColor(new Vector3(0, -0.5f, -4.5f), Color.White);
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// DrawUsingCurrentEffect - Assuemes higher level function has started a basic effect draw sequence
        /// ***********************************************************************
        public void DrawUsingCurrentEffect(GraphicsDevice device)
        {
            device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 9);
        }
        #endregion
    }
}
