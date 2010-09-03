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

        Vector2 Position; // X and Y (no movement in Z plane for 2.5D)
        float Rotation; // In radians

        Matrix rot, trans, scale, world;
        bool dirtyWorldMatrix; // Only perform matrix multiplication when we need to

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
        public void LoadContent()
        {
            model = h_game.Content.Load<Model>(modelFile);
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// DrawUsingCurrentEffect - Assuemes higher level function has started the correct effect draw sequence
        /// ***********************************************************************
        public void DrawUsingCurrentEffect(GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
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
        public void ChangeEffectUsedByModel(Effect replacementEffect)
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
    }
}
