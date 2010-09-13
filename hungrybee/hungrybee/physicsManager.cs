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
    #region enum types
    public enum boundingObjType { UNDEFINED, SPHERE, AABB };
    #endregion

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
        private static bool pauseGame = false;

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
            if (!pauseGame) 
                TakeRK4Step(gameTime, h_game.GetGameObjectManager().h_GameObjects);

            // Coarse Collision detection

            // Fine Collision detection
            // This might be useful: http://nehe.gamedev.net/data/lessons/lesson.asp?lesson=30
            // Also useful: http://www.realtimerendering.com/intersections.html

            // *******************************
            // ********** TEST CODE **********
            // *******************************
            float Tcollision = 0.0f;
            Vector3 point = Vector3.Zero;
            bool retVal = collisionUtils.testCollision(h_game.GetGameObjectManager().h_GameObjects[0], 
                                                       h_game.GetGameObjectManager().h_GameObjects[1],
                                                       ref Tcollision,
                                                       ref point);
            if (!pauseGame && retVal) // if we haven't yet paused and there is a collision
            {
                pauseGame = true;
                h_game.GetGameObjectManager().SpawnCollidables();
            }

            // *******************************
            // ******** END TEST CODE ********
            // *******************************


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
                rboState.CopyDynamicQuantitiesAtoB(ref curObject.state, ref curObject.prevState);
                curObject.prevState.time = time;

                if (curObject.movable)
                {
                    // Calculate piecewise derivatives
                    float halfDeltaTime = 0.5f * deltaTime;
                    D1.Evaluate(curObject.state, time, curObject);
                    D2.Evaluate(curObject.state, curObject.prevState, time + halfDeltaTime, halfDeltaTime, D1, curObject);
                    D3.Evaluate(curObject.state, curObject.prevState, time + halfDeltaTime, halfDeltaTime, D2, curObject);
                    D4.Evaluate(curObject.state, curObject.prevState, time + deltaTime, deltaTime, D3, curObject);

                    // Calculate rbo State quantities from forth order interpolation of piecewise derivatives
                    float sixthDeltaTime = deltaTime / 6.0f;
                    curObject.state.pos         = curObject.prevState.pos +
                                                  sixthDeltaTime * (D1.linearVel + 2.0f * (D2.linearVel + D3.linearVel) + D4.linearVel);
                    curObject.state.linearMom   = curObject.prevState.linearMom +
                                                  sixthDeltaTime * (D1.force + 2.0f * (D2.force + D3.force) + D4.force);
                    curObject.state.orient      = curObject.prevState.orient +
                                                  Quaternion.Multiply(D1.spin + Quaternion.Multiply((D2.spin + D3.spin), 2.0f) + D4.spin, sixthDeltaTime);
                    curObject.state.angularMom  = curObject.prevState.angularMom +
                                                  sixthDeltaTime * (D1.torque + 2.0f * (D2.torque + D3.torque) + D4.torque);

                    // Calculate rbo Derived quantities
                    curObject.state.RecalculateDerivedQuantities();
                    curObject.state.time = time + deltaTime;
                    // physicsManager.ClipVelocity(ref curObject.state.linearVel, h_game.GetGameSettings().physicsMinVel, curObject.maxVel);
                }
            }
            base.Update(gameTime);

            
        }
        #endregion


        #region ClipVelocity()
        /// ClipVelocity() - Clip the velocity to 0 or maxVel
        /// ***********************************************************************
        public static void ClipVelocity(ref Vector3 velocity, float minVel, float maxVel)
        {
            // Clip velocity at max and nin value
            float magVel = (float)Math.Sqrt(Vector3.Dot(velocity, velocity));
            if (magVel > maxVel)
                velocity = Vector3.Normalize(velocity) * maxVel;
            else if (magVel < minVel)
                velocity = Vector3.Zero;
            // else velocity is ok, so do nothing
        }
        #endregion
    }

}
