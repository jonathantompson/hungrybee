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
    /// **                          gameObjectManager                        **
    /// ** A class to store and manage  the game objects, including loading  **
    /// ** DataDriven content from disk                                      **
    /// ***********************************************************************
    /// </summary>
    public class gameObjectManager : GameComponent
    {
        #region Local Variables

        game h_game;
        public List<gameObject> h_GameObjects;      // Handler to the list of game objects

        int numPlayers, numHeightMaps, numEnemys;

        #endregion

        #region Constructor - gameObjectManager(game game)
        /// Initializes to default values
        /// ***********************************************************************
        public gameObjectManager(game game) : base(game)  
        {
            h_game = (game)game;
            h_GameObjects = new List<gameObject>();
            numPlayers = 0; numHeightMaps = 0; numEnemys = 0;
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

        #region Update() - Update accelerations for physicsManager
        /// Perform initialization - Nothing to initialize
        /// ***********************************************************************
        public override void Update(GameTime gameTime)
        {
            // enumerate through each element in the list and update them
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                ListEnum.Current.Update(gameTime);
            }
            base.Update(gameTime);
        }
        #endregion

        #region ChangeEffectUsedByModels()
        /// Perform initialization - Nothing to initialize
        /// ***********************************************************************
        public void ChangeEffectUsedByModels(Effect replacementEffect)
        {
            // some help

            // enumerate through each model element in the loaded content and change its effects
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                try
                {
                    ListEnum.Current.ChangeEffectUsedByModel(replacementEffect);
                }
                catch
                {
                    // I expect to catch an exception here if we're tyring to change the model effects twice
                    // This happens when two game objects share the same model content
                    // This is a shitty solution --> But only performed once on startup
                }
            }
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Reserve List capacity and load in the first level
        /// ***********************************************************************
        public void LoadContent()
        {
            h_GameObjects.Capacity = h_game.GetGameSettings().startingGameObjectCapacity;
            
            // Load in the object descriptions from the csv file for the first level
            LoadLevel(1);

            // Now initialize the physics manager content
            h_game.GetPhysicsManager().LoadContent();
        }
        #endregion

        #region LoadLevel()
        /// LoadContent - Load in the data-driver objects for the specified level
        /// ***********************************************************************
        void LoadLevel(int levelNumber)
        {
            csvHandleRead reader = new csvHandleRead(".\\Level_" + String.Format("{0}", levelNumber) + ".csv");
            if (!reader.IsOpen())
                throw new Exception("gameObjectManager::LoadContent(): Cannot find file Level_" + String.Format("{0}", levelNumber) + ".csv");

            List<string> curToken = new List<string>();
            gameObject curObject;
            while (reader.ReadNextToken(ref curToken)) // ReadNextToken returns false when nothing to read
            {
                switch (curToken[0])
                {
                    case "player":
                        if (numPlayers == 0 && curToken.Count == 7 )
                        {
                            curObject = new gameObjectPlayer(h_game, 
                                                             curToken[1],
                                                             GetBoundingObjTypeFromString(curToken[2]),
                                                             float.Parse(curToken[3]),
                                                             float.Parse(curToken[4]),
                                                             float.Parse(curToken[5]),
                                                             float.Parse(curToken[6]));
                            numPlayers += 1;
                        }
                        else
                            throw new Exception("gameObjectManager::LoadContent(): Error reading player settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
                        break;

                    case "heightMap":
                        if (numHeightMaps == 0 && curToken.Count == 10)
                        {
                            curObject = new gameObjectHeightMap(h_game,
                                                                bool.Parse(curToken[1]),
                                                                curToken[2],
                                                                curToken[3],
                                                                new Vector3(float.Parse(curToken[4]), float.Parse(curToken[5]), float.Parse(curToken[6])),
                                                                new Vector3(float.Parse(curToken[7]), float.Parse(curToken[8]), float.Parse(curToken[9])));
                            numHeightMaps += 1;
                        }
                        else
                            throw new Exception("gameObjectManager::LoadContent(): Error reading heightMap settings from Level_" + String.Format("{0}",levelNumber) + ".csv");
                        break;
                    case "enemy":
                        if (curToken.Count == 8)
                        {
                            curObject = new gameObjectEnemy(h_game,
                                                            curToken[1],
                                                            GetBoundingObjTypeFromString(curToken[2]),
                                                            float.Parse(curToken[3]),
                                                            float.Parse(curToken[4]),
                                                            new Vector3(float.Parse(curToken[5]), float.Parse(curToken[6]), float.Parse(curToken[7])));
                            numEnemys += 1;
                        }
                        else
                            throw new Exception("gameObjectManager::LoadContent(): Error reading enemy settings from Level_" + String.Format("{0}", levelNumber) + ".csv");
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

        #region SpanGameObjectPhysicsDebug()
        /// SpanGameObjectPhysicsDebug - create and initialize a debug object
        /// ***********************************************************************
        public gameObject SpawnGameObjectPhysicsDebug(gameObject inputObject)
        {
            gameObject newObj = new gameObjectPhysicsDebug(h_game,
                                                           inputObject.boundingObjType,
                                                           inputObject.boundingObj,
                                                           inputObject);
            newObj.LoadContent();
            return newObj;
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

        #region UpdateBoundingBoxes()
        /// UpdateBoundingBoxes - Update all the bounding boxes if they are dirty
        /// ***********************************************************************
        public void UpdateBoundingBoxes()
        {
            List<gameObject>.Enumerator ListEnum = h_GameObjects.GetEnumerator();
            while (ListEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                 ListEnum.Current.UpdateBoundingBox();
            }
        }
        #endregion
    }
}
