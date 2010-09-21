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
using ExtensionMethods;
#endregion

namespace hungrybee
{

    /// <summary>
    /// **********************************************************************************
    /// **                          collisionUtils                                      **
    /// ** Static library of collision detection routines.  Some code here isn't        ** 
    /// ** mine.  Particularly the lower-level bounding volume intersection tests. The  **
    /// ** sources that were used are:                                                  **
    /// ** 1. David Eberly's Geometric Tools (Sphere Vs. Box and Box Vs. Box)           **
    /// ** Gamasutra article: Simple Intersection Tests For Games (Sphere Vs. Sphere)   **
    /// ** ALL TESTS ARE SWEPT VOLUME TESTS.  There should be NO tunnelling between     **
    /// ** frames & for each collision a point and a normalized time [0,1] is returned. **
    /// **********************************************************************************
    /// </summary>
    class collisionUtils
    {
        // Temporary variables to avoid continuous allocation and deallocation on the heap
        static Matrix objAMat_t0 = new Matrix();
        static Matrix objBMat_t0 = new Matrix();
        static Matrix objBMatInt_t0 = new Matrix();
        static Matrix objBMatInt_t1 = new Matrix();
        static Matrix objAMat_t1 = new Matrix();
        static Matrix objBMat_t1 = new Matrix();
        static Vector3 objACenter_t0 = new Vector3();
        static Vector3 objBCenter_t0 = new Vector3();
        static Vector3 objACenter_t1 = new Vector3();
        static Vector3 objBCenter_t1 = new Vector3();
        static Vector3[] aabbverticies = new Vector3[8];
        static Vector3 objAMin_t0 = new Vector3();
        static Vector3 objAMax_t0 = new Vector3();
        static Vector3 objBMin_t0 = new Vector3();
        static Vector3 objBMax_t0 = new Vector3();
        static Vector3 objAMin_t1 = new Vector3();
        static Vector3 objAMax_t1 = new Vector3();
        static Vector3 objBMin_t1 = new Vector3();
        static Vector3 objBMax_t1 = new Vector3();
        static float[] Ea = new float[3];
        static float[] Eb = new float[3];

        static float objARadius_t0 = 0.0f;
        static float objBRadius_t0 = 0.0f;
        static float objARadius_t1 = 0.0f;
        static float objBRadius_t1 = 0.0f;

        static float EPSILON = 0.000000000001f;
        static float coeffRestitution = 0.8f;

        #region TestCollision()
        /// TestCollision() - Just grab input objects and send them to their proper functions
        /// ***********************************************************************
        public static bool TestCollision(gameObject objA, gameObject objB, ref List<collision> _cols ) // normal defined for objA --> objB is just negative
        {
            // Big dumb if else chain --> But easiest way to quickly parse through input types
            // --> Probably data directed programming would be better here, but not too many types
            if (objA.boundingObjType == boundingObjType.UNDEFINED || objB.boundingObjType == boundingObjType.UNDEFINED)
                throw new Exception("collisionUtils::TestCollision(): Trying to test collision on UNDEFINED object types (maybe forgot to initialize objects?)");

            // Inputs are: SPHERES
            else if (objA.boundingObjType == boundingObjType.SPHERE && objB.boundingObjType == boundingObjType.SPHERE)
                return collisionUtils.TestCollisionSphereSphere(objA, objB, ref _cols);

            // Inputs are: SPHERE AND AABB
            else if (objA.boundingObjType == boundingObjType.SPHERE && objB.boundingObjType == boundingObjType.AABB)
                return collisionUtils.TestCollisionSphereAABB(objA, objB, ref _cols);

            else if (objA.boundingObjType == boundingObjType.AABB && objB.boundingObjType == boundingObjType.SPHERE)
                return collisionUtils.TestCollisionSphereAABB(objB, objA, ref _cols);

            // Inputs are: AABB
            else if (objA.boundingObjType == boundingObjType.AABB && objB.boundingObjType == boundingObjType.AABB)
                return collisionUtils.TestCollisionAABBAABB(objA, objB, ref _cols);

            else
                throw new Exception("collisionUtils::TestCollision(): Trying to test collision on unrecognized object types");
        }
        #endregion

        #region TestStaticCollision()
        /// TestStaticCollision() - Just grab input objects and send them to their proper functions
        /// ***********************************************************************
        public static bool TestStaticCollision(gameObject objA, gameObject objB, ref float separationDistance) // normal defined for objA --> objB is just negative
        {
            // Big dumb if else chain --> But easiest way to quickly parse through input types
            // --> Probably data directed programming would be better here, but not too many types
            if (objA.boundingObjType == boundingObjType.UNDEFINED || objB.boundingObjType == boundingObjType.UNDEFINED)
                throw new Exception("collisionUtils::TestStaticCollision(): Trying to test collision on UNDEFINED object types (maybe forgot to initialize objects?)");

            // Inputs are: SPHERES
            else if (objA.boundingObjType == boundingObjType.SPHERE && objB.boundingObjType == boundingObjType.SPHERE)
                return collisionUtils.TestCollisionSphereSphereStatic(objA, objB, ref separationDistance);

            // Inputs are: SPHERE AND AABB
            else if (objA.boundingObjType == boundingObjType.SPHERE && objB.boundingObjType == boundingObjType.AABB)
                return collisionUtils.TestCollisionSphereAABBStatic(objA, objB, ref separationDistance);

            else if (objA.boundingObjType == boundingObjType.AABB && objB.boundingObjType == boundingObjType.SPHERE)
                return collisionUtils.TestCollisionSphereAABBStatic(objB, objA, ref separationDistance);

            // Inputs are: AABB
            else if (objA.boundingObjType == boundingObjType.AABB && objB.boundingObjType == boundingObjType.AABB)
                return collisionUtils.TestCollisionAABBAABBStatic(objA, objB, ref separationDistance);

            else
                throw new Exception("collisionUtils::TestStaticCollision(): Trying to test collision on unrecognized object types");
        }
        #endregion

        #region TestCollisionAABBAABB()
        /// TestCollisionAABBAABB() - Test whether a swept AABB collides with another AABB between prevState and state
        /// A FEW ASSUMPTIONS:  1. Scale factor is constant between frames.
        ///                     2. No acceleration --> sphere and AABB are linearly swept between the two points with constant velocity
        /// Code derived from: http://www.gamasutra.com/view/feature/3383/simple_intersection_tests_for_games.php?page3
        /// ***********************************************************************
        protected static bool TestCollisionAABBAABB(gameObject objA, gameObject objB, ref List<collision> _cols ) // objA is a sphere, objB is an AABB
        {
            throw new Exception("AABB-AABB collisions are no longer supported.  Use AABB-Sphere or Sphere-Sphere");

            #region OLD CODE
            /*

            BoundingBox mBox1 = (BoundingBox)objA.boundingObj;
            BoundingBox mBox2 = (BoundingBox)objB.boundingObj;

            // Move the bounding boxes into world frame and update their size --> Boxes may grow
            objAMat_t0 = objA.CreateScale(objA.prevState.scale) * Matrix.CreateFromQuaternion(objA.prevState.orient) * Matrix.CreateTranslation(objA.prevState.pos);
            UpdateBoundingBox(mBox1, objAMat_t0, ref objAMin_t0, ref objAMax_t0);
            Vector3 velA = objA.state.pos - objA.prevState.pos;
            objACenter_t0 = 0.5f * (objAMax_t0 + objAMin_t0);

            objBMat_t0 = objB.CreateScale(objB.prevState.scale) * Matrix.CreateFromQuaternion(objB.prevState.orient) * Matrix.CreateTranslation(objB.prevState.pos);
            UpdateBoundingBox(mBox2, objBMat_t0, ref objBMin_t0, ref objBMax_t0);
            Vector3 velB = objB.state.pos - objB.prevState.pos;
            objBCenter_t0 = 0.5f * (objBMax_t0 + objBMin_t0);

            // Algorithm uses half-width extents, so get them:
            Ea[0] = 0.5f * (objAMax_t0.X - objAMin_t0.X);
            Ea[1] = 0.5f * (objAMax_t0.Y - objAMin_t0.Y);
            Ea[2] = 0.5f * (objAMax_t0.Z - objAMin_t0.Z);
            Eb[0] = 0.5f * (objBMax_t0.X - objBMin_t0.X);
            Eb[1] = 0.5f * (objBMax_t0.Y - objBMin_t0.Y);
            Eb[2] = 0.5f * (objBMax_t0.Z - objBMin_t0.Z);

            // Continue with the algorithm in: http://www.gamasutra.com/view/feature/3383/simple_intersection_tests_for_games.php?page3
            Vector3 v = velB - velA; // Revative velocity in normalized time

            //check if they were overlapping on the previous frame
            if (testCollisionAABBAABBStatic(objACenter_t0, objBCenter_t0, Ea, Eb))
            {
                _cols.Add(new collision(collisionType.COL_UNDEFINED, objA, objB, 0.0f, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, coeffRestitution, 1.0f));
                return true;
            }

            if (v.Mag() == 0.0f ) // If the velocity is zero we've done enough
                return false;

            float tfirst = 0.0f;
            float tlast = 1.0f;

            for (int i = 0; i < 3; i++)
            {
                if (v.GetAt(i) < 0.0f)
                {
                    if (objBMax_t0.GetAt(i) < objAMin_t0.GetAt(i))
                        return false;
                    if (objAMax_t0.GetAt(i) < objBMin_t0.GetAt(i))
                        tfirst = Max((objAMax_t0.GetAt(i) - objBMin_t0.GetAt(i)) / v.GetAt(i), tfirst);
                    if (objBMax_t0.GetAt(i) > objAMin_t0.GetAt(i))
                        tlast = Min((objAMin_t0.GetAt(i) - objBMax_t0.GetAt(i)) / v.GetAt(i), tlast);
                }
                if (v.GetAt(i) > 0.0f)
                {
                    if (objBMin_t0.GetAt(i) > objAMax_t0.GetAt(i))
                        return false;
                    if (objBMax_t0.GetAt(i) < objAMin_t0.GetAt(i))
                        tfirst = Max((objAMin_t0.GetAt(i) - objBMax_t0.GetAt(i)) / v.GetAt(i), tfirst);
                    if (objAMax_t0.GetAt(i) > objBMin_t0.GetAt(i))
                        tlast = Min((objAMax_t0.GetAt(i) - objBMin_t0.GetAt(i)) / v.GetAt(i), tlast);
                }
                // No overlap possible if time of first contact occurs after time of last contact
                if (tfirst > tlast)
                    return false;
            }
            
            float Tcollision = tfirst;

            // Find new min and max for each AABB at the time of the collision
            objAMin_t0 = objAMin_t0 + velA * Tcollision;
            objAMax_t0 = objAMax_t0 + velA * Tcollision;

            objBMin_t0 = objBMin_t0 + velB * Tcollision;
            objBMax_t0 = objBMax_t0 + velB * Tcollision;

            FindAABBColPoints(Tcollision, ref objA, ref objB,  ref objAMin_t0, ref objAMax_t0, ref objBMin_t0, ref objBMax_t0, ref _cols);

            return true;
            
            */

            #endregion
        }
        #endregion

