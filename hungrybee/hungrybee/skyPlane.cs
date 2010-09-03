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
        private game    h_game;
        VertexBuffer    skyPlaneVertexBuffer;
        TextureCube     skyPlaneTexture;
        Effect          skyPlaneEffect;
        #endregion

        #region Constructor - skyPlane(game game)
        /// Constructor - Just register the cameraInterface services
        /// ***********************************************************************
        public skyPlane(game game) : base(game)  
        {
            h_game = (game)game;
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
            skyPlaneTexture = h_game.Content.Load<TextureCube>(h_game.GetGameSettings().skyPlaneTextureFile);
            skyPlaneEffect = h_game.Content.Load<Effect>(h_game.GetGameSettings().skyPlaneEffectsFile);
            CreateSkyPlaneVertexBuffer();
        }
        #endregion

        #region Draw()
        /// Draw - MUST BE DRAWN FIRST.  
        /// ***********************************************************************
        public void Draw(GameTime gameTime)
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
        #endregion

        #region CreateSkyPlaneVertexBuffer()
        /// CreateSkyPlaneVertexBuffer - Hard code the verticies
        /// ***********************************************************************
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
        #endregion
    }
}
