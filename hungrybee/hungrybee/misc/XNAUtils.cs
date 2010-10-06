using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                             XNAUtils                              **
    /// ** Some helper functions taken from the book:                        **
    /// ** XNA 3.0 - Game Programming Recipies: A Problem-Solution Approach  **
    /// ***********************************************************************
    /// </summary>
    public static class XNAUtils
    {
        static float EPSILON = 0.0000000001f;

        // Jonno Tompson Code
        public static float GetAABBVolume(BoundingBox bBox)
        {
            return (bBox.Max.X - bBox.Min.X) * (bBox.Max.Y - bBox.Min.Y) * (bBox.Max.Z - bBox.Min.Z);
        }

        // Jonno Tompson Code
        public static float GetSphereVolume(BoundingSphere bSphere)
        {
            return 2.0f * (float)Math.PI * bSphere.Radius * bSphere.Radius;
        }

        // Jonno Tompson Code
        // Calculate from http://en.wikipedia.org/wiki/List_of_moment_of_inertia_tensors
        public static Matrix CalculateItensorFromBoundingSphere(BoundingSphere bSphere, float mass)
        {
            Matrix Itensor = Matrix.Identity;
            float diagValue = (2.0f / 3.0f) * mass * (bSphere.Radius) * (bSphere.Radius);
            Itensor.M11 = diagValue; Itensor.M22 = diagValue; Itensor.M33 = diagValue;

            return Itensor;
        }

        // Jonno Tompson Code
        // Calculate from http://en.wikipedia.org/wiki/List_of_moment_of_inertia_tensors
        public static Matrix CalculateItensorFromBoundingBox(BoundingBox bBox, float mass)
        {
            Matrix Itensor = Matrix.Identity;
            float width = bBox.Max.X - bBox.Min.X;
            float height = bBox.Max.Y - bBox.Min.Y;
            float depth = bBox.Max.Z - bBox.Min.Z;
            Itensor.M11 = (1.0f / 12.0f) * mass * ((height * height) + (depth * depth));
            Itensor.M22 = (1.0f / 12.0f) * mass * ((width * width) + (depth * depth));
            Itensor.M22 = (1.0f / 12.0f) * mass * ((width * width) + (height * height));

            return Itensor;
        }

        // Jonno Tompson Code
        // Calculate Min and Max verticies and return a BoundingBox
        public static BoundingBox CreateAABBFromVerticies(VertexPositionNormalTexture[] verticies)
        {
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();

            // Initialize the min and max verticies to the first point
            min.X = verticies[0].Position.X; min.Y = verticies[0].Position.Y; min.Z = verticies[0].Position.Z;
            max.X = verticies[0].Position.X; max.Y = verticies[0].Position.Y; max.Z = verticies[0].Position.Z;

            // Now try and find the min and max verticies
            for (int curVertex = 0; curVertex < verticies.Length; curVertex++)
            {
                if (min.X > verticies[curVertex].Position.X)
                    min.X = verticies[curVertex].Position.X;
                if (min.Y > verticies[curVertex].Position.Y)
                    min.Y = verticies[curVertex].Position.Y;
                if (min.Z > verticies[curVertex].Position.Z)
                    min.Z = verticies[curVertex].Position.Z;

                if (max.X < verticies[curVertex].Position.X)
                    max.X = verticies[curVertex].Position.X;
                if (max.Y < verticies[curVertex].Position.Y)
                    max.Y = verticies[curVertex].Position.Y;
                if (max.Z < verticies[curVertex].Position.Z)
                    max.Z = verticies[curVertex].Position.Z;
            }

            BoundingBox retVal = new BoundingBox(min, max);
            return retVal;
        }

        // Jonno Tompson Code
        // Calculate Min and Max verticies and return a BoundingBox
        public static BoundingBox CreateAABBFromVerticies(VertexPosition[] verticies)
        {
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();

            // Initialize the min and max verticies to the first point
            min.X = verticies[0].Position.X; min.Y = verticies[0].Position.Y; min.Z = verticies[0].Position.Z;
            max.X = verticies[0].Position.X; max.Y = verticies[0].Position.Y; max.Z = verticies[0].Position.Z;

            // Now try and find the min and max verticies
            for (int curVertex = 0; curVertex < verticies.Length; curVertex++)
            {
                if (min.X > verticies[curVertex].Position.X)
                    min.X = verticies[curVertex].Position.X;
                if (min.Y > verticies[curVertex].Position.Y)
                    min.Y = verticies[curVertex].Position.Y;
                if (min.Z > verticies[curVertex].Position.Z)
                    min.Z = verticies[curVertex].Position.Z;

                if (max.X < verticies[curVertex].Position.X)
                    max.X = verticies[curVertex].Position.X;
                if (max.Y < verticies[curVertex].Position.Y)
                    max.Y = verticies[curVertex].Position.Y;
                if (max.Z < verticies[curVertex].Position.Z)
                    max.Z = verticies[curVertex].Position.Z;
            }

            BoundingBox retVal = new BoundingBox(min, max);
            return retVal;
        }

        // Jonno Tompson Code
        // Calculate Min and Max verticies and return a BoundingBox
        public static BoundingBox CreateAABBFromModel(Model model)
        {
            BoundingBox retVal = new BoundingBox();

            Matrix []m_transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(m_transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                // Get the verticies
                VertexPositionNormalTexture[] vertices = null;
                GetModelMeshVertices(ref vertices, mesh);          

                // Find min, max xyz for this mesh - assumes will be centred on 0,0,0 as BB is initialised to 0,0,0
                Vector3 min = vertices[0].Position;
                Vector3 max = vertices[0].Position;

                for (int i = 1; i < vertices.Length; i++)
                {
                    min = Vector3.Min(min, vertices[i].Position);
                    max = Vector3.Max(max, vertices[i].Position);
                }                

                // We need to take into account the fact that the mesh may have a bone transform
                min = Vector3.Transform(min, m_transforms[mesh.ParentBone.Index]);
                max = Vector3.Transform(max, m_transforms[mesh.ParentBone.Index]);

                // Now expand main bb by this mesh's box
               retVal.Min = Vector3.Min(retVal.Min, min);
               retVal.Max = Vector3.Max(retVal.Max, max);
            }

            return retVal;
        }

        public static Vector3 GetModelCenter(Model model)
        {
            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix boneMat = Matrix.Identity;
            Vector3 center = new Vector3();
            Vector3 center_inBone = new Vector3();
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                // Load the verticies from the model
                VertexPositionNormalTexture[] vertices = null;
                XNAUtils.GetModelMeshVertices(ref vertices, modelMesh);

                for (int i = 0; i < vertices.Length; i++)
                    center_inBone += vertices[i].Position;
                center_inBone = center_inBone / vertices.Length;  // This is the average

                boneMat = modelTransforms[modelMesh.ParentBone.Index];
                center += Vector3.Transform(center_inBone, boneMat);
            }
            center = center / model.Meshes.Count;
            return center;
        }

        public static void GetModelMeshVertices(ref VertexPositionNormalTexture[] vertices, ModelMesh mesh)
        {
            // Get the bone vertex declaration
            ModelMeshPart part = mesh.MeshParts[0];  // a model can contain multiple MeshParts (just need first one to get declaration)
            VertexElement[] vertexElements = part.VertexDeclaration.GetVertexElements();
            int sizeInBytes = part.VertexStride;

            // HACK CODE --> Some models have VertexPositionNormalTexture and some have VertexPositionNormal
            // --> Check sizeInBytes and hope that we get the correct vertex declaration...  Works on the limited models for this project
            // but might not work in future for other models
            if (sizeInBytes == 24)
            {
                VertexPositionNormal[] verticesPosNormal = new VertexPositionNormal[mesh.VertexBuffer.SizeInBytes / sizeInBytes];
                mesh.VertexBuffer.GetData<VertexPositionNormal>(verticesPosNormal);
                vertices = new VertexPositionNormalTexture[mesh.VertexBuffer.SizeInBytes / sizeInBytes];
                for (int i = 0; i < mesh.VertexBuffer.SizeInBytes / sizeInBytes; i++)
                {
                    vertices[i].Position = verticesPosNormal[i].Position;
                    vertices[i].Normal = verticesPosNormal[i].Normal;
                    vertices[i].TextureCoordinate = Vector2.Zero;
                }
            }
            else
            {
                vertices = new VertexPositionNormalTexture[mesh.VertexBuffer.SizeInBytes / sizeInBytes];
                mesh.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);
            }
        }

        public static void SetModelMeshVertices(ref VertexPositionNormalTexture[] vertices, ModelMesh mesh)
        {
            // Get the bone vertex declaration
            ModelMeshPart part = mesh.MeshParts[0];  // a model can contain multiple MeshParts (just need first one to get declaration)
            VertexElement[] vertexElements = part.VertexDeclaration.GetVertexElements();
            int sizeInBytes = part.VertexStride;

            // HACK CODE --> Some models have VertexPositionNormalTexture and some have VertexPositionNormal
            // --> Check sizeInBytes and hope that we get the correct vertex declaration...  Works on the limited models for this project
            // but might not work in future for other models
            if (sizeInBytes == 24)
            {
                VertexPositionNormal[] verticesPosNormal = new VertexPositionNormal[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    verticesPosNormal[i].Position = vertices[i].Position;
                    verticesPosNormal[i].Normal = vertices[i].Normal;
                }
                mesh.VertexBuffer.SetData<VertexPositionNormal>(verticesPosNormal);
            }
            else
            {
                mesh.VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
            }
        }

        public static BoundingBox TransformBoundingBox(BoundingBox origBox, Matrix matrix)
        {
            Vector3 origCorner1 = origBox.Min;
            Vector3 origCorner2 = origBox.Max;

            Vector3 transCorner1 = Vector3.Transform(origCorner1, matrix);
            Vector3 transCorner2 = Vector3.Transform(origCorner2, matrix);

            return new BoundingBox(transCorner1, transCorner2);
        }

        public static BoundingSphere TransformBoundingSphere(BoundingSphere originalBoundingSphere, Matrix transformationMatrix)
        {
            Vector3 trans;
            Vector3 scaling;
            Quaternion rot;
            transformationMatrix.Decompose(out scaling, out rot, out trans);

            float maxScale = scaling.X;
            if (maxScale < scaling.Y)
                maxScale = scaling.Y;
            if (maxScale < scaling.Z)
                maxScale = scaling.Z;

            float transformedSphereRadius = originalBoundingSphere.Radius * maxScale;
            Vector3 transformedSphereCenter = Vector3.Transform(originalBoundingSphere.Center, transformationMatrix);

            BoundingSphere transformedBoundingSphere = new BoundingSphere(transformedSphereCenter, transformedSphereRadius);

            return transformedBoundingSphere;
        }

        public class ModelTag
        {
            public BoundingSphere bSphere;
            public bool modelRecentered;

            public ModelTag(BoundingSphere _bSphere)
            {
                bSphere = _bSphere;
            }
        }

        public static Model LoadModelWithBoundingSphere(ref Matrix[] modelTransforms, string asset, ContentManager content)
        {
            Model newModel = content.Load<Model>(asset);

            modelTransforms = new Matrix[newModel.Bones.Count];
            newModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

            BoundingSphere completeBoundingSphere = GetBoundingSphereFromModel(ref newModel);

            ModelTag tag = new ModelTag(completeBoundingSphere);
            newModel.Tag = tag;

            return newModel;
        }

        public static BoundingSphere GetBoundingSphereFromModel(ref Model model)
        {
            BoundingSphere completeBoundingSphere = new BoundingSphere();

            Vector3 ModelCenter = XNAUtils.GetModelCenter(model);
            float ModelRadius = GetRadiusFromModelAndCenter(ref model, ref ModelCenter);

            Vector3 curCenterPos = new Vector3();
            float curRadiusPos = 0.0f;
            Vector3 curCenterNeg = new Vector3();
            float curRadiusNeg = 0.0f;

            // NOW BRUTE FORCE ITERATE THE CENTER POSITION, TRYING TO IMPROVE THIS
            float StepSize = 0.01f; // Scaled by the curRadius
            float conversionNorm = 1.0f;
            float Norm = float.PositiveInfinity;
            float PreviousBestRadius = ModelRadius;
            Vector3[] directions = {Vector3.Right,Vector3.Up,Vector3.Down};

            while (Norm > conversionNorm)
            {
                for (int curDirection = 0; curDirection < directions.Length; curDirection++)
                {
                    // Optimize in each direction
                    bool iterate = true;
                    while (iterate)
                    {
                        // try a step in the positive direction
                        curCenterPos = ModelCenter + directions[curDirection] * StepSize * ModelRadius;
                        curRadiusPos = GetRadiusFromModelAndCenter(ref model, ref curCenterPos);

                        // try a step in the negative direction
                        curCenterNeg = ModelCenter - directions[curDirection] * StepSize * ModelRadius;
                        curRadiusNeg = GetRadiusFromModelAndCenter(ref model, ref curCenterPos);

                        if (curRadiusPos > curRadiusNeg)
                        { curCenterPos = curCenterNeg; curRadiusPos = curRadiusNeg; }

                        // If the best radius we've found is less than the model radius --> We've found a new optimization point
                        if (ModelRadius > curRadiusPos)
                        { ModelCenter = curCenterPos; ModelRadius = curRadiusPos; }
                        // Otherwise we've gone as far as we can
                        else
                        { iterate = false; }
                    }
                }
                if (PreviousBestRadius == ModelRadius)
                    break; // Gone as far as we can
                Norm = Math.Abs(ModelRadius - PreviousBestRadius);
                PreviousBestRadius = ModelRadius;
            }

            completeBoundingSphere.Center = ModelCenter;
            completeBoundingSphere.Radius = ModelRadius;
            return completeBoundingSphere;
        }

        public static float GetRadiusFromModelAndCenter(ref Model model, ref Vector3 ModelCenter)
        {
            // With this model center (geometric mean) --> Find the model bounds
            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            float radius = 0.0f;

            Vector3 curPos = new Vector3();
            Vector3 disp = new Vector3();
            float curRadius = 0.0f;
            Matrix boneMat;
            Matrix boneMatInv;
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                // Load the verticies from the model
                VertexPositionNormalTexture[] vertices = null;
                XNAUtils.GetModelMeshVertices(ref vertices, modelMesh);

                boneMat = modelTransforms[modelMesh.ParentBone.Index];
                boneMatInv = Matrix.Invert(boneMat);

                for (int i = 0; i < vertices.Length; i++)
                {
                    curPos = Vector3.Transform(vertices[i].Position, boneMat);
                    disp = curPos - ModelCenter;
                    curRadius = (float)Math.Sqrt(disp.X * disp.X + disp.Y * disp.Y + disp.Z * disp.Z);
                    if (radius < curRadius)
                        radius = curRadius;
                }

            }
            return radius;
        }

        public static void DrawBoundingBox(BoundingBox bBox, GraphicsDevice device, BasicEffect basicEffect, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            Vector3 v1 = bBox.Min;
            Vector3 v2 = bBox.Max;

            VertexPositionColor[] cubeLineVertices = new VertexPositionColor[8];
            cubeLineVertices[0] = new VertexPositionColor(v1, Color.White);
            cubeLineVertices[1] = new VertexPositionColor(new Vector3(v2.X, v1.Y, v1.Z), Color.Red);
            cubeLineVertices[2] = new VertexPositionColor(new Vector3(v2.X, v1.Y, v2.Z), Color.Green);
            cubeLineVertices[3] = new VertexPositionColor(new Vector3(v1.X, v1.Y, v2.Z), Color.Blue);

            cubeLineVertices[4] = new VertexPositionColor(new Vector3(v1.X, v2.Y, v1.Z), Color.White);
            cubeLineVertices[5] = new VertexPositionColor(new Vector3(v2.X, v2.Y, v1.Z), Color.Red);
            cubeLineVertices[6] = new VertexPositionColor(v2, Color.Green);
            cubeLineVertices[7] = new VertexPositionColor(new Vector3(v1.X, v2.Y, v2.Z), Color.Blue);

            short[] cubeLineIndices = { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;
            device.RenderState.FillMode = FillMode.Solid;            
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
                device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, cubeLineVertices, 0, 8, cubeLineIndices, 0, 12);
                pass.End();
            }
            basicEffect.End();
        }

        public static void DrawSphereSpikes(BoundingSphere sphere, GraphicsDevice device, BasicEffect basicEffect, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            Vector3 up = sphere.Center + sphere.Radius * Vector3.Up;
            Vector3 down = sphere.Center + sphere.Radius * Vector3.Down;
            Vector3 right = sphere.Center + sphere.Radius * Vector3.Right;
            Vector3 left = sphere.Center + sphere.Radius * Vector3.Left;
            Vector3 forward = sphere.Center + sphere.Radius * Vector3.Forward;
            Vector3 back = sphere.Center + sphere.Radius * Vector3.Backward;

            VertexPositionColor[] sphereLineVertices = new VertexPositionColor[6];
            sphereLineVertices[0] = new VertexPositionColor(up, Color.White);
            sphereLineVertices[1] = new VertexPositionColor(down, Color.White);
            sphereLineVertices[2] = new VertexPositionColor(left, Color.White);
            sphereLineVertices[3] = new VertexPositionColor(right, Color.White);
            sphereLineVertices[4] = new VertexPositionColor(forward, Color.White);
            sphereLineVertices[5] = new VertexPositionColor(back, Color.White);

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;            
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, sphereLineVertices, 0, 3);
                pass.End();
            }
            basicEffect.End();
            
        }

        public static VertexPositionColor[] VerticesFromVector3List(List<Vector3> pointList, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[pointList.Count];

            int i = 0;
            foreach (Vector3 p in pointList)
                vertices[i++] = new VertexPositionColor(p, color);

            return vertices;
        }

        public static BoundingBox CreateBoxFromSphere(BoundingSphere sphere)
        {
            float radius = sphere.Radius;
            Vector3 outerPoint = new Vector3(radius, radius, radius);

            Vector3 p1 = sphere.Center + outerPoint;
            Vector3 p2 = sphere.Center - outerPoint;

            return new BoundingBox(p1, p2);
        }

        public static Matrix[] AutoScaleModelTransform(ref Model model, float requestedSize, ref float scalingFactor)
        {
            BoundingSphere bSphere = ((ModelTag)model.Tag).bSphere;
            float originalSize = bSphere.Radius * 2;
            scalingFactor = requestedSize / originalSize - EPSILON; // Add a small value so they can sit next to each other

            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            return modelTransforms;
        }


    }
}
