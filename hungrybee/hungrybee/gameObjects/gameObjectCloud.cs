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
    /// **                       gameObjectCloud                             **
    /// ** This is a class to store the data for each cloud game object      **
    /// ** ie, the bee to be controlled by the human                         **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = FALSE                                                   **
    /// ***********************************************************************
    /// </summary>
    class gameObjectCloud : gameObject
    {
        #region Local Variables

        protected int numCircles;
        protected VertexBuffer vb;
        protected IndexBuffer ib;
        protected VertexDeclaration vdec;
        protected Effect effect;
        protected int strideSize;
        protected int nVertices;
        protected int nFaces;
        protected int nStacks;
        protected int nSlices;
        protected Vector3 min;
        protected Vector3 max;

        #endregion

        #region Constructor - gameObjectCloud()
        /// Constructor - gameObjectCloud()
        /// ***********************************************************************
        public gameObjectCloud(game game, int _nStacks, int _nSlices, Vector3 _min, Vector3 _max)
            : base(game, null, boundingObjType.UNDEFINED, false, false)
        {
            state.scale = new Vector3(1.0f, 1.0f, 1.0f);
            base.movable = false;
            base.collidable = true;

            nStacks = _nStacks;
            nSlices = _nSlices;

            max = _max;
            min = _min;
        }
        #endregion

        #region LoadContent() OLD V0
        /*
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void LoadContent()
        {
            int nVerticiesCube = 8;
            int nIndiciesCube = 36;

            //calculates the resulting number of vertices, indices and faces
            nVertices = (nStacks + 1) * (nSlices + 1) * numCircles;
            int dwIndices = nSlices * nStacks * 6 * numCircles;
            VertexPosition[] vertices = new VertexPosition[nVertices + nVerticiesCube];
            nFaces = dwIndices / 3 + nIndiciesCube / 3;
            int[] indices = new int[dwIndices + nIndiciesCube];

            float StackAngle = MathHelper.Pi / (float)nStacks;
            float SliceAngle = (float)(Math.PI * 2.0) / (float)nSlices;
            float phi, theta; int index = 0; int indexOffset; float x, y, z, sc;
            float Radius = 0.0f;
            Random rand = new Random(1);

            Vector3 boxSize = max - min; Vector3 boxCenter = new Vector3();
            if (boxSize.X < 0.0f || boxSize.Y < 0.0f || boxSize.Z < 0.0f)
                throw new Exception("gameObjectCloud::LoadContent() - The box size is negative! check inputs");

            for (int curCircle = 0; curCircle < numCircles; curCircle += 1)
            {
                // Choose a random sphere center within a box that is 10% within the boarders of Min and Max
                boxCenter.X = ((float)rand.NextDouble() * 0.8f + 0.1f) * boxSize.X + (min.X); // Parenteses varies 0.1-->0.9
                boxCenter.Y = ((float)rand.NextDouble() * 0.8f + 0.1f) * boxSize.Y + (min.Y);
                //boxCenter.Z = ((float)rand.NextDouble() * 0.6f + 0.2f) * boxSize.Z + (min.Z);
                boxCenter.Z = 0.0f;

                // Calculate the Radius so it just touches the outside of the box
                Radius = GetDistanceToAABB(boxCenter, min, max);
                if (Radius <= 0.0f)
                    throw new Exception("gameObjectCloud::LoadContent() - radius <= 0!  Something went wrong");

                index = (nVertices / numCircles) * curCircle;
                for (int stack = 0; stack <= nStacks; stack++)
                {
                    phi = MathHelper.PiOver2 - stack * SliceAngle;
                    y = Radius * (float)Math.Sin(phi);
                    sc = -Radius * (float)Math.Cos(phi);

                    for (int slice = 0; slice <= nSlices; slice++)
                    {
                        theta = slice * SliceAngle;
                        x = sc * (float)Math.Sin(theta);
                        z = sc * (float)Math.Cos(theta);
                        vertices[index++] = new VertexPosition(new Vector3(x, y, z) + boxCenter);
                    }
                }

                index = (dwIndices / numCircles) * curCircle;
                indexOffset = (nVertices / numCircles) * curCircle;
                int k = nSlices + 1;

                for (int stack = 0; stack < nStacks; stack++)
                {
                    for (int slice = 0; slice < nSlices; slice++)
                    {
                        indices[index++] = (stack + 0) * k + slice + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + indexOffset;
                        indices[index++] = (stack + 0) * k + slice + 1 + indexOffset;

                        indices[index++] = (stack + 0) * k + slice + 1 + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + 1 + indexOffset;
                    }
                }
            }

            // Now add verticies and indicies to draw a cube in the center --> Avoids holes in our cloud
            Vector3 cubeVec = (max - min);
            Vector3 cubeMin = Vector3.Zero;
            cubeMin.X = min.X + 0.0f * cubeVec.X; cubeMin.Y = min.Y + 0.2f * cubeVec.Y; cubeMin.Z = min.Z + 0.2f * cubeVec.Z;
            Vector3 cubeMax = Vector3.Zero;
            cubeMax.X = min.X + 1.0f * cubeVec.X; cubeMax.Y = min.Y + 0.8f * cubeVec.Y; cubeMax.Z = min.Z + 0.8f * cubeVec.Z;
            index = nVertices;
            vertices[index++] = new VertexPosition(new Vector3(cubeMin.X, cubeMax.Y, cubeMin.Z)); // 0
            vertices[index++] = new VertexPosition(new Vector3(cubeMax.X, cubeMax.Y, cubeMin.Z)); // 1
            vertices[index++] = new VertexPosition(new Vector3(cubeMin.X, cubeMin.Y, cubeMin.Z)); // 2
            vertices[index++] = new VertexPosition(new Vector3(cubeMax.X, cubeMin.Y, cubeMin.Z)); // 3
            vertices[index++] = new VertexPosition(new Vector3(cubeMin.X, cubeMax.Y, cubeMax.Z)); // 4
            vertices[index++] = new VertexPosition(new Vector3(cubeMax.X, cubeMax.Y, cubeMax.Z)); // 5
            vertices[index++] = new VertexPosition(new Vector3(cubeMin.X, cubeMin.Y, cubeMax.Z)); // 6
            vertices[index++] = new VertexPosition(new Vector3(cubeMax.X, cubeMin.Y, cubeMax.Z)); // 7

            index = dwIndices;
            indexOffset = nVertices;
            indices[index++] = 0 + indexOffset; indices[index++] = 1 + indexOffset; indices[index++] = 2 + indexOffset; // Front Face, Tri 1
            indices[index++] = 1 + indexOffset; indices[index++] = 3 + indexOffset; indices[index++] = 2 + indexOffset; // Front Face, Tri 2 
            indices[index++] = 5 + indexOffset; indices[index++] = 4 + indexOffset; indices[index++] = 7 + indexOffset; // Back Face, Tri 1 
            indices[index++] = 4 + indexOffset; indices[index++] = 6 + indexOffset; indices[index++] = 7 + indexOffset; // Back Face, Tri 2 
            indices[index++] = 4 + indexOffset; indices[index++] = 0 + indexOffset; indices[index++] = 6 + indexOffset; // Left Face, Tri 1 
            indices[index++] = 0 + indexOffset; indices[index++] = 2 + indexOffset; indices[index++] = 6 + indexOffset; // Left Face, Tri 2
            indices[index++] = 1 + indexOffset; indices[index++] = 5 + indexOffset; indices[index++] = 3 + indexOffset; // Right Face, Tri 1
            indices[index++] = 5 + indexOffset; indices[index++] = 7 + indexOffset; indices[index++] = 3 + indexOffset; // Right Face, Tri 2
            indices[index++] = 4 + indexOffset; indices[index++] = 5 + indexOffset; indices[index++] = 0 + indexOffset; // Top Face, Tri 1
            indices[index++] = 5 + indexOffset; indices[index++] = 1 + indexOffset; indices[index++] = 0 + indexOffset; // Top Face, Tri 2
            indices[index++] = 2 + indexOffset; indices[index++] = 3 + indexOffset; indices[index++] = 6 + indexOffset; // Bottom Face, Tri 1
            indices[index++] = 3 + indexOffset; indices[index++] = 7 + indexOffset; indices[index++] = 6 + indexOffset; // Bottom Face, Tri 2

            vb = new VertexBuffer(h_game.h_GraphicsDevice, typeof(VertexPositionTexture), nVertices + nVerticiesCube, BufferUsage.None);
            vb.SetData(vertices, 0, vertices.Length);
            ib = new IndexBuffer(h_game.h_GraphicsDevice, typeof(int), dwIndices + nIndiciesCube, BufferUsage.None);
            ib.SetData(indices, 0, indices.Length);

            effect = new BasicEffect(h_game.h_GraphicsDevice, null);
            effect.TextureEnabled = false;

            // Initialize the vertex declaration
            vdec = new VertexDeclaration(h_game.h_GraphicsDevice, VertexPosition.VertexElements);
            strideSize = VertexPosition.SizeInBytes;

            // Create the AABB and mark object as using AABB for collision detection
            base.boundingObjType = boundingObjType.AABB;
            BoundingBox bBox = new BoundingBox(min, max);
            base.boundingObj = (Object)bBox;
            base.boundingObjCenter = (bBox.Max + bBox.Min) / 2.0f;
            base.sweepAndPruneAABB = bBox;
            base.dirtyAABB = true;

            // Calculate the Itensor --> Not really required since floor doesn't move, but anyway
            base.state.Itensor = XNAUtils.CalculateItensorFromBoundingBox(bBox, base.state.mass);
            base.state.InvItensor = Matrix.Invert(base.state.Itensor);

            // Make sure both starting states are equal
            rboState.CopyAtoB(ref state, ref prevState);
        }
         */
        #endregion
        
        #region LoadContent() OLD V1
        /*
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void LoadContent()
        {
            Vector3 boxSize = max - min; Vector3 boxCenter = (max + min) / 2;
            if (boxSize.X < 0.0f || boxSize.Y < 0.0f || boxSize.Z < 0.0f)
                throw new Exception("gameObjectCloud::LoadContent() - The box size is negative! check inputs");

            // THESE ARE THE SIZES OF THE CIRCLES IN THE CLOUD --> THEY SCALE WITH BOX SIZE.
            // NOTE, THERE ARE LOTS OF "MAGIC NUMBERS" IN HERE.  I THINK IT'S OK... ;-)

            List<Vector3> centers = new List<Vector3>(numCircles);
            List<Vector3> radii = new List<Vector3>(numCircles);
            // The first 4 cover the corners --> Easier for player to jump there if there is a well defined corner

            float maxCircleSpacing_percentRadius = 0.3f;
            float radius = boxSize.Y / 2.0f;
            int numCirclesInRow = (int)Math.Floor((boxSize.X - radius * 2.0f) / maxCircleSpacing_percentRadius) + 1;
            float circleSpacing = (boxSize.X - radius * 2.0f) / (float)numCirclesInRow;

            if (numCirclesInRow < 2)
                throw new Exception("gameObjectCloud::LoadContent() - Cloud size must have [Width > 1.25 * Height]");

            for (int i = 0; i <= numCirclesInRow; i++)
            {
                centers.Add(new Vector3(min.X + circleSpacing * i + radius, min.Y + boxSize.Y / 2.0f, 0.0f));
                radii.Add(new Vector3(radius, radius, boxSize.Z / 6.0f));
            }

            // Now Add 4 random circles
            Random rand = new Random(1);
            for (int i = 0; i < 10; i++)
            {
                // Choose a random sphere center within a box that is 10% within the boarders of Min and Max
                centers.Add(new Vector3(((float)rand.NextDouble() * 0.8f + 0.1f) * boxSize.X + (min.X), // Parenteses varies 0.1-->0.9
                                        ((float)rand.NextDouble() * 0.8f + 0.1f) * boxSize.Y + (min.Y),
                                        0.0f));

                // Calculate the Radius so it just touches the outside of the box
                float curRad = GetDistanceToAABB(centers.Last(), min, max);
                radii.Add(new Vector3(curRad, curRad, curRad));
                if (curRad <= 0.0f)
                    throw new Exception("gameObjectCloud::LoadContent() - curRad <= 0!  Something went wrong");
            }

            numCircles = centers.Count;
            if (numCircles != radii.Count)
                throw new Exception("gameObjectCloud::LoadContent() - number of circles != number of radii");

            //calculates the resulting number of vertices, indices and faces
            nVertices = (nStacks + 1) * (nSlices + 1) * numCircles;
            int dwIndices = nSlices * nStacks * 6 * numCircles;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[nVertices];
            nFaces = dwIndices / 3;
            int[] indices = new int[dwIndices];

            float StackAngle = MathHelper.Pi / (float)nStacks;
            float SliceAngle = (float)(Math.PI * 2.0) / (float)nSlices;
            float phi, theta; int index = 0; int indexOffset; float x, y, z, sc;
            Vector3 Radius = new Vector3();
            Vector3 curCenter = new Vector3();

            for (int curCircle = 0; curCircle < numCircles; curCircle += 1)
            {
                // Choose a random sphere center within a box that is 10% within the boarders of Min and Max
                curCenter = centers[curCircle];

                // Calculate the Radius so it just touches the outside of the box
                Radius = radii[curCircle];

                index = (nVertices / numCircles) * curCircle;
                for (int stack = 0; stack <= nStacks; stack++)
                {
                    phi = MathHelper.PiOver2 - stack * SliceAngle;
                    y = Radius.Y * (float)Math.Sin(phi);
                    sc = -Radius.X * (float)Math.Cos(phi);

                    for (int slice = 0; slice <= nSlices; slice++)
                    {
                        theta = slice * SliceAngle;
                        x = (-Radius.X * (float)Math.Cos(phi)) * (float)Math.Sin(theta);
                        z = (-Radius.Z * (float)Math.Cos(phi)) * (float)Math.Cos(theta);
                        vertices[index] = new VertexPositionNormalTexture();
                        vertices[index].Position = new Vector3(x, y, z) + curCenter;
                        vertices[index].Normal = Vector3.Normalize(new Vector3(x, y, z));
                        vertices[index].TextureCoordinate = Vector2.Zero;
                        index += 1;
                    }
                }

                index = (dwIndices / numCircles) * curCircle;
                indexOffset = (nVertices / numCircles) * curCircle;
                int k = nSlices + 1;

                for (int stack = 0; stack < nStacks; stack++)
                {
                    for (int slice = 0; slice < nSlices; slice++)
                    {
                        indices[index++] = (stack + 0) * k + slice + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + indexOffset;
                        indices[index++] = (stack + 0) * k + slice + 1 + indexOffset;

                        indices[index++] = (stack + 0) * k + slice + 1 + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + 1 + indexOffset;
                    }
                }
            }

            vb = new VertexBuffer(h_game.h_GraphicsDevice, typeof(VertexPositionNormalTexture), nVertices, BufferUsage.None);
            vb.SetData(vertices, 0, vertices.Length);
            ib = new IndexBuffer(h_game.h_GraphicsDevice, typeof(int), dwIndices, BufferUsage.None);
            ib.SetData(indices, 0, indices.Length);

            effect = new BasicEffect(h_game.h_GraphicsDevice, null);
            base.textureEnabled = false;
            base.vertexColorEnabled = true;

            // Initialize the vertex declaration
            vdec = new VertexDeclaration(h_game.h_GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            strideSize = VertexPositionNormalTexture.SizeInBytes;

            // Create the AABB and mark object as using AABB for collision detection
            base.boundingObjType = boundingObjType.AABB;
            BoundingBox bBox = new BoundingBox(min, max);
            base.boundingObj = (Object)bBox;
            base.boundingObjCenter = (bBox.Max + bBox.Min) / 2.0f;
            base.sweepAndPruneAABB = bBox;
            base.dirtyAABB = true;

            // Calculate the Itensor --> Not really required since floor doesn't move, but anyway
            base.state.Itensor = XNAUtils.CalculateItensorFromBoundingBox(bBox, base.state.mass);
            base.state.InvItensor = Matrix.Invert(base.state.Itensor);

            // Make sure both starting states are equal
            rboState.CopyAtoB(ref state, ref prevState);
        }
        */
        #endregion

        #region LoadContent()
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void LoadContent()
        {
            Vector3 boxSize = max - min; Vector3 boxCenter = (max + min) / 2;
            if (boxSize.X < 0.0f || boxSize.Y < 0.0f || boxSize.Z < 0.0f)
                throw new Exception("gameObjectCloud::LoadContent() - The box size is negative! check inputs");

            // THESE ARE THE SIZES OF THE CIRCLES IN THE CLOUD --> THEY SCALE WITH BOX SIZE.
            // NOTE, THERE ARE LOTS OF "MAGIC NUMBERS" IN HERE.  I THINK IT'S OK... ;-)

            List<Vector3> centers = new List<Vector3>(numCircles);
            List<Vector3> radii = new List<Vector3>(numCircles);
            // The first 4 cover the corners --> Easier for player to jump there if there is a well defined corner

            float maxCircleSpacing_percentRadius = 0.3f;
            float radius = boxSize.Y / 2.0f;
            int numCirclesInRow = (int)Math.Floor((boxSize.X - radius * 2.0f) / maxCircleSpacing_percentRadius) + 1;
            float circleSpacing = (boxSize.X - radius * 2.0f) / (float)numCirclesInRow;

            if (numCirclesInRow < 2) // Align the circles vertically instead
            {
                radius = boxSize.X / 2.0f;
                numCirclesInRow = (int)Math.Floor((boxSize.Y - radius * 2.0f) / maxCircleSpacing_percentRadius) + 1;
                circleSpacing = (boxSize.Y - radius * 2.0f) / (float)numCirclesInRow;
                for (int i = 0; i <= numCirclesInRow; i++)
                {
                    centers.Add(new Vector3(min.X + boxSize.X / 2.0f, min.Y + circleSpacing * i + radius, 0.0f));
                    radii.Add(new Vector3(radius, radius, boxSize.Z / 6.0f));
                }
            }
            else
            {
                for (int i = 0; i <= numCirclesInRow; i++)
                {
                    centers.Add(new Vector3(min.X + circleSpacing * i + radius, min.Y + boxSize.Y / 2.0f, 0.0f));
                    radii.Add(new Vector3(radius, radius, boxSize.Z / 6.0f));
                }
            }

            // Now Add 4 random circles
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                // Choose a random sphere center within a box that is 10% within the boarders of Min and Max
                centers.Add(new Vector3(((float)rand.NextDouble() * 0.8f + 0.1f) * boxSize.X + (min.X), // Parenteses varies 0.1-->0.9
                                        ((float)rand.NextDouble() * 0.8f + 0.1f) * boxSize.Y + (min.Y),
                                        0.0f));

                // Calculate the Radius so it just touches the outside of the box
                float curRad = GetDistanceToAABB(centers.Last(), min, max);
                radii.Add(new Vector3(curRad, curRad, curRad));
                if (curRad <= 0.0f)
                    throw new Exception("gameObjectCloud::LoadContent() - curRad <= 0!  Something went wrong");
            }

            numCircles = centers.Count;
            if (numCircles != radii.Count)
                throw new Exception("gameObjectCloud::LoadContent() - number of circles != number of radii");

            //calculates the resulting number of vertices, indices and faces
            nVertices = (nStacks + 1) * (nSlices + 1) * numCircles;
            int dwIndices = nSlices * nStacks * 6 * numCircles;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[nVertices];
            nFaces = dwIndices / 3;
            int[] indices = new int[dwIndices];

            float StackAngle = MathHelper.Pi / (float)nStacks;
            float SliceAngle = (float)(Math.PI * 2.0) / (float)nSlices;
            float phi, theta; int index = 0; int indexOffset; float x, y, z, sc;
            Vector3 Radius = new Vector3();
            Vector3 curCenter = new Vector3();

            for (int curCircle = 0; curCircle < numCircles; curCircle += 1)
            {
                // Choose a random sphere center within a box that is 10% within the boarders of Min and Max
                curCenter = centers[curCircle];

                // Calculate the Radius so it just touches the outside of the box
                Radius = radii[curCircle];

                index = (nVertices / numCircles) * curCircle;
                for (int stack = 0; stack <= nStacks; stack++)
                {
                    phi = MathHelper.PiOver2 - stack * SliceAngle;
                    y = Radius.Y * (float)Math.Sin(phi);
                    sc = -Radius.X * (float)Math.Cos(phi);

                    for (int slice = 0; slice <= nSlices; slice++)
                    {
                        theta = slice * SliceAngle;
                        x = (-Radius.X * (float)Math.Cos(phi)) * (float)Math.Sin(theta);
                        z = (-Radius.Z * (float)Math.Cos(phi)) * (float)Math.Cos(theta);
                        vertices[index] = new VertexPositionNormalTexture();
                        vertices[index].Position = new Vector3(x, y, z) + curCenter;
                        vertices[index].Normal = Vector3.Normalize(new Vector3(x, y, z));
                        vertices[index].TextureCoordinate = Vector2.Zero;
                        index += 1;
                    }
                }

                index = (dwIndices / numCircles) * curCircle;
                indexOffset = (nVertices / numCircles) * curCircle;
                int k = nSlices + 1;

                for (int stack = 0; stack < nStacks; stack++)
                {
                    for (int slice = 0; slice < nSlices; slice++)
                    {
                        indices[index++] = (stack + 0) * k + slice + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + indexOffset;
                        indices[index++] = (stack + 0) * k + slice + 1 + indexOffset;

                        indices[index++] = (stack + 0) * k + slice + 1 + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + indexOffset;
                        indices[index++] = (stack + 1) * k + slice + 1 + indexOffset;
                    }
                }
            }

            vb = new VertexBuffer(h_game.h_GraphicsDevice, typeof(VertexPositionNormalTexture), nVertices, BufferUsage.None);
            vb.SetData(vertices, 0, vertices.Length);
            ib = new IndexBuffer(h_game.h_GraphicsDevice, typeof(int), dwIndices, BufferUsage.None);
            ib.SetData(indices, 0, indices.Length);

            effect = new BasicEffect(h_game.h_GraphicsDevice, null);
            base.textureEnabled = false;
            base.vertexColorEnabled = true;

            // Initialize the vertex declaration
            vdec = new VertexDeclaration(h_game.h_GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            strideSize = VertexPositionNormalTexture.SizeInBytes;

            // Create the AABB and mark object as using AABB for collision detection
            base.boundingObjType = boundingObjType.AABB;
            BoundingBox bBox = new BoundingBox(min, max);
            base.boundingObj = (Object)bBox;
            base.boundingObjCenter = (bBox.Max + bBox.Min) / 2.0f;
            base.sweepAndPruneAABB = bBox;
            base.dirtyAABB = true;

            // Calculate the Itensor --> Not really required since floor doesn't move, but anyway
            base.state.Itensor = XNAUtils.CalculateItensorFromBoundingBox(bBox, base.state.mass);
            base.state.InvItensor = Matrix.Invert(base.state.Itensor);

            // Make sure both starting states are equal
            rboState.CopyAtoB(ref state, ref prevState);
        }
        #endregion

        #region GetDistanceToAABB
        static float GetDistanceToAABB(Vector3 p, Vector3 min, Vector3 max)
        {
            if (p.X <= min.X || p.X >= max.X || // If the point is OUTSIDE the AABB
                p.Y <= min.Y || p.Y >= max.Y ||
                p.Z <= min.Z || p.Z >= max.Z)
            {
                Vector3 p_projected = collisionUtils.ClosestPointOnAABB(p, min, max);
                Vector3 vec = p_projected - p;
                return -1.0f * (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
            }
            else
            {
                Vector3 p_projected = Vector3.Zero;
                if ((p.X - min.X) < (max.X - p.X))
                    p_projected.X = min.X; // Point is closer to the minimum point
                else
                    p_projected.X = max.X; // Point is closer to the maximum point

                if ((p.Y - min.Y) < (max.Y - p.Y))
                    p_projected.Y = min.Y; // Point is closer to the minimum point
                else
                    p_projected.Y = max.Y; // Point is closer to the maximum point

                if ((p.Z - min.Z) < (max.Z - p.Z))
                    p_projected.Z = min.Z; // Point is closer to the minimum point
                else
                    p_projected.Z = max.Z; // Point is closer to the maximum point

                // return the smallest dimension
                Vector3 vec = p_projected - p;
                if (Math.Abs(vec.X) < Math.Abs(vec.Y) && Math.Abs(vec.X) < Math.Abs(vec.Z))
                    return Math.Abs(vec.X); // X is the smallest
                if (Math.Abs(vec.Y) < Math.Abs(vec.X) && Math.Abs(vec.Y) < Math.Abs(vec.Z))
                    return Math.Abs(vec.Y); // Y is the smallest
                if (Math.Abs(vec.Z) < Math.Abs(vec.X) && Math.Abs(vec.Z) < Math.Abs(vec.Y))
                    return Math.Abs(vec.Z); // Z is the smallest
                else
                    throw new Exception("gameObjectCloud::GetDistanceToAABB() - Couldn't find smallest dimension");
            }
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void DrawUsingCurrentEffect(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
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
            drawState.orient = Quaternion.Slerp(prevState.orient, state.orient, percentInterp);
            drawState.pos = Interp(prevState.pos, state.pos, percentInterp);
            Matrix localWorld = CreateScale(drawState.scale) * Matrix.CreateFromQuaternion(drawState.orient) * Matrix.CreateTranslation(drawState.pos);
          
            // Specify which effect technique to use.
            effect.CurrentTechnique = effect.Techniques[effectTechniqueName];
            effect.Parameters["World"].SetValue(localWorld);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            device.Indices = ib;
            device.VertexDeclaration = vdec;
            device.Vertices[0].SetSource(vb, 0, strideSize);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nVertices, 0, nFaces);

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
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();
            Effect oldEffect = effect;
            Effect newEffect = null;

            // If we haven't already seen this effect...
            if (!effectMapping.ContainsKey(oldEffect))
            {
                // Make a clone of our replacement effect. We can't just use
                // it directly, because the same effect might need to be
                // applied several times to different parts of the model using
                // a different texture each time, so we need a fresh copy each
                // time we want to set a different texture into it.
                newEffect = replacementEffect.Clone(
                                            replacementEffect.GraphicsDevice);

                // Copy across the texture from the original effect.
                newEffect.Parameters["TextureEnabled"].SetValue(textureEnabled);

                newEffect.Parameters["DiffuseColor"].SetValue(new Vector3(1.0f,1.0f,1.0f));
                newEffect.Parameters["VertexColorEnabled"].SetValue(vertexColorEnabled);

                effectMapping.Add(oldEffect, newEffect);
            }
            effect = newEffect;
        }
        #endregion
    }


}
