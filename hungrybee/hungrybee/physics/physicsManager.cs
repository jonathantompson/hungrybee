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
        public int numObjects; // Not known until gameObjectManager.LoadLevel() is complete
        public int numCollidableObjects;
        rboDerivative D1, D2, D3, D4;

        // Coarse collision detection
        private List<AABBOverlap> AABBOverlapStatus;
        private LinkedList<int> AABBActiveList; // Linked list of active values
        private List<int> AABBXaxis;   // Sorted indices of gameObjects --> xaxis values
        private List<int> AABBYaxis;
        private List<int> AABBZaxis;

        // Fine collision detection
        protected List<collision> collisions;
        protected float           time;
        protected float           Tstep_remaining;
        protected bool            intersection;

        // Resting contacts
        protected List<collision> restingContacts;
        protected static Vector3  lImpulse = new Vector3();

        // Phantom contacts
        protected List<collision> phantomContacts;

        protected static float EPSILON = 0.00000001f;
        public static float BISECTION_TOLLERANCE = 0.0005f;
        public static float RESTING_CONTACT_TOLLERANCE = 0.001f; // typically this is >= 2 * BISECTION_TOLLERANCE
        protected static int BISECTION_MAXITERATIONS = 1000;
        protected static int MAX_PHYSICS_ITERATIONS = 10;
        protected static bool pauseGame = false;
        protected static bool pauseGameDebounce = false;

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
            numCollidableObjects = 0;
            numObjects = 0;
            intersection = false;
        }
        #endregion

        #region Initialize()
        /// Nothing to initialize - bulk of startup code is performed once gameObjects are loaded
        /// ***********************************************************************
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion

        #region LoadContent()
        /// LoadContent() -  Just initilaize physics structures that require knowledge of how many game elements exist.
        /// ***********************************************************************
        public void LoadContent()
        {
            numCollidableObjects = h_game.h_GameObjectManager.GetNumberCollidableObjects();
            numObjects = h_game.h_GameObjectManager.h_GameObjects.Count;
            initCoarseCollisionDetection();
            initFineCollisionDetection();
        }
        #endregion

        #region Update() --> THIS IS WHERE MAIN PHYSICS STEP IS TAKEN
        /// Update()
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // Pause the game if the user presses p
            if (Keyboard.GetState().IsKeyDown(Keys.P) && pauseGameDebounce == false)
            { pauseGame = !pauseGame; pauseGameDebounce = true; }
            if (Keyboard.GetState().IsKeyUp(Keys.P))
            { pauseGameDebounce = false; }

            time = (float)gameTime.TotalGameTime.TotalSeconds;
            Tstep_remaining = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float estColTime = float.PositiveInfinity;
            float Tstep_to_collision = 0.0f;

            int curIteration = 0;
            while (Tstep_remaining > 0.0f && !pauseGame) // While there is still time to process
            {
                if (curIteration >= MAX_PHYSICS_ITERATIONS)
                    throw new Exception("physicsManager::Update() - Hit max number of iterations.  Physics system cannot make any progress.");

                CheckRestingContacts();

                CheckPhantomContacts();

                // Try taking a RK4 Step for each object in h_game.gameObjectManager
                TakeRK4Step(time, Tstep_remaining, h_game.h_GameObjectManager.h_GameObjects);

                // Run the coarse and fine collision detections --> Full detection routines with swept shape tests (to catch tunnelling)
                intersection = SweptCollisionDetection(ref estColTime, true);

                /// ************************************
                /// ************* HACK CODE ************
                /// *** *********************************
                /// Swept collision detection sometimes triggers intersection at 
                /// 0.0f if objects started very close together
                if (intersection && estColTime <= 0.0f)
                {
                    float seperationDist = 0.0f;
                    intersection = StaticCollisionDetection(ref seperationDist, false, true);
                    if(intersection || seperationDist <= 0.0f)
                        throw new Exception("physicsManager::Update() - Objects were interpenetrating before first physics step!  Something went wrong");
                }

                /// Swept collision detection fails for glancing sphere AABB collisions 
                /// (where velocity is close to perpendicular to face normal).
                /// Do a static check on the final step. To remove obvious omissions.
                if (!intersection)
                {
                    float seperationDist = 0.0f;
                    if (StaticCollisionDetection(ref seperationDist, false, true))
                        throw new Exception("physicsManager::Update() - STATIC TEST Objects were interpenetrating before first physics step!  Something went wrong");
                    if (StaticCollisionDetection(ref seperationDist, true, true))
                    {
                        intersection = true; // The objects will end up intersecting
                        estColTime = Tstep_remaining * 0.5f; // Arbitrarily estimate collision at half the time step
                    }
                }
                /// ************************************
                /// *********** END HACK CODE **********
                /// ************************************

                // Resolve Collisions
                if (intersection)
                {
                    // Find time to collision using the firstCollision value as an estimate --> Also rebuilds the collision list with more accurate collision data
                    Tstep_to_collision = StepToCollisionTimeByBisection(Tstep_remaining, estColTime);

                    // Refresh collision list (previous collision estimate may have included more collisions than is realistic)
                    RefreshCollisionList(Tstep_remaining, Tstep_to_collision);

                    // Resolve Collision
                    ResolveCollisions(h_game.h_GameObjectManager.h_GameObjects);

                    if(h_game.h_GameSettings.renderCollisions)
                        h_game.h_GameObjectManager.SpawnCollisions(ref collisions);

                    if (h_game.h_GameSettings.pauseOnCollision && (pauseGame == false))
                    {
                        // Pause the game
                        pauseGame = true;
                        break;
                    }
                    else
                    {
                        Tstep_remaining -= Tstep_to_collision; // Remove the piecewise step from the time remaining
                        time += Tstep_to_collision;
                        curIteration++;
                    }

#if DEBUG
                    /// If we're being really paranoid, check for collision after
                    float d1 = 0.0f;
                    bool I1 = StaticCollisionDetection(ref d1, false, true);
                    if (I1 == true)
                    {
                        StaticCollisionDetection(ref d1, false, false);
                        throw new Exception("Something went wrong...  Objects interpenetrate after step");
                    }
#endif
                }
                else
                {
                    // No colliding collision detected, just check for phantom collisions and step the remaining amount
                    // Refresh collision list (previous collision estimate may have included more collisions than is realistic)
                    RefreshCollisionList(Tstep_remaining, Tstep_to_collision);

                    // Resolve Collision
                    ResolveCollisions(h_game.h_GameObjectManager.h_GameObjects);

                    Tstep_remaining -= Tstep_remaining;
                    CopyStateToPrevState(time, h_game.h_GameObjectManager.h_GameObjects); // Update the "new state" from last frame to the "old state" for this frame
                }
            }

            base.Update(gameTime);
        }
        #endregion

        #region SweptCollisionDetection()
        /// SweptCollisionDetection() - Top level collision detection routine
        /// Uses swept shape tests to detect collision
        /// ***********************************************************************
        protected bool SweptCollisionDetection(ref float estColTime, bool ingoreSoftBoundry)
        {
            // Coarse Collision detection
            CoarseCollisionDetection();

            // Fine Collision detection
            return SweptFineCollisionDetection(ref estColTime, ingoreSoftBoundry);
        }
        #endregion

        #region StepToCollisionTimeByBisection()
        /// StepToCollisionTimeByBisection() - Uses an iterative algorithm to find the time (with some error) 
        /// ***********************************************************************
        protected float StepToCollisionTimeByBisection(float Time_remaining, float estColTime)
        {
            float Tstep_to_collision = estColTime * Tstep_remaining - EPSILON; // colTime from swept tests are normalized 0->1.
            float bisectionTimeRemaining = Tstep_to_collision;
            bool collided = false;
            float minSeparationDistance = float.PositiveInfinity;
            int curIteration = 1;

            while (minSeparationDistance > BISECTION_TOLLERANCE || collided) // While they're still interpenetrating & we haven't reached the desired accuracy
            {
                if (curIteration > BISECTION_MAXITERATIONS)
                    throw new Exception("physicsManager::StepToCollisionTimeByBisection() - Could not reach convergence after " +
                                        String.Format("{0}", BISECTION_MAXITERATIONS) + " iterations. Final Separation distance = " +
                                        String.Format("{0:e}", minSeparationDistance) + ", collided = " + (collided ? "true" : "false"));

                // Take a step to the estimated collision time
                TakeRK4Step(time, Tstep_to_collision, h_game.h_GameObjectManager.h_GameObjects);

                if (collided = StaticCollisionDetection(ref minSeparationDistance, true, true)) // Doesn't use swept tests.  Just return binary if objects overlap
                {
                    Tstep_to_collision -= bisectionTimeRemaining; // we went too far and now the objects overlap, add the bisection time back
                    bisectionTimeRemaining /= 2.0f;
                    Tstep_to_collision += bisectionTimeRemaining; // Try a half step
                }
                else 
                {
                    if (minSeparationDistance < BISECTION_TOLLERANCE) // We've gone far enough
                        break;
                    if (Tstep_to_collision >= Time_remaining) // We've gone further than the current time step
                    {
                        ClearCollisions();
                        Tstep_to_collision = Time_remaining;
                        break;
                    }
                    Tstep_to_collision += bisectionTimeRemaining;  // We didn't go far enough, add time
                }
                curIteration ++;
            }

            // Move the simulator forward to this new time
            CopyStateToPrevState(time, h_game.h_GameObjectManager.h_GameObjects); // Update the "new state" from last frame to the "old state" for this frame

            return Tstep_to_collision;
        }
        #endregion

        #region RefreshCollisionList()
        // RefreshCollisionList() - Calculate the collision point based on the updated positions
        protected void RefreshCollisionList(float Time_remaining, float Tstep_to_collision)
        {
            ClearCollisions();

            gameObject objA = null;
            gameObject objB = null;

            // Loop through object pairs and check if they potentially overlap from the sweep and prune test
            for (int i = 0; i < (numObjects - 1); i++)
            {
                for (int j = i + 1; j < (numObjects); j++)
                {
                    if (h_game.h_GameObjectManager.h_GameObjects[i].collidable && h_game.h_GameObjectManager.h_GameObjects[j].collidable)
                    {
                        AABBOverlap curOverlap = AABBOverlap.GetOverlapStatus(ref AABBOverlapStatus, i, j, numObjects);
                        if (curOverlap.xAxisOverlap && curOverlap.yAxisOverlap && curOverlap.zAxisOverlap)
                        {
                            objA = h_game.h_GameObjectManager.h_GameObjects[i];
                            objB = h_game.h_GameObjectManager.h_GameObjects[j];
                            // Objects potentially overlap --> Check the low level collision routines
                            collisionUtils.AddCollisionStatic(objA, objB, ref collisions, ref objA.state, ref objB.state);
                        }
                    }
                }
            }

        }
        #endregion 

        #region StaticCollisionDetection()
        /// StaticCollisionDetection() - Top level collision detection routine
        /// ***********************************************************************
        protected bool StaticCollisionDetection(ref float minSeparationDistance, bool endState, bool ignoreSoftBoundry )
        {
            // Coarse Collision detection
            // CoarseCollisionDetection(); --> DON'T RE-RUN THE COARSE COLLISION DETECTION - USE THE OBJECTS CACHED FROM BEFORE

            // Fine Collision detection
            return StaticFineCollisionDetection(ref minSeparationDistance, endState, ignoreSoftBoundry);
        }
        #endregion

        #region TakeRK4Step()
        /// TakeStep() - Perform RK4 step on each gameObject
        /// ***********************************************************************
        public void TakeRK4Step(float time, float deltaTime, List<gameObject> gameObjects)
        {
            gameObject curObject = null;

            // enumerate through each element in the list and update the quaternion values
            List<gameObject>.Enumerator ListEnum = gameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                curObject = ListEnum.Current;

                if (curObject.movable)
                {
                    // Calculate piecewise derivatives
                    float halfDeltaTime = 0.5f * deltaTime;
                    D1.Evaluate(ref curObject.prevState, time, curObject);
                    D2.Evaluate(ref curObject.state, ref curObject.prevState, time + halfDeltaTime, halfDeltaTime, D1, curObject);
                    D3.Evaluate(ref curObject.state, ref curObject.prevState, time + halfDeltaTime, halfDeltaTime, D2, curObject);
                    D4.Evaluate(ref curObject.state, ref curObject.prevState, time + deltaTime, deltaTime, D3, curObject);

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
                    // physicsManager.ClipVelocity(ref curObject.state.linearVel, h_game.h_GameSettings.physicsMinVel, curObject.maxVel);
                }
            }

            // We've moved the objects, so the AABB data is no longer valid --> Flag it
            h_game.h_GameObjectManager.SetDirtyBoundingBoxes(); 
        }
        #endregion

        #region ResolveCollisions()
        /// ResolveCollisions() - Add impulse for collision contacts, and add contact forces for contact collisions
        /// ***********************************************************************
        protected void ResolveCollisions(List<gameObject> gameObjects)
        {
            int restingContact = 0;
            // Step through each collision and resolve them
            for (int i = 0; i < collisions.Count; i++)
            {
                gameObject obj1 = (gameObject)collisions[i].obj1;
                gameObject obj2 = (gameObject)collisions[i].obj2;

                // Resolve collisions
                // PLAYER - ENEMY
                if((obj1 is gameObjectPlayer && obj2 is gameObjectEnemy) || (obj1 is gameObjectEnemy && obj2 is gameObjectPlayer))
                    restingContact = collisions[i].ResolvePlayerEnemyCollision(gameObjects, ref obj1.prevState, ref obj2.prevState, h_game.h_GameSettings.enemyCollisionAngleTollerence);
                
                // GENERIC COLLISION --> RESOLVE WITH IMPULSE RESPONCE OR RESTING CONTACT
                else // Resolve Generic Collsion
                    restingContact = collisions[i].ResolveCollision(gameObjects, ref obj1.prevState, ref obj2.prevState);
                
                // Add a resting contact, if the collision normal is vertical (ie a collision against the ground)
                // --> This adds an antigravity force to the two objects to counteract downward motion AND Y velocity and momentum set to zero
                switch (restingContact)
                {
                    case 0:                         // COLLIDING CONTACT
                        break;
                    case 1:                         // RESTING CONTACT
                        ProcessRestingContact(i, ref obj1, ref obj2);
                        break;
                    case 2:                         // PHANTOM CONTACT
                        ProcessPhantomContact(i, ref obj1, ref obj2);    
                        break;
                    default:
                        throw new Exception("physicsManager::ResolveCollisions() - Unrecognised contact type");
                }
            }

            // This is a hack --> Just turns off gravity for those objects that have collided with the floor
            ComputeContactForces();

            ComputePhantomForces();
        }
        #endregion

        #region ProcessRestingContact()
        protected void ProcessRestingContact(int index, ref gameObject obj1, ref gameObject obj2)
        {
            if ((Math.Abs(collisions[index].colNorm.X) < RESTING_CONTACT_TOLLERANCE) && (Math.Abs(collisions[index].colNorm.Z) < RESTING_CONTACT_TOLLERANCE))
            {
                if (!FindRestingContact(ref obj1, ref obj2))
                    restingContacts.Add(collisions[index]); // If a resting contact between the two doesn't exist, then add one
            }
            else
            {
                // Otherwise add a small impulse to push objects APART so that we are out of resting contact threshold
                lImpulse = collisions[index].GetMomForCollidingContact(ref obj1.prevState, ref obj2.prevState);
                lImpulse.X *= gameSettings.collisionMask.X; lImpulse.Y *= gameSettings.collisionMask.Y; lImpulse.Z *= gameSettings.collisionMask.Z;

                if (obj1.movable && obj2.movable)
                {
                    obj1.prevState.linearMom += lImpulse * 0.5f;
                    obj2.prevState.linearMom -= lImpulse * 0.5f;
                    obj1.prevState.RecalculateDerivedQuantities();
                    obj2.prevState.RecalculateDerivedQuantities();
                }
                else if (obj1.movable && !obj2.movable)
                {
                    obj1.prevState.linearMom += lImpulse * 1.0f;
                    obj1.prevState.RecalculateDerivedQuantities();
                }
                else if (!obj1.movable && obj2.movable)
                {
                    obj2.prevState.linearMom -= lImpulse * 1.0f;
                    obj2.prevState.RecalculateDerivedQuantities();
                }
                else
                    throw new Exception("physicsManager::ResolveCollisions() - Could not prevent resting contact, objects are not movable");

                /*
                // Then recover the collision as we normally would.
                if(collisions[i].CheckCollidingContact(ref obj1.prevState, ref obj2.prevState))
                    collisions[i].ResolveCollidingCollision(ref obj1.prevState, ref obj2.prevState);
                else
                    throw new Exception("physicsManager::ResolveCollisions() - Could not prevent resting contact, check GetVelAForCollidingContact() results");
                */
            }
        }
        #endregion

        #region FindRestingContact()
        // Linearly search through the resting contacts and see if we can find one that exists between the two objects
        protected bool FindRestingContact(ref gameObject obj1, ref gameObject obj2)
        {
            for (int i = 0; i < restingContacts.Count; i++)
            {
                if ((restingContacts[i].obj1.Equals(obj1) && restingContacts[i].obj2.Equals(obj2)) ||
                   (restingContacts[i].obj2.Equals(obj1) && restingContacts[i].obj1.Equals(obj2)))
                    return true;
            }
            return false;
        }
        #endregion

        #region ProcessPhantomContact()
        protected void ProcessPhantomContact(int index, ref gameObject obj1, ref gameObject obj2)
        {
            if (!FindPhantomContact(ref obj1, ref obj2))
                phantomContacts.Add(collisions[index]); // If a phantom contact between the two doesn't exist, then add one
        }
        #endregion

        #region FindPhantomContact()
        // Linearly search through the phantom contacts and see if we can find one that exists between the two objects
        protected bool FindPhantomContact(ref gameObject obj1, ref gameObject obj2)
        {
            for (int i = 0; i < phantomContacts.Count; i++)
            {
                if ((phantomContacts[i].obj1.Equals(obj1) && phantomContacts[i].obj2.Equals(obj2)) ||
                   (phantomContacts[i].obj2.Equals(obj1) && phantomContacts[i].obj1.Equals(obj2)))
                    return true;
            }
            return false;
        }
        #endregion

        #region CopyStateToPrevState()
        /// CopyStateToPrevState() - Get ready for next physics step by making the previous frame's "new state", the current frame's "old state"
        /// ***********************************************************************
        public void CopyStateToPrevState(float time, List<gameObject> gameObjects)
        {
            gameObject curObject = null;

            // enumerate through each element in the list and update the quaternion values
            List<gameObject>.Enumerator ListEnum = gameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                curObject = ListEnum.Current;

                // Copy: prevState = state
                rboState.CopyDynamicQuantitiesAtoB(ref curObject.state, ref curObject.prevState);
                curObject.prevState.time = time;
            }
        }
        #endregion

        #region CopyPrevStateToState()
        /// CopyPrevStateToState() - Undo the step by setting state to previous state.
        /// ***********************************************************************
        public void CopyPrevStateToState(List<gameObject> gameObjects)
        {
            gameObject curObject = null;

            // enumerate through each element in the list and update the quaternion values
            List<gameObject>.Enumerator ListEnum = gameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                curObject = ListEnum.Current;

                // Copy: prevState = state
                rboState.CopyDynamicQuantitiesAtoB(ref curObject.prevState, ref curObject.state);
            }
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

        #region initCoarseCollisionDetection()
        /// initCoarseCollisionDetection() --> Performs first insertion sort of AABB arrays. O(n^2).
        /// ***********************************************************************
        public void initCoarseCollisionDetection()
        {
            int STARTING_VECTOR_SIZE = h_game.h_GameSettings.physicsObjectsStartingCapacity;

            // Reset all object overlap statuses.
            AABBOverlapStatus = new List<AABBOverlap>(STARTING_VECTOR_SIZE);
            // Add as many elements as there are objects --> I'm being wastefull and adding space for non-collidables, but indexing is faster and easier
            for (int i = 0; i < numObjects * numObjects; i++) 
                AABBOverlapStatus.Add(new AABBOverlap(false, false, false));

            // Initialize axis lists as just the RBOBJECTS that are collidable only! (they will be in topographically unsorted order to start)
            AABBXaxis = new List<int>(STARTING_VECTOR_SIZE);
            AABBYaxis = new List<int>(STARTING_VECTOR_SIZE);
            AABBZaxis = new List<int>(STARTING_VECTOR_SIZE);
            for (int i = 0; i < numObjects; i++)
            {
                if (h_game.h_GameObjectManager.h_GameObjects[i].collidable)
                {
                    AABBXaxis.Add(i); AABBYaxis.Add(i); AABBZaxis.Add(i);
                }
            }

            AABBActiveList = new LinkedList<int>();

            // Perform a first time sort and sweep --> Expected slow due to insertion sort on unsorted list.
            CoarseCollisionDetection();
        }
        #endregion

        #region CoarseCollisionDetection()
        /// CoarseCollisionDetection()				
        /// Use Axis aligned bounding boxes to find all sets of potential collisions. Expected O(n+k+c)
        /// ***********************************************************************
        public void CoarseCollisionDetection()
        {
	        // Update Bounding boxes
            h_game.h_GameObjectManager.UpdateCoarseBoundingBoxes();

            // Do an insertion sort on each axis list to order objects, by minimum BB vertex
            // Insertion sort is O(n) when almost sorted, therefore best for slowly moving objects
            InsertionSortAABBs(ref AABBXaxis, 
                               ref h_game.h_GameObjectManager.h_GameObjects, 
                               numCollidableObjects, 
                               new FUNC_AXISSELECT(xAxisMinSel));
            InsertionSortAABBs(ref AABBYaxis, 
                               ref h_game.h_GameObjectManager.h_GameObjects,
                               numCollidableObjects, 
                               new FUNC_AXISSELECT(yAxisMinSel));
            InsertionSortAABBs(ref AABBZaxis, 
                               ref h_game.h_GameObjectManager.h_GameObjects,
                               numCollidableObjects, 
                               new FUNC_AXISSELECT(zAxisMinSel));

            // Now Find all overlaps by doing sweep of each axis lists.
            SweepAxisList(ref AABBXaxis, 
                          ref h_game.h_GameObjectManager.h_GameObjects,
                          numCollidableObjects,
                          numObjects,
                          new FUNC_AXISSELECT(xAxisMinSel),
                          new FUNC_AXISSELECT(xAxisMaxSel),
                          new FUNC_STATUSSELECT(SetOverlapStatusXaxis));
            SweepAxisList(ref AABBYaxis,
                          ref h_game.h_GameObjectManager.h_GameObjects,
                          numCollidableObjects,
                          numObjects,
                          new FUNC_AXISSELECT(yAxisMinSel),
                          new FUNC_AXISSELECT(yAxisMaxSel),
                          new FUNC_STATUSSELECT(SetOverlapStatusYaxis));
            SweepAxisList(ref AABBZaxis,
                          ref h_game.h_GameObjectManager.h_GameObjects,
                          numCollidableObjects,
                          numObjects,
                          new FUNC_AXISSELECT(zAxisMinSel),
                          new FUNC_AXISSELECT(zAxisMaxSel),
                          new FUNC_STATUSSELECT(SetOverlapStatusZaxis));
        }
        #endregion

        #region InsertionSortAABBs()
        /// InsertionSortAABBs()				
        /// Takes a list of rbobjects indices, and sorts them using the value derived from the selection function.
        /// ***********************************************************************
        protected static void InsertionSortAABBs(ref List<int> pArray, ref List<gameObject> gameObjects, int arraySize, FUNC_AXISSELECT FUNC)
        {
        	if(arraySize<1)
	        throw new Exception("physicsManager::InsertionSortAABBs: Array is empty, nothing to sort!");

            int rbobjectToSortIndex; float curVal = 0.0f, valueToSort = 0.0f;
	        for(int i=1; i<arraySize;i++) {  // Place the next value
		        rbobjectToSortIndex = pArray[i];
                valueToSort = FUNC(gameObjects[rbobjectToSortIndex]);
		        for(int j=0; j<=i;j++) {
			        curVal = FUNC(gameObjects[pArray[j]]);
			        if(curVal > valueToSort) {
				        // push the other value forward to insert
				        for(int m = i; m>j; m--) {
					        pArray[m] = pArray[m-1];
				        }
                        pArray[j] = rbobjectToSortIndex;
				        break;
			        }
		        }
	        }
        }
        #endregion

        #region SweepAxisList()
        /// SweepAxisList()				
        /// Takes a list of rbobjects, performs a sweep determining all overlaps, and sets the 
        /// appropriate values in the overlap list (using a set_func). pArray must already be
        /// ordered by minimum AABB verticies.
        /// ***********************************************************************
        protected void SweepAxisList(ref List<int> pArray, ref List<gameObject> gameObjects, int pArraySize, int numObjects, FUNC_AXISSELECT MinSel, FUNC_AXISSELECT MaxSel, FUNC_STATUSSELECT Set)
        {
            int curArrayIndex = 0; LinkedListNode<int> curNode; LinkedListNode<int> tempNode;
	        AABBActiveList.Clear();
            while (curArrayIndex < pArraySize)
	        {
                if (gameObjects[pArray[curArrayIndex]].collidable)
                {
                    // Check the current object against objects on the active list
                    curNode = AABBActiveList.First;
                    while (curNode != null) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
                    {
                        // If the new object start is after the active list end, then: REMOVE INDEX FROM ACTIVE LIST
                        if (MaxSel(gameObjects[curNode.Value]) < MinSel(gameObjects[pArray[curArrayIndex]]))
                        {
                            Set(ref AABBOverlapStatus, numObjects, curNode.Value, pArray[curArrayIndex], false);
                            tempNode = curNode.Next;
                            AABBActiveList.Remove(curNode);
                            curNode = tempNode;
                        }
                        else // OTHERWISE THERE IS OVERLAP BETWEEN THESE OBJECTS, so set overlap status
                        {
                            Set(ref AABBOverlapStatus, numObjects, curNode.Value, pArray[curArrayIndex], true);
                            curNode = curNode.Next;
                        }
                    }

                    // Put the current rbobject index on the active list
                    AABBActiveList.AddLast(new LinkedListNode<int>(pArray[curArrayIndex]));
                }

                // Go to the next object
		        curArrayIndex ++;
	        }
        }
        #endregion

        #region Delegates (FUNCTION POINTERS) for variable selection in coarse collision's sweep and prune
        /// A bunch of delegate functions so that sort and sweep functions are written once, but can be sorted on numerous targest
        /// ***********************************************************************
        public delegate float FUNC_AXISSELECT(gameObject x);
        public float xAxisMinSel(gameObject _obj)
        {
            return _obj.AABB_min.X;
        }
        public static float yAxisMinSel(gameObject _obj)
        {
            return _obj.AABB_min.Y;
        }
        public static float zAxisMinSel(gameObject _obj)
        {
            return _obj.AABB_min.Z;
        }
        public static float xAxisMaxSel(gameObject _obj)
        {
            return _obj.AABB_max.X;
        }
        public static float yAxisMaxSel(gameObject _obj)
        {
            return _obj.AABB_max.Y;
        }
        public static float zAxisMaxSel(gameObject _obj)
        {
            return _obj.AABB_max.Z;
        }

        public delegate void FUNC_STATUSSELECT(ref List<AABBOverlap> AABBOverlapStatus, int arraySize, int index1, int index2, bool setval);
        void SetOverlapStatusXaxis(ref List<AABBOverlap> AABBOverlapStatus, int arraySize, int index1, int index2, bool setval)
        {
            if (index1 < index2)
            {
                AABBOverlapStatus[index1 * arraySize + index2].xAxisOverlap = setval;
            }
	        else
            {
                AABBOverlapStatus[index2 * arraySize + index1].xAxisOverlap = setval;
            }
        }
        void SetOverlapStatusYaxis(ref List<AABBOverlap> AABBOverlapStatus, int arraySize, int index1, int index2, bool setval)
        {
            if (index1 < index2)
            {
                AABBOverlapStatus[index1 * arraySize + index2].yAxisOverlap = setval;
            }
            else
            {
                AABBOverlapStatus[index2 * arraySize + index1].yAxisOverlap = setval;
            }
        }
        void SetOverlapStatusZaxis(ref List<AABBOverlap> AABBOverlapStatus, int arraySize, int index1, int index2, bool setval)
        {
            if (index1 < index2)
            {
                AABBOverlapStatus[index1 * arraySize + index2].zAxisOverlap = setval;
            }
            else
            {
                AABBOverlapStatus[index2 * arraySize + index1].zAxisOverlap = setval;
            }
        }

        #endregion

        #region initFineCollisionDetection()
        /// initFineCollisionDetection() --> Just initialize data structures (ie 
        /// ***********************************************************************
        public void initFineCollisionDetection()
        {
            // Worst case number of collsions O(n^2) from n choose 2 (and AABB can collide with 4 contacts)
            collisions = new List<collision>(numCollidableObjects * numCollidableObjects * 4);
            restingContacts = new List<collision>(numCollidableObjects * numCollidableObjects * 4);
            phantomContacts = new List<collision>(numCollidableObjects * numCollidableObjects * 4);
        }
        #endregion

        #region SweptFineCollisionDetection()
        /// SweptFineCollisionDetection() --> Go through the coarse collision routine results and get run fine collision detection
        /// I thought O(n^2) through array was dumb, but Christer Ericson does the same thing (Real Time Collision Detection, page 329-338)
        /// We're going to all the trouble to get O(n) in sweap and prune, then we just do an O(n^2) loop
        /// ***********************************************************************
        protected bool SweptFineCollisionDetection(ref float estColTime, bool ignoreSoftBoundry)
        {
            gameObject objA = null;
            gameObject objB = null;

            // Loop through object pairs and check if they potentially overlap from the sweep and prune test
            for(int i = 0; i < (numObjects-1); i ++)
			{
                if(h_game.h_GameObjectManager.h_GameObjects[i].collidable)
                {
                    for (int j = i + 1; j < (numObjects); j++)
                    {

                        if (h_game.h_GameObjectManager.h_GameObjects[j].collidable)
                        {
                            AABBOverlap curOverlap = AABBOverlap.GetOverlapStatus(ref AABBOverlapStatus, i, j, numObjects);
                            if (curOverlap.xAxisOverlap && curOverlap.yAxisOverlap && curOverlap.zAxisOverlap)
                            {

                                objA = h_game.h_GameObjectManager.h_GameObjects[i];
                                objB = h_game.h_GameObjectManager.h_GameObjects[j];
                                if (ignoreSoftBoundry &&
                                    ((objA is gameObjectPhantom && ((gameObjectPhantom)objA).phantomType == phantomType.SOFT_BOUNDRY) ||
                                     (objB is gameObjectPhantom && ((gameObjectPhantom)objB).phantomType == phantomType.SOFT_BOUNDRY)))
                                {
                                    // Do nothing if we're asked to ignore the SOFT_BOUNDRY
                                }
                                else
                                {
                                    // Objects potentially overlap --> Check the low level collision routines
                                    if (collisionUtils.TestSweptCollision(objA, objB,
                                                                         ref estColTime,
                                                                         ref objA.prevState, ref objA.state,
                                                                         ref objB.prevState, ref objB.state))
                                        return true; // Don't need to continue if we've found at least one object collides
                                }
                            }
                        } // if (h_game.h_GameObjectManager.h_GameObjects[j].collidable)
                    } // for (int j = i + 1; j < (numObjects); j++)
                } // if(h_game.h_GameObjectManager.h_GameObjects[i].collidable)
            } // for(int i = 0; i < (numObjects-1); i ++)

            return false;
        }
        #endregion

        #region StaticFineCollisionDetection()
        /// StaticFineCollisionDetection() --> Don't do swept tests, just to quick bool operations
        /// ***********************************************************************
        protected bool StaticFineCollisionDetection(ref float minSeparationDistance, bool endstate, bool ignoreSoftBoundry)
        {
            bool colDetected = false;
            minSeparationDistance = float.PositiveInfinity;
            float curSeparationDistance = float.PositiveInfinity;
            gameObject objA = null;
            gameObject objB = null;
            // Loop through object pairs and check if they potentially overlap from the sweep and prune test
            for (int i = 0; i < (numObjects - 1); i++)
            {
                for (int j = i + 1; j < (numObjects); j++)
                {
                    if (h_game.h_GameObjectManager.h_GameObjects[i].collidable && h_game.h_GameObjectManager.h_GameObjects[j].collidable)
                    {
                        AABBOverlap curOverlap = AABBOverlap.GetOverlapStatus(ref AABBOverlapStatus, i, j, numObjects);
                        if (curOverlap.xAxisOverlap && curOverlap.yAxisOverlap && curOverlap.zAxisOverlap)
                        {
                            objA = h_game.h_GameObjectManager.h_GameObjects[i];
                            objB = h_game.h_GameObjectManager.h_GameObjects[j];
                            if (ignoreSoftBoundry &&
                               ((objA is gameObjectPhantom && ((gameObjectPhantom)objA).phantomType == phantomType.SOFT_BOUNDRY) ||
                                (objB is gameObjectPhantom && ((gameObjectPhantom)objB).phantomType == phantomType.SOFT_BOUNDRY)))
                            {
                                // Do nothing if we're asked to ignore the SOFT_BOUNDRY
                            }
                            else
                            {
                                // Objects potentially overlap --> Check the low level collision routines
                                if (endstate)
                                    colDetected = collisionUtils.TestStaticCollision(objA, objB,
                                                                                     ref curSeparationDistance,
                                                                                     ref objA.state, ref objB.state) ? true : colDetected;
                                else
                                    colDetected = collisionUtils.TestStaticCollision(objA, objB,
                                                                                     ref curSeparationDistance,
                                                                                     ref objA.prevState, ref objB.prevState) ? true : colDetected;

                                if (minSeparationDistance > curSeparationDistance)
                                    minSeparationDistance = curSeparationDistance;
                            }
                        }
                    }
                }
            }

            return colDetected;
        }
        #endregion

        #region ClearCollisions()
        /// ClearCollisions() --> Clear the collisions list
        /// ***********************************************************************
        protected void ClearCollisions()
        {
            collisions.Clear();
        }
        #endregion

        #region ComputeContactForces()
        /// ComputeContactForces() --> A massive hack.  See "TO DO.TXT" (listing 4th from the bottom) for what I should do instead.           
        /// ***********************************************************************
        protected void ComputeContactForces()
        {
            /// This is a MASSIVE hack.
            /// For each resting contact just turn off the gravity force by adding anti gravity force.  
            /// Will only work with resting contacts against the floor.
            /// Won't allow for stackable objects --> Might be ok under most gameplay situations.

            for (int i = 0; i < restingContacts.Count; i++)
            {
                if (!((gameObject)restingContacts[i].obj1).CheckAntiGravityForce() &&
                    ((gameObject)restingContacts[i].obj1).movable)
                {
                    ((gameObject)restingContacts[i].obj1).AddAntiGravityForce(h_game.h_GameSettings.gravity);
                    ((gameObject)restingContacts[i].obj1).resting = true;
                    ((gameObject)restingContacts[i].obj1).prevState.linearMom.Y = 0.0f;
                    ((gameObject)restingContacts[i].obj1).prevState.linearVel.Y = 0.0f;
                    ((gameObject)restingContacts[i].obj1).prevState.pos.Y += BISECTION_TOLLERANCE; // Push the object up slightly
                }
                if (!((gameObject)restingContacts[i].obj2).CheckAntiGravityForce() &&
                    ((gameObject)restingContacts[i].obj2).movable)
                {
                    ((gameObject)restingContacts[i].obj2).AddAntiGravityForce(h_game.h_GameSettings.gravity);
                    ((gameObject)restingContacts[i].obj2).resting = true;
                    ((gameObject)restingContacts[i].obj2).prevState.linearMom.Y = 0.0f;
                    ((gameObject)restingContacts[i].obj2).prevState.linearVel.Y = 0.0f;
                    ((gameObject)restingContacts[i].obj2).prevState.pos.Y += BISECTION_TOLLERANCE; // Push the object up slightly
                }
            }
        }
        #endregion

        #region ComputePhantomForces()
        /// ComputePhantomForces() --> Add the phantom forces        
        /// ***********************************************************************
        protected void ComputePhantomForces()
        {
            // Iterate through the phantom list and add the phantom force if it doesn't exist
            for (int i = 0; i < phantomContacts.Count; i++)
            {
                if (phantomContacts[i].obj1 is gameObjectPhantom)
                {
                    if (!((gameObject)phantomContacts[i].obj2).CheckPhantomForce())
                        if (phantomContacts[i].obj2 is gameObjectPlayer && ((gameObjectPhantom)phantomContacts[i].obj1).softBoundryPlayerReact)
                            ((gameObject)phantomContacts[i].obj2).AddPhantomForce(((gameObjectPhantom)phantomContacts[i].obj1).softBoundaryForceVector);
                        else if (phantomContacts[i].obj2 is gameObjectNPC && ((gameObjectPhantom)phantomContacts[i].obj1).softBoundryNPCReact)
                            ((gameObject)phantomContacts[i].obj2).AddPhantomForce(((gameObjectPhantom)phantomContacts[i].obj1).softBoundaryForceVector);
                }
                else if (phantomContacts[i].obj2 is gameObjectPhantom)
                {
                    if (!((gameObject)phantomContacts[i].obj1).CheckPhantomForce())
                        if (phantomContacts[i].obj1 is gameObjectPlayer && ((gameObjectPhantom)phantomContacts[i].obj2).softBoundryPlayerReact)
                            ((gameObject)phantomContacts[i].obj1).AddPhantomForce(((gameObjectPhantom)phantomContacts[i].obj2).softBoundaryForceVector);
                        else if (phantomContacts[i].obj1 is gameObjectNPC && ((gameObjectPhantom)phantomContacts[i].obj2).softBoundryNPCReact)
                            ((gameObject)phantomContacts[i].obj1).AddPhantomForce(((gameObjectPhantom)phantomContacts[i].obj2).softBoundaryForceVector);
                }
                else
                    throw new Exception("physicsManager::ComputePhantomForces() - Neither objects is a phantom force");
            }
        }
        #endregion

        #region CheckRestingContacts()
        /// CheckRestingContacts() --> A massive hack.  See "TO DO.TXT" (listing 4th from the bottom)              
        /// ***********************************************************************
        protected void CheckRestingContacts()
        {
            float separationDistance = 0.0f;
            bool anotherContactExists = false;

            /// Go through each resting contact and check that they objects are still
            /// in close proximity.  If they are not, and no other resting contact exists for this object
            /// in the list then remove the antigravity force
            for (int i = 0; i < restingContacts.Count; i++)
            {
                collisionUtils.TestStaticCollision((gameObject)restingContacts[i].obj1, (gameObject)restingContacts[i].obj2, 
                                                   ref separationDistance,
                                                   ref ((gameObject)restingContacts[i].obj1).prevState, ref ((gameObject)restingContacts[i].obj2).prevState);
                // If the objects are no longer within close proximity (objects have been knocked apart or objs have slid away from each other)
                if (separationDistance > RESTING_CONTACT_TOLLERANCE)
                {
                    if (((gameObject)restingContacts[i].obj1).collidable)
                    {
                        // Iterate through the rest of the restingContacts list and see if we can find another resing contact with the object
                        anotherContactExists = false;
                        for (int j = i + 1; j < restingContacts.Count; j++)
                            if (restingContacts[j].obj1.Equals(restingContacts[i].obj1) || restingContacts[j].obj2.Equals(restingContacts[i].obj1))
                                anotherContactExists = true;

                        if (!anotherContactExists)
                        {
                            ((gameObject)restingContacts[i].obj1).RemoveAntiGravityForce();
                            ((gameObject)restingContacts[i].obj1).resting = false;
                        }
                    }

                    if (((gameObject)restingContacts[i].obj2).collidable)
                    {
                        // Iterate through the rest of the restingContacts list and see if we can find another resing contact with the object
                        anotherContactExists = false;
                        for (int j = i + 1; j < restingContacts.Count; j++)
                            if (restingContacts[j].obj1.Equals(restingContacts[i].obj2) || restingContacts[j].obj2.Equals(restingContacts[i].obj2))
                                anotherContactExists = true;

                        if (!anotherContactExists)
                        {
                            ((gameObject)restingContacts[i].obj2).RemoveAntiGravityForce();
                            ((gameObject)restingContacts[i].obj2).resting = false;
                        }
                    }

                    // Remove the resting contact from the list
                    restingContacts.RemoveAt(i);

                }

            }
        }
        #endregion

        #region CheckPhantomContacts()
        /// CheckPhantomContacts() -> Check if each phantom force is still valid.  If it isn't, remove it         
        /// ***********************************************************************
        protected void CheckPhantomContacts()
        {
            float separationDistance = 0.0f;

            /// Go through each phantom contact and check that they objects are still overlapping.
            for (int i = 0; i < phantomContacts.Count; i++)
            {
                collisionUtils.TestStaticCollision((gameObject)phantomContacts[i].obj1, (gameObject)phantomContacts[i].obj2,
                                                   ref separationDistance,
                                                   ref ((gameObject)phantomContacts[i].obj1).prevState, ref ((gameObject)phantomContacts[i].obj2).prevState);
                // If the objects are no longer within close proximity (objects have been knocked apart or objs have slid away from each other)
                if (separationDistance > 0.0f)
                {
                    if (!(phantomContacts[i].obj1 is gameObjectPhantom))
                        ((gameObject)phantomContacts[i].obj1).RemovePhantomForce();
                    if (!(phantomContacts[i].obj2 is gameObjectPhantom))
                        ((gameObject)phantomContacts[i].obj2).RemovePhantomForce();

                    // Remove the resting contact from the list
                    phantomContacts.RemoveAt(i);
                }

            }
        }
        #endregion

        #region ProcessRemoval(int index)
        public void ProcessRemoval(int index)
        {
            // Swap the input index with the last object
            ProcessRemoval(index, ref AABBXaxis);
            ProcessRemoval(index, ref AABBYaxis);
            ProcessRemoval(index, ref AABBZaxis);

            ClearAABBOverlapStatus();

            // See if there are any restingContacts involving this object and if there are, remove them
            for (int j = 0; j < restingContacts.Count; j++)
                if (restingContacts[j].obj1.Equals(h_game.h_GameObjectManager.h_GameObjects[index]) ||
                   restingContacts[j].obj2.Equals(h_game.h_GameObjectManager.h_GameObjects[index]))
                { restingContacts.RemoveAt(j); j--; }

            // See if there are any phantomContacts involving this object and if there are, remove them
            for (int j = 0; j < phantomContacts.Count; j++)
                if (phantomContacts[j].obj1.Equals(h_game.h_GameObjectManager.h_GameObjects[index]) ||
                   phantomContacts[j].obj2.Equals(h_game.h_GameObjectManager.h_GameObjects[index]))
                { phantomContacts.RemoveAt(j); j--; }
        }
        #endregion

        #region ProcessRemoval(int index, ref List<int> index)
        public void ProcessRemoval(int index, ref List<int> axisList)
        {
            // Find the index in the list
            int axisListIndex = -1;
            for (int i = 0; i < numCollidableObjects; i++)
                if (axisList[i] == index)
                { axisListIndex = i; break; }
            if (axisListIndex == -1)
                throw new Exception("physicsManager::ProcessRemoval() - Could not find input index in the AABB axisList");

            // Now swap the last element with the current element
            if (axisListIndex != (numCollidableObjects - 1))
                axisList[axisListIndex] = axisList[numCollidableObjects - 1]; // Store the last value in the place occupied by the input index

            // Remove the last index
            axisList.RemoveAt(numCollidableObjects - 1);

            // Now re-number the indexes from index to numCollidableObjects - 1;
            for (int i = 0; i < numCollidableObjects - 1; i++)
                if (axisList[i] >= index)
                    axisList[i] = axisList[i] - 1;
        }
        #endregion

        #region ClearAxisList(ref List<int> index)
        public void ClearAxisList(ref List<int> axisList)
        {
            // Remove the last index
            axisList.RemoveAt(numCollidableObjects - 1);

            // Arbitrarily set the indexes from index to numCollidableObjects - 1;
            for (int i = 0; i < numCollidableObjects - 1; i++)
                axisList[i] = i;
        }
        #endregion

        #region ClearAABBOverlapStatus()
        public void ClearAABBOverlapStatus()
        {
            // Go through each overlap status and clear it
            for (int i = 0; i < AABBOverlapStatus.Count; i++)
            {
                AABBOverlapStatus[i].xAxisOverlap = false;
                AABBOverlapStatus[i].yAxisOverlap = false;
                AABBOverlapStatus[i].zAxisOverlap = false;
            }
        }
        #endregion
    }

}
