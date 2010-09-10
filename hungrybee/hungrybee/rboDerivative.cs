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
    /// **                          rboDerivative                            **
    /// ** Structure to hold the rboState information                        **
    /// ***********************************************************************
    /// </summary>
    public class rboDerivative
    {
        #region Local Variables

        public Vector3 linearVel;
        public Vector3 force;
        public Quaternion spin;
        public Vector3 torque;

        #endregion

        #region Constructor - rboDerivative()
        /// Constructor - rboDerivative()
        /// ***********************************************************************
        public rboDerivative()
        {
            linearVel = Vector3.Zero;
            force = Vector3.Zero;
            spin = Quaternion.Identity;
            torque = Vector3.Zero;
        }
        #endregion

        #region Evaluate() - Evaluate Piecewise derivative
        /// Evaluate() - Evaluate derivative values (1 Euler step) at t+dt
        /// ***********************************************************************
        public void Evaluate(rboState state, rboState initialState, float time, float deltaTime, rboDerivative derivative, gameObject rboObject)
        {
            state.pos           = initialState.pos          + derivative.linearVel * deltaTime;
            state.linearMom     = initialState.linearMom    + derivative.force * deltaTime;
            state.orient        = initialState.orient       + derivative.spin * deltaTime;
            state.angularMom    = initialState.angularMom   + derivative.torque * deltaTime;
            state.RecalculateDerivedQuantities();

            this.linearVel = state.linearVel;
            //physicsManager.ClipVelocity(ref this.linearVel, rboObject.h_game.GetGameSettings().physicsMinVel, rboObject.maxVel);

            this.spin = state.spin;
            // Get force and torque from rboObject at time & deltaTime
            rboObject.GetForceTorque(ref this.force, ref this.torque, ref state, time);

        }
        #endregion

        #region Evaluate() - Evaluate single derivative
        /// Evaluate() - Evaluate derivative values (1 Euler step) at t+dt
        /// ***********************************************************************
        public void Evaluate(rboState state, float time, gameObject rboObject)
        {
            this.linearVel = state.linearVel;
            //physicsManager.ClipVelocity(ref this.linearVel, rboObject.h_game.GetGameSettings().physicsMinVel, rboObject.maxVel);

            this.spin = state.spin;
            // Get force and torque from rboObject at time & deltaTime
            rboObject.GetForceTorque(ref this.force, ref this.torque, ref state, time);
        }
        #endregion
    }
}
