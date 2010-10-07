#region using statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    /// **                          gameObjectManager                        **
    /// ** A class to store and manage  the game objects, including loading  **
    /// ** DataDriven content from disk                                      **
    /// ***********************************************************************
    /// </summary>
    public class gameObjectManager : GameComponent
    {
        #region Local Variables

        game h_game;
        public int loadedLevel;
        public List<gameObject> h_GameObjects;      // Handler to the list of game objects
        public List<gameObject> h_GameObjectsRemoveList;

        public gameObject player;
        public List<gameObject> enemyList;
        public List<gameObject> friendList;

        int numPlayers, numHeightMaps, numEnemys, numPhantoms, numClouds, numFriends;
        static float frustrumBoundBoxThickness = 2.0f;
        static float frustrumBoundBoxDepth = 20.0f;
        static float EPSILON = 0.00001f;

        #endregion

        #region Constructor - gameObjectManager(game game)
        /// Initializes to default values
        /// ***********************************************************************
        public gameObjectManager(game game) : base(game)  
        {
            h_game = (game)game;
            h_GameObjects = new List<gameObject>();
            numPlayers = 0; numHeightMaps = 0; numEnemys = 0; numPhantoms = 0; numClouds = 0; numFriends = 0;
            h_GameObjectsRemoveList = new List<gameObject>();
            enemyList = new List<gameObject>();
            friendList = new List<gameObject>();
            loadedLevel = -1; // No level loaded
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

        #region Update() - Remove objects on h_GameObjectsRemoveList and Update() each object
        /// Update()
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            if (!h_game.h_Menu.menusRunning && !h_game.h_PhysicsManager.gamePaused)
            {
                // Remove all objects on the remove list
                ProcessRemovals();

                if (numFriends <= 0)
                    FinishLevel();
                else
                {
                    // enumerate through each element in the list and update them
                    List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
                    while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
                    {
                        ListEnum.Current.Update(gameTime);
                    }
                }
            }
            base.Update(gameTime);
        }
        #endregion

        #region ProcessRemovals()
        protected void ProcessRemovals()
        {
            // Remove all objects on the remove list
            for (int i = 0; i < h_GameObjectsRemoveList.Count; i++)
            {
                int j;

                // Find which index of the gameObjects list we want to remove
                int curIndex = -1;
                for (j = 0; i < h_GameObjects.Count; j++)
                    if (h_GameObjects[j].Equals(h_GameObjectsRemoveList[i]))
                    { curIndex = j; break; }

                // AABB insertion sort lists now contain an index that doesn't exist --> Fix it
                // Also remove any phatomContacts or restingContacts involving this object
                h_game.h_PhysicsManager.ProcessRemoval(curIndex);

                // Need to find the gameObjectPhysicsDebug that is tied to this gameObject if we're rendering it.
                if (h_game.h_GameSettings.renderBoundingObjects)
                    for (j = 0; j < h_GameObjects.Count; j++)
                        if (h_GameObjects[j] is gameObjectPhysicsDebug && ((gameObjectPhysicsDebug)h_GameObjects[j]).attachedGameObject.Equals(h_GameObjectsRemoveList[i]))
                        { h_GameObjects.RemoveAt(j); h_game.h_PhysicsManager.numObjects--; break; }

                // If the object is an enemy, remove it from the enemy list
                if (h_GameObjectsRemoveList[i] is gameObjectEnemy)
                {
                    for (j = 0; j < enemyList.Count; j++)
                        if (enemyList[j].Equals(h_GameObjectsRemoveList[i]))
                        { enemyList.RemoveAt(j); numEnemys--; break; }
                }
                // If the object is a friend, remove it from the friend list
                else if (h_GameObjectsRemoveList[i] is gameObjectFriend)
                {
                    for (j = 0; j < friendList.Count; j++)
                        if (friendList[j].Equals(h_GameObjectsRemoveList[i]))
                        { friendList.RemoveAt(j); numFriends--; break; }
                }
                else if (h_GameObjectsRemoveList[i] is gameObjectCloud)
                    numClouds--;
                else if (h_GameObjectsRemoveList[i] is gameObjectHeightMap)
                    numHeightMaps--;
                else if (h_GameObjectsRemoveList[i] is gameObjectPhantom)
                    numPhantoms--;
                else if (h_GameObjectsRemoveList[i] is gameObjectPlayer)
                    numPlayers--;

                // Remove the actual object
                h_GameObjects.Remove(h_GameObjectsRemoveList[i]);
                h_game.h_PhysicsManager.numObjects--;
                h_game.h_PhysicsManager.numCollidableObjects--;
            }
            h_GameObjectsRemoveList.Clear();
        }
        #endregion

        #region ChangeEffectUsedByModels()
        /// Perform initialization - Nothing to initialize
        /// ***********************************************************************
        public void ChangeEffectUsedByModels(Effect replacementEffect)
        {
            // enumerate through each model element in the loaded content and change its effects
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
                    ListEnum.Current.ChangeEffectUsedByModel(replacementEffect);
        }
        #endregion

        #region StartLevel(int level)
        /// StartLevel - Just start the new game
        /// ***********************************************************************
        public void StartLevel(int level)
        {
            // Load in the object descriptions from the csv file for the first level
            loadedLevel = LoadLevel(1);
            h_game.h_PhysicsManager.UnpauseGame();
        }  
        #endregion

        #region FinishLevel(int level)
        /// StartLevel - Just start the new game
        /// ***********************************************************************
        public void FinishLevel()
        {
            // See if another level exists and it does then load it...
            if (File.Exists(GetLevelFilename(loadedLevel + 1)))
            {
                // Load in the object descriptions from the csv file for the next level
                loadedLevel = LoadLevel(loadedLevel + 1);
                h_game.h_PhysicsManager.UnpauseGame();
            }
            else
            {
                ClearLevel();
                h_game.h_PhysicsManager.PauseGame();
                h_game.h_Menu.EnterMainMenu();
            }
        }
        #endregion

        #region GetLevelFilename(int levelNumber)
        protected string GetLevelFilename(int levelNumber)
        {
            return ".\\gameSettings\\Level_" + String.Format("{0}", levelNumber) + ".csv";
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Reserve List capacity and load in the first level
        /// ***********************************************************************
        public void LoadContent()
        {
            h_GameObjects.Capacity = h_game.h_GameSettings.startingGameObjectCapacity;
        }
        #endregion

        #region ClearLevel()
        /// ClearLevel - Empty all game objects and free up space
        /// ***********************************************************************
        protected void ClearLevel()
        {
            h_GameObjects.Clear();
            h_GameObjectsRemoveList.Clear();
            friendList.Clear();
            enemyList.Clear();
            player = null;
            numPlayers = 0; numHeightMaps = 0; numEnemys = 0; numPhantoms = 0; numClouds = 0; numFriends = 0;

            // Give the physics system a chance to clear itself
            h_game.h_PhysicsManager.ClearLevel();

            // Give the render system a chance to clear itself
            h_game.h_RenderManager.ClearLevel();

            loadedLevel = -1;
        }
        #endregion

        #region LoadLevel()
        /// LoadContent - Load in the data-driver objects for the specified level
        /// ***********************************************************************
        int LoadLevel(int levelNumber)
        {
            if (loadedLevel != -1)
                ClearLevel(); // If a level is already loaded, then clear it!

            csvHandleRead reader = new csvHandleRead(GetLevelFilename(levelNumber));
            if (!reader.IsOpen())
                throw new Exception("gameObjectManager::LoadContent(): Cannot find file Level_" + String.Format("{0}", levelNumber) + ".csv");

            List<string> curToken = new List<string>();
            gameObject curObject;
            while (reader.ReadNextToken(ref curToken)) // ReadNextToken returns false when nothing to read
            {
                switch (curToken[0])
                {
                    case "player":
                        curObject = SpawnPlayerFromToken(ref curToken, levelNumber);
                        break;
                    case "heightMap":
                        curObject = SpawnHeightMapFromToken(ref curToken, levelNumber);
                        break;
                    case "enemy":
                        curObject = SpawnEnemyFromToken(ref curToken, levelNumber);
                        break;
                    case "phantom":
                        curObject = SpawnPhantomFromToken(ref curToken, levelNumber);
                        break;
                    case "cloud":
                        curObject = SpawnCloudFromToken(ref curToken, levelNumber);
                        break;
                    case "friend":
                        curObject = SpawnFriendFromToken(ref curToken, levelNumber);
                        break;
                    case "//": // Comment
                        curObject = null;
                        break;
                    case "": // Empty line
                        curObject = null;
                        break;

                    //// ************************************
                    //// *** 3. INSERT OBJECT TYPES HERE ****
                    //// ************************************

                    default:
                        throw new Exception("gameObjectManager::LoadContent(): Corrupt Level_x.csv, setting " + curToken[0] + " is not recognised");
                }

                if (curObject != null)
                {
                    // Append the newly created object to the list
                    h_GameObjects.Add(curObject);
                    // Load the content of the current object
                    curObject.LoadContent();
                }

            }
            reader.Close(); // Destructor would do this anyway once out of scope, but just to be safe.

            // If we're limiting XY collision Responces, then sphere's must be placed so that the sphere origin has Z=0
            if (h_game.h_GameSettings.limitXYCollisionResponce)
                for (int i = 0; i < h_GameObjects.Count; i++)
                    if (h_GameObjects[i].boundingObjType == boundingObjType.SPHERE)
                        h_GameObjects[i].CenterObjectAboutBoundingSphere();


            // Build the bounding boxes at the frustrum bounds.
            BuildFrustrumBounds();

            if (h_game.h_GameSettings.renderBoundingObjects) // Add bounding objects to be rendered if we want
                SpawnCollidables();

            // Now initialize the physicsManager
            h_game.h_PhysicsManager.LoadContent();

            return levelNumber;
        }
        #endregion

        #region SpawnPlayerFromToken()
        protected gameObject SpawnPlayerFromToken(ref List<string> curToken, int levelNumber)
        {
            gameObject retObject = null;
            if (numPlayers == 0 && curToken.Count == 9)
            {
                // public gameObjectPlayer(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, float _scale, Vector3 _pos)
                retObject = new gameObjectPlayer(h_game,
                                                 curToken[1],
                                                 GetBoundingObjTypeFromString(curToken[2]),
                                                 int.Parse(curToken[3]) == 1, int.Parse(curToken[4]) == 1,
                                                 float.Parse(curToken[5]),
                                                 new Vector3(float.Parse(curToken[6]), float.Parse(curToken[7]), float.Parse(curToken[8])));
                numPlayers += 1;
                player = retObject;
            }
            else
                throw new Exception("gameObjectManager::LoadContent(): Error reading player settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
            return retObject;
        }
        #endregion

        #region SpawnHeightMapFromToken()
        protected gameObject SpawnHeightMapFromToken(ref List<string> curToken, int levelNumber)
        {
            gameObject retObject = null;
            if (numHeightMaps == 0 && curToken.Count == 10)
            {
                retObject = new gameObjectHeightMap(h_game,
                                                    bool.Parse(curToken[1]),
                                                    curToken[2],
                                                    curToken[3],
                                                    new Vector3(float.Parse(curToken[4]), float.Parse(curToken[5]), float.Parse(curToken[6])),
                                                    new Vector3(float.Parse(curToken[7]), float.Parse(curToken[8]), float.Parse(curToken[9])));
                numHeightMaps += 1;
            }
            else
                throw new Exception("gameObjectManager::LoadContent(): Error reading heightMap settings from Level_" + String.Format("{0}", levelNumber) + ".csv"); 
            return retObject;
        }
        #endregion

        #region SpawnEnemyFromToken()
        protected gameObject SpawnEnemyFromToken(ref List<string> curToken, int levelNumber)
        {
            gameObject retObject = null;
            if (curToken.Count == 12)
            {
                // public gameObjectEnemy(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, float _scale, Vector3 startingPos, Vector3 startingMom)
                retObject = new gameObjectEnemy(h_game,
                                                curToken[1],
                                                GetBoundingObjTypeFromString(curToken[2]),
                                                int.Parse(curToken[3]) == 1, int.Parse(curToken[4]) == 1,
                                                float.Parse(curToken[5]),
                                                new Vector3(float.Parse(curToken[6]), float.Parse(curToken[7]), float.Parse(curToken[8])),
                                                new Vector3(float.Parse(curToken[9]), float.Parse(curToken[10]), float.Parse(curToken[11])));
                numEnemys += 1;
                enemyList.Add(retObject);
            }
            else
                throw new Exception("gameObjectManager::LoadContent(): Error reading enemy settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
            return retObject;
        }
        #endregion

        #region SpawnFriendFromToken()
        protected gameObject SpawnFriendFromToken(ref List<string> curToken, int levelNumber)
        {
            gameObject retObject = null;
            if (curToken.Count == 12)
            {
                // public gameObjectFriend(game game, string modelfile, boundingObjType _objType, bool textureEnabled, bool vertexColorEnabled, float _scale, Vector3 startingPos, Vector3 startingMom)
                retObject = new gameObjectFriend(h_game,
                                                curToken[1],
                                                GetBoundingObjTypeFromString(curToken[2]),
                                                int.Parse(curToken[3]) == 1, int.Parse(curToken[4]) == 1,
                                                float.Parse(curToken[5]),
                                                new Vector3(float.Parse(curToken[6]), float.Parse(curToken[7]), float.Parse(curToken[8])),
                                                new Vector3(float.Parse(curToken[9]), float.Parse(curToken[10]), float.Parse(curToken[11])));
                numFriends += 1;
                friendList.Add(retObject);
            }
            else
                throw new Exception("gameObjectManager::LoadContent(): Error reading friend settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
            return retObject;
        }
        #endregion

        #region SpawnPhantomFromToken()
        protected gameObject SpawnPhantomFromToken(ref List<string> curToken, int levelNumber)
        {
            gameObject retObject = null;
            bool error = false;
            if (curToken.Count > 2)
            {
                if (curToken[1] == "AABB")
                {
                    if ((curToken.Count == 9 && curToken[8] == "HARD_BOUNDRY"))
                        retObject = new gameObjectPhantom(h_game, boundingObjType.AABB,
                                                          (Object)new BoundingBox(new Vector3(float.Parse(curToken[2]), float.Parse(curToken[3]), float.Parse(curToken[4])),
                                                                                  new Vector3(float.Parse(curToken[5]), float.Parse(curToken[6]), float.Parse(curToken[7]))));
                    else if ((curToken.Count == 14 && curToken[8] == "SOFT_BOUNDRY"))
                        retObject = new gameObjectPhantom(h_game, boundingObjType.AABB,
                                                          (Object)new BoundingBox(new Vector3(float.Parse(curToken[2]), float.Parse(curToken[3]), float.Parse(curToken[4])),
                                                                                  new Vector3(float.Parse(curToken[5]), float.Parse(curToken[6]), float.Parse(curToken[7]))),
                                                          new Vector3(float.Parse(curToken[9]), float.Parse(curToken[10]), float.Parse(curToken[11])),
                                                          int.Parse(curToken[12]) == 1, int.Parse(curToken[13]) == 1);
                    else
                        error = true;
                }
                else if (curToken[1] == "SPHERE")
                {
                    if ((curToken.Count == 7 && curToken[6] == "HARD_BOUNDRY"))
                        retObject = new gameObjectPhantom(h_game, boundingObjType.SPHERE,
                                                          (Object)new BoundingSphere(new Vector3(float.Parse(curToken[2]), float.Parse(curToken[3]), float.Parse(curToken[4])),
                                                                                     float.Parse(curToken[5])));
                    else if ((curToken.Count == 12 && curToken[6] == "SOFT_BOUNDRY"))
                        retObject = new gameObjectPhantom(h_game, boundingObjType.AABB,
                                                          (Object)new BoundingSphere(new Vector3(float.Parse(curToken[2]), float.Parse(curToken[3]), float.Parse(curToken[4])),
                                                                                     float.Parse(curToken[5])),
                                                          new Vector3(float.Parse(curToken[7]), float.Parse(curToken[8]), float.Parse(curToken[9])),
                                                          int.Parse(curToken[10]) == 1, int.Parse(curToken[11]) == 1);
                    else
                        error = true;
                }
                else
                    throw new Exception("gameObjectManager::LoadContent(): Error reading phantom settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
                numPhantoms += 1;
            }
            else
                error = true;

            if(error)
                throw new Exception("gameObjectManager::LoadContent(): Error reading phantom settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
            else
                return retObject;
        }
        #endregion

        #region SpawnCloudFromToken()
        protected gameObject SpawnCloudFromToken(ref List<string> curToken, int levelNumber)
        {
            gameObject retObject = null;
            if (curToken.Count == 9)
            {
                retObject = new gameObjectCloud(h_game,
                                                int.Parse(curToken[1]),
                                                int.Parse(curToken[2]),
                                                new Vector3(float.Parse(curToken[3]), float.Parse(curToken[4]), float.Parse(curToken[5])),
                                                new Vector3(float.Parse(curToken[6]), float.Parse(curToken[7]), float.Parse(curToken[8])));
                numClouds += 1;
            }
            else
                throw new Exception("gameObjectManager::LoadContent(): Error reading cloud settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
            return retObject;
        }
        #endregion

        #region BuildFrustrumBounds()
        /// SpanGameObjectPhysicsDebug - create phantoms at the extents of the bounding frustrum
        /// ***********************************************************************
        protected void BuildFrustrumBounds()
        {
            gameObjectHeightMap heightMap = GetHeightMap();
            if (heightMap == null)
                throw new Exception("gameObjectManager::BuildFrustrumBounds() - Could not find a heightmap!  Check Level_X.csv");

            // Build a frustrum from the camera values and get the corners
            camera camera = h_game.h_Camera;
            BoundingFrustum frustrum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
            // The Plane structure defines a plane by specifying it's normal vector and its NEGATIVE DISTANCE D TO THE ORIGIN
            // ALONG THE DIRECTION OF IT'S NORMAL VECTOR

            // Equation of a plane: n . (r - r0) = 0 --> n_x(x - x_0) + n_y(y - y_0) + n_z(z - z_0) = 0
            // want to solve plane equation for:
            // Left plane:     y = 0, z = 0, x = ? --> x = (n_y*y_0 + n_z*z_0) / n_x + x_0
            // Right plane:    y = 0, z = 0, x = ? --> x = (n_y*y_0 + n_z*z_0) / n_x + x_0
            // Top plane:      y = ?, z = 0, x = 0 --> y = (n_x*x_0 + n_z*z_0) / n_y + y_0
            // Bottom plane:   y = ?, z = 0, x = 0 --> y = (n_x*x_0 + n_z*z_0) / n_y + y_0

            Vector3 pointOnPlane;
            Plane plane;

            // Find the bound coordinates above
            plane = frustrum.Left;
            pointOnPlane = plane.Normal * (-1.0f) * plane.D;
            float leftCoord = (plane.Normal.Y * pointOnPlane.Y + plane.Normal.Z * pointOnPlane.Z) / plane.Normal.X + pointOnPlane.X;

            plane = frustrum.Top;
            pointOnPlane = plane.Normal * (-1.0f) * plane.D;
            float topCoord = (plane.Normal.X * pointOnPlane.X + plane.Normal.Z * pointOnPlane.Z) / plane.Normal.Y + pointOnPlane.Y;

            plane = frustrum.Right;
            pointOnPlane = plane.Normal * (-1.0f) * plane.D;
            float rightCoord = (plane.Normal.Y * pointOnPlane.Y + plane.Normal.Z * pointOnPlane.Z) / plane.Normal.X + pointOnPlane.X;

            plane = frustrum.Bottom;
            pointOnPlane = plane.Normal * (-1.0f) * plane.D;
            float bottomCoord = (plane.Normal.Y * pointOnPlane.Y + plane.Normal.Z * pointOnPlane.Z) / plane.Normal.X + pointOnPlane.X;

            // Bottom coordinate is either set by the heightMap OR the bottom frustrum plane
            heightMap.UpdateCoarseBoundingBox();
            bottomCoord = Math.Max(heightMap.AABB_max.Y, bottomCoord);

            if (bottomCoord > topCoord)
                throw new Exception("gameObjectManager::BuildFrustrumBounds() - bottom coord is above top coord: maybe heightmap is too tall and is off screen?");

            BoundingBox bAABB;
            gameObject gameObj;

            // Add the left bounding box
            bAABB = new BoundingBox(new Vector3(leftCoord - frustrumBoundBoxThickness, bottomCoord + EPSILON, frustrumBoundBoxDepth * -0.5f),
                                    new Vector3(leftCoord - EPSILON, topCoord, frustrumBoundBoxDepth * +0.5f));
            gameObj = new gameObjectPhantom(h_game, boundingObjType.AABB, bAABB);
            // Append the newly created object to the list
            h_GameObjects.Add(gameObj);
            // Load the content of the current object
            gameObj.LoadContent();

            // Add the Right bounding box
            bAABB = new BoundingBox(new Vector3(rightCoord + EPSILON, bottomCoord + EPSILON, frustrumBoundBoxDepth * -0.5f),
                                    new Vector3(rightCoord + frustrumBoundBoxThickness, topCoord, frustrumBoundBoxDepth * +0.5f));
            gameObj = new gameObjectPhantom(h_game, boundingObjType.AABB, bAABB);
            // Append the newly created object to the list
            h_GameObjects.Add(gameObj);
            // Load the content of the current object
            gameObj.LoadContent();

            // Add the Top bounding box
            bAABB = new BoundingBox(new Vector3(leftCoord, topCoord, frustrumBoundBoxDepth * -0.5f),
                                    new Vector3(rightCoord, topCoord + frustrumBoundBoxThickness, frustrumBoundBoxDepth * +0.5f));
            gameObj = new gameObjectPhantom(h_game, boundingObjType.AABB, bAABB);
            // Append the newly created object to the list
            h_GameObjects.Add(gameObj);
            // Load the content of the current object
            gameObj.LoadContent();


        }
        #endregion

        #region GetHeightMap()
        /// GetHeightMap - The LoadLevel function ensures there is only one heightmap.  Iterate through to find it.
        /// ***********************************************************************
        public gameObjectHeightMap GetHeightMap()
        {
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                if (ListEnum.Current is gameObjectHeightMap)
                    return (gameObjectHeightMap)ListEnum.Current;
            }
            return null;
        }
        #endregion

        #region DrawModels()
        /// DrawModels - For each drawableGameObject draw the models
        /// ***********************************************************************
        public void DrawModels(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // Just enumerate through each element in the list and draw them
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                ListEnum.Current.DrawUsingCurrentEffect(gameTime, device, view, projection, effectTechniqueName);
            }
        }
        #endregion

        #region UnloadContent()
        /// UnloadContent - Nothing to do
        /// ***********************************************************************
        public void UnloadContent()
        {
        }
        #endregion

        #region SpanGameObjectPhysicsDebug(gameObject inputObject)
        /// SpanGameObjectPhysicsDebug - create and initialize a debug object
        /// ***********************************************************************
        public gameObject SpawnGameObjectPhysicsDebug(gameObject inputObject)
        {
            gameObject newObj = new gameObjectPhysicsDebug(h_game,
                                                           inputObject.boundingObjType,
                                                           inputObject.boundingObj,
                                                           inputObject, Color.White);
            newObj.LoadContent();
            return newObj;
        }
        #endregion

        #region SpanGameObjectPhysicsDebug(collision inputObject)
        /// SpanGameObjectPhysicsDebug - create and initialize a debug object
        /// ***********************************************************************
        public gameObject SpawnGameObjectPhysicsDebug(collision inputObject)
        {
            // Just make a game object with position equal to the collision and a bounding Sphere with radius 0.1
            gameObject gameObj = new gameObject(h_game);
            gameObj.boundingObj = (Object)new BoundingSphere(Vector3.Zero, 0.1f);
            gameObj.boundingObjType = boundingObjType.SPHERE;
            gameObj.state.pos = inputObject.colPoint; // This is all we care about

            // Make sure both starting states are equal
            rboState.CopyAtoB(ref gameObj.state, ref gameObj.prevState);

            Color color;
            switch (inputObject.colType)
            {
                case collisionType.VERTEX_FACE:
                    color = Color.Blue;
                    break;
                case collisionType.EDGE_EDGE:
                    color = Color.Red;
                    break;
                default:
                    throw new Exception("gameObjectManager::SpawnGameObjectPhysicsDebug(collision inputObject) - collision type unrecognised or is COL_UNDEFINED");
            }

            gameObject retObj = new gameObjectPhysicsDebug(h_game,
                                                           gameObj.boundingObjType,
                                                           gameObj.boundingObj,
                                                           gameObj,
                                                           color);
            retObj.LoadContent();
            return retObj;
        }
        #endregion

        #region SpawnCollidables()
        /// SpawnCollidables - create and initialize all debug objects
        /// Will create multiple objects if called multiple times --> ONLY CALL ONCE
        /// ***********************************************************************
        public void SpawnCollidables()
        {
            // Just enumerate through each element in the list and spawn a debug object to a new list
            // Cannot enumerate and add to the list at the same time (list must be constant when using enumerators)
            List<gameObject> newGameObjects = new List<gameObject>();
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                if(ListEnum.Current.boundingObjType != boundingObjType.UNDEFINED)
                    newGameObjects.Add(SpawnGameObjectPhysicsDebug(ListEnum.Current));
            }

            // Now merge the lists
            ListEnum = newGameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                h_GameObjects.Add(ListEnum.Current);
            }
        }
        #endregion

        #region SpawnCollisions()
        /// SpawnCollisions - create and initialize a debug object for each collision
        /// ***********************************************************************
        public void SpawnCollisions(ref List<collision> collisions)
        {
            for (int i = 0; i < collisions.Count; i++)
                h_GameObjects.Add(SpawnGameObjectPhysicsDebug(collisions[i]));
        }
        #endregion

        #region GetBoundingObjTypeFromString()
        protected static boundingObjType GetBoundingObjTypeFromString(string token)
        {
            boundingObjType objType = boundingObjType.UNDEFINED;
            if (token == "AUTO")
                objType = boundingObjType.UNDEFINED;
            else if (token == "SPHERE")
                objType = boundingObjType.SPHERE;
            else if (token == "AABB")
                objType = boundingObjType.AABB;
            else
                throw new Exception("gameObjectManager::GetBoundingObjTypeFromString() - Unrecognized obj type " + token);
            return objType;
        }
        #endregion

        #region SetDirtyBoundingBoxes()
        /// SetDirtyBoundingBoxes - Set all the AABB boxes to be marked as dirty and need of updating
        /// ***********************************************************************
        public void SetDirtyBoundingBoxes()
        {
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                ListEnum.Current.SetDirtyAABB();
            }
        }
        #endregion

        #region UpdateCoarseBoundingBoxes()
        /// UpdateCoarseBoundingBoxes - Update all the bounding boxes if they are dirty ONLY FOR COLLIDABLE OBJECTS
        /// ***********************************************************************
        public void UpdateCoarseBoundingBoxes()
        {
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                if(ListEnum.Current.collidable)
                    ListEnum.Current.UpdateCoarseBoundingBox();
            }
        }
        #endregion

        #region GetNumberCollidableObjects()
        /// GetNumberCollidableObjects - Linearly search through array and find out how many objects are collidable
        /// ***********************************************************************
        public int GetNumberCollidableObjects()
        {
            int retVal = 0;
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                if (ListEnum.Current.collidable)
                    retVal += 1;
            }
            return retVal;
        }
        #endregion

        #region GetNumberMovableObjects()
        /// GetNumberMovableObjects - Linearly search through array and find out how many objects are collidable
        /// ***********************************************************************
        public int GetNumberMovableObjects()
        {
            int retVal = 0;
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                if (ListEnum.Current.movable)
                    retVal += 1;
            }
            return retVal;
        }
        #endregion
    }
}
