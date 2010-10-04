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
    #region enum types
    public enum phantomType { UNDEFINED, HARD_BOUNDRY, SOFT_BOUNDRY };
    #endregion

    /// <summary>
    /// ***********************************************************************
    /// **                        gameObjectPhantom                          **
    /// ** This is a class to store the data for the physics phantom objects **
    /// ** COLLIDABLE = TRUE                                                 **
    /// ** MOVABLE = FALSE                                                   **
    /// ***********************************************************************
    /// </summary>
    class gameObjectPhantom : gameObject
    {
        #region Local Variables
        /// Local Variables
        /// ***********************************************************************    

        boundingObjType objType;
        Color color;

        public phantomType phantomType;
        public Vector3     softBoundaryForceVector;
        public bool        softBoundryPlayerReact;
        public bool        softBoundryNPCReact;

        Object obj;

        #endregion

        #region Constructor - gameObjectPhantom() - HARD BOUNDRY
        /// Constructor - Basic constructor for HARD_BOUNDRY
        /// ***********************************************************************
        public gameObjectPhantom(game game, boundingObjType _objType, Object _obj)
            : base(game, null, _objType)
        {
            objType = _objType;
            obj = _obj;
            base.boundingObj = _obj;
            base.movable = false;
            base.collidable = true;
            color = Color.DarkGreen;
            phantomType = phantomType.HARD_BOUNDRY;
            softBoundaryForceVector = Vector3.Zero;
            softBoundryPlayerReact = false;
            softBoundryNPCReact = false;
        }
        #endregion

        #region Constructor - gameObjectPhantom() - SOFT BOUDNRY
        /// Constructor - Basic constructor for SOFT_BOUNDRY
        /// ***********************************************************************
        public gameObjectPhantom(game game, boundingObjType _objType, Object _obj, Vector3 _softBoundaryForceVector, bool playerReact, bool NPCReact)
            : base(game, null, _objType)
        {
            objType = _objType;
            obj = _obj;
            base.boundingObj = _obj;
            base.movable = false;
            base.collidable = true;
            color = Color.DarkGreen;
            phantomType = phantomType.SOFT_BOUNDRY;
            softBoundaryForceVector = _softBoundaryForceVector;
            softBoundryPlayerReact = playerReact;
            softBoundryNPCReact = NPCReact;
        }
        #endregion

        #region LoadContent()
        /// LoadContent - Load in any necessary content
        /// ***********************************************************************
        public override void LoadContent()
        {
            switch (base.boundingObjType)
            {
                case (boundingObjType.SPHERE):
                    base.boundingObjCenter = ((BoundingSphere)obj).Center;
                    // Calculate moment of Inertia from bounding sphere:
                    base.state.Itensor = XNAUtils.CalculateItensorFromBoundingSphere((BoundingSphere)obj, base.state.mass);
                    base.state.InvItensor = Matrix.Invert(base.state.Itensor);
                    base.sweepAndPruneAABB = new BoundingBox(((BoundingSphere)obj).Center - new Vector3(((BoundingSphere)obj).Radius),
                                                             ((BoundingSphere)obj).Center + new Vector3(((BoundingSphere)obj).Radius));
                    break;
                case (boundingObjType.AABB):
                    base.boundingObjCenter = (((BoundingBox)obj).Max + ((BoundingBox)obj).Min) / 2.0f;
                    // Calculate moment of Inertia from bounding box:
                    base.state.Itensor = XNAUtils.CalculateItensorFromBoundingBox(((BoundingBox)obj), state.mass);
                    base.state.InvItensor = Matrix.Invert(base.state.Itensor);
                    base.sweepAndPruneAABB = (BoundingBox)obj;
                    break;
                default:
                    throw new Exception("gameObjectPhantom::LoadContent() - Something went wrong setting up bounding object");
            }

            // Make sure both starting states are equal
            rboState.CopyAtoB(ref base.state, ref base.prevState);  
        }
        #endregion

        #region DrawUsingCurrentEffect()
        /// DrawUsingCurrentEffect - Draw the bounding object
        /// ***********************************************************************
        public override void DrawUsingCurrentEffect(GameTime gameTime, GraphicsDevice device, Matrix view, Matrix projection, string effectTechniqueName)
        {
            // Nothing to draw for collidables
        }
        #endregion

        #region ChangeEffectUsedByModel()
        /// ChangeEffectUsedByModel - Change the effect being used
        /// ***********************************************************************
        public override void ChangeEffectUsedByModel(Effect replacementEffect)
        {
            // Empty, don't override yet
        }
        #endregion
    }
}
