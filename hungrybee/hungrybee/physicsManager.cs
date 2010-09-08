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
    /// **                          physicsManager                           **
    /// ** This is a singleton class to preform RK4 integration & bounding   **
    /// ** sphere collision detection                                        **
    /// ***********************************************************************
    /// </summary>
    public class physicsManager : GameComponent
    {
        #region Local Variables

        // Local variables
        private game h_game;
        rboDerivative D1, D2, D3, D4;

        #endregion

        #region Constructor - renderManager(game game)
        /// Initializes to default values
        /// ***********************************************************************
        public physicsManager(game game)
            : base(game)
        {
            h_game = (game)game;
            D1 = new rboDerivative();
            D2 = new rboDerivative();
            D3 = new rboDerivative();
            D4 = new rboDerivative();
        }
        #endregion

        #region Initialize()
        /// Perform initialization - Nothing to initialize
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region Update()
        /// Update()
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Take RK4 Step for each object in h_game.gameObjectManager
            TakeRK4Step(gameTime, h_game.GetGameObjectManager().h_GameObjects);

            // Coarse Collision detection

            // Fine Collision detection

            // Resolve Collisions

            base.Update(gameTime);
        }
        #endregion

        #region TakeStep()
        /// TakeStep() - Perform RK4 step on each gameObject
        /// ***********************************************************************
        public void TakeRK4Step(GameTime gameTime, List<gameObject> gameObjects)
        {
            gameObject curObject = null;
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // enumerate through each element in the list and update the quaternion values
            List<gameObject>.Enumerator ListEnum = gameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                curObject = ListEnum.Current;

                // Copy: prevState = state
                rboState.CopyStateQuantitiesAtoB(curObject.state, curObject.prevState);

                if (curObject.movable)
                {
                    // Calculate piecewise derivatives
                    D1.Evaluate(curObject.state, time);
                    D2.Evaluate(curObject.state, time, 0.5f * deltaTime, D1);
                    D3.Evaluate(curObject.state, time, 0.5f * deltaTime, D2);
                    D4.Evaluate(curObject.state, time, 1.0f * deltaTime, D3);

                    // Calculate rbo State quantities from forth order interpolation of piecewise derivatives
                    curObject.state.pos         = curObject.prevState.pos + 
                                                  (1.0f / 6.0f) * deltaTime * (D1.linearVel + 2.0f * (D2.linearVel + D3.linearVel) + D4.linearVel);
                    curObject.state.linearMom   = curObject.prevState.linearMom + 
                                                  (1.0f / 6.0f) * deltaTime * (D1.force + 2.0f * (D2.force + D3.force) + D4.force);
                    curObject.state.orient      = curObject.prevState.orient + 
                                                  Quaternion.Multiply(D1.spin + Quaternion.Multiply((D2.spin + D3.spin), 2.0f) + D4.spin, (1.0f / 6.0f) * deltaTime);
                    curObject.state.angularMom  = curObject.prevState.angularMom + 
                                                  (1.0f / 6.0f) * deltaTime * (D1.torque + 2.0f * (D2.torque + D3.torque) + D4.torque);

                    // Calculate rbo Derived quantities
                    curObject.state.RecalculateDerivedQuantities();

                    // Calculate the object's transform from state variables
                    curObject.model.Root.Transform = curObject.CreateScale(curObject.state.scale) *
                                                     Matrix.CreateFromQuaternion(curObject.state.orient) *
                                                     Matrix.CreateTranslation(curObject.state.pos);
                }
            }
            base.Update(gameTime);

            
        }
        #endregion


    }

}
