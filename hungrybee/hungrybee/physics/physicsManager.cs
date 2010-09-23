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
        private int numObjects; // Not known until gameObjectManager.LoadLevel() is complete
        private int numCollidableObjects;
        rboDerivative D1, D2, D3, D4;

        // Coarse collision detection
        private List<AABBOverlap> AABBOverlapStatus;
        private LinkedList<int> AABBActiveList; // Linked list of active values
        private List<int> AABBXaxis;   // Sorted indices of gameObjects --> xaxis values
        private List<int> AABBYaxis;
        private List<int> AABBZaxis;

        // Fine collision detection
        protected List<collision> collisions;
        protected collision firstCollision;
        protected float time;
        protected float Tstep_remaining;

        // Resting contacts
        protected List<collision> restingContacts;

        protected static float EPSILON = 0.00000001f;
        public static float BISECTION_TOLLERANCE = 0.001f;
        public static float RESTING_CONTACT_TOLLERANCE = 0.01f; // typically this is 10 x BISECTION_TOLLERANCE
        protected static int BISECTION_MAXITERATIONS = 30;
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
            firstCollision = null;
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
            numCollidableObjects = h_game.GetGameObjectManager().GetNumberCollidableObjects();
            numObjects = h_game.GetGameObjectManager().h_GameObjects.Count;
            initCoarseCollisionDetection();
            initFineCollisionDetection();
        }
        #endregion

        #region Update() --> THIS IS WHERE MAIN PHYSICS STEP IS TAKEN
        /// Update()
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.P) && pauseGameDebounce == false)
            { pauseGame = !pauseGame; pauseGameDebounce = true; }
            if (Keyboard.GetState().IsKeyUp(Keys.P))
            { pauseGameDebounce = false; }


            time = (float)gameTime.TotalGameTime.TotalSeconds;
            Tstep_remaining = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(!pauseGame)
                CopyStateToPrevState(time, h_game.GetGameObjectManager().h_GameObjects); // Update the "new state" from last frame to the "old state" for this frame

            int curIteration = 0;
            while (Tstep_remaining > 0.0f && !pauseGame) // While there is still time to process
            {
                if (curIteration >= MAX_PHYSICS_ITERATIONS)
                    throw new Exception("physicsManager::Update() - Hit max number of iterations.  Physics system cannot make any progress.");

                CheckRestingContacts();

                // Try taking a RK4 Step for each object in h_game.gameObjectManager
                TakeRK4Step(time, Tstep_remaining, h_game.GetGameObjectManager().h_GameObjects);

                // Run the coarse and fine collision detections
                firstCollision = CollisionDetection();
                if (firstCollision != null && firstCollision.colTime <= 0.0f)
                    throw new Exception("physicsManager::Update() - Objects were interpenetrating before first physics step!  Something went wrong");

                // Resolve Collisions
                if (firstCollision != null)
                {
                    // Find time to collision using the firstCollision value as an estimate --> Also rebuilds the collision list with more accurate collision data
                    float Tstep_to_colision = StepToCollisionTimeByBisection(Tstep_remaining);

                    // Step to the collision time
                    TakeRK4Step(time, Tstep_to_colision, h_game.GetGameObjectManager().h_GameObjects);

                    // Resolve Collision
                    ResolveCollisions(time, firstCollision.colTime, h_game.GetGameObjectManager().h_GameObjects);

                    if(h_game.GetGameSettings().renderCollisions)
                        h_game.GetGameObjectManager().SpawnCollisions(ref collisions);

                    if (h_game.GetGameSettings().pauseOnCollision && (pauseGame == false))
                    {
                        // Pause the game
                        pauseGame = true;
                        break;
                    }
                    else
                        Tstep_remaining -= Tstep_to_colision; // Remove the piecewise step from the time remaining
                }
                else
                {
                    // No collision detected, just step the remaining amount
                    Tstep_remaining -= Tstep_remaining;
                }
            }

            base.Update(gameTime);
        }
        #endregion

        #region CollisionDetection()
        /// CollisionDetection() - Top level collision detection routine
        /// ***********************************************************************
        protected collision CollisionDetection()
        {
            // Coarse Collision detection
            CoarseCollisionDetection();

            // Fine Collision detection
            FineCollisionDetection();

            // Return the (estimated) first collision
            return GetFirstCollision();
        }
        #endregion

        #region StepToCollisionTimeByBisection()
        /// StepToCollisionTimeByBisection() - Uses an iterative algorithm to find the time (with some error) 
        /// ***********************************************************************
        protected float StepToCollisionTimeByBisection(float Time_remaining)
        {
            float Tstep_to_colision = firstCollision.colTime * Tstep_remaining - EPSILON; // colTime from swept tests are normalized 0->1.
            float bisectionTimeRemaining = Tstep_to_colision;
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
                TakeRK4Step(time, Tstep_to_colision, h_game.GetGameObjectManager().h_GameObjects);

                if (collided = StaticCollisionDetection(ref minSeparationDistance)) // Doesn't use swept tests.  Just return binary if objects overlap
                {
                    Tstep_to_colision -= bisectionTimeRemaining; // we went too far and now the objects overlap, add the bisection time back
                    bisectionTimeRemaining /= 2.0f;
                    Tstep_to_colision += bisectionTimeRemaining; // Try a half step
                }
                else 
                {
                    if (minSeparationDistance < BISECTION_TOLLERANCE) // We've gone far enough
                        break;
                    if (Tstep_to_colision >= Time_remaining) // We've gone further than the current time step
                    {
                        ClearCollisions();
                        Tstep_to_colision = Time_remaining;
                        break;
                    }
                    Tstep_to_colision += bisectionTimeRemaining;  // We didn't go far enough, add time
                }
                curIteration ++;
            }

            if (Tstep_to_colision < Time_remaining) // The intial list of collisions might have been wrong --> rebuild it
            {
                curIteration = 1;
                ClearCollisions(); firstCollision = null;
                float additionalTime = (Time_remaining - Tstep_to_colision) / 10.0f;
                while (firstCollision == null)
                {
                    if (curIteration > BISECTION_MAXITERATIONS)
                        throw new Exception("physicsManager::StepToCollisionTimeByBisection() - Could not find a new collision in " +
                                            String.Format("{0}", BISECTION_MAXITERATIONS) + " iterations.");
                    // Keep adding back small incruments to the bisectionTimeRemaining until the objects collide again
                    TakeRK4Step(time, Tstep_to_colision + (curIteration * additionalTime), h_game.GetGameObjectManager().h_GameObjects);
                    firstCollision = CollisionDetection(); // Full collision detection

                    curIteration++;
                }
            }

            return Tstep_to_colision;
        }
        #endregion

        #region StaticCollisionDetection()
        /// StaticCollisionDetection() - Top level collision detection routine
        /// ***********************************************************************
        protected bool StaticCollisionDetection(ref float minSeparationDistance)
        {
            // Coarse Collision detection
            // CoarseCollisionDetection(); --> DON'T RE-RUN THE COARSE COLLISION DETECTION - USE THE OBJECTS CACHED FROM BEFORE

            // Fine Collision detection
            return StaticFineCollisionDetection(ref minSeparationDistance);
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
                    // physicsManager.ClipVelocity(ref curObject.state.linearVel, h_game.GetGameSettings().physicsMinVel, curObject.maxVel);
                }
            }

            // We've moved the objects, so the AABB data is no longer valid --> Flag it
            h_game.GetGameObjectManager().SetDirtyBoundingBoxes(); 
        }
        #endregion

        #region ResolveCollisions()
        /// ResolveCollisions() - Add impulse for collision contacts, and add contact forces for contact collisions
        /// ***********************************************************************
        protected void ResolveCollisions(float time, float deltaTime, List<gameObject> gameObjects)
        {
            // Make the previous state equal to the time of collision
            CopyStateToPrevState(time, gameObjects); // Collision point velocities will be derived from "prevState" and added to "state"

            // Step through each collision and only resolve those collisions that happened from time to (time + deltaTime)
            // All other contacts will be resolved on later iterations of the physics loop
            // This ensures that if two objects collide at EXACTLY the same time they will both be resolved --> Rare!
            for (int i = 0; i < collisions.Count; i++)
                if (collisions[i].colTime <= deltaTime)
                {
                    if (collisions[i].ResolveCollision(time, deltaTime, gameObjects)) // if resolve collisions return true --> resting contact.
                        restingContacts.Add(collisions[i]);
                }

            // This is a hack --> Just turns off gravity for those objects that have collided with the floor
            ComputeContactForces();

            // Make the previous state equal to the time of collision again to copy over the impulse's added by the collisions
            CopyStateToPrevState(time, gameObjects); // Collision point velocities will be derived from "prevState" and added to "state"
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
            int STARTING_VECTOR_SIZE = h_game.GetGameSettings().physicsObjectsStartingCapacity;

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
                if (h_game.GetGameObjectManager().h_GameObjects[i].collidable)
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
            h_game.GetGameObjectManager().UpdateCoarseBoundingBoxes();

            // Do an insertion sort on each axis list to order objects, by minimum BB vertex
            // Insertion sort is O(n) when almost sorted, therefore best for slowly moving objects
            InsertionSortAABBs(ref AABBXaxis, 
                               ref h_game.GetGameObjectManager().h_GameObjects, 
                               numCollidableObjects, 
                               new FUNC_AXISSELECT(xAxisMinSel));
            InsertionSortAABBs(ref AABBYaxis, 
                               ref h_game.GetGameObjectManager().h_GameObjects,
                               numCollidableObjects, 
                               new FUNC_AXISSELECT(yAxisMinSel));
            InsertionSortAABBs(ref AABBZaxis, 
                               ref h_game.GetGameObjectManager().h_GameObjects,
                               numCollidableObjects, 
                               new FUNC_AXISSELECT(zAxisMinSel));

            // Now Find all overlaps by doing sweep of each axis lists.
            SweepAxisList(ref AABBXaxis, 
                          ref h_game.GetGameObjectManager().h_GameObjects,
                          numCollidableObjects,
                          numObjects,
                          new FUNC_AXISSELECT(xAxisMinSel),
                          new FUNC_AXISSELECT(xAxisMaxSel),
                          new FUNC_STATUSSELECT(SetOverlapStatusXaxis));
            SweepAxisList(ref AABBYaxis,
                          ref h_game.GetGameObjectManager().h_GameObjects,
                          numCollidableObjects,
                          numObjects,
                          new FUNC_AXISSELECT(yAxisMinSel),
                          new FUNC_AXISSELECT(yAxisMaxSel),
                          new FUNC_STATUSSELECT(SetOverlapStatusYaxis));
            SweepAxisList(ref AABBZaxis,
                          ref h_game.GetGameObjectManager().h_GameObjects,
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
        }
        #endregion

        #region FineCollisionDetection()
        /// FineCollisionDetection() --> Go through the coarse collision routine results and get run fine collision detection
        /// I thought O(n^2) through array was dumb, but Christer Ericson does the same thing (Real Time Collision Detection, page 329-338)
        /// We're going to all the trouble to get O(n) in sweap and prune, then we just do an O(n^2) loop
        /// ***********************************************************************
        protected void FineCollisionDetection()
        {
            ClearCollisions();

            // Loop through object pairs and check if they potentially overlap from the sweep and prune test
            for(int i = 0; i < (numObjects-1); i ++)
			{
				for(int j = i+1; j < (numObjects); j ++)
				{
                    if (h_game.GetGameObjectManager().h_GameObjects[i].collidable && h_game.GetGameObjectManager().h_GameObjects[j].collidable)
                    {
                        AABBOverlap curOverlap = AABBOverlap.GetOverlapStatus(ref AABBOverlapStatus, i, j, numObjects);
                        if (curOverlap.xAxisOverlap && curOverlap.yAxisOverlap && curOverlap.zAxisOverlap)
                        {
                            // Objects potentially overlap --> Check the low level collision routines
                            collisionUtils.TestCollision(h_game.GetGameObjectManager().h_GameObjects[i],
                                                         h_game.GetGameObjectManager().h_GameObjects[j],
                                                         ref collisions);
                        }
                    }
				}
			}
        }
        #endregion

        #region StaticFineCollisionDetection()
        /// StaticFineCollisionDetection() --> Don't do swept tests, just to quick bool operations
        /// ***********************************************************************
        protected bool StaticFineCollisionDetection(ref float minSeparationDistance)
        {
            bool colDetected = false;
            float curSeparationDistance = float.PositiveInfinity;
            // Loop through object pairs and check if they potentially overlap from the sweep and prune test
            for (int i = 0; i < (numObjects - 1); i++)
            {
                for (int j = i + 1; j < (numObjects); j++)
                {
                    if (h_game.GetGameObjectManager().h_GameObjects[i].collidable && h_game.GetGameObjectManager().h_GameObjects[j].collidable)
                    {
                        AABBOverlap curOverlap = AABBOverlap.GetOverlapStatus(ref AABBOverlapStatus, i, j, numObjects);
                        if (curOverlap.xAxisOverlap && curOverlap.yAxisOverlap && curOverlap.zAxisOverlap)
                        {
                            // Objects potentially overlap --> Check the low level collision routines
                            colDetected = collisionUtils.TestStaticCollision(h_game.GetGameObjectManager().h_GameObjects[i],
                                                                             h_game.GetGameObjectManager().h_GameObjects[j],
                                                                             ref curSeparationDistance) ? true : colDetected;
                            {
                                if(minSeparationDistance > curSeparationDistance)
                                    minSeparationDistance = curSeparationDistance;
                            }
                        }
                    }
                }
            }

            return colDetected;
        }
        #endregion

        #region GetFirstCollision()
        /// GetFirstCollision() --> Return the collison that happens first
        /// ***********************************************************************
        protected collision GetFirstCollision()
        {
            if( collisions.Count < 1)
                return null; // No collisions detected, return null

            // Otherwise, linear search to find the first collison
            collision firstCol = collisions[0];
            for (int i = 1; i < collisions.Count; i++)
                if (collisions[i].colTime < firstCol.colTime)
                    firstCol = collisions[i];

            return firstCol;
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
                    ((gameObject)restingContacts[i].obj1).movable )
                    ((gameObject)restingContacts[i].obj1).AddAntiGravityForce(h_game.GetGameSettings().gravity);
                if (!((gameObject)restingContacts[i].obj2).CheckAntiGravityForce() &&
                    ((gameObject)restingContacts[i].obj2).movable)
                    ((gameObject)restingContacts[i].obj2).AddAntiGravityForce(h_game.GetGameSettings().gravity);
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
                collisionUtils.TestStaticCollision((gameObject)restingContacts[i].obj1, 
                                                   (gameObject)restingContacts[i].obj2, ref separationDistance);
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
                            ((gameObject)restingContacts[i].obj1).RemoveAntiGravityForce();
                    }

                    if (((gameObject)restingContacts[i].obj2).collidable)
                    {
                        // Iterate through the rest of the restingContacts list and see if we can find another resing contact with the object
                        anotherContactExists = false;
                        for (int j = i + 1; j < restingContacts.Count; j++)
                            if (restingContacts[j].obj1.Equals(restingContacts[i].obj2) || restingContacts[j].obj2.Equals(restingContacts[i].obj2))
                                anotherContactExists = true;

                        if (!anotherContactExists)
                            ((gameObject)restingContacts[i].obj2).RemoveAntiGravityForce();
                    }

                    // Remove the resting contact from the list
                    restingContacts.RemoveAt(i);

                }

            }
        }
        #endregion
    }

}
