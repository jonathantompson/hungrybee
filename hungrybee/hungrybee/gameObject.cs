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
    /// **                           gameObject                              **
    /// ** Game object with a mesh instance to be drawn                      **
    /// ***********************************************************************
    /// </summary>
    class gameObject
    {
        #region Local Variables

        game h_game;
        public Model model;
        string modelFile;
        Matrix[] modelTransforms;

        protected Vector2 Position; // X and Y (no movement in Z plane for 2.5D)
        protected float Rotation; // In radians

        protected Matrix rot, trans, scale, world;
        protected bool dirtyWorldMatrix; // Only perform matrix multiplication when we need to

        #endregion

        #region Constructor DrawableGameObject(game game, string ModelFile)
        /// Constructor - World transform is identity by default
        /// ***********************************************************************
        public gameObject(game game, string modelfile) : base()
        {
            Position = new Vector2(0.0f, 0.0f);
            Rotation = 0.0f;
            h_game = game;
            modelFile = modelfile;
            rot = Matrix.Identity;
            trans = Matrix.Identity;
            scale = Matrix.Identity;
            world = Matrix.Identity;
            dirtyWorldMatrix = false;
        }
        #endregion

        #region Update(GameTime gameTime)
        /// Update - If we've moved the object then update the world transform
        /// ***********************************************************************
        public void Update(GameTime gameTime)
        {
            if (dirtyWorldMatrix == true)
            {
                world = trans * rot * scale;
                dirtyWorldMatrix = false;
            }
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Load in the model from file
        /// ***********************************************************************
        public virtual void LoadContent()
        {
            // Load the model from file
            model = XNAUtils.LoadModelWithBoundingSphere(ref modelTransforms, modelFile, h_game.Content);

            // ScaleModel(model); // ScaleModel() function no longer in use!  Now scaling transform instead.
            modelTransforms = XNAUtils.AutoScaleModelTransform(ref model, 1.0f);
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// DrawUsingCurrentEffect - Assuemes higher level function has started the correct effect draw sequence
        /// ***********************************************************************
        public virtual void DrawUsingCurrentEffect(GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // Set suitable renderstates for drawing a 3D model.
            RenderState renderState = h_game.GetGraphicsDevice().RenderState;

            renderState.AlphaBlendEnable = false;
            renderState.AlphaTestEnable = false;
            renderState.DepthBufferEnable = true;

            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques[effectTechniqueName];

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                }

                mesh.Draw();
            }
        }
        #endregion

        #region ChangeEffectUsedByModel()
        /// Alters a model so it will draw using a custom effect, while preserving
        /// whatever textures were set on it as part of the original effects.
        /// /// ***********************************************************************
        public virtual void ChangeEffectUsedByModel(Effect replacementEffect)
        {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                // Scan over all the effects currently on the mesh.
                foreach (BasicEffect oldEffect in mesh.Effects)
                {
                    // If we haven't already seen this effect...
                    if (!effectMapping.ContainsKey(oldEffect))
                    {
                        // Make a clone of our replacement effect. We can't just use
                        // it directly, because the same effect might need to be
                        // applied several times to different parts of the model using
                        // a different texture each time, so we need a fresh copy each
                        // time we want to set a different texture into it.
                        Effect newEffect = replacementEffect.Clone(
                                                    replacementEffect.GraphicsDevice);

                        // Copy across the texture from the original effect.
                        newEffect.Parameters["Texture"].SetValue(oldEffect.Texture);

                        newEffect.Parameters["TextureEnabled"].SetValue(
                                                            oldEffect.TextureEnabled);

                        effectMapping.Add(oldEffect, newEffect);
                    }
                }

                // Now that we've found all the effects in use on this mesh,
                // update it to use our new replacement versions.
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effectMapping[meshPart.Effect];
                }
            }
        }
        #endregion

        #region ScaleModel()
        /// ScaleModel - Scales the actual model verticies --> No longer in use!
        /// ***********************************************************************
        public static void ScaleModel(Model model)
        {
            // All meshes in the model share the same vertex buffer: Get the vertex data
            ModelMesh mesh = model.Meshes[0];
            ModelMeshPart part = mesh.MeshParts[0];
            VertexElement[] vertexElements = part.VertexDeclaration.GetVertexElements();
            int sizeInBytes = part.VertexStride;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[mesh.VertexBuffer.SizeInBytes / part.VertexStride];
            mesh.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

            // Calculate the Center of Mass
            Vector3 COM = Vector3.Zero;
            for (int curVertex = 0; curVertex < vertices.Length; curVertex++)
            {
                COM += vertices[curVertex].Position;
            }
            COM = COM / vertices.Length;

            // Offset each vertex position data with the -COM
            for (int curVertex = 0; curVertex < vertices.Length; curVertex++)
                vertices[curVertex].Position -= COM;

            // Find the Max/Min in each direction
            float MaxX = vertices[0].Position.X;
            float MinX = vertices[0].Position.X;
            float MaxY = vertices[0].Position.Y;
            float MinY = vertices[0].Position.Y;
            float MaxZ = vertices[0].Position.Z;
            float MinZ = vertices[0].Position.Z;
            for (int curVertex = 1; curVertex < vertices.Length; curVertex++)
            {
                if (vertices[curVertex].Position.X > MaxX)
                    MaxX = vertices[curVertex].Position.X;
                if (vertices[curVertex].Position.X < MinX)
                    MinX = vertices[curVertex].Position.X;
                if (vertices[curVertex].Position.Y > MaxY)
                    MaxY = vertices[curVertex].Position.Y;
                if (vertices[curVertex].Position.Y < MinY)
                    MinY = vertices[curVertex].Position.Y;
                if (vertices[curVertex].Position.Z > MaxZ)
                    MaxZ = vertices[curVertex].Position.Z;
                if (vertices[curVertex].Position.Z < MinZ)
                    MinZ = vertices[curVertex].Position.Z;
            }
            float Multratio = 1.0f;
            // If X span is largest, scale by this ratio
            if ((MaxX - MinX) > (MaxY - MinY) && (MaxX - MinX) > (MaxZ - MinZ))
                Multratio = Math.Abs((MaxX - MinX) / MinX);
            if ((MaxY - MinY) > (MaxX - MinX) && (MaxY - MinY) > (MaxZ - MinZ))
                Multratio = Math.Abs((MaxY - MinY) / MinY);
            if ((MaxZ - MinZ) > (MaxX - MinX) && (MaxZ - MinZ) > (MaxY - MinY))
                Multratio = 1.0f / Math.Abs((MaxZ - MinZ));

            // Scale all the vertex Positions by this value (so that Max - Min = 1 for every model)
            for (int curVertex = 0; curVertex < vertices.Length; curVertex++)
            {
                vertices[curVertex].Position *= Multratio;
            }

            // Now copy the new vertex data back
            mesh.VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
        }
        #endregion
    }
}
