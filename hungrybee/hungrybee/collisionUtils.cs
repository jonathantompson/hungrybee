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

    class collisionUtils
    {
        // Temporary variables to avoid continuous allocation and deallocation on the stack
        static Matrix objAMat_t0 = new Matrix();
        static Matrix objBMat_t0 = new Matrix();
        static Matrix objAMat_t1 = new Matrix();
        static Matrix objBMat_t1 = new Matrix();
        static Vector3 objACenter_t0 = new Vector3();
        static Vector3 objBCenter_t0 = new Vector3();
        static Vector3 objACenter_t1 = new Vector3();
        static Vector3 objBCenter_t1 = new Vector3();
        static float objARadius_t0 = 0.0f;
        static float objBRadius_t0 = 0.0f;

        static float EPSILON = 0.0000000001f;

        #region testCollision()
        /// testCollision() - Just grab input objects and send them to their proper functions
        /// ***********************************************************************
        public static bool testCollision(gameObject objA, gameObject objB, ref float Tcollision )
        {
            // Big dumb if else chain
            // --> Probably data directed programming would be better here, but not too many types
            if (objA.boundingObjType == boundingObjType.UNDEFINED || objB.boundingObjType == boundingObjType.UNDEFINED)
                throw new Exception("collisionUtils::testCollision(): Trying to test collision on UNDEFINED object types (maybe forgot to initialize objects?)");
            else if (objA.boundingObjType == boundingObjType.SPHERE && objB.boundingObjType == boundingObjType.SPHERE)
                return collisionUtils.testCollisionSphereSphere(objA, objB, ref Tcollision);
            else
                throw new Exception("collisionUtils::testCollision(): Trying to test collision on unrecognized object types");
        }
        #endregion

        #region testCollisionSphereSphere()
        /// testCollisionSphereSphere() - Test whether two swept sphere collide between prevState and state
        /// A FEW ASSUMPTIONS:  1. Scale factor is constant between frames.
        ///                     2. No acceleration --> spheres are linearly swept between the two points with constant velocity
        /// Code derived from: http://www.gamasutra.com/view/feature/3383/simple_intersection_tests_for_games.php?page2
        /// NOTE: There are faster methods to perform this test --> to avoid sqrt calls AND to avoid floating point accuracy issues for large velocities
        /// ***********************************************************************
        protected static bool testCollisionSphereSphere(gameObject objA, gameObject objB, ref float Tcollision )
        {
            // Bring both spheres into common world coordinates to find the center points at t0 and t1
            // note, model center not necessarily at 0,0,0 in model coords --> Therefore need rotation as well
            objAMat_t0 = objA.CreateScale(objA.prevState.scale) * Matrix.CreateFromQuaternion(objA.prevState.orient) * Matrix.CreateTranslation(objA.prevState.pos);
            objBMat_t0 = objB.CreateScale(objB.prevState.scale) * Matrix.CreateFromQuaternion(objB.prevState.orient) * Matrix.CreateTranslation(objB.prevState.pos);
            objACenter_t0 = Vector3.Transform(((BoundingSphere)objA.boundingObj).Center, objAMat_t0);
            objBCenter_t0 = Vector3.Transform(((BoundingSphere)objB.boundingObj).Center, objBMat_t0);

            objAMat_t1 = objA.CreateScale(objA.state.scale) * Matrix.CreateFromQuaternion(objA.state.orient) * Matrix.CreateTranslation(objA.state.pos);
            objBMat_t1 = objB.CreateScale(objB.state.scale) * Matrix.CreateFromQuaternion(objB.state.orient) * Matrix.CreateTranslation(objB.state.pos);
            objACenter_t1 = Vector3.Transform(((BoundingSphere)objA.boundingObj).Center, objAMat_t1);
            objBCenter_t1 = Vector3.Transform(((BoundingSphere)objB.boundingObj).Center, objBMat_t1);

            // Scale radius by the largest scale factor (may be non-uniform), then by gameObject normalization factor
            // Actually radius * modelScaleToNormalizeSize = 1 ALWAYS...  But do it anyway for completeness
            objARadius_t0 = ((BoundingSphere)objA.boundingObj).Radius *                                                     // Origional radius
                            Math.Max(Math.Max(objA.prevState.scale.X, objA.prevState.scale.Y), objA.prevState.scale.Z) *    // Scale factor
                            objA.modelScaleToNormalizeSize;                                                                 // Normalization factor
            objBRadius_t0 = ((BoundingSphere)objB.boundingObj).Radius *                                                     // Origional radius
                            Math.Max(Math.Max(objB.prevState.scale.X, objB.prevState.scale.Y), objB.prevState.scale.Z) *    // Scale factor
                            objB.modelScaleToNormalizeSize;                                                                 // Normalization factor

            // Find the velocity by gettting change in position and devide by time (V' = dx' / dt)
            Vector3 va = objACenter_t1 - objACenter_t0; // Vector from A0 to A1
            Vector3 vb = objBCenter_t1 - objBCenter_t0; // Vector from B0 to B1
            Vector3 AB = objBCenter_t0 - objACenter_t0; // Vector from A0 to B0
            Vector3 vab = vb - va; // Relative velocity (in normalized time)
            float rab = objARadius_t0 + objBRadius_t0;
            float a = Vector3.Dot(vab,vab); // u * u coefficient
            float b = Vector3.Dot(2 * vab, AB);
            float c = Vector3.Dot(AB, AB) - rab * rab;

            // Check if they're currently overlapping
            if ( Vector3.Dot(AB,AB) <= rab * rab )
            {
                Tcollision = 0.0f;
                return true;
            }

            float t0 = 0.0f, t1 = 0.0f;
            if ( QuadraticFormula( a, b, c, ref t0, ref t1 ) )
            {
                if(t0 < t1) // t0 occured first
                {
                    if( t0 >= 0.0f && t0 <= 1.0f) // t0 is within 0-->1
                    {
                    Tcollision = t0 * ( objA.state.time - objA.prevState.time) + objA.prevState.time;
                    return true;
                    }
                }
                else 
                {
                    if (t1 >= 0.0f && t1 <= 1.0f)  // t1 occured first and is within 0-->1
                    {
                    Tcollision = t1 * ( objA.state.time - objA.prevState.time) + objA.prevState.time;
                    return true;
                    }
                }
            }

            // Otherwise no contact
            return false;
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
    }
}
