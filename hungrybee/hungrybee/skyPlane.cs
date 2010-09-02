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

namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                            skyPlane                               **
    /// ** Like a skyBox class, but with only the front side.                **
    /// ***********************************************************************
    /// </summary>
    class skyPlane : DrawableGameComponent
    {
        /// <summary>
        /// Local Variables
        /// ***********************************************************************
        /// </summary>      
        private game    h_game;
        VertexBuffer    skyPlaneVertexBuffer;
        TextureCube     skyPlaneTexture;
        Effect          skyPlaneEffect;

        /// <summary>
        /// Constructor - Just register the cameraInterface services
        /// ***********************************************************************
        /// </summary>
        public skyPlane(game game) : base(game)  
        {
            h_game = (game)game;
        }

        /// <summary>
        /// Initialize - Nothing to Initialize --> All done in LoadContent()
        /// ***********************************************************************
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Update - Nothing to update
        /// ***********************************************************************
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// LoadContent - Load in the textures and effects file
        /// ***********************************************************************
        /// </summary>
        protected override void LoadContent()
        {
            skyPlaneTexture = h_game.Content.Load<TextureCube>(h_game.GetGameSettings().skyPlaneTextureFile);
            skyPlaneEffect = h_game.Content.Load<Effect>(h_game.GetGameSettings().skyPlaneEffectsFile);
            CreateSkyPlaneVertexBuffer();
            base.LoadContent();
        }

        /// <summary>
        /// Draw - MUST BE DRAWN FIRST.  
        /// ***********************************************************************
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            h_game.GetGraphicsDevice().RenderState.DepthBufferWriteEnable = false;
            skyPlaneEffect.CurrentTechnique = skyPlaneEffect.Techniques["SkyPlane"];
            skyPlaneEffect.Parameters["xWorld"].SetValue(Matrix.CreateTranslation(((camera)h_game.GetCamera()).Position));
            skyPlaneEffect.Parameters["xView"].SetValue(((camera)h_game.GetCamera()).ViewMatrix);
            skyPlaneEffect.Parameters["xProjection"].SetValue(((camera)h_game.GetCamera()).ProjectionMatrix);
            skyPlaneEffect.Parameters["xTexture"].SetValue(skyPlaneTexture);
            skyPlaneEffect.Begin();
            foreach (EffectPass pass in skyPlaneEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                h_game.GetGraphicsDevice().VertexDeclaration = new VertexDeclaration(h_game.GetGraphicsDevice(), VertexPosition.VertexElements);
                h_game.GetGraphicsDevice().Vertices[0].SetSource(skyPlaneVertexBuffer, 0, VertexPosition.SizeInBytes);
                h_game.GetGraphicsDevice().DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

                pass.End();
            }
            skyPlaneEffect.End();
            h_game.GetGraphicsDevice().RenderState.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// CreateSkyPlaneVertexBuffer - Hard code the verticies
        /// ***********************************************************************
        /// </summary>
        private void CreateSkyPlaneVertexBuffer()
        {
            Vector3 forwardBottomLeft = new Vector3(-1, -1, -1);
            Vector3 forwardBottomRight = new Vector3(1, -1, -1);
            Vector3 forwardUpperLeft = new Vector3(-1, 1, -1);
            Vector3 forwardUpperRight = new Vector3(1, 1, -1);

            Vector3 backBottomLeft = new Vector3(-1, -1, 1);
            Vector3 backBottomRight = new Vector3(1, -1, 1);
            Vector3 backUpperLeft = new Vector3(-1, 1, 1);
            Vector3 backUpperRight = new Vector3(1, 1, 1);

            VertexPosition[] vertices = new VertexPosition[6];
            int i = 0;

            //face in front of the camera
            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(forwardUpperRight);

            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperRight);
            vertices[i++] = new VertexPosition(forwardBottomRight);

            skyPlaneVertexBuffer = new VertexBuffer(h_game.GetGraphicsDevice(), vertices.Length * VertexPosition.SizeInBytes, BufferUsage.WriteOnly);
            skyPlaneVertexBuffer.SetData<VertexPosition>(vertices);
        }
    }
}
