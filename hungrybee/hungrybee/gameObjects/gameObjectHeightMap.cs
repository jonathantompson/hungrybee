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
    /// **                      gameObjectHeightMap                          **
    /// ** This is a class to store the data for the heightmap and render it **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = FALSE                                                   **
    /// ***********************************************************************
    /// </summary>
    class gameObjectHeightMap : gameObject
    {
        #region Local Variables
        /// Local Variables
        /// ***********************************************************************    

        Texture2D heightMapTexture;
        string heightMapFile;
        string heightMapTextureFile;
        float[,] heightMapData;
        VertexDeclaration heightMapVertexDeclaration;
        VertexBuffer heightMapVertexBuffer;
        IndexBuffer heightMapIndexBuffer;

        Vector3 minDim;
        Vector3 maxDim;

        bool heightMapFromFile;

        BasicEffect heightMapEffect;
        #endregion

        #region Constructor - gameObjectHeightMap(game game, string file, string textureFile, Vector3 min, Vector3 max) : base(game, null)
        public gameObjectHeightMap(game game, bool fromFile, string file, string textureFile, Vector3 min, Vector3 max)
            : base(game, null, boundingObjType.AABB)
        {
            heightMapFromFile = fromFile;
            heightMapFile = file;
            heightMapTextureFile = textureFile;
            minDim = min;
            maxDim = max;
            base.movable = false;
            base.collidable = true;
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void LoadContent()
        {
            if (heightMapFromFile)
            {
                // Load in the texture data from file and process the raw data
                Texture2D heightMap = base.h_game.Content.Load<Texture2D>(heightMapFile);
                // Parse the heightMap data, creating the verticies
                heightMapData = LoadHeightDataFromFile(heightMap);
            }
            else
            {
                // Create heightMap data from some function
                heightMapData = LoadHeightDataFromFunction();
            }

            // Create a basic effect
            heightMapEffect = new BasicEffect(base.h_game.GetGraphicsDevice(), null);

            // Parse the heightMap data, creating the indicies and Normals
            heightMapVertexDeclaration = new VertexDeclaration(base.h_game.GetGraphicsDevice(), VertexPositionNormalTexture.VertexElements);
            VertexPositionNormalTexture[] terrainVertices = CreateTerrainVertices();
            int[] terrainIndices = CreateTerrainIndices();
            terrainVertices = GenerateNormalsForTriangleStrip(terrainVertices, terrainIndices);
                        
            // Load the texture file
            heightMapTexture = base.h_game.Content.Load<Texture2D>("grass");

            // Initialize the vertex and index buffers
            CreateBuffers(terrainVertices, terrainIndices);

            // Create the AABB and mark object as using AABB for collision detection
            base.boundingObjType = boundingObjType.AABB;
            BoundingBox bBox = XNAUtils.CreateAABBFromVerticies(terrainVertices);
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

        #region DrawUsingCurrentEffect()
        /// LoadContent - Load in the textures and initialize the heightmap
        /// ***********************************************************************
        public override void DrawUsingCurrentEffect(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // THE HEIGHT MAP DOESN'T MOVE --> SO DON'T BOTHER INTERPOLATING BETWEEN FRAMES

            //draw terrain
            int width = heightMapData.GetLength(0);
            int height = heightMapData.GetLength(1);
            heightMapEffect.World = Matrix.Identity;
            heightMapEffect.View = ((camera)base.h_game.GetCamera()).ViewMatrix;
            heightMapEffect.Projection = ((camera)base.h_game.GetCamera()).ProjectionMatrix;
            heightMapEffect.Texture = heightMapTexture;
            heightMapEffect.TextureEnabled = true;

            heightMapEffect.EnableDefaultLighting();
            heightMapEffect.DirectionalLight0.Direction = new Vector3(1, -1, 1);
            heightMapEffect.DirectionalLight0.Enabled = true;
            heightMapEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            heightMapEffect.DirectionalLight1.Enabled = false;
            heightMapEffect.DirectionalLight2.Enabled = false;
            heightMapEffect.SpecularColor = new Vector3(0, 0, 0);

            heightMapEffect.Begin();
            foreach (EffectPass pass in heightMapEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(heightMapVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                device.Indices = heightMapIndexBuffer;
                device.VertexDeclaration = heightMapVertexDeclaration;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, width * height, 0, width * 2 * (height - 1) - 2);

                pass.End();
            }
            heightMapEffect.End();
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

        #region LoadHeightDataFromFile()
        /// LoadHeightDataFromFile - Load in the textures and initialize the heightmap from a texture file
        /// ***********************************************************************
        private float[,] LoadHeightDataFromFile(Texture2D heightMap)
        {
            float minimumHeight = 255;
            float maximumHeight = 0;

            int width = heightMap.Width;
            int height = heightMap.Height;

            Color[] heightMapColors = new Color[width * height];
            heightMap.GetData<Color>(heightMapColors);

            float[,] heightData = new float[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * width].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            float y_start = minDim.Y;
            float y_spacing = (maxDim.Y - minDim.Y);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    heightData[x, y] = ((heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight)) *y_spacing + y_start;

            return heightData;
        }
        #endregion

        #region LoadHeightDataFromFunction()
        /// LoadHeightDataFromFunction - Load in the textures and initialize the heightmap from a predefined function
        /// ***********************************************************************
        private float[,] LoadHeightDataFromFunction()
        {

            Texture2D heightMap = base.h_game.Content.Load<Texture2D>(heightMapFile);

            int width = 128;
            int height = 128;

            float[,] heightData = new float[width, height];

            float y_start = minDim.Y;
            float y_spacing = (maxDim.Y - minDim.Y);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float funcVal = (float)Math.Sin(((double)y / (double)width) * Math.PI);
                    heightData[x, y] = funcVal * y_spacing + y_start;
                }
            }

            return heightData;
        }
        #endregion

        #region CreateTerrainVertices()
        /// CreateTerrainVertices - Create the verticies from the heightmap data
        /// ***********************************************************************
        private VertexPositionNormalTexture[] CreateTerrainVertices()
        {
            int dataWidth = heightMapData.GetLength(0);
            int dataHeight = heightMapData.GetLength(1);
            VertexPositionNormalTexture[] terrainVertices = new VertexPositionNormalTexture[dataWidth * dataHeight];

            int i = 0;
            float z_start = maxDim.Z;
            float x_start = minDim.X;
            float z_spacing = (minDim.Z - maxDim.Z) / dataHeight;
            float x_spacing = (maxDim.X - minDim.X) / dataHeight;
            for (int z = 0; z < dataHeight; z++)
            {
                for (int x = 0; x < dataWidth; x++)
                {
                    Vector3 position = new Vector3(x_start + x_spacing * x, heightMapData[x, z], z_start + z_spacing * z);
                    Vector3 normal = new Vector3(0, 0, 1); // To be found later
                    Vector2 texCoord = new Vector2((float)x / 30.0f, (float)z / 30.0f);

                    terrainVertices[i++] = new VertexPositionNormalTexture(position, normal, texCoord);
                }
            }

            return terrainVertices;
        }
        #endregion

        #region CreateTerrainIndices()
        /// CreateTerrainIndices - Create the indicies from the heightmap data
        /// ***********************************************************************
        private int[] CreateTerrainIndices()
        {
            int width = heightMapData.GetLength(0);
            int height = heightMapData.GetLength(1);

            int[] terrainIndices = new int[(width) * 2 * (height - 1)];

            int i = 0;
            int z = 0;
            while (z < height - 1)
            {
                for (int x = 0; x < width; x++)
                {
                    terrainIndices[i++] = x + z * width;
                    terrainIndices[i++] = x + (z + 1) * width;
                }
                z++;

                if (z < height - 1)
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        terrainIndices[i++] = x + (z + 1) * width;
                        terrainIndices[i++] = x + z * width;
                    }
                }
                z++;
            }

            return terrainIndices;
        }
        #endregion

        #region GenerateNormalsForTriangleStrip()
        /// GenerateNormalsForTriangleStrip - Create the Normals from the heightmap data
        /// ***********************************************************************
        private static VertexPositionNormalTexture[] GenerateNormalsForTriangleStrip(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            bool swappedWinding = false;
            for (int i = 2; i < indices.Length; i++)
            {
                Vector3 firstVec = vertices[indices[i - 1]].Position - vertices[indices[i]].Position;
                Vector3 secondVec = vertices[indices[i - 2]].Position - vertices[indices[i]].Position;
                Vector3 normal = Vector3.Cross(firstVec, secondVec);
                normal.Normalize();

                if (swappedWinding)
                    normal *= -1;

                if (!float.IsNaN(normal.X))
                {
                    vertices[indices[i]].Normal += normal;
                    vertices[indices[i - 1]].Normal += normal;
                    vertices[indices[i - 2]].Normal += normal;
                }

                swappedWinding = !swappedWinding;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }
        #endregion

        #region CreateBuffers()
        /// CreateBuffers - Initialize the heightmap buffers
        /// ***********************************************************************
        private void CreateBuffers(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            heightMapVertexBuffer = new VertexBuffer(base.h_game.GetGraphicsDevice(), VertexPositionNormalTexture.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            heightMapVertexBuffer.SetData(vertices);

            heightMapIndexBuffer = new IndexBuffer(base.h_game.GetGraphicsDevice(), typeof(int), indices.Length, BufferUsage.WriteOnly);
            heightMapIndexBuffer.SetData(indices);
        }
        #endregion
    }
}
