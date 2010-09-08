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
    /// **                             rboState                              **
    /// ** Structure to hold the rboState information                        **
    /// ***********************************************************************
    /// </summary>
    public class rboState
    {
        #region Local Variables

        // State RBO quantities
        public Vector3 pos;
        public Vector3 linearMom;
        public Quaternion orient;
        public Vector3 angularMom;
        public Vector3 scale; // Scale in X, Y and Z

        // Derived RBO quantities
        public Vector3 linearVel;
        public Quaternion spin;
        public Vector3 angularVel;

        // Constant RBO quantities
        public float mass;
        public float inverseMass;
        public Matrix Itensor;
        public Matrix InvItensor;

        #endregion

        #region Constructor - rboState()
        /// Constructor - rboState()
        /// ***********************************************************************
        public rboState()
        {
            // State RBO quantities
            pos = Vector3.Zero;
            linearMom = Vector3.Zero;
            orient = Quaternion.Identity;
            angularMom = Vector3.Zero;

            // Derived RBO quantities
            linearVel = Vector3.Zero;
            spin = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            angularVel = Vector3.Zero;

            // Constant RBO quantities
            mass = 1.0f;
            inverseMass = 1.0f / mass;
            Itensor = Matrix.Identity;
            InvItensor = Matrix.Identity;
        }
        #endregion

        #region Constructor - rboState()
        /// Constructor - rboState()
        /// ***********************************************************************
        public static void CopyStateQuantitiesAtoB(rboState A, rboState B)
        {
            // Just copy state RBO quantities
            B.pos.X = A.pos.X; B.pos.Y = A.pos.Y; B.pos.Z = A.pos.Z;
            B.linearMom.X = A.linearMom.X; B.linearMom.Y = A.linearMom.Y; B.linearMom.Z = A.linearMom.Z;
            B.orient.W = A.orient.W; B.orient.X = A.orient.X; B.orient.Y = A.orient.Y; B.orient.Z = A.orient.Z;
            B.angularMom.X = A.angularMom.X; B.angularMom.Y = A.angularMom.Y; B.angularMom.Z = A.angularMom.Z;
            B.scale.X = A.scale.X; B.scale.Y = A.scale.Y; B.scale.Z = A.scale.Z;
        }
        #endregion

        #region RecalculateDerivedQuantities()
        /// RecalculateDerivedQuantities()
        /// ***********************************************************************
        public void RecalculateDerivedQuantities()
        {
            linearVel = linearMom * inverseMass;
            angularVel = Vector3.Transform(angularMom, InvItensor);
            orient.Normalize();
            spin.W = 0.0f; spin.X = angularVel.X; spin.Y = angularVel.Y; spin.Z = angularVel.Z;
            spin = Quaternion.Multiply(spin, 0.5f);
            spin = spin * orient;
        }
        #endregion
    }
    
}
