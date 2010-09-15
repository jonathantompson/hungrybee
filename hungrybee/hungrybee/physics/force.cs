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
    /// **                              force                                **
    /// ** An interface class to add different forces to an RBO              **
    /// ***********************************************************************
    /// </summary>
    public abstract class force
    {
        public abstract Vector3 GetForce(ref rboState state, float time);
    }

    /// <summary>
    /// ***********************************************************************
    /// **                         forceSlowDown                             **
    /// ** Force to slow down an object                                      **
    /// ***********************************************************************
    /// </summary>
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

        public void SetStopTime(float _stopTime)
        {
            stopTime = _stopTime;
        }

    }
    /// <summary>
    /// ***********************************************************************
    /// **                         forceGravity                              **
    /// ** Force to apply gravity to an object                               **
    /// ***********************************************************************
    /// </summary>
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
    }

    /// <summary>
    /// ***********************************************************************
    /// **                       forcePlayerInput                            **
    /// ** Force to apply a constant acceleration for as long as a button is **
    /// ** pressed                                                           **
    /// ***********************************************************************
    /// </summary>
    public class forcePlayerInput : force
    {
        public Vector3 acceleration;

        public forcePlayerInput(Vector3 _acceleration)
        {
            acceleration = _acceleration;
        }

        public void SetAcceleration(Vector3 _acceleration)
        {
            acceleration = _acceleration;
        }

        public override Vector3 GetForce(ref rboState state, float time)
        {
            return acceleration * state.mass;
        }
    }
}
