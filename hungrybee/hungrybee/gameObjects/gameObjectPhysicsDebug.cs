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
    /// ** and render them.                                                  **
    /// ** COLLIDABLE = FALSE                                                **
    /// ** MOVABLE = FALSE                                                   **
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

        public gameObject attachedGameObject;
        boundingObjType objType;
        Color color;

        Object obj;

        #endregion

        #region Constructor - gameObjectPhysicsDebug(game game, string file, string textureFile, Vector3 min, Vector3 max) : base(game, null)
        /// Constructor - Basic constructor
        /// ***********************************************************************
        public gameObjectPhysicsDebug(game game, boundingObjType _objType, 
                                      Object _obj, gameObject _attachedGameObject, Color _color)
            : base(game, null, _objType, false, true) // texture DISABLED, vertexColor ENABLED
        {
            objType = _objType;
            obj = _obj;
            attachedGameObject = _attachedGameObject;
            lineVertices = null;
            vertexDeclaration = null;
            numLines = 0;
            world = Matrix.Identity;
            base.movable = false;
            base.collidable = false;
            effect = null;
            color = _color;
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void LoadContent()
        {
            if( effect == null)
                effect = new BasicEffect(base.h_game.h_GraphicsDevice, null); // Create a basic effect if we haven't already
            
            if(vertexDeclaration == null)
                vertexDeclaration = new VertexDeclaration(base.h_game.h_GraphicsDevice, VertexPositionColor.VertexElements); // Create the vertexDeclaration if we haven't already

            UpdateContent();
        }
        #endregion

        #region UpdateContent()
        /// UpdateContent - Update the bounding object for rendering
        /// ***********************************************************************
        protected void UpdateContent()
        {
            // Create the vertex buffer depending on what the object type is
            if (objType == boundingObjType.SPHERE)
            {
                Vector3 Center = Vector3.Zero;
                float Radius = 0.0f;
                collisionUtils.UpdateBoundingSphere((BoundingSphere)obj, world, drawState.scale, attachedGameObject, ref Center, ref Radius);

                Vector3 up =        Center + Radius * Vector3.Up;
                Vector3 down =      Center + Radius * Vector3.Down;
                Vector3 right =     Center + Radius * Vector3.Right;
                Vector3 left =      Center + Radius * Vector3.Left;
                Vector3 forward =   Center + Radius * Vector3.Forward;
                Vector3 back =      Center + Radius * Vector3.Backward;

                numLines = 3;
                if (lineVertices == null)
                    lineVertices = new VertexPositionColor[numLines * 2];

                lineVertices[0] = new VertexPositionColor(up, color);
                lineVertices[1] = new VertexPositionColor(down, color);
                lineVertices[2] = new VertexPositionColor(left, color);
                lineVertices[3] = new VertexPositionColor(right, color);
                lineVertices[4] = new VertexPositionColor(forward, color);
                lineVertices[5] = new VertexPositionColor(back, color);
            }
            else if (objType == boundingObjType.AABB)
            {
                Vector3 min = new Vector3();
                Vector3 max = new Vector3();

                collisionUtils.UpdateBoundingBox((BoundingBox)obj, world, ref min, ref max);

                numLines = 12;
                if (lineVertices == null)
                    lineVertices = new VertexPositionColor[numLines * 2];

                // top back
                lineVertices[0] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
                lineVertices[1] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
                // top right
                lineVertices[2] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
                lineVertices[3] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
                // top front
                lineVertices[4] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
                lineVertices[5] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
                // top left
                lineVertices[6] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
                lineVertices[7] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);

                // side back left
                lineVertices[8] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
                lineVertices[9] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
                // side back right
                lineVertices[10] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
                lineVertices[11] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
                // side front right
                lineVertices[12] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
                lineVertices[13] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
                // side front left
                lineVertices[14] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
                lineVertices[15] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);

                // bottom back
                lineVertices[16] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
                lineVertices[17] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
                // bottom right
                lineVertices[18] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
                lineVertices[19] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
                // bottom front
                lineVertices[20] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
                lineVertices[21] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
                // bottom left
                lineVertices[22] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
                lineVertices[23] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            }
            else // object type is not supported
                throw new Exception("gameObjectPhysicsDebug::LoadContent() - Physics debug object is not yet supported");
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// DrawUsingCurrentEffect - Draw the bounding object
        /// ***********************************************************************
        public override void DrawUsingCurrentEffect(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // Create a world matrix which is from the attachedGameObject
            // This is probably redundant since we're drawing both the game object and the debug object --> But it's ok for debug types
            float percentInterp = 0.0f;
            float deltaT = attachedGameObject.state.time - attachedGameObject.prevState.time;


            // DOESN'T WORK!!! --> NEED TO DEBUG HOW XNA IS UPDATING DRAW DELTAT (FIX FOR GAMEOBJECT AND GAMEOBJECTPHYSICSDEBUG)
            //if (deltaT > 0.0f)
            //    percentInterp = gameTime.ElapsedGameTime.Seconds / deltaT;
            percentInterp = 1.0f;

            drawState.scale = Interp(attachedGameObject.prevState.scale, attachedGameObject.state.scale, percentInterp);
            drawState.orient = Quaternion.Slerp(attachedGameObject.prevState.orient, attachedGameObject.state.orient, percentInterp);
            drawState.pos = Interp(attachedGameObject.prevState.pos, attachedGameObject.state.pos, percentInterp);
            world = attachedGameObject.CreateScale(drawState.scale) *
                    Matrix.CreateFromQuaternion(drawState.orient) *
                    Matrix.CreateTranslation(drawState.pos);

            UpdateContent();

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                base.h_game.h_GraphicsDevice.VertexDeclaration = vertexDeclaration;
                base.h_game.h_GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, lineVertices, 0, numLines);
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