        #region FindAABBColPoints()
        /// Given the Min and Max extents for the two AABBs AT the time of the collision -> find the points and add them to the list of collisions
        /// ***********************************************************************
        public static void FindAABBColPoints(float Tcollision, 
                                             ref gameObject A, ref gameObject B, 
                                             ref Vector3 minA, ref Vector3 maxA, 
                                             ref Vector3 minB, ref Vector3 maxB, 
                                             ref List<collision> _cols )
        {
            // 1. FIRST FIGURE OUT WHICH AXES THE CONTACT HAS OCCURED --> 6 POSSIBILITIES
            int[] commonCollisionCoord = new int[3]; // Automatically starts at (0,0,0) in C#
            // AND BUILD A NEW VECTOR2 MIN / MAX WITH THE COMMON COORDINATE REMOVED
            Vector2 minA_flat = Vector2.Zero;
            Vector2 maxA_flat = Vector2.Zero;
            Vector2 minB_flat = Vector2.Zero;
            Vector2 maxB_flat = Vector2.Zero;

            ReduceDownTo2DPoint(ref minA, ref maxA,
                                ref minB, ref maxB,
                                ref commonCollisionCoord,
                                ref minA_flat, ref maxA_flat,
                                ref minB_flat, ref maxB_flat);
            

            // 2. THEN FIGURE OUT WHAT THE 4 POINTS ARE --> 4 POSSIBILITIES
            //   A)      +------+         2 vertex/face, 2 edge/edge
            //           |      |
            //           | +----+-+
            //           | |    | | 
            //           +-+----+ |
            //             |      |
            //             +------+
            //   B)      +-------+        2 vertex/face, 2 edge/edge
            //           |       |
            //           | +---+ |
            //           | |   | | 
            //           +-+---+-+
            //             |   |
            //             +---+
            //   C)      +-------+        4 vertex/face
            //           | +---+ |
            //           | |   | |
            //           | +---+ | 
            //           +-+---+-+
            //   C)        +---+          4 edge/edge
            //             |   |
            //           +-+   +-+
            //           | |   | |
            //           | |   | |
            //           | |   | | 
            //           +-+---+-+
            //             |   |
            //             +---+
            // A Vertex / face happens if the internal corner is derived from points from the same object.

            // Find the two internal points along the X direction
            float[] contactPoints_Xaxis = new float[2];
            int[] contactObj_Xaxis = new int[2];        // 0 --> It was an object A point, 1 --> It was an object B point
            float[] contactPoints_Yaxis = new float[2];
            int[] contactObj_Yaxis = new int[2];        // 0 --> It was an object A point, 1 --> It was an object B point
            GetInternalPoints(ref minA_flat, ref maxA_flat,
                              ref minB_flat, ref maxB_flat,
                              ref contactPoints_Xaxis, ref contactPoints_Yaxis,
                              ref contactObj_Xaxis, ref contactObj_Yaxis);

            collisionType curCollisionType = collisionType.COL_UNDEFINED;
            gameObject curObjA = null;
            gameObject curObjB = null;
            Vector3 normal;
            Vector3 point;
            Vector3 e1 = Vector3.Zero;
            Vector3 e2 = Vector3.Zero;

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (contactObj_Xaxis[i] == contactObj_Yaxis[j])
                    {
                        curCollisionType = collisionType.VERTEX_FACE; // The collision point is INSIDE one of the boxes
                        if (contactObj_Xaxis[i] == 0)
                        {
                            curObjA = A;
                            curObjB = B;
                            // Normal is defined positive out of obj 2
                            // INPUT OBJECT A IS THE VERTEX, INPUT OBJECT B IS THE FACE
                            normal = -1.0f * new Vector3(commonCollisionCoord[0], commonCollisionCoord[1], commonCollisionCoord[2]); 
                        }
                        else
                        {
                            curObjA = B;
                            curObjB = A;
                            // Normal is defined positive out of obj 2
                            // INPUT OBJECT B IS THE VERTEX, INPUT OBJECT A IS THE FACE
                            normal = +1.0f * new Vector3(commonCollisionCoord[0], commonCollisionCoord[1], commonCollisionCoord[2]);
                        }
                    }
                    else
                    {
                        curCollisionType = collisionType.EDGE_EDGE; // The collision point is an edge / edge
                        curObjA = A; curObjB = B; // Both are edges so order is not important
                        BuildEdgeVectors(ref e1, ref e2, ref commonCollisionCoord, (contactObj_Xaxis[i] == 0));
                        normal = -1.0f * new Vector3(commonCollisionCoord[0], commonCollisionCoord[1], commonCollisionCoord[2]); // If positive minA is touching maxB, if negative maxA is touching maxB
                        e2 = Vector3.Cross(normal, e1); // Quick hack to fix the edge vectors
                    }
                    point = BuildUpTo3DPoint(contactPoints_Xaxis[i], contactPoints_Yaxis[j], ref minA, ref maxA, ref commonCollisionCoord);
                    // Add the point
                    _cols.Add(new collision(curCollisionType, curObjA, curObjB, Tcollision, point, Vector3.Normalize(normal), e1, e2, coeffRestitution, 0.25f)); // Vector3 should be normal
                }
            }
            
        }
        #endregion

        #region BuildEdgeVectors()
        protected static void BuildEdgeVectors(ref Vector3 e1, ref Vector3 e2, ref int[] commonCollisionCoord, bool firstAxisDefinedByA )
        {
            if (commonCollisionCoord[0] != 0)
            {
                if (firstAxisDefinedByA)
                { e1 = new Vector3(0, 0, 1); e2 = new Vector3(0, 1, 0); } // Yaxis is defined by A's edge, Zaxis is defined by B's edge
                else
                { e1 = new Vector3(0, 1, 0); e2 = new Vector3(0, 0, 1); } // Yaxis is defined by B's edge, Zaxis is defined by A's edge
            } // tested
            else if (commonCollisionCoord[1] != 0)
            {
                if (firstAxisDefinedByA)
                { e1 = new Vector3(0, 0, 1); e2 = new Vector3(1, 0, 0); } // Zaxis is defined by A's edge, Zaxis is defined by B's edge
                else
                { e1 = new Vector3(1, 0, 0); e2 = new Vector3(0, 0, 1); } // Zaxis is defined by B's edge, Zaxis is defined by A's edge
            }
            else if (commonCollisionCoord[2] != 0)
            {
                if (firstAxisDefinedByA)
                { e1 = new Vector3(1, 0, 0); e2 = new Vector3(0, 1, 0); } // Xaxis is defined by A's edge, Yaxis is defined by B's edge
                else
                { e1 = new Vector3(0, 1, 0); e2 = new Vector3(1, 0, 0); } // Xaxis is defined by B's edge, Yaxis is defined by A's edge
            }
            else
                throw new Exception("BuildEdgeVectors() - Couldn't find a common collision coordinate!");
        }
        #endregion

        #region BuildUpTo3DPoint
        protected static Vector3 BuildUpTo3DPoint(float p0, float p1, 
                                                  ref Vector3 minA, ref Vector3 maxA, 
                                                  ref int[] commonCollisionCoord )
        {
           // Six possibilities like with ReduceDownTo2DPoint
            if (commonCollisionCoord[0] == -1)      // CASE 1: minA = maxB ON X AXIS
                return new Vector3(minA.X, p0, p1);
            else if (commonCollisionCoord[0] == +1) // CASE 2: maxA = minB ON X AXIS
                return new Vector3(maxA.X, p0, p1);
            else if (commonCollisionCoord[1] == -1) // CASE 3: maxA = minB ON Y AXIS
                return new Vector3(p0, minA.Y, p1);
            else if (commonCollisionCoord[1] == +1) // CASE 4: maxA = minB ON Y AXIS
                return new Vector3(p0, maxA.Y, p1);
            else if (commonCollisionCoord[2] == -1) // CASE 5: maxA = minB ON Z AXIS
                return new Vector3(p0, p1, minA.Z);
            else if (commonCollisionCoord[2] == +1) // CASE 6: maxA = minB ON Z AXIS
                return new Vector3(p0, p1, maxA.Z);
            else
                throw new Exception("collisionUtils::BuildUpTo3DPoint() - Couldn't rebuild 3D point");
        }
        #endregion

        #region GetInternalPoints
        protected static void GetInternalPoints(ref Vector2 minA_flat, ref Vector2 maxA_flat,
                                                ref Vector2 minB_flat, ref Vector2 maxB_flat,
                                                ref float[] contactPoints_Xaxis, ref float[] contactPoints_Yaxis,
                                                ref int[] contactObj_Xaxis, ref int[] contactObj_Yaxis)
        {
            if (minA_flat.X < minB_flat.X)
            {
                contactPoints_Xaxis[0] = minB_flat.X; contactObj_Xaxis[0] = 1; // B corner on inside
            }
            else
            {
                contactPoints_Xaxis[0] = minA_flat.X; contactObj_Xaxis[0] = 0; // A corner on inside
            }
            if (maxA_flat.X > maxB_flat.X)
            {
                contactPoints_Xaxis[1] = maxB_flat.X; contactObj_Xaxis[1] = 1; // B corner on inside
            }
            else
            {
                contactPoints_Xaxis[1] = maxA_flat.X; contactObj_Xaxis[1] = 0; // A corner on inside
            }
            // Find the two internal points along the Y direction

            if (minA_flat.Y < minB_flat.Y)
            {
                contactPoints_Yaxis[0] = minB_flat.Y; contactObj_Yaxis[0] = 1; // B corner on inside
            }
            else
            {
                contactPoints_Yaxis[0] = minA_flat.Y; contactObj_Yaxis[0] = 0; // A corner on inside
            }
            if (maxA_flat.Y > maxB_flat.Y)
            {
                contactPoints_Yaxis[1] = maxB_flat.Y; contactObj_Yaxis[1] = 1; // B corner on inside
            }
            else
            {
                contactPoints_Yaxis[1] = maxA_flat.Y; contactObj_Yaxis[1] = 0; // A corner on inside
            }
        }
        #endregion

        #region ReduceDownTo2DPoint
        protected static void ReduceDownTo2DPoint(ref Vector3 minA, ref Vector3 maxA, 
                                                  ref Vector3 minB, ref Vector3 maxB,
                                                  ref int[] commonCollisionCoord,
                                                  ref Vector2 minA_flat, ref Vector2 maxA_flat,
                                                  ref Vector2 minB_flat, ref Vector2 maxB_flat )
        {
            // Six possibilities 
            if (MyExtensions.testFloatEquality(minA.X, maxB.X))      // CASE 1: minA = maxB ON X AXIS
            {
                commonCollisionCoord[0] = -1;
                minA_flat.X = minA.Y; minA_flat.Y = minA.Z; maxA_flat.X = maxA.Y; maxA_flat.Y = maxA.Z;
                minB_flat.X = minB.Y; minB_flat.Y = minB.Z; maxB_flat.X = maxB.Y; maxB_flat.Y = maxB.Z;
            }
            else if (MyExtensions.testFloatEquality(maxA.X, minB.X)) // CASE 2: maxA = minB ON X AXIS
            {
                commonCollisionCoord[0] = +1;
                minA_flat.X = minA.Y; minA_flat.Y = minA.Z; maxA_flat.X = maxA.Y; maxA_flat.Y = maxA.Z;
                minB_flat.X = minB.Y; minB_flat.Y = minB.Z; maxB_flat.X = maxB.Y; maxB_flat.Y = maxB.Z;
            }
            else if (MyExtensions.testFloatEquality(minA.Y, maxB.Y)) // CASE 3: maxA = minB ON Y AXIS
            {
                commonCollisionCoord[1] = -1;
                minA_flat.X = minA.X; minA_flat.Y = minA.Z; maxA_flat.X = maxA.X; maxA_flat.Y = maxA.Z;
                minB_flat.X = minB.X; minB_flat.Y = minB.Z; maxB_flat.X = maxB.X; maxB_flat.Y = maxB.Z;
            }
            else if (MyExtensions.testFloatEquality(maxA.Y, minB.Y)) // CASE 4: maxA = minB ON Y AXIS
            {
                commonCollisionCoord[1] = +1;
                minA_flat.X = minA.X; minA_flat.Y = minA.Z; maxA_flat.X = maxA.X; maxA_flat.Y = maxA.Z;
                minB_flat.X = minB.X; minB_flat.Y = minB.Z; maxB_flat.X = maxB.X; maxB_flat.Y = maxB.Z;
            }
            else if (MyExtensions.testFloatEquality(minA.Z, maxB.Z)) // CASE 5: maxA = minB ON Z AXIS
            {
                commonCollisionCoord[2] = -1;
                minA_flat.X = minA.X; minA_flat.Y = minA.Y; maxA_flat.X = maxA.X; maxA_flat.Y = maxA.Y;
                minB_flat.X = minB.X; minB_flat.Y = minB.Y; maxB_flat.X = maxB.X; maxB_flat.Y = maxB.Y;
            }
            else if (MyExtensions.testFloatEquality(maxA.Z, minB.Z)) // CASE 6: maxA = minB ON Z AXIS
            {
                commonCollisionCoord[2] = +1;
                minA_flat.X = minA.X; minA_flat.Y = minA.Y; maxA_flat.X = maxA.X; maxA_flat.Y = maxA.Y;
                minB_flat.X = minB.X; minB_flat.Y = minB.Y; maxB_flat.X = maxB.X; maxB_flat.Y = maxB.Y;
            }
            else
                throw new Exception("collisionUtils::FindAABBColPoints() - Couldn't find contact axis");
        }
        #endregion

        #region TestCollisionAABBAABBStatic(Vector3 Pa, Vector3 Pb, float[] Ea, float[] Eb)
        protected static bool testCollisionAABBAABBStatic(Vector3 Pa, Vector3 Pb, float[] Ea, float[] Eb)
        {
            throw new Exception("AABB-AABB collisions are no longer supported.  Use AABB-Sphere or Sphere-Sphere");

            #region OLD CODE
            /*
            Vector3 T = Pb - Pa;
            return ((float)Math.Abs(T.X)) <= (Ea[0] + Eb[0]) &&
                   ((float)Math.Abs(T.Y)) <= (Ea[1] + Eb[1]) &&
                   ((float)Math.Abs(T.Z)) <= (Ea[2] + Eb[2]); // All three regions must be overlapping (otherwise there's a seperating axis)
            */
            #endregion
        }
        #endregion

        #region TestCollisionAABBAABBStatic(Vector3 Pa, Vector3 Pb, float[] Ea, float[] Eb, ref float separationDistance)
        protected static bool testCollisionAABBAABBStatic(Vector3 Pa, Vector3 Pb, float[] Ea, float[] Eb, ref float separationDistance)
        {
            throw new Exception("AABB-AABB collisions are no longer supported.  Use AABB-Sphere or Sphere-Sphere");

            #region OLD CODE
            /*
            Vector3 T = Pb - Pa;
            separationDistance = 0.0f;
            separationDistance += ((float)Math.Abs(T.X)) <= (Ea[0] + Eb[0]) ? 0.0f : (((float)Math.Abs(T.X)) - (Ea[0] + Eb[0])) * (((float)Math.Abs(T.X)) - (Ea[0] + Eb[0]));
            separationDistance += ((float)Math.Abs(T.Y)) <= (Ea[1] + Eb[1]) ? 0.0f : (((float)Math.Abs(T.Y)) - (Ea[1] + Eb[1])) * (((float)Math.Abs(T.Y)) - (Ea[1] + Eb[1]));
            separationDistance += ((float)Math.Abs(T.Z)) <= (Ea[2] + Eb[2]) ? 0.0f : (((float)Math.Abs(T.Z)) - (Ea[2] + Eb[2])) * (((float)Math.Abs(T.Z)) - (Ea[2] + Eb[2]));
                
            // All three regions must be overlapping (otherwise there's a seperating axis)
            if (separationDistance <= 0.0f)
                return true;
            else
            {
                separationDistance = (float)Math.Sqrt(separationDistance);
                return false;
            }
            */
            #endregion
        }
        #endregion

        #region TestCollisionAABBAABBStatic(gameObject objA, gameObject objB)
        protected static bool TestCollisionAABBAABBStatic(gameObject objA, gameObject objB, ref float separationDistance)
        {
            throw new Exception("AABB-AABB collisions are no longer supported.  Use AABB-Sphere or Sphere-Sphere");

            #region OLD CODE
            /*
            BoundingBox mBox1 = (BoundingBox)objA.boundingObj;
            BoundingBox mBox2 = (BoundingBox)objB.boundingObj;

            // Move the bounding boxes into world frame and update their size --> Boxes may grow
            objAMat_t1 = objA.CreateScale(objA.state.scale) * Matrix.CreateFromQuaternion(objA.state.orient) * Matrix.CreateTranslation(objA.state.pos);
            UpdateBoundingBox(mBox1, objAMat_t1, ref objAMin_t1, ref objAMax_t1);

            objBMat_t1 = objB.CreateScale(objB.state.scale) * Matrix.CreateFromQuaternion(objB.state.orient) * Matrix.CreateTranslation(objB.state.pos);
            UpdateBoundingBox(mBox2, objBMat_t1, ref objBMin_t1, ref objBMax_t1);

            objACenter_t1 = 0.5f * (objAMax_t1 + objAMin_t1);
            objBCenter_t1 = 0.5f * (objBMax_t1 + objBMin_t1);

            // Algorithm uses half-width extents, so get them:
            Ea[0] = 0.5f * (objAMax_t1.X - objAMin_t1.X);
            Ea[1] = 0.5f * (objAMax_t1.Y - objAMin_t1.Y);
            Ea[2] = 0.5f * (objAMax_t1.Z - objAMin_t1.Z);
            Eb[0] = 0.5f * (objBMax_t1.X - objBMin_t1.X);
            Eb[1] = 0.5f * (objBMax_t1.Y - objBMin_t1.Y);
            Eb[2] = 0.5f * (objBMax_t1.Z - objBMin_t1.Z);

            return testCollisionAABBAABBStatic(objACenter_t1, objBCenter_t1, Ea, Eb, ref separationDistance);
            */
            #endregion
        }
        #endregion

        #region Max(float x, float y)
        /// Just return the largest - throw an exception if both are the same value
        /// ***********************************************************************
        public static float Max(float x, float y)
        {
            if (x >= y)
                return x;
            else
                return y;
        }
        #endregion

        #region Min(float x, float y)
        /// Just return the largest - throw an exception if both are the same value
        /// ***********************************************************************
        public static float Min(float x, float y)
        {
            if (x < y)
                return x;
            else
                return y;
        }
        #endregion

        #region UpdateBoundingBox()
        /// UpdateBoundingBox() - Rotate the bounding box and fix the AABB coordinates
        /// --> Simply wrap the AABB in another AABB --> Least space efficient by fastest.
        /// ***********************************************************************
        public static void UpdateBoundingBox(BoundingBox origBox, Matrix mat, ref Vector3 newMin, ref Vector3 newMax)
        {
            // transform all 8 points and find min and max extents
            // This is the dumbest method but it works
            aabbverticies[0].X = origBox.Min.X; aabbverticies[0].Y = origBox.Max.Y; aabbverticies[0].Z = origBox.Max.Z;  // left top back      - + +
            aabbverticies[1].X = origBox.Max.X; aabbverticies[1].Y = origBox.Max.Y; aabbverticies[1].Z = origBox.Max.Z;  // right top back     + + +
            aabbverticies[2].X = origBox.Min.X; aabbverticies[2].Y = origBox.Max.Y; aabbverticies[2].Z = origBox.Min.Z;  // left top front     - + -
            aabbverticies[3].X = origBox.Max.X; aabbverticies[3].Y = origBox.Max.Y; aabbverticies[3].Z = origBox.Min.Z;  // right top front    + + -

            aabbverticies[4].X = origBox.Min.X; aabbverticies[4].Y = origBox.Min.Y; aabbverticies[4].Z = origBox.Max.Z;  // left bottom back   - - +
            aabbverticies[5].X = origBox.Max.X; aabbverticies[5].Y = origBox.Min.Y; aabbverticies[5].Z = origBox.Max.Z;  // right bottom back  + - +
            aabbverticies[6].X = origBox.Min.X; aabbverticies[6].Y = origBox.Min.Y; aabbverticies[6].Z = origBox.Min.Z;  // left bottom front  - - -
            aabbverticies[7].X = origBox.Max.X; aabbverticies[7].Y = origBox.Min.Y; aabbverticies[7].Z = origBox.Min.Z;  // right bottom front + - -

            aabbverticies[0] = Vector3.Transform(aabbverticies[0], mat);
            newMin = newMax = aabbverticies[0];

            for (int i = 1; i < 8; i++)
            {
                aabbverticies[i] = Vector3.Transform(aabbverticies[i], mat);
                if (aabbverticies[i].X < newMin.X)
                    newMin.X = aabbverticies[i].X;
                if (aabbverticies[i].Y < newMin.Y)
                    newMin.Y = aabbverticies[i].Y;
                if (aabbverticies[i].Z < newMin.Z)
                    newMin.Z = aabbverticies[i].Z;

                if (aabbverticies[i].X > newMax.X)
                    newMax.X = aabbverticies[i].X;
                if (aabbverticies[i].Y > newMax.Y)
                    newMax.Y = aabbverticies[i].Y;
                if (aabbverticies[i].Z > newMax.Z)
                    newMax.Z = aabbverticies[i].Z;
            }
        }
        #endregion

        #region TestCollisionSphereSphere()
        /// TestCollisionSphereSphere() - Test whether two swept sphere collide between prevState and state
        /// A FEW ASSUMPTIONS:  1. Scale factor is constant between frames.
        ///                     2. No acceleration --> spheres are linearly swept between the two points with constant velocity
        /// Code derived from: http://www.gamasutra.com/view/feature/3383/simple_intersection_tests_for_games.php?page2
        /// NOTE: There are faster methods to perform this test --> to avoid sqrt calls AND to avoid floating point accuracy issues for large velocities
        /// ***********************************************************************
        protected static bool TestCollisionSphereSphere(gameObject objA, gameObject objB, ref List<collision> _cols)
        {
            float Tcollision = 0.0f;
            Vector3 point = Vector3.Zero;
            Vector3 normal = Vector3.Zero;
            Vector3 e1 = Vector3.Zero;
            Vector3 e2 = Vector3.Zero;

            // Bring both spheres into common world coordinates to find the center points at t0 and t1
            // note, model center not necessarily at 0,0,0 in model coords --> Therefore need rotation as well
            objAMat_t0 = objA.CreateScale(objA.prevState.scale) * Matrix.CreateFromQuaternion(objA.prevState.orient) * Matrix.CreateTranslation(objA.prevState.pos);
            objAMat_t1 = objA.CreateScale(objA.state.scale) * Matrix.CreateFromQuaternion(objA.state.orient) * Matrix.CreateTranslation(objA.state.pos);
            UpdateBoundingSphere((BoundingSphere)objA.boundingObj, objAMat_t0, objA.prevState.scale, objA, ref objACenter_t0, ref objARadius_t0);
            UpdateBoundingSphere((BoundingSphere)objA.boundingObj, objAMat_t1, objA.state.scale, objA, ref objACenter_t1, ref objARadius_t1);

            objBMat_t0 = objB.CreateScale(objB.prevState.scale) * Matrix.CreateFromQuaternion(objB.prevState.orient) * Matrix.CreateTranslation(objB.prevState.pos);
            objBMat_t1 = objB.CreateScale(objB.state.scale) * Matrix.CreateFromQuaternion(objB.state.orient) * Matrix.CreateTranslation(objB.state.pos);
            UpdateBoundingSphere((BoundingSphere)objB.boundingObj, objBMat_t0, objB.prevState.scale, objB, ref objBCenter_t0, ref objBRadius_t0);
            UpdateBoundingSphere((BoundingSphere)objB.boundingObj, objBMat_t1, objB.state.scale, objB, ref objBCenter_t1, ref objBRadius_t1);

            // Find the velocities
            Vector3 va = objACenter_t1 - objACenter_t0; // Vector from A0 to A1
            Vector3 vb = objBCenter_t1 - objBCenter_t0; // Vector from B0 to B1
            Vector3 AB = objBCenter_t0 - objACenter_t0; // Vector from A0 to B0
            Vector3 vab = vb - va; // Relative velocity (in normalized time)
            float rab = objARadius_t0 + objBRadius_t0;
            // Quadratic equation coefficients: ax^2 + bx + c = 0
            float a = Vector3.Dot(vab,vab); // u * u coefficient
            float b = Vector3.Dot(2 * vab, AB);
            float c = Vector3.Dot(AB, AB) - rab * rab;

            // Check if they're currently overlapping --> We actually don't want this.  It means that there are two points of collision
            if ( Vector3.Dot(AB,AB) <= rab * rab )
            {
                Tcollision = 0.0f;
                _cols.Add(new collision(collisionType.COL_UNDEFINED, objA, objB, Tcollision, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, coeffRestitution, 1.0f)); // Normal points out of face of B
                return true;
            }

            bool retVal = false;
            float t0 = 0.0f, t1 = 0.0f;
            if ( QuadraticFormula( a, b, c, ref t0, ref t1 ) )
            {
                if(t0 < t1) // t0 occured first
                {
                    if( t0 >= 0.0f && t0 <= 1.0f) // t0 is within 0-->1
                    {
                    Tcollision = t0;
                    retVal = true;
                    }
                }
                else 
                {
                    if (t1 >= 0.0f && t1 <= 1.0f)  // t1 occured first and is within 0-->1
                    {
                    Tcollision = t1;
                    retVal = true;
                    }
                }
            }

            // Calculate the point of contact if it exists
            if (retVal)
            {
                // Get the two centers at collision time
                objACenter_t1 = objACenter_t0 + va * Tcollision;  // Reuse t1 variables to save stack (or heap) space
                objBCenter_t1 = objBCenter_t0 + vb * Tcollision;
                AB = objBCenter_t1 - objACenter_t1; // Vector from the center of A to the center of B at the time of collision
                normal = Vector3.Normalize(AB);
                point = objACenter_t1 + normal * objARadius_t0; // Center is along the vector between the two centers, at a distance of the radius

                // sphere collisions ALWAYS result in 1 vertex/face collision --> Add it to the collision array
                _cols.Add(new collision(collisionType.VERTEX_FACE, objA, objB, Tcollision, point, Vector3.Negate(normal), e1, e2, coeffRestitution, 1.0f)); // Normal points out of face of B
                return true;
            }
            else
                return false;
        }
        #endregion

        #region TestCollisionSphereSphereStatic(gameObject objA, gameObject objB)
        protected static bool TestCollisionSphereSphereStatic(gameObject objA, gameObject objB, ref float separationDistance)
        {
            // Bring both spheres into common world coordinates to find the center points at t0 and t1
            // note, model center not necessarily at 0,0,0 in model coords --> Therefore need rotation as well
            objAMat_t1 = objA.CreateScale(objA.state.scale) * Matrix.CreateFromQuaternion(objA.state.orient) * Matrix.CreateTranslation(objA.state.pos);
            UpdateBoundingSphere((BoundingSphere)objA.boundingObj, objAMat_t1, objA.state.scale, objA, ref objACenter_t1, ref objARadius_t1);

            objBMat_t1 = objB.CreateScale(objB.state.scale) * Matrix.CreateFromQuaternion(objB.state.orient) * Matrix.CreateTranslation(objB.state.pos);
            UpdateBoundingSphere((BoundingSphere)objB.boundingObj, objBMat_t1, objB.state.scale, objB, ref objBCenter_t1, ref objBRadius_t1);

            float rab = objARadius_t1 + objBRadius_t1;
            Vector3 AB = objBCenter_t1 - objACenter_t1; // Vector from A0 to B0

            separationDistance = (float)Math.Sqrt(Vector3.Dot(AB, AB)) - rab;

            if (separationDistance <= 0.0f)
            {
                separationDistance = 0.0f;
                return true;
            }
            else
                return false;
        }
        #endregion

        #region UpdateBoundingSphere
        public static void UpdateBoundingSphere(BoundingSphere bSphere, Matrix world, Vector3 scale, gameObject obj, ref Vector3 newCenter, ref float newRadius)
        {
            newCenter = Vector3.Transform(bSphere.Center, world);

            // Scale radius by the largest scale factor (may be non-uniform), then by gameObject normalization factor
            // Actually radius * modelScaleToNormalizeSize = 1 ALWAYS...  But do it anyway for completeness
            newRadius = bSphere.Radius *                                                                                // Origional radius
                        Math.Max(Math.Max(scale.X, scale.Y), scale.Z) *                                                 // Scale factor
                        obj.modelScaleToNormalizeSize;                                                                  // Normalization factor
        }
        #endregion

        #region QuadraticFormula()
        /// QuadraticFormula() - For ax^2 + bx + c = 0 --> solve: (-b +/- sqrt(b^2 - 4ac))/(2a)
        ///                      Returns true if roots are real and sets t0 and t1 to the solutions
        /// Code derived from: http://www.gamasutra.com/view/feature/3383/simple_intersection_tests_for_games.php?page2
        /// ***********************************************************************
        public static bool QuadraticFormula( float a, float b, float c, ref float t0, ref float t1)
        {
            float discriminant  = b*b - 4*a*c;
            if( discriminant < 0 )
                return false; // complex / imaginary roots
            else
            {
                float sq = (float)Math.Sqrt(discriminant);
                float one_over2a = 1.0f/ (2*a);
                t0 = (-b + sq) * one_over2a;
                t1 = (-b - sq) * one_over2a;
                return true; // real roots
            }
        }
        #endregion

        #region TestCollisionSphereAABB()
        /// TestCollisionSphereSphere() - Test whether a swept sphere collides with a stationary AABB between prevState and state
        /// A FEW ASSUMPTIONS:  1. Scale factor is constant between frames.
        ///                     2. No acceleration --> sphere and AABB are linearly swept between the two points with constant velocity
        /// I tried code derived from: Pages 228-229 Real Time Collision Detection, Christer Ericson --> But it didn't work
        /// I also tried http://www.gamedev.net/community/forums/topic.asp?topic_id=335465 (much simpler anyway) --> Doesn't sweep box
        /// I finally used http://www.geometrictools.com/LibMathematics/Intersection/Intersection.html --> "Intersection of boxes and spheres (3D). Includes the cases of moving spheres and boxes. "
        /// ***********************************************************************
        protected static bool TestCollisionSphereAABB(gameObject objA, gameObject objB, ref List<collision> _cols ) // objA is a sphere, objB is an AABB
        {
            BoundingSphere mSphere = (BoundingSphere)objA.boundingObj;
            BoundingBox mBox = (BoundingBox)objB.boundingObj;

            float Tcollision = 0.0f; 
            Vector3 point = Vector3.Zero;
            Vector3 normal = Vector3.Zero;
            Vector3 e1 = Vector3.Zero;
            Vector3 e2 = Vector3.Zero;

            // Bring the sphere into common world coordinates to find the center points at t0 and t1
            // note, model center not necessarily at 0,0,0 in model coords --> Therefore need rotation as well
            objAMat_t0 = objA.CreateScale(objA.prevState.scale) * Matrix.CreateFromQuaternion(objA.prevState.orient) * Matrix.CreateTranslation(objA.prevState.pos);
            UpdateBoundingSphere(mSphere, objAMat_t0, objA.prevState.scale, objA, ref objACenter_t0, ref objARadius_t0);
            

            // Bring BoundingSphere into box coordinate system --> Means we don't need to recalculate AABB dimensions in world 
            objBMat_t0 = objB.CreateScale(objB.prevState.scale) * Matrix.CreateFromQuaternion(objB.prevState.orient) * Matrix.CreateTranslation(objB.prevState.pos);
            objBMatInt_t0 = Matrix.Invert(objBMat_t0);
            objACenter_t0 = Vector3.Transform(objACenter_t0, objBMatInt_t0); // bring objA starting center into B's fram
            objARadius_t0 = objARadius_t0 * 
                            (1.0f / Math.Max(Math.Max(objB.prevState.scale.X, objB.prevState.scale.Y), objB.prevState.scale.Z)) *
                            (1.0f / objB.modelScaleToNormalizeSize); // bring objA radius in B's frame
            
            // Find the velocities and then the relative velocity
            Vector3 va = objA.state.pos - objA.prevState.pos; 
            Vector3 vb = objB.state.pos - objB.prevState.pos;
            //Vector3 vab = vb - va; // Relative velocity (in normalized time)
            Vector3 vab = va - vb; // Relative velocity (in normalized time)
            vab = Vector3.Transform(vab, objBMatInt_t0); // bring relative velocity into A's frame
            float vx = vab.X; float vy = vab.Y; float vz = vab.Z; 

            // Find the Box Center and then the relative starting position in B's frame
            Vector3 boxCenter = (mBox.Min + mBox.Max) * 0.5f;  // Just average of the two positions
            Vector3 cdiff = objACenter_t0 - boxCenter;
            cdiff = Vector3.Transform(cdiff, objBMatInt_t0);
            float ax = cdiff.X; float ay = cdiff.Y; float az = cdiff.Z;

            // Also need the box's half-lengths --> David Eberly defines a box as a center, some basis vectors and extents
            float [] Extent = new float[3];
            Extent[0] = (mBox.Max.X - mBox.Min.X) * 0.5f;
            Extent[1] = (mBox.Max.Y - mBox.Min.Y) * 0.5f;
            Extent[2] = (mBox.Max.Z - mBox.Min.Z) * 0.5f;

            // Now perform the routine in :
            // http://www.geometrictools.com/LibMathematics/Intersection/Intersection.html --> "Intersection of boxes and spheres (3D)" --> Find (...)

            // Flip coordinate frame into the first octant.
            int signX = 1;
            if (ax < 0.0f)
            {
                ax = -ax;
                vx = -vx;
                signX = -1;
            }

            int signY = 1;
            if (ay < 0.0f)
            {
                ay = -ay;
                vy = -vy;
                signY = -1;
            }

            int signZ = 1;
            if (az < 0.0f)
            {
                az = -az;
                vz = -vz;
                signZ = -1;
            }

            // Intersection coordinates.
            float ix = 0.0f, iy = 0.0f, iz = 0.0f;
            int retVal;

            if (ax <= Extent[0])
            {
                if (ay <= Extent[1])
                {
                    if (az <= Extent[2])
                    {
                        // The sphere center is inside box.  Return it as the contact
                        // point, but report an "other" intersection type.
                        Tcollision = 0.0f;
                        retVal = -1;
                    }
                    else
                    {
                        // Sphere above face on axis Z.
                        retVal = FindFaceRegionIntersection(Extent[0],
                            Extent[1], Extent[2], ax, ay, az, vx, vy,
                            vz, ref ix, ref iy, ref iz, true, objARadius_t0, ref Tcollision);
                    }
                }
                else
                {
                    if (az <= Extent[2])
                    {
                        // Sphere above face on axis Y.
                        retVal = FindFaceRegionIntersection(Extent[0],
                            Extent[2], Extent[1], ax, az, ay, vx, vz,
                            vy, ref ix, ref iz, ref iy, true, objARadius_t0, ref Tcollision);
                    }
                    else
                    {
                        // Sphere is above the edge formed by faces y and z.
                        retVal = FindEdgeRegionIntersection(Extent[1],
                            Extent[0], Extent[2], ay, ax, az, vy, vx,
                            vz, ref iy, ref ix, ref iz, true, objARadius_t0, ref Tcollision);
                    }
                }
            }
            else
            {
                if (ay <= Extent[1])
                {
                    if (az <= Extent[2])
                    {
                        // Sphere above face on axis X.
                        retVal = FindFaceRegionIntersection(Extent[1],
                            Extent[2], Extent[0], ay, az, ax, vy, vz,
                            vx, ref iy, ref iz, ref ix, true, objARadius_t0, ref Tcollision);
                    }
                    else
                    {
                        // Sphere is above the edge formed by faces x and z.
                        retVal = FindEdgeRegionIntersection(Extent[0],
                            Extent[1], Extent[2], ax, ay, az, vx, vy,
                            vz, ref ix, ref iy, ref iz, true, objARadius_t0, ref Tcollision);
                    }
                }
                else
                {
                    if (az <= Extent[2])
                    {
                        // Sphere is above the edge formed by faces x and y.
                        retVal = FindEdgeRegionIntersection(Extent[0],
                            Extent[2], Extent[1], ax, az, ay, vx, vz,
                            vy, ref ix, ref iz, ref iy, true, objARadius_t0, ref Tcollision);
                    }
                    else
                    {
                        // sphere is above the corner formed by faces x,y,z
                        retVal = FindVertexRegionIntersection(Extent[0],
                            Extent[1], Extent[2], ax, ay, az, vx, vy,
                            vz, ref ix, ref iy, ref iz, objARadius_t0, ref Tcollision);
                    }
                }
            }

            if (retVal == 0 || Tcollision > 1.0f)
            {
                return false;
            }

            if (retVal == -1 || Tcollision == 0.0f) // -1 indicates collision before sweep
            {
                Tcollision = 0.0f;
                _cols.Add(new collision(collisionType.COL_UNDEFINED, objA, objB, Tcollision, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, coeffRestitution, 1.0f)); // Normal points out of face of B
                return true;
            }

            // Calculate actual intersection (move point back into world coordinates).
            Vector3 i = new Vector3(signX * ix, signY * iy, signZ * iz);
            point = boxCenter + i;
            Vector3 point_on_box = ClosestPointOnAABB(point, mBox.Min, mBox.Max);
            // Make sure the collision point and the projection aren't too far away 
            // --> they should be within the binomial stepping routine tollerence.
            if (Vector3.Dot(point - point_on_box, point - point_on_box) > physicsManager.BISECTION_TOLLERANCE)
                throw new Exception("collisionUtils::TestCollisionSphereAABB() - Point projection onto box is > 0.0001f, check inputs");
            normal = GetAABBNormalFromPointOnAABB(point_on_box, mBox.Min, mBox.Max);
            point = Vector3.Transform(point, objBMat_t0);
            normal = Vector3.Transform(normal, objBMat_t0);

            _cols.Add(new collision(collisionType.VERTEX_FACE, objA, objB, Tcollision, point, normal, e1, e2, coeffRestitution, 1.0f)); // Normal points out of face of B

            return true;
            
        }
        #endregion

        #region TestCollisionSphereAABBStatic(gameObject objA, gameObject objB)
        protected static bool TestCollisionSphereAABBStatic(gameObject objA, gameObject objB, ref float separationDistance)
        {
            BoundingSphere mSphere = (BoundingSphere)objA.boundingObj;
            BoundingBox mBox = (BoundingBox)objB.boundingObj;

            // Bring the sphere into common world coordinates to find the center points at t1 and t1
            // note, model center not necessarily at 0,0,0 in model coords --> Therefore need rotation as well
            objAMat_t1 = objA.CreateScale(objA.state.scale) * Matrix.CreateFromQuaternion(objA.state.orient) * Matrix.CreateTranslation(objA.state.pos);
            UpdateBoundingSphere(mSphere, objAMat_t1, objA.state.scale, objA, ref objACenter_t1, ref objARadius_t1);


            // Bring BoundingSphere into box coordinate system --> Means we don't need to recalculate AABB dimensions in world 
            objBMat_t1 = objB.CreateScale(objB.state.scale) * Matrix.CreateFromQuaternion(objB.state.orient) * Matrix.CreateTranslation(objB.state.pos);
            objBMatInt_t1 = Matrix.Invert(objBMat_t1);
            objACenter_t1 = Vector3.Transform(objACenter_t1, objBMatInt_t1); // bring objA starting center into B's fram
            objARadius_t1 = objARadius_t1 *
                            (1.0f / Math.Max(Math.Max(objB.state.scale.X, objB.state.scale.Y), objB.state.scale.Z)) *
                            (1.0f / objB.modelScaleToNormalizeSize); // bring objA radius in B's frame

            Vector3 sphereProjectedOnAABB = ClosestPointOnAABB(objACenter_t1, (BoundingBox)objB.boundingObj);
            Vector3 AB = sphereProjectedOnAABB - objACenter_t1;

            separationDistance = (float)Math.Sqrt(Vector3.Dot(AB, AB)) - objARadius_t1;

            return (separationDistance <= 0.0f);
        }
        #endregion

        #region GetAABBNormalFromPointOnAABB()
        protected static Vector3 GetAABBNormalFromPointOnAABB(Vector3 point, Vector3 min, Vector3 max)
        {
            Vector3 retVal = Vector3.Zero;
            if (MyExtensions.testFloatEquality(point.X, min.X))
                retVal += new Vector3(-1.0f, 0.0f, 0.0f);
            else if (MyExtensions.testFloatEquality(point.X, max.X))
                retVal += new Vector3(+1.0f, 0.0f, 0.0f);
            if (MyExtensions.testFloatEquality(point.Y, min.Y))
                retVal += new Vector3(0.0f, -1.0f, 0.0f);
            else if (MyExtensions.testFloatEquality(point.Y, max.Y))
                retVal += new Vector3(0.0f, +1.0f, 0.0f);
            if (MyExtensions.testFloatEquality(point.Z, min.Z))
                retVal += new Vector3(0.0f, 0.0f, -1.0f);
            else if (MyExtensions.testFloatEquality(point.Z, max.Z))
                retVal += new Vector3(0.0f, 0.0f, +1.0f);
            
            if((float)Math.Abs(retVal.X)+(float)Math.Abs(retVal.Y)+(float)Math.Abs(retVal.Z) == 0.0f) // If the vector is still zero, the point must not be on the AABB
                throw new Exception("GetAABBNormalFromPointOnAABB() - Input point is not on the bounds of the AABB -> Cannot get normal");

            return Vector3.Normalize(retVal);
        }
        #endregion

        #region FindFaceRegionIntersection()
        /// FindFaceRegionIntersection() - Direct port of David Eberly's function
        /// ***********************************************************************
        protected static int FindFaceRegionIntersection(float ex, float ey, float ez,
                                                        float cx, float cy, float cz,
                                                        float vx, float vy, float vz,
                                                        ref float ix, ref float iy, ref float iz,
                                                        bool aboveFace,
                                                        float sphereRadius,
                                                        ref float Tcollision)
        {
            // Returns when and whether a sphere in the region above face +Z
            // intersects face +Z or any of its vertices or edges.  The input
            // aboveFace is true when the x and y coordinates are within the x and y
            // extents.  The function will still work if they are not, but it needs
            // to be false then, to avoid some checks that assume that x and y are
            // within the extents.  This function checks face z, and the vertex and
            // two edges that the velocity is headed towards on the face.

            // Check for already intersecting if above face.
            if (cz <= ez + sphereRadius && aboveFace)
            {
                Tcollision = 0.0f;
                return -1;
            }

            // Check for easy out (moving away on Z axis).
            if (vz >= 0.0f)
            {
                return 0;
            }

            float rsqr = sphereRadius*sphereRadius;

            float vsqrX = vz * vz + vx * vx;
            float vsqrY = vz * vz + vy * vy;
            float dx, dy, dz = cz - ez;
            float crossX, crossY;
            int signX, signY;

            // This determines which way the box is heading and finds the values of
            // CrossX and CrossY which are positive if the sphere center will not
            // pass through the box.  Then it is only necessary to check two edges,
            // the face and the vertex for intersection.

            if (vx >= 0.0f)
            {
                signX = 1;
                dx = cx - ex;
                crossX = vx*dz - vz*dx;
            }
            else
            {
                signX = -1;
                dx = cx + ex;
                crossX = vz*dx - vx*dz;
            }

            if (vy >= 0.0f)
            {
                signY = 1;
                dy = cy - ey;
                crossY = vy*dz - vz*dy;
            }
            else
            {
                signY = -1;
                dy = cy + ey;
                crossY = vz*dy - vy*dz;
            }

            // Does the circle intersect along the x edge?
            if (crossX > sphereRadius*vx*signX)
            {
                if (crossX*crossX > rsqr*vsqrX)
                {
                    // Sphere overshoots box on the x-axis (either side).
                    return 0;
                }

                // Does the circle hit the y edge?
                if (crossY > sphereRadius*vy*signY)
                {
                    // Potential vertex intersection.
                    if (crossY*crossY > rsqr*vsqrY)
                    {
                        // Sphere overshoots box on the y-axis (either side).
                        return 0;
                    }

                    Vector3 relVelocity = new Vector3(vx,vy,vz);
                    Vector3 D = new Vector3(dx,dy,dz);
                    Vector3 cross = Vector3.Cross(D, relVelocity);
                    if (cross.SquaredLength() > rsqr*relVelocity.SquaredLength())
                    {
                        // Sphere overshoots the box on the corner.
                        return 0;
                    }

                    Tcollision = GetVertexIntersection(dx, dy, dz, vx, vy, vz,
                        rsqr);
                    ix = ex*signX;
                    iy = ey*signY;
                }
                else
                {
                    // x-edge intersection
                    Tcollision = GetEdgeIntersection(dx, dz, vx, vz, vsqrX, rsqr);
                    ix = ex*signX;
                    iy = cy + vy*Tcollision;
                }
            }
            else
            {
                // Does the circle hit the y edge?
                if (crossY > sphereRadius*vy*signY)
                {
                    // Potential y-edge intersection.
                    if (crossY*crossY > rsqr*vsqrY)
                    {
                        // Sphere overshoots box on the y-axis (either side).
                        return 0;
                    }

                    Tcollision = GetEdgeIntersection(dy, dz, vy, vz, vsqrY, rsqr);
                    ix = cx + vx*Tcollision;
                    iy = ey*signY;
                }
                else
                {
                    // Face intersection (easy).
                    Tcollision = (-dz + sphereRadius)/vz;
                    ix = Tcollision*vx + cx;
                    iy = Tcollision*vy + cy;
                }
            }

            // z coordinate of any intersection must be the face of z.
            iz = ez;
            return 1;

        }
        #endregion

        #region GetVertexIntersection()
        /// GetVertexIntersection() - Direct port of David Eberly's function
        /// ***********************************************************************
        protected static float GetVertexIntersection(float dx, float dy, float dz,
                                                     float vx, float vy, float vz, 
                                                     float rsqr)
        {
            // Finds the time of a 3D line-sphere intersection between a line
            // P = Dt, where P = (dx, dy, dz) and D = (vx, vy, vz) and
            // a sphere of radius^2 rsqr.  Note: only valid if there is, in fact,
            // an intersection.

            float vsqr = vx*vx + vy*vy + vz*vz;
            float dot = dx*vx + dy*vy + dz*vz;
            float diff = dx*dx + dy*dy + dz*dz - rsqr;
            //float inv = Math<Real>::InvSqrt(Math<Real>::FAbs(dot*dot - vsqr*diff));
            float inv = 1.0f / (float)Math.Sqrt(Math.Abs(dot * dot - vsqr * diff));  // Don't worry about optimization
            return diff*inv/(1.0f - dot*inv);
        }
        #endregion

        #region GetEdgeIntersection()
        /// GetEdgeIntersection() - Direct port of David Eberly's function
        /// ***********************************************************************
        protected static float GetEdgeIntersection(float dx, float dz,
                                                   float vx, float vz,
                                                   float vsqr,
                                                   float rsqr)
        {
            float dot = vx*dx + vz*dz;
            float diff = dx*dx + dz*dz - rsqr;
            // float inv = Math<Real>::InvSqrt(Math<Real>::FAbs(dot*dot - vsqr*diff));
            float inv = 1.0f / (float)Math.Sqrt(Math.Abs(dot * dot - vsqr * diff)); // Don't worry about optimization
            return diff*inv/(1.0f - dot*inv);
        }
        #endregion

        #region FindEdgeRegionIntersection()
        /// FindEdgeRegionIntersection() - Direct port of David Eberly's function
        /// ***********************************************************************
        protected static int FindEdgeRegionIntersection(float ex, float ey, float ez, 
                                                        float cx, float cy, float cz, 
                                                        float vx, float vy, float vz,
                                                        ref float ix, ref float iy, ref float iz,
                                                        bool aboveEdge,
                                                        float sphereRadius,
                                                        ref float Tcollision )
        {
            // Assumes the sphere center is in the region above the x and z planes.
            // The input aboveEdge is true when the y coordinate is within the y
            // extents.  The function will still work if it is not, but it needs to be
            // false then, to avoid some checks that assume that y is within the
            // extent.  This function checks the edge that the region is above, as
            // well as does a "face region" check on the face it is heading towards.

            float dx = cx - ex;
            float dz = cz - ez;
            float rsqr = sphereRadius * sphereRadius;

            if (aboveEdge)
            {
                float diff = dx * dx + dz * dz - rsqr;
                if (diff <= 0.0f)
                {
                    // Circle is already intersecting the box.
                    Tcollision = 0.0f;
                    return -1;
                }
            }

            float dot = vx * dx + vz * dz;
            if (dot >= 0.0f)
            {
                // Circle not moving towards box.
                return 0;
            }

            // The value dotPerp splits the region of interest along the edge in the
            // middle of that region.
            float dotPerp = vz * dx - vx * dz;
            if (dotPerp >= 0.0f)
            {
                // Sphere moving towards +z face.
                if (vx >= 0.0f)
                {
                    // Passed corner, moving away from box.
                    return 0;
                }

                // Intersection with x-z edge.  If there is trouble with objects that
                // "scrape" the surface (velocity perpendicular to face normal, and
                // point of contact with a radius direction parallel to the face
                // normal), this check may need to be a little more inclusive (small
                // tolerance due to floating point errors) as the edge check needs
                // to get "scraping" objects (as they hit the edge with the point)
                // and the face region check will not catch it because the object is
                // not moving towards the face.
                if (dotPerp <= -sphereRadius * vx)
                {
                    return FindJustEdgeIntersection(cy, ez, ey, ex, dz, dx, vz, vy,
                        vx, ref iz, ref iy, ref ix, sphereRadius, ref Tcollision);
                }

                // Now, check the face of z for intersections.
                return FindFaceRegionIntersection(ex, ey, ez, cx, cy, cz, vx, vy,
                    vz, ref ix, ref iy, ref iz, false, sphereRadius, ref Tcollision);
            }
            else
            {
                // Sphere moving towards +x face.
                if (vz >= 0.0f)
                {
                    // Passed corner, moving away from box.
                    return 0;
                }

                // Check intersection with x-z edge.  See the note above about
                // "scraping" objects.
                if (dotPerp >= sphereRadius * vz)
                {
                    // Possible edge/vertex intersection.
                    return FindJustEdgeIntersection(cy, ex, ey, ez, dx, dz, vx, vy,
                        vz, ref ix, ref iy, ref iz, sphereRadius, ref Tcollision);
                }

                // Now, check the face of x for intersections.
                return FindFaceRegionIntersection(ez, ey, ex, cz, cy, cx, vz, vy,
                    vx, ref iz, ref iy, ref ix, false, sphereRadius, ref Tcollision);
            }
        }
        #endregion

        #region FindJustEdgeIntersection()
        /// FindJustEdgeIntersection() - Direct port of David Eberly's function
        /// ***********************************************************************
        protected static int FindJustEdgeIntersection (float cy, 
                                                       float ex, float ey, float ez, 
                                                       float dx, float dz, 
                                                       float vx, float vy, float vz,
                                                       ref float ix, ref float iy, ref float iz,
                                                       float sphereRadius,
                                                       ref float Tcollision)
        {
            // Finds the intersection of a point dx and dz away from an edge with
            // direction y.  The sphere is at a point cy, and the edge is at the
            // point ex.  Checks the edge and the vertex the velocity is heading
            // towards.

            float rsqr = sphereRadius*sphereRadius;
            float dy, crossZ, crossX;  // possible edge/vertex intersection
            int signY;

            // Depending on the sign of Vy, pick the vertex that the velocity is
            // heading towards on the edge, as well as creating crossX and crossZ
            // such that their sign will always be positive if the sphere center goes
            // over that edge.

            if (vy >= 0.0f)
            {
                signY = 1;
                dy = cy - ey;
                crossZ = dx*vy - dy*vx;
                crossX = dz*vy - dy*vz;
            }
            else
            {
                signY = -1;
                dy = cy + ey;
                crossZ = dy*vx - dx*vy;
                crossX = dy*vz - dz*vy;
            }

            // Check where on edge this intersection will occur.
            if (crossZ >= 0.0f && crossX >= 0.0f
            &&  crossX*crossX + crossZ*crossZ >
                vy*vy*sphereRadius*sphereRadius)
            {
                // Sphere potentially intersects with vertex.
                Vector3 relVelocity = new Vector3(vx, vy, vz);
                Vector3 D = new Vector3(dx, dy, dz);
                Vector3 cross = Vector3.Cross(D,relVelocity);
                if (cross.SquaredLength() > rsqr*relVelocity.SquaredLength())
                {
                    // Sphere overshoots the box on the vertex.
                    return 0;
                }

                // Sphere actually does intersect the vertex.
                Tcollision = GetVertexIntersection(dx, dy, dz, vx, vy, vz, rsqr);
                ix = ex;
                iy = signY*ey;
                iz = ez;
            }
            else
            {
                // Sphere intersects with edge.
                float vsqrX = vz*vz + vx*vx;
                Tcollision = GetEdgeIntersection(dx, dz, vx, vz, vsqrX, rsqr);
                ix = ex;
                iy = cy + Tcollision * vy;
                iz = ez;
            }
            return 1;
        }
        #endregion

        #region FindVertexRegionIntersection()
        /// FindVertexRegionIntersection() - Direct port of David Eberly's function
        /// ***********************************************************************
        protected static int FindVertexRegionIntersection(float ex, float ey, float ez, 
                                                          float cx, float cy, float cz, 
                                                          float vx, float vy, float vz,
                                                          ref float ix, ref float iy, ref float iz,
                                                          float sphereRadius,
                                                          ref float Tcollision)
        {
            // Assumes the sphere is above the vertex +ex, +ey, +ez.

            float dx = cx - ex;
            float dy = cy - ey;
            float dz = cz - ez;
            float rsqr = sphereRadius * sphereRadius;
            float diff = dx * dx + dy * dy + dz * dz - rsqr;
            if (diff <= 0.0f)
            {
                // Sphere is already intersecting the box.
                Tcollision = 0.0f;
                return -1;
            }

            if (vx * dx + vy * dy + vz * dz >= 0.0f)
            {
                // Sphere not moving towards box.
                return 0;
            }

            // The box can be divided up into 3 regions, which simplifies checking to
            // see what the sphere hits.  The regions are divided by which edge
            // (formed by the vertex and some axis) is closest to the velocity
            // vector.
            //
            // To check if it hits the vertex, look at the edge (E) it is going
            // towards.  Create a plane formed by the other two edges (with E as the
            // plane normal) with the vertex at the origin.  The intercept along an
            // axis, in that plane, of the line formed by the sphere center and the
            // velocity as its direction, will be fCrossAxis/fVEdge.  Thus, the
            // distance from the origin to the point in the plane that that line from
            // the sphere in the velocity direction crosses will be the squared sum
            // of those two intercepts.  If that sum is less than the radius squared,
            // then the sphere will hit the vertex otherwise, it will continue on
            // past the vertex.  If it misses, since it is known which edge the box
            // is near, the find edge region test can be used.
            //
            // Also, due to the constrained conditions, only those six cases (two for
            // each region, since fCrossEdge can be + or -) of signs of fCross values
            // can occur.
            //
            // The 3rd case will also pick up cases where D = V, causing a ZERO cross.

            float crossX = vy * dz - vz * dy;
            float crossY = vx * dz - vz * dx;
            float crossZ = vy * dx - vx * dy;
            float crX2 = crossX * crossX;
            float crY2 = crossY * crossY;
            float crZ2 = crossZ * crossZ;
            float vx2 = vx * vx;
            float vy2 = vy * vy;
            float vz2 = vz * vz;

            // Intersection with the vertex?
            if (crossY < 0.0f
            && crossZ >= 0.0f
            && crY2 + crZ2 <= rsqr * vx2

            || crossZ < 0.0f
            && crossX < 0.0f
            && crX2 + crZ2 <= rsqr * vy2

            || crossY >= 0.0f
            && crossX >= 0.0f
            && crX2 + crY2 <= rsqr * vz2)
            {
                // Standard line-sphere intersection.
                Tcollision = GetVertexIntersection(dx, dy, dz, vx, vy, vz,
                    sphereRadius * sphereRadius);
                ix = Tcollision * vx + cx;
                iy = Tcollision * vy + cy;
                iz = Tcollision * vz + cz;
                return 1;
            }
            else if (crossY < 0.0f && crossZ >= 0.0f)
            {
                // x edge region, check y,z planes.
                return FindEdgeRegionIntersection(ey, ex, ez, cy, cx, cz, vy, vx,
                    vz, ref iy, ref ix, ref iz, false, sphereRadius, ref Tcollision);
            }
            else if (crossZ < 0.0f && crossX < 0.0f)
            {
                // y edge region, check x,z planes.
                return FindEdgeRegionIntersection(ex, ey, ez, cx, cy, cz, vx, vy,
                    vz, ref ix, ref iy, ref iz, false, sphereRadius, ref Tcollision);
            }
            else // crossY >= 0 && crossX >= 0
            {
                // z edge region, check x,y planes.
                return FindEdgeRegionIntersection(ex, ez, ey, cx, cz, cy, vx, vz,
                    vy, ref ix, ref iz, ref iy, false, sphereRadius, ref Tcollision);
            }
        }
        #endregion

        #region ClosestPointOnAABB(Vector3 point, BoundingBox xBox)
        protected static Vector3 ClosestPointOnAABB(Vector3 Point, BoundingBox xBox)
        {
            Vector3 xClosestPoint;
            xClosestPoint.X = (Point.X < xBox.Min.X) ? xBox.Min.X : (Point.X > xBox.Max.X) ? xBox.Max.X : Point.X;
            xClosestPoint.Y = (Point.Y < xBox.Min.Y) ? xBox.Min.Y : (Point.Y > xBox.Max.Y) ? xBox.Max.Y : Point.Y;
            xClosestPoint.Z = (Point.Z < xBox.Min.Z) ? xBox.Min.Z : (Point.Z > xBox.Max.Z) ? xBox.Max.Z : Point.Z;

            return xClosestPoint;
        }
        #endregion

        #region ClosestPointOnAABB(Vector3 point, Vector3 Min, Vector3 Max)
        protected static Vector3 ClosestPointOnAABB(Vector3 point, Vector3 Min, Vector3 Max)
        {
            Vector3 xClosestPoint;
            xClosestPoint.X = (point.X < Min.X) ? Min.X : (point.X > Max.X) ? Max.X : point.X;
            xClosestPoint.Y = (point.Y < Min.Y) ? Min.Y : (point.Y > Max.Y) ? Max.Y : point.Y;
            xClosestPoint.Z = (point.Z < Min.Z) ? Min.Z : (point.Z > Max.Z) ? Max.Z : point.Z;

            return xClosestPoint;
        }
        #endregion

        #region TestCollisionPointAABB(Vector3 point, BoundingBox xBox) - NOT TESTED
        protected static bool TestCollisionPointAABB(Vector3 point, BoundingBox xBox)
        {
            // For the point to be in the bounding box, it must be in all the ranges
            if (point.X < xBox.Min.X || point.X > xBox.Max.X)
                return false;
            else if (point.Y < xBox.Min.Y || point.Y > xBox.Max.Y)
                return false;
            else if (point.Z < xBox.Min.Z || point.Z > xBox.Max.Z)
                return false;
            else
                return true;
        }
        #endregion

        #region TestCollisionRayAABB() - NOT TESTED
        /// testCollisionRayAABB() - Test whether a ray intersects an AABB
        /// Code derived from: Pages 179-181 Real Time Collision Detection, Christer Ericson
        /// Intersect ray R(t) = p + t*d against AABB
        /// ***********************************************************************
        protected static bool TestCollisionRayAABB(Vector3 p, Vector3 d, BoundingBox AABB, ref float Tcollision, ref Vector3 q)
        {
            Tcollision = 0.0f;
            float tmax = float.MaxValue;

            // for all three slabs
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(d.GetAt(i)) < EPSILON)
                {
                    // Ray is parallel to the slab --> No hit if origin not within slab
                    if (p.GetAt(i) < AABB.Min.GetAt(i) || p.GetAt(i) > AABB.Max.GetAt(i))
                        return false;
                }
                else
                {
                    // Compute intersection t value of ray with near and far plane of slab
                    float ood = 1.0f / d.GetAt(i);
                    float t1 = (AABB.Min.GetAt(i) - p.GetAt(i)) * ood;
                    float t2 = (AABB.Max.GetAt(i) - p.GetAt(i)) * ood;
                    // Make t1 be intersection with near plane, t2 with far plane
                    if (t1 > t2)
                        Swap(t1, t2);
                    // Compute the intersection of slab intersection intervals
                    Tcollision = Max(Tcollision, t1);
                    tmax = Min(tmax, t2);
                    // Exit with no collision as soon as slab intersection becomes empty
                    if (Tcollision > tmax)
                        return false;
                }
            }

            // Ray intersections all 3 slabs. Return point q and intersection time
            q = p + d * Tcollision;
            return true;
        }
        #endregion

        #region Swap(float x, float y) - NOT TESTED
        /// Just swap the two variables
        /// ***********************************************************************
        protected static void Swap(float x, float y)
        {
            float temp = y;
            y = x;
            x = temp;
        }
        #endregion

        #region Corner(BoundingBox b, int n) - NOT TESTED
        /// Support function that returns the AABB vertex with index n
        /// ***********************************************************************
        protected static Vector3 Corner(BoundingBox b, int n)
        {
            Vector3 p = new Vector3();
            p.X = (((n & 1)!=0) ? b.Max.X : b.Min.X);
            p.Y = (((n & 2)!=0) ? b.Max.Y : b.Min.Y);
            p.Z = (((n & 4)!=0) ? b.Max.Z : b.Min.Z);
            return p;
        }
        #endregion
    }

}
