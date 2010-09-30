﻿#region using statements
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
    public enum collisionType { COL_UNDEFINED, VERTEX_FACE, EDGE_EDGE };
    #endregion

    public class collision
    {
        #region local and static variables
        public collisionType colType;
        public Object obj1;       // Body containing vertex
        public Object obj2;       // Body containing face
        public Vector3 colPoint;  // In world coordinates
        public Vector3 colNorm;   // Outwards pointing normal of face for obj2
        public Vector3 e1;        // Edge direction for obj1
        public Vector3 e2;        // Edge direction for obj2
        public float coeffRestitution;
        public float colTime;
        public float collisionScale; // If AABB Vs. AABB intersect, then momentum will be distributed accross 4 points

        // Some static variables to prevent constant allocation on the heap --> Using David Barraf's RBO introduction code
        protected static Vector3 padot = new Vector3();
        protected static Vector3 pbdot = new Vector3();
        protected static Vector3 ra = new Vector3();
        protected static Vector3 rb = new Vector3();
        protected static Vector3 force = new Vector3();
        protected static Vector3 n = new Vector3();
        protected static float vrel = 0.0f;
        protected static float numerator = 0.0f;
        protected static float term1 = 1.0f, term2 = 1.0f, term3 = 1.0f, term4 = 1.0f;
        protected static float j;
        protected static float V_COLLIDING_THRESHOLD = 0.001f;
        protected static float IMPULSE_TO_PREVENT_RESTING = 0.0001f;
        protected static Matrix zeroMat = new Matrix(0.0f, 0.0f, 0.0f, 0.0f,
                                                     0.0f, 0.0f, 0.0f, 0.0f,
                                                     0.0f, 0.0f, 0.0f, 0.0f,
                                                     0.0f, 0.0f, 0.0f, 0.0f);
        protected static Vector3 temp = new Vector3();
        #endregion

        #region Constructor - collision(...full constructor...)
        public collision(collisionType _colType, gameObject _obj1, gameObject _obj2, float _Tcollision, 
                         Vector3 _colPoint, Vector3 _colNorm, Vector3 _e1, Vector3 _e2, float _coeffRestitution, float _collisionScale)
        {
            colType = _colType;
            obj1 = _obj1;
            obj2 = _obj2;
            colPoint = _colPoint;
            colNorm = _colNorm;
            colTime = _Tcollision;
            e1 = _e1;
            e2 = _e2;
            coeffRestitution = _coeffRestitution;
            collisionScale = _collisionScale;

        }
        #endregion

        #region Constructor - collision(float Tcollision)
        public collision(float _Tcollision)
        {
            colType = collisionType.COL_UNDEFINED;
            obj1 = null;
            obj2 = null;
            colPoint = Vector3.Zero;
            colNorm = Vector3.Zero;
            colTime = _Tcollision;
            e1 = Vector3.Zero;
            e2 = Vector3.Zero;
            coeffRestitution = 1.0f;
            collisionScale = 1.0f;

        }
        #endregion

        #region ResolveCollision()
        /// ResolveCollision() - Add impulse for collision contacts and return false in this case.  
        /// Return true if the contact is a resting contact and needs processing later.
        /// ***********************************************************************
        public bool ResolveCollision(List<gameObject> gameObjects, ref rboState obj1State, ref rboState obj2State)
        {

            if (CheckCollidingContact(ref obj1State, ref obj2State))
            {
                ResolveCollidingCollision(ref obj1State, ref obj2State);
                return false;
            }
            else
                return true;
        }
        #endregion

        #region ResolveCollidingCollision()
        /// ResolveCollidingCollision() - Add impulses for collision contacts
        /// ***********************************************************************
        public void ResolveCollidingCollision(ref rboState obj1State, ref rboState obj2State)
        {
            padot = GetPointVelocity(ref obj1State, colPoint);
            pbdot = GetPointVelocity(ref obj2State, colPoint);
            n = colNorm;
            ra = colPoint - obj1State.pos;
            rb = colPoint - obj2State.pos;
            vrel = Vector3.Dot(n, (padot - pbdot));
            numerator = -(1.0f + coeffRestitution) * vrel;

            // Calculate the denominator in four parts
            if (((gameObject)obj1).movable)
            {
                term1 = 1.0f / obj1State.mass;
                term3 = Vector3.Dot(n, Vector3.Cross(Vector3.Transform(Vector3.Cross(ra, n), obj1State.Iinv), ra));
            }
            else
            {
                term1 = 0.0f; // Infinite mass
                term3 = Vector3.Dot(n, Vector3.Cross(Vector3.Transform(Vector3.Cross(ra, n), zeroMat), ra)); // Infinite mass
            }
            if (((gameObject)obj2).movable)
            {
                term2 = 1.0f / obj2State.mass;
                term4 = Vector3.Dot(n, Vector3.Cross(Vector3.Transform(Vector3.Cross(rb, n), obj2State.Iinv), rb));
            }
            else
            {
                term2 = 0.0f; // Infinite mass
                term4 = Vector3.Dot(n, Vector3.Cross(Vector3.Transform(Vector3.Cross(rb, n), zeroMat), rb));  // Infinite mass
            }

            // Compute the impulse magnitude
            j = numerator / (term1 + term2 + term3 + term4);
            force = j * n;

            // Apply impulse to the two bodies
            temp = collisionScale * force;
            temp.X *= gameSettings.collisionMask.X; temp.Y *= gameSettings.collisionMask.Y; temp.Z *= gameSettings.collisionMask.Z; 
            if (((gameObject)obj1).movable)
                obj1State.linearMom += temp;
            if (((gameObject)obj2).movable)
                obj2State.linearMom -= temp;

            temp = collisionScale * Vector3.Cross(ra, force);
            temp.X *= gameSettings.collisionMask.X; temp.Y *= gameSettings.collisionMask.Y; temp.Z *= gameSettings.collisionMask.Z; 
            if (((gameObject)obj2).movable)
                obj1State.angularMom += temp;
            obj1State.RecalculateDerivedQuantities();

            temp = collisionScale * Vector3.Cross(rb, force);
            temp.X *= gameSettings.collisionMask.X; temp.Y *= gameSettings.collisionMask.Y; temp.Z *= gameSettings.collisionMask.Z; 
            if (((gameObject)obj2).movable)
                obj2State.angularMom -= temp;
            obj2State.RecalculateDerivedQuantities();
        }
        #endregion

        #region GetPointVelocity()
        /// GetPointVelocity() - Return the velocity of a point on a rigid body
        /// ***********************************************************************
        protected static Vector3 GetPointVelocity(ref rboState objState, Vector3 point)
        {
            return objState.linearVel + Vector3.Cross(objState.angularVel, (point - objState.pos));
        }
        #endregion

        #region CheckCollidingContact()
        /// CheckCollidingContact() - Check if the current collision is colliding
        /// ***********************************************************************
        public bool CheckCollidingContact(ref rboState obj1State, ref rboState obj2State)
        {
            padot = GetPointVelocity(ref obj1State, colPoint);
            pbdot = GetPointVelocity(ref obj2State, colPoint);
            vrel = Vector3.Dot(colNorm,(padot - pbdot));

            if (vrel > V_COLLIDING_THRESHOLD)
                return false;
            if (vrel > -V_COLLIDING_THRESHOLD) // -vrel < V_COLLIDING_THESHOLD
                return false;
            else
                return true;
        }
        #endregion

        #region GetMomForCollidingContact()
        /// GetMomForCollidingContact() - Find the Momentum impulse if we want to satisfy vref < -V_COLLIDING_THRESHOLD
        /// ***********************************************************************
        public Vector3 GetMomForCollidingContact(ref rboState obj1State, ref rboState obj2State)
        {
            #region OLD CODE
            /*
            // Want, -V_COLLIDING_THRESHOLD > vrel
            //   --> -V_COLLIDING_THRESHOLD > (padot - pbdot) . colNorm
            //   --> -V_COLLIDING_THRESHOLD > (padot . colNorm) - (pbdot . colNorm) 
            //   --> -V_COLLIDING_THRESHOLD + (pbdot . colNorm) > (padot . colNorm)
            pbdot = GetPointVelocity(ref obj2State, colPoint);
            float LHS = -V_COLLIDING_THRESHOLD + Vector3.Dot(colNorm, pbdot);

            //   -->  LHS > (linearVelA + [angularVelA x (p - posA)]) . colNorm
            //   -->  LHS > (linearVelA . colNorm) + ([angularVelA x (p - posA)]) . colNorm)
            //   -->  LHS - ([angularVelA x (p - posA)]) . colNorm) > (linearVelA . colNorm)
            float LHS2 = LHS - Vector3.Dot(Vector3.Cross(obj1State.angularVel, (colPoint - obj1State.pos)), colNorm);

            //   -->  LHS2 > (linearVelA + (deltaV*colNorm)) . colNorm)
            //   -->  LHS2 > (linearVelA . colNorm) + (deltaV*colNorm . colNorm) 
            //   Note: colNorm is normalized --> colNorm . colNorm = 1!
            //   -->  LHS2 - linearVelA . colNorm > deltaV
            float LHS3 = LHS2 - Vector3.Dot(obj1State.linearVel,colNorm);

            // Add a little margin
            float deltaV = LHS3 - V_IMPULSE_TO_PREVENT_RESTING;
            // Return the deltaV in the direction of the collision norm added to the origional velocity.
            return deltaV * colNorm + obj1State.linearVel;
            */
            #endregion

            float vrel = GetRelativeVelocity(ref obj1State, ref obj2State);
            return (colNorm * (vrel + IMPULSE_TO_PREVENT_RESTING));
        }
        #endregion

        #region GetRelativeVelocity()
        public float GetRelativeVelocity(ref rboState obj1State, ref rboState obj2State)
        {
            padot = GetPointVelocity(ref obj1State, colPoint);
            pbdot = GetPointVelocity(ref obj2State, colPoint);
            vrel = Vector3.Dot(colNorm,(padot - pbdot));
            return vrel;
        }
        #endregion
    }
}
