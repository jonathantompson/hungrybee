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

    /// ** force
    /// ** An interface class to add different forces to an RBO
    /// ***********************************************************************
    #region abstract class force
    public abstract class force
    {
        public abstract Vector3 GetForce(ref rboState state, float time);
        public abstract Vector3 GetTorque(ref rboState state, float time);
    }
    #endregion

    /// ** forceSlowDown
    /// ** Force to slow down an object
    /// ***********************************************************************
    #region forceSlowDown : force
    public class forceSlowDown : force
    {
        public float stopTime;

        public forceSlowDown()
        {
            stopTime = 0.0f;
        }

        public override Vector3 GetForce(ref rboState state, float time)
        {
            float velocity = (float)Math.Sqrt(Vector3.Dot(state.linearVel, state.linearVel));
            if (time > stopTime || velocity < 0.00000001f)
                return Vector3.Zero;

            // Get direction of the force to slow down
            Vector3 force = new Vector3();
            force.X = state.linearVel.X; force.Y = state.linearVel.Y; force.Z = state.linearVel.Z;
            force = Vector3.Normalize(force); // Decellerate in the direction oposite the velocity

            // Calculate force required to stop in desired time
            force = Vector3.Negate(force) * state.mass * velocity / (stopTime - time); // accel = dV/dT
            return force;
        }

        public override Vector3 GetTorque(ref rboState state, float time)
        {
            return Vector3.Zero;
        }

        public void SetStopTime(float _stopTime)
        {
            stopTime = _stopTime;
        }

    }
    #endregion

    /// ** forceGravity 
    /// ** Force to apply gravity to an object
    /// ***********************************************************************
    #region forceGravity : force
    public class forceGravity : force
    {
        public Vector3 acceleration;

        public forceGravity(Vector3 _acceleration)
        {
            acceleration = _acceleration;
        }

        public override Vector3 GetForce(ref rboState state, float time)
        {
            return acceleration * state.mass;
        }

        public override Vector3 GetTorque(ref rboState state, float time)
        {
            return Vector3.Zero;
        }
    }
    #endregion

    /// **  forceAntiGravity 
    /// ** Force to apply gravity to an object 
    /// ***********************************************************************
    #region forceAntiGravity : force
    public class forceAntiGravity : force
    {
        public Vector3 acceleration;

        public forceAntiGravity(Vector3 _acceleration)
        {
            acceleration = _acceleration;
        }

        public override Vector3 GetForce(ref rboState state, float time)
        {
            return acceleration * state.mass;
        }

        public override Vector3 GetTorque(ref rboState state, float time)
        {
            return Vector3.Zero;
        }
    }
    #endregion

    /// ** forcePlayerInput
    /// ** Force to apply a constant acceleration for as long as a button is
    /// ** pressed and set the desired orientation by applying torque
    /// ***********************************************************************
    #region forcePlayerInput : force
    public class forcePlayerInput : force
    {
        public Vector3 desiredVelocity;
        public Quaternion desiredOrientation;
        public float timeToReachVelocity;
        public float timeToReachOrientation;
        public float maxAcceleration;
        public float maxAcceleration_squared;

        // Some temp values to avoid constant memory allocation
        public static Vector3 rotationAxis = new Vector3();
        public static float rotationAngle = 0.0f;
        public static Quaternion rotError = new Quaternion();
        public static Quaternion orientionInverse = new Quaternion();
        public static float EPSILON = 0.00000000001f;

        public forcePlayerInput(Vector3 _desiredVelocity, Quaternion _desiredOrientation, float _timeToReachVelocity, float _maxAcceleration, float _timeToReachOrientation)
        {
            desiredVelocity = _desiredVelocity;
            timeToReachVelocity = _timeToReachVelocity;
            timeToReachOrientation = _timeToReachOrientation;
            maxAcceleration_squared = _maxAcceleration * _maxAcceleration;
            maxAcceleration = _maxAcceleration;
            desiredOrientation = _desiredOrientation;
        }

        public void SetVelocity(Vector3 _desiredVelocity)
        {
            desiredVelocity = _desiredVelocity;
        }

        public override Vector3 GetForce(ref rboState state, float time)
        {
            Vector3 a = (desiredVelocity - state.linearVel) / timeToReachVelocity;
            if( (a.X * a.X + a.Y * a.Y + a.Z * a.Z) > maxAcceleration_squared)
                a = Vector3.Normalize(a) * maxAcceleration;
            a.Y = 0.0f; a.Z = 0.0f; // Remove vertical and z component accelerations --> Now just horizontal
            return a * state.mass;
        }

        public void SetDesiredOrientationFromForwardVector(Vector3 forward)
        {
            // Want desiredOrientation to be a Quaterion to move player from <0,0,1> to the forward vector
            rotationAxis = Vector3.Normalize(Vector3.Cross(Vector3.Backward, forward));
            // http://en.wikipedia.org/wiki/Dot_product --> theta = arccos( a . b / (||a|| ||b||))
            //                                          --> theta = arccos( a . b )       <-- IF A AND B ARE UNIT NORMAL
            // Could also get angle from un-normalized cross product a x b = a.b.sin(theta).n^
            rotationAngle = (float)Math.Acos(Vector3.Dot(Vector3.Backward, Vector3.Normalize(forward)));
            desiredOrientation = Quaternion.CreateFromAxisAngle(rotationAxis, rotationAngle);
        }

        public override Vector3 GetTorque(ref rboState state, float time)
        {
            // Note: q^-1 = q* / ||q||^2     (q* is the conjugate)
            // Therefore for unit quaternions, q^-1 = q*
            orientionInverse = state.orient; orientionInverse.Conjugate();
            orientionInverse = Quaternion.Normalize(orientionInverse);
            rotError = Quaternion.Multiply(orientionInverse, desiredOrientation);
            rotError = Quaternion.Normalize(rotError);

            // NOW GET AN AXIS ANGLE REPRESENTATION TO GO FROM Q1 TO Q2
            MyExtensions.GetAxisAngleFromQuaternion(ref rotError, ref rotationAxis, ref rotationAngle);

            // Work out the current angular velocity around the axis
            float angularVel = Vector3.Dot(state.angularVel, rotationAxis);

            // Calculate the acceleration around the axis needed to get to the target orientation in the specified time
            // OMEGA = w_0 * t + 1/2 * a * t * t
            // a = 2 * (OMEGA - w_0 * t) / (t * t)                  (rad / s^2 )
            float angularAccel = -2.0f * (rotationAngle - angularVel * timeToReachOrientation) / (timeToReachOrientation * timeToReachOrientation);
        
            // torque = moment of inertia * angularAcceleration (angularAcceleration is a vector in direction of axis and length
            return Vector3.Transform(angularAccel * rotationAxis, state.Itensor);

        }
    }
    #endregion
}
