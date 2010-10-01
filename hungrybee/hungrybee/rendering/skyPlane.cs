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
    /// **                            skyPlane                               **
    /// ** Like a skyBox class, but with only the front side.                **
    /// ***********************************************************************
    /// </summary>
    public class skyPlane : GameComponent
    {
        #region Local Variables
        /// Local Variables
        /// ***********************************************************************    
        private game h_game;
        VertexBuffer skyPlaneVertexBuffer;
        Texture2D skyPlaneTexture;
        BasicEffect skyPlaneEffect;
        VertexDeclaration vertexDec;
        Matrix skyViewMat;
        float skyPlaneScale;
        #endregion

        #region Constructor - skyPlane(game game)
        /// Constructor - Just register the cameraInterface services
        /// ***********************************************************************
        public skyPlane(game game) : base(game)  
        {
            h_game = (game)game;
            skyViewMat = Matrix.Identity;
            skyPlaneScale = 1.0f;
        }
        #endregion

        #region Initialize()
        /// Initialize - Nothing to Initialize --> All done in LoadContent()
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region Update()
        /// Update - Nothing to update
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion

        #region LoadContent
        /// LoadContent - Load in the textures and effects file
        /// ***********************************************************************
        public void LoadContent()
        {
            skyPlaneTexture = h_game.Content.Load<Texture2D>(h_game.h_GameSettings.skyPlaneTextureFile);
            skyPlaneScale = h_game.h_GameSettings.skyPlaneScale;
            skyPlaneEffect = new BasicEffect(h_game.h_GraphicsDevice, null);
            CreateSkyPlaneVertexBuffer();
            vertexDec = new VertexDeclaration(h_game.h_GraphicsDevice, VertexPositionTexture.VertexElements);
        }
        #endregion

        #region Draw()
        /// Draw - MUST BE DRAWN FIRST.  
        /// ***********************************************************************
        public void Draw(GraphicsDevice device, Matrix view, Matrix projection)
        {
            h_game.h_GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            h_game.h_GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            h_game.h_GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            skyPlaneEffect.World = Matrix.Identity;
            skyViewMat = view;
            skyViewMat.M41 = 0; skyViewMat.M42 = 0; skyViewMat.M43 = -1; // Place skyPlane a constant offset from camera
            skyPlaneEffect.View = skyViewMat;
            skyPlaneEffect.Projection = projection;
            skyPlaneEffect.Begin();
            skyPlaneEffect.Texture = skyPlaneTexture;
            skyPlaneEffect.TextureEnabled = true;
            foreach (EffectPass pass in skyPlaneEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                h_game.h_GraphicsDevice.VertexDeclaration = vertexDec;
                h_game.h_GraphicsDevice.Vertices[0].SetSource(skyPlaneVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                h_game.h_GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

                pass.End();
            }
            skyPlaneEffect.End();

            h_game.h_GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            h_game.h_GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            h_game.h_GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }
        #endregion

        #region CreateSkyPlaneVertexBuffer()
        /// CreateSkyPlaneVertexBuffer - Hard code the verticies
        /// ***********************************************************************
        private void CreateSkyPlaneVertexBuffer()
        {
            Vector3 Zoffset = new Vector3(0, 0, -0.5f);
            Vector3 DL = new Vector3(-0.5f, -0.5f, 0) * skyPlaneScale + Zoffset;      // Down Left
            Vector3 UR = new Vector3(0.5f, 0.5f, 0) * skyPlaneScale + Zoffset;        // Upper Right
            Vector3 UL = new Vector3(-0.5f, 0.5f, 0) * skyPlaneScale + Zoffset;       // Upper Left
            Vector3 DR = new Vector3(0.5f, -0.5f, 0) * skyPlaneScale + Zoffset;       // Down Right

            Vector2 texDR = new Vector2(1, 1);        // Texture Down Right
            Vector2 texUL = new Vector2(0, 0);        // Texture Upper Left
            Vector2 texDL = new Vector2(0, 1);        // Texture Down Left
            Vector2 texUR = new Vector2(1, 0);        // Texture Upper Right

            VertexPositionTexture[] vertices = new VertexPositionTexture[6];
            int i = 0;
            // Triangle 1
            vertices[i++] = new VertexPositionTexture(DL, texDL);
            vertices[i++] = new VertexPositionTexture(UL, texUL);
            vertices[i++] = new VertexPositionTexture(UR, texUR);
            // Triangle 2
            vertices[i++] = new VertexPositionTexture(DL, texDL);
            vertices[i++] = new VertexPositionTexture(UR, texUR);
            vertices[i++] = new VertexPositionTexture(DR, texDR);

            skyPlaneVertexBuffer = new VertexBuffer(h_game.h_GraphicsDevice, vertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);
            skyPlaneVertexBuffer.SetData<VertexPositionTexture>(vertices);
        }
        #endregion
    }
}
