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
    /// ** Game object with a mesh instance to be drawn.                     **
    /// ** COLLIDABLE = FALSE (default)                                      **
    /// ** MOVABLE = FALSE (default)                                         **
    /// ***********************************************************************
    /// </summary>
    public class gameObject
    {
        #region Local Variables

        public game h_game;
        public Model model;
        public bool textureEnabled;
        public bool vertexColorEnabled;
        string modelFile;
        Matrix[] modelTransforms;
        public float modelScaleToNormalizeSize;
        public Vector3 modelOffsetToCenterOnSphere;

        public boundingObjType boundingObjType;
        public Object boundingObj;
        public Vector3 boundingObjCenter;

        // Floating animation
        public float floatingPhase;
        public float floatingAngularVel;
        public float floatingAmplitude;
        public float floatingOffset;

        public BoundingBox sweepAndPruneAABB;
        public Vector3 AABB_min, AABB_max;
        public bool dirtyAABB; // Flag for collision detection to avoid computing translated aabb twice
        public static Vector3 AABB_min_t0 = new Vector3();
        public static Vector3 AABB_max_t0 = new Vector3();
        public static Vector3 AABB_min_t1 = new Vector3();
        public static Vector3 AABB_max_t1 = new Vector3();
        public static Matrix mat_t0 = new Matrix();
        public static Matrix mat_t1 = new Matrix();

        public rboState state;
        public rboState prevState;
        public rboState drawState;
        public bool movable; // Does the object react to RK4 integrator or move after collisions
        public bool collidable; // Does the object take part in collision detection
        public bool resting;

        public List<force> forceList;

        #endregion

        #region Constructor gameObject()
        /// Constructor
        /// ***********************************************************************
        public gameObject( game game )
            : base()
        {
            state = new rboState();
            prevState = new rboState();
            drawState = new rboState();
            movable = false;
            collidable = false;
            h_game = game;
            modelFile = null;
            modelScaleToNormalizeSize = 1.0f;
            forceList = new List<force>(game.h_GameSettings.forceListCapacity);
            boundingObjType = boundingObjType.UNDEFINED;
            dirtyAABB = true;
            textureEnabled = true;
            vertexColorEnabled = false;
            floatingPhase = 0.0f;
            floatingAngularVel = 0.0f;
            floatingAmplitude = 0.0f;
            floatingOffset = 0.0f;
        }
        #endregion

        #region Constructor gameObject( game game, string modelfile, boundingObjType objType, bool _textureEnabled, bool _vertexColorEnabled )
        /// Constructor
        /// ***********************************************************************
        public gameObject(game game, string modelfile, boundingObjType objType, bool _textureEnabled, bool _vertexColorEnabled )
            : base()
        {
            state = new rboState();
            prevState = new rboState();
            drawState = new rboState();
            movable = false;
            collidable = false;
            h_game = game;
            modelFile = modelfile;
            modelScaleToNormalizeSize = 1.0f;
            forceList = new List<force>(game.h_GameSettings.forceListCapacity);
            boundingObjType = objType;
            dirtyAABB = true;
            resting = false;
            textureEnabled = _textureEnabled;
            vertexColorEnabled = _vertexColorEnabled;
            floatingPhase = 0.0f;
            floatingAngularVel = 0.0f;
            floatingAmplitude = 0.0f;
            floatingOffset = 0.0f;
        }
        #endregion

        #region virtual Update(GameTime gameTime)
        /// Update - If we've moved the object then update the world transform
        /// ***********************************************************************
        public virtual void Update(GameTime gameTime)
        {
            if (!h_game.h_PhysicsManager.gamePaused)
            {
                // Update the floating offset animation
                floatingPhase += floatingAngularVel * (float)gameTime.ElapsedGameTime.TotalSeconds;
                floatingOffset = (float)Math.Sin(floatingPhase) * floatingAmplitude + floatingAmplitude; // Want offset to go from 0 --> floatingAmplitude (to avoid going through floor)
            }
        }
        #endregion

        #region virtual LoadContent()
        /// LoadContent - Load in the model from file
        /// ***********************************************************************
        public virtual void LoadContent()
        {
            // Load the model from file
            model = XNAUtils.LoadModelWithBoundingSphere(ref modelTransforms, modelFile, h_game.Content);

            // ScaleModel(model); // ScaleModel() function no longer in use!  Now scaling transform instead.
            modelTransforms = XNAUtils.AutoScaleModelTransform(ref model, 1.0f, ref modelScaleToNormalizeSize); 
            // NOTE: bounding sphere wont change, only obj->world transform

            // Grab the Bounding sphere data
            BoundingSphere bSphere = ((XNAUtils.ModelTag)model.Tag).bSphere;

            // Create a Bounding Box
            BoundingBox bBox = XNAUtils.CreateAABBFromModel(model);
            sweepAndPruneAABB = bBox;

            // Choose the bounding volume that has the smallest volume...
            if( boundingObjType == boundingObjType.UNDEFINED )
                if (XNAUtils.GetSphereVolume(bSphere) < XNAUtils.GetAABBVolume(bBox))
                    boundingObjType = boundingObjType.SPHERE;
                else
                    boundingObjType = boundingObjType.AABB;

            switch (boundingObjType)
            {
                case (boundingObjType.SPHERE):
                    boundingObj = (Object)bSphere;
                    boundingObjCenter = bSphere.Center;
                    // Calculate moment of Inertia from bounding sphere:
                    state.Itensor = XNAUtils.CalculateItensorFromBoundingSphere(bSphere, state.mass);
                    state.InvItensor = Matrix.Invert(state.Itensor);
                    break;
                case (boundingObjType.AABB):
                    boundingObj = (Object)bBox;
                    boundingObjCenter = (bBox.Max + bBox.Min) / 2.0f;
                    // Calculate moment of Inertia from bounding box:
                    state.Itensor = XNAUtils.CalculateItensorFromBoundingBox(bBox, state.mass);
                    state.InvItensor = Matrix.Invert(state.Itensor);
                    break;
                default:
                    throw new Exception("gameObject::LoadContent() - Something went wrong setting up bounding object");
            }

            // Make sure both starting states are equal
            rboState.CopyAtoB(ref state, ref prevState);   

            // Make the phase that the objects float at random
            Random rand = new Random(); // Seed off the time
            floatingPhase = (float)rand.NextDouble() * 2.0f * (float)Math.PI; // 0 --> 2PI
            floatingAngularVel = (0.5f + 0.5f * (float)rand.NextDouble()) * h_game.h_GameSettings.floatingAngularVel; // floatingAngularVel/2 --> floatingAngularVel
            floatingAmplitude = (0.5f + 0.5f * (float)rand.NextDouble()) * h_game.h_GameSettings.floatingAmplitude; // floatingAmplitude/2 --> floatingAmplitude
        }
        #endregion

        #region virtual DrawUsingCurrentEffect()
        /// DrawUsingCurrentEffect - Assuemes higher level function has started the correct effect draw sequence
        /// ***********************************************************************
        public virtual void DrawUsingCurrentEffect(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // Set suitable renderstates for drawing a 3D model.
            RenderState renderState = h_game.h_GraphicsDevice.RenderState;

            renderState.AlphaBlendEnable = false;
            renderState.AlphaTestEnable = false;
            renderState.DepthBufferEnable = true;

            // Calculate the object's transform from state variables --> Need to do lerp and slerp between states
            float deltaT = state.time - prevState.time;
            float percentInterp = 0.0f;

            // DOESN'T WORK!!! --> NEED TO DEBUG HOW XNA IS UPDATING DRAW DELTAT (FIX FOR GAMEOBJECT AND GAMEOBJECTPHYSICSDEBUG)
            //if (deltaT > 0.0f)
            //    percentInterp = gameTime.ElapsedGameTime.Seconds / deltaT;
            percentInterp = 1.0f;

            drawState.scale = Interp(prevState.scale, state.scale, percentInterp);
            drawState.orient = Quaternion.Normalize(Quaternion.Slerp(prevState.orient, state.orient, percentInterp));
            drawState.pos = Interp(prevState.pos, state.pos, percentInterp) + new Vector3(0.0f, floatingOffset, 0.0f);
            model.Root.Transform = Matrix.CreateTranslation(modelOffsetToCenterOnSphere) * 
                                   CreateScale(drawState.scale) *
                                   Matrix.CreateFromQuaternion(drawState.orient) *
                                   Matrix.CreateTranslation(drawState.pos);

            // Look up the bone transform matrices.
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                string curEffectTechniqueName = null;
                if (mesh.MeshParts[0].VertexStride == 24) // If model doesn't have texture data
                    switch (effectTechniqueName)
                    {
                        case "NormalDepth":
                            curEffectTechniqueName = "NormalDepth";
                            break;
                        case "Toon":
                            curEffectTechniqueName = "Toon_noTexture";
                            break;
                        case "Lambert":
                            curEffectTechniqueName = "Lambert_noTexture";
                            break;
                        default:
                            throw new Exception("gameObject::DrawUsingCurrentEffect() - Unrecognised effect Technique name");
                    }
                else
                    curEffectTechniqueName = effectTechniqueName;

                foreach (Effect effect in mesh.Effects)
                {
                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques[curEffectTechniqueName];

                    Matrix localWorld = modelTransforms[mesh.ParentBone.Index];

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                }

                mesh.Draw();
            }
        }
        #endregion

        #region virtual ChangeEffectUsedByModel()
        /// Alters a model so it will draw using a custom effect, while preserving
        /// whatever textures were set on it as part of the original effects.
        /// If no texture data is found, it will preserve vertex colors
        /// ***********************************************************************
        public virtual void ChangeEffectUsedByModel(Effect replacementEffect)
        {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();
            bool changeEffect = false;
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Effects[0] is BasicEffect)
                {
                    changeEffect = true;
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

                            newEffect.Parameters["TextureEnabled"].SetValue(textureEnabled);

                            newEffect.Parameters["DiffuseColor"].SetValue(oldEffect.DiffuseColor);
                            newEffect.Parameters["VertexColorEnabled"].SetValue(vertexColorEnabled);

                            effectMapping.Add(oldEffect, newEffect);
                        }
                    }
                }
                else
                {
                    // don't change anything
                }

                // Now that we've found all the effects in use on this mesh,
                // update it to use our new replacement versions.
                if (changeEffect)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = effectMapping[meshPart.Effect];
                    }
                }
            }
        }
        #endregion

        #region MatrixCreateScale(Vector3 Scale)
        /// MatrixCreateScale - Create a scale matrix, but also add in the normalization factor
        /// ***********************************************************************
        public Matrix CreateScale(Vector3 Scale)
        {
            return Matrix.CreateScale(Vector3.Multiply(Scale, modelScaleToNormalizeSize));
        }
        #endregion

        #region virtual GetForceTorque()
        /// GetForceTorque - Get the force and torque for each object to be used in the integrator
        /// ***********************************************************************
        public virtual void GetForceTorque(ref Vector3 force, ref Vector3 torque, ref rboState rboState, float time)
        {
            // Nothing to do by default, just zero out vectors.
            force.X = 0.0f; force.Y = 0.0f; force.Z = 0.0f;
            torque.X = 0.0f; torque.Y = 0.0f; torque.Z = 0.0f;

            // Step through the forceList adding each force and removeing ones that have expired
            // List<>.Enumerator is a read-only interface... Step through the list manually
            for (int curForce = 0; curForce < forceList.Count; curForce++)
            {
                force += forceList[curForce].GetForce(ref rboState, time);
                torque += forceList[curForce].GetTorque(ref rboState, time);
            }
        }
        #endregion

        #region Interpolation Routines
        public static float Interp(float start, float end, float percentInterp)
        {
            return (end - start) * percentInterp + start;
        }
        public static Vector3 Interp(Vector3 start, Vector3 end, float percentInterp)
        {
            return (end - start) * percentInterp + start;
        }
        #endregion

        #region virtual UpdateCoarseBoundingBox()
        /// UpdateCoarseBoundingBox() - Update the bounding box used in Collision detection
        /// Box is formed by the origional static box swept through the vector (state.pos - prevState.pos)
        /// For fast moving objects this will be a very large Bounding box
        /// ***********************************************************************
        public virtual void UpdateCoarseBoundingBox()
        {
            if (dirtyAABB)
            {
                mat_t0 = CreateScale(prevState.scale) * Matrix.CreateFromQuaternion(prevState.orient) * Matrix.CreateTranslation(prevState.pos);
                mat_t1 = CreateScale(state.scale) * Matrix.CreateFromQuaternion(state.orient) * Matrix.CreateTranslation(state.pos);

                if (boundingObjType == boundingObjType.AABB)
                {
                    collisionUtils.UpdateBoundingBox(sweepAndPruneAABB, mat_t0, ref AABB_min_t0, ref AABB_max_t0);
                    collisionUtils.UpdateBoundingBox(sweepAndPruneAABB, mat_t1, ref AABB_min_t1, ref AABB_max_t1);
                }
                else if (boundingObjType == boundingObjType.SPHERE)
                {
                    Vector3 center = Vector3.Zero;
                    float radius = 0.0f;
                    collisionUtils.UpdateBoundingSphere((BoundingSphere)boundingObj, mat_t0, prevState.scale, this, ref center, ref radius);
                    AABB_min_t0 = center - new Vector3(radius, radius, radius);
                    AABB_max_t0 = center + new Vector3(radius, radius, radius);

                    collisionUtils.UpdateBoundingSphere((BoundingSphere)boundingObj, mat_t1, prevState.scale, this, ref center, ref radius);
                    AABB_min_t1 = center - new Vector3(radius, radius, radius);
                    AABB_max_t1 = center + new Vector3(radius, radius, radius);
                }
                else
                    throw new Exception("UpdateCoarseBoundingBox() - Bounding object type not supported");

                // Find smallest vector from t0 or t1
                AABB_min.X = collisionUtils.Min(AABB_min_t0.X, AABB_min_t1.X);
                AABB_min.Y = collisionUtils.Min(AABB_min_t0.Y, AABB_min_t1.Y);
                AABB_min.Z = collisionUtils.Min(AABB_min_t0.Z, AABB_min_t1.Z);

                // Find largest vector by sweeping sphere along the displacement between the two states
                AABB_max.X = collisionUtils.Max(AABB_max_t0.X, AABB_max_t1.X);
                AABB_max.Y = collisionUtils.Max(AABB_max_t0.Y, AABB_max_t1.Y);
                AABB_max.Z = collisionUtils.Max(AABB_max_t0.Z, AABB_max_t1.Z);

                dirtyAABB = false;
            }
        }
        #endregion

        #region SetDirtyAABB()
        /// SetDirtyAABB()
        /// ***********************************************************************
        public void SetDirtyAABB()
        {
            dirtyAABB = true;
        }
        #endregion

        #region CheckAntiGravityForce() --> Returns true if an antigravity force exists
        public bool CheckAntiGravityForce()
        {
            for (int i = forceList.Count - 1; i >= 0; i-- ) // if the force exists, it will be somewhere near the end of the list
            {
                if (forceList[i] is forceAntiGravity)
                    return true;
            }
            return false;
        }
        #endregion

        #region CheckPhantomForce() --> Returns true if a forcePhantom exists
        public bool CheckPhantomForce()
        {
            for (int i = forceList.Count - 1; i >= 0; i--) // if the force exists, it will be somewhere near the end of the list
            {
                if (forceList[i] is forcePhantom)
                    return true;
            }
            return false;
        }
        #endregion

        #region AddAntiGravityForce()
        public void AddAntiGravityForce(float acceleration)
        {
            forceList.Add(new forceAntiGravity(new Vector3(0.0f, acceleration, 0.0f)));
        }
        #endregion

        #region AddPhantomForce()
        public void AddPhantomForce(Vector3 force)
        {
            forceList.Add(new forcePhantom(force));
        }
        #endregion

        #region RemoveAntiGravityForce()
        public void RemoveAntiGravityForce()
        {
            for (int i = forceList.Count - 1; i >= 0; i--) // if the force exists, it will be somewhere near the end of the list
            {
                if (forceList[i] is forceAntiGravity)
                    forceList.RemoveAt(i);
            }
        }
        #endregion

        #region RemovePhantomForce()
        public void RemovePhantomForce()
        {
            for (int i = forceList.Count - 1; i >= 0; i--) // if the force exists, it will be somewhere near the end of the list
            {
                if (forceList[i] is forcePhantom)
                    forceList.RemoveAt(i);
            }
        }
        #endregion

        #region CenterObjectAboutBoundingSphere()
        // Centers the object so it's bounding object center lies on the Z=0 plane
        public void CenterObjectAboutBoundingSphere()
        {
            if (boundingObjType == boundingObjType.SPHERE)
            {
                BoundingSphere sphere = (BoundingSphere)boundingObj;
                XNAUtils.ModelTag tag = (XNAUtils.ModelTag)model.Tag;

                //// Offset all the verticies by the BoundingSphere.Center if we haven't already
                //if (!tag.modelRecentered)
                //{
                //    //XNAUtils.OffsetVertices(ref model, -sphere.Center); // <- DOESN'T WORK!!
                //    model.Root.Transform *= Matrix.CreateTranslation(-sphere.Center);
                //}

                modelOffsetToCenterOnSphere = -sphere.Center;

                // Now offset the BoundingSphere
                sphere.Center = Vector3.Zero;
                tag.modelRecentered = true;
                // tag.bSphere = sphere;  // DON'T DO THIS, LEAVE THE MODEL SPHERE WHERE IT IS
                model.Tag = tag;
                boundingObj = (Object)sphere;
            }
            else
                throw new Exception("gameObject::CenterObjectAboutBoundingSphere() - Function was incorrectly called on a bounding object other than SPHERE");
        }
        #endregion

        #region GetAbsoluteTransform()
        public static Matrix GetAbsoluteTransform(ModelBone bone)
        {
            if (bone == null)
                return Matrix.Identity;
            return bone.Transform * GetAbsoluteTransform(bone.Parent);
        }
        #endregion
    }
}
