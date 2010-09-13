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
    /// <summary>
    /// ***********************************************************************
    /// **                    gameObjectPhysicsDebug                         **
    /// ** This is a class to store the data for the physics debug objects   **
    /// ** and render them **
    /// ***********************************************************************
    /// </summary>
    class gameObjectPhysicsDebug : gameObject
    {
        #region Local Variables
        /// Local Variables
        /// ***********************************************************************    

        VertexPositionColor[] lineVertices;
        int numLines;
        VertexDeclaration vertexDeclaration;

        BasicEffect effect;
        Matrix world;

        gameObject attachedGameObject;
        boundingObjType objType;
        Object obj;

        #endregion

        #region Constructor - gameObjectPhysicsDebug(game game, string file, string textureFile, Vector3 min, Vector3 max) : base(game, null)
        public gameObjectPhysicsDebug(game game, boundingObjType _objType, Object _obj, gameObject _attachedGameObject)
            : base(game, null, _objType)
        {
            objType = _objType;
            obj = _obj;
            attachedGameObject = _attachedGameObject;
            lineVertices = null;
            vertexDeclaration = null;
            numLines = 0;
            world = Matrix.Identity;
        }
        #endregion

        #region LoadContent
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void LoadContent()
        {
            // Create a basic effect and vertexDeclaration
            effect = new BasicEffect(base.h_game.GetGraphicsDevice(), null);
            vertexDeclaration =  new VertexDeclaration(base.h_game.GetGraphicsDevice(), VertexPositionColor.VertexElements);

            // Create the vertex buffer depending on what the object type is
            if (objType == boundingObjType.SPHERE)
            {
                Vector3 up = ((BoundingSphere)obj).Center + ((BoundingSphere)obj).Radius * Vector3.Up;
                Vector3 down = ((BoundingSphere)obj).Center + ((BoundingSphere)obj).Radius * Vector3.Down;
                Vector3 right = ((BoundingSphere)obj).Center + ((BoundingSphere)obj).Radius * Vector3.Right;
                Vector3 left = ((BoundingSphere)obj).Center + ((BoundingSphere)obj).Radius * Vector3.Left;
                Vector3 forward = ((BoundingSphere)obj).Center + ((BoundingSphere)obj).Radius * Vector3.Forward;
                Vector3 back = ((BoundingSphere)obj).Center + ((BoundingSphere)obj).Radius * Vector3.Backward;

                numLines = 3;
                lineVertices = new VertexPositionColor[numLines * 2];
                lineVertices[0] = new VertexPositionColor(up, Color.White);
                lineVertices[1] = new VertexPositionColor(down, Color.White);
                lineVertices[2] = new VertexPositionColor(left, Color.White);
                lineVertices[3] = new VertexPositionColor(right, Color.White);
                lineVertices[4] = new VertexPositionColor(forward, Color.White);
                lineVertices[5] = new VertexPositionColor(back, Color.White);
            }
            else if (objType == boundingObjType.AABB)
            {
                Vector3 min = ((BoundingBox)obj).Min;
                Vector3 max = ((BoundingBox)obj).Max;

                numLines = 12;
                lineVertices = new VertexPositionColor[numLines * 2];
                // top back
                lineVertices[0] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), Color.White);
                lineVertices[1] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), Color.White);
                // top right
                lineVertices[2] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), Color.White);
                lineVertices[3] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), Color.White);
                // top front
                lineVertices[4] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), Color.White);
                lineVertices[5] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), Color.White);
                // top left
                lineVertices[6] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), Color.White);
                lineVertices[7] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), Color.White);

                // side back left
                lineVertices[8] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), Color.White);
                lineVertices[9] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), Color.White);
                // side back right
                lineVertices[10] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), Color.White);
                lineVertices[11] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), Color.White);
                // side front right
                lineVertices[12] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), Color.White);
                lineVertices[13] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), Color.White);
                // side front left
                lineVertices[14] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), Color.White);
                lineVertices[15] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), Color.White);

                // bottom back
                lineVertices[16] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), Color.White);
                lineVertices[17] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), Color.White);
                // bottom right
                lineVertices[18] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), Color.White);
                lineVertices[19] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), Color.White);
                // bottom front
                lineVertices[20] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), Color.White);
                lineVertices[21] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), Color.White);
                // bottom left
                lineVertices[22] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), Color.White);
                lineVertices[23] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), Color.White);
            }
            else // object type is not supported
                throw new Exception("gameObjectPhysicsDebug::LoadContent() - Physics debug object is not yet supported");
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void DrawUsingCurrentEffect(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // Create a world matrix which is from the attachedGameObject
            // This is probably redundant since we're drawing both the game object and the debug object --> But it's ok for debug types
            float percentInterp = 0.0f;
            float deltaT = attachedGameObject.state.time - attachedGameObject.prevState.time;
            if(deltaT > 0.0f)
                percentInterp = gameTime.ElapsedGameTime.Seconds / deltaT;
            drawState.scale = Interp(attachedGameObject.prevState.scale, attachedGameObject.state.scale, percentInterp);
            drawState.orient = Quaternion.Slerp(attachedGameObject.prevState.orient, attachedGameObject.state.orient, percentInterp);
            drawState.pos = Interp(attachedGameObject.prevState.pos, attachedGameObject.state.pos, percentInterp);
            world = attachedGameObject.CreateScale(drawState.scale) *
                    Matrix.CreateFromQuaternion(drawState.orient) *
                    Matrix.CreateTranslation(drawState.pos);

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                base.h_game.GetGraphicsDevice().VertexDeclaration = vertexDeclaration;
                base.h_game.GetGraphicsDevice().DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, lineVertices, 0, numLines);
                pass.End();
            }
            effect.End();

        }
        #endregion

        #region ChangeEffectUsedByModel()
        /// ChangeEffectUsedByModel - Change the effect being used
        /// ***********************************************************************
        public override void ChangeEffectUsedByModel(Effect replacementEffect)
        {
            // Empty, don't override yet
        }
        #endregion
    }
}
