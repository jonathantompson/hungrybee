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
    /// <summary>
    /// ***********************************************************************
    /// **                             rboState                              **
    /// ** Structure to hold the rboState information                        **
    /// ***********************************************************************
    /// </summary>
    public class rboState
    {
        #region Local Variables

        // ************************************************************************
        // ************************************************************************
        // ******* IMPORTANT ADD ALL ADDITIONAL QUANTITIES TO COPY ROUTINES *******
        // ************************************************************************
        // ************************************************************************

        // State RBO quantities --> A dynamic quantity
        public Vector3 pos;
        public Vector3 linearMom;
        public Quaternion orient;
        public Vector3 angularMom;
        public Vector3 scale; // Scale in X, Y and Z
        public float time;

        // Derived RBO quantities --> A dynamic quantity
        public Vector3 linearVel;
        public Quaternion spin;
        public Vector3 angularVel;
        public Matrix Iinv;             // Inverse RBO tensor after transform
        public Matrix Rot;              // Rotational matrix

        // Constant RBO quantities --> A static quantity
        public float mass;
        public float inverseMass;
        public Matrix Itensor;          // Body tensor
        public Matrix InvItensor;       // Inverse Body tensor

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
            scale = Vector3.One;
            time = 0.0f;

            // Derived RBO quantities
            linearVel = Vector3.Zero;
            spin = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            angularVel = Vector3.Zero;
            Iinv = Matrix.Identity;
            Rot = Matrix.Identity;

            // Constant RBO quantities
            mass = 1.0f;
            inverseMass = 1.0f / mass;
            Itensor = Matrix.Identity;
            InvItensor = Matrix.Identity;
        }
        #endregion

        #region CopyStateQuantitiesAtoB()
        /// CopyStateQuantitiesAtoB() -  Deep copy only state quantities not derived or constant quantities
        /// ***********************************************************************
        public static void CopyStateQuantitiesAtoB(ref rboState A, ref rboState B)
        {
            // Just copy state RBO quantities
            B.pos.X = A.pos.X; B.pos.Y = A.pos.Y; B.pos.Z = A.pos.Z;
            B.linearMom.X = A.linearMom.X; B.linearMom.Y = A.linearMom.Y; B.linearMom.Z = A.linearMom.Z;
            B.orient.W = A.orient.W; B.orient.X = A.orient.X; B.orient.Y = A.orient.Y; B.orient.Z = A.orient.Z;
            B.angularMom.X = A.angularMom.X; B.angularMom.Y = A.angularMom.Y; B.angularMom.Z = A.angularMom.Z;
            B.scale.X = A.scale.X; B.scale.Y = A.scale.Y; B.scale.Z = A.scale.Z;
            A.time = B.time;
        }
        #endregion

        #region CopyDynamicQuantitiesAtoB()
        /// CopyDynamicQuantitiesAtoB() -  Deep copy state and derived quantities not constant quantities
        /// ***********************************************************************
        public static void CopyDynamicQuantitiesAtoB(ref rboState A, ref rboState B)
        {
            // Copy state RBO quantities
            B.pos.X = A.pos.X; B.pos.Y = A.pos.Y; B.pos.Z = A.pos.Z;
            B.linearMom.X = A.linearMom.X; B.linearMom.Y = A.linearMom.Y; B.linearMom.Z = A.linearMom.Z;
            B.orient.W = A.orient.W; B.orient.X = A.orient.X; B.orient.Y = A.orient.Y; B.orient.Z = A.orient.Z;
            B.angularMom.X = A.angularMom.X; B.angularMom.Y = A.angularMom.Y; B.angularMom.Z = A.angularMom.Z;
            B.scale.X = A.scale.X; B.scale.Y = A.scale.Y; B.scale.Z = A.scale.Z;
            A.time = B.time;

            // Copy derived RBO quantities
            B.linearVel.X = A.linearVel.X; B.linearVel.Y = A.linearVel.Y; B.linearVel.Z = A.linearVel.Z;
            B.spin.X = A.spin.X; B.spin.Y = A.spin.Y; B.spin.Z = A.spin.Z; B.spin.W = A.spin.W;
            B.angularVel.X = A.angularVel.X; B.angularVel.Y = A.angularVel.Y; B.angularVel.Z = A.angularVel.Z;
            B.Iinv.M11 = A.Iinv.M11; B.Iinv.M12 = A.Iinv.M12; B.Iinv.M13 = A.Iinv.M13; B.Iinv.M14 = A.Iinv.M14;
            B.Iinv.M21 = A.Iinv.M21; B.Iinv.M22 = A.Iinv.M22; B.Iinv.M23 = A.Iinv.M23; B.Iinv.M24 = A.Iinv.M24;
            B.Iinv.M31 = A.Iinv.M31; B.Iinv.M32 = A.Iinv.M32; B.Iinv.M33 = A.Iinv.M33; B.Iinv.M34 = A.Iinv.M34;
            B.Iinv.M41 = A.Iinv.M41; B.Iinv.M42 = A.Iinv.M42; B.Iinv.M43 = A.Iinv.M43; B.Iinv.M44 = A.Iinv.M44;
            B.Rot.M11 = A.Rot.M11; B.Rot.M12 = A.Rot.M12; B.Rot.M13 = A.Rot.M13; B.Rot.M14 = A.Rot.M14;
            B.Rot.M21 = A.Rot.M21; B.Rot.M22 = A.Rot.M22; B.Rot.M23 = A.Rot.M23; B.Rot.M24 = A.Rot.M24;
            B.Rot.M31 = A.Rot.M31; B.Rot.M32 = A.Rot.M32; B.Rot.M33 = A.Rot.M33; B.Rot.M34 = A.Rot.M34;
            B.Rot.M41 = A.Rot.M41; B.Rot.M42 = A.Rot.M42; B.Rot.M43 = A.Rot.M43; B.Rot.M44 = A.Rot.M44;
        }
        #endregion

        #region CopyAtoB()
        /// CopyAtoB() - Copy everything
        /// ***********************************************************************
        public static void CopyAtoB(ref rboState A, ref rboState B)
        {
            // Copy state RBO quantities
            B.pos.X = A.pos.X; B.pos.Y = A.pos.Y; B.pos.Z = A.pos.Z;
            B.linearMom.X = A.linearMom.X; B.linearMom.Y = A.linearMom.Y; B.linearMom.Z = A.linearMom.Z;
            B.orient.W = A.orient.W; B.orient.X = A.orient.X; B.orient.Y = A.orient.Y; B.orient.Z = A.orient.Z;
            B.angularMom.X = A.angularMom.X; B.angularMom.Y = A.angularMom.Y; B.angularMom.Z = A.angularMom.Z;
            B.scale.X = A.scale.X; B.scale.Y = A.scale.Y; B.scale.Z = A.scale.Z;
            A.time = B.time;

            // Copy derived RBO quantities
            B.linearVel.X = A.linearVel.X; B.linearVel.Y = A.linearVel.Y; B.linearVel.Z = A.linearVel.Z;
            B.spin.X = A.spin.X; B.spin.Y = A.spin.Y; B.spin.Z = A.spin.Z; B.spin.W = A.spin.W;
            B.angularVel.X = A.angularVel.X; B.angularVel.Y = A.angularVel.Y; B.angularVel.Z = A.angularVel.Z;
            B.Iinv.M11 = A.Iinv.M11; B.Iinv.M12 = A.Iinv.M12; B.Iinv.M13 = A.Iinv.M13; B.Iinv.M14 = A.Iinv.M14;
            B.Iinv.M21 = A.Iinv.M21; B.Iinv.M22 = A.Iinv.M22; B.Iinv.M23 = A.Iinv.M23; B.Iinv.M24 = A.Iinv.M24;
            B.Iinv.M31 = A.Iinv.M31; B.Iinv.M32 = A.Iinv.M32; B.Iinv.M33 = A.Iinv.M33; B.Iinv.M34 = A.Iinv.M34;
            B.Iinv.M41 = A.Iinv.M41; B.Iinv.M42 = A.Iinv.M42; B.Iinv.M43 = A.Iinv.M43; B.Iinv.M44 = A.Iinv.M44;

            // Constant RBO quantities
            B.mass = A.mass;
            B.inverseMass = A.inverseMass;
            B.Itensor.M11 = A.Itensor.M11; B.Itensor.M12 = A.Itensor.M12; B.Itensor.M13 = A.Itensor.M13; B.Itensor.M14 = A.Itensor.M14;
            B.Itensor.M21 = A.Itensor.M21; B.Itensor.M22 = A.Itensor.M22; B.Itensor.M23 = A.Itensor.M23; B.Itensor.M24 = A.Itensor.M24;
            B.Itensor.M31 = A.Itensor.M31; B.Itensor.M32 = A.Itensor.M32; B.Itensor.M33 = A.Itensor.M33; B.Itensor.M34 = A.Itensor.M34;
            B.Itensor.M41 = A.Itensor.M41; B.Itensor.M42 = A.Itensor.M42; B.Itensor.M43 = A.Itensor.M43; B.Itensor.M44 = A.Itensor.M44;
            B.InvItensor.M11 = A.InvItensor.M11; B.InvItensor.M12 = A.InvItensor.M12; B.InvItensor.M13 = A.InvItensor.M13; B.InvItensor.M14 = A.InvItensor.M14;
            B.InvItensor.M21 = A.InvItensor.M21; B.InvItensor.M22 = A.InvItensor.M22; B.InvItensor.M23 = A.InvItensor.M23; B.InvItensor.M24 = A.InvItensor.M24;
            B.InvItensor.M31 = A.InvItensor.M31; B.InvItensor.M32 = A.InvItensor.M32; B.InvItensor.M33 = A.InvItensor.M33; B.InvItensor.M34 = A.InvItensor.M34;
            B.InvItensor.M41 = A.InvItensor.M41; B.InvItensor.M42 = A.InvItensor.M42; B.InvItensor.M43 = A.InvItensor.M43; B.InvItensor.M44 = A.InvItensor.M44;
        }
        #endregion

        #region RecalculateDerivedQuantities()
        /// RecalculateDerivedQuantities()
        /// ***********************************************************************
        public void RecalculateDerivedQuantities()
        {
            linearVel = linearMom * inverseMass;
            Rot = Matrix.CreateFromQuaternion(Quaternion.Normalize(orient));
            Iinv = Rot * InvItensor * Matrix.Transpose(Rot);
            angularVel = Vector3.Transform(angularMom, Iinv);
            orient.Normalize();
            spin.W = 0.0f; spin.X = angularVel.X; spin.Y = angularVel.Y; spin.Z = angularVel.Z;
            spin = Quaternion.Multiply(spin, 0.5f);
            spin = spin * orient;
        }
        #endregion
    }
    
}
