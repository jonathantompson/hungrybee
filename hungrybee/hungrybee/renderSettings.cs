#region File Description
//-----------------------------------------------------------------------------
// NonPhotoRealisticSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace hungrybee
{
    /// <summary>
    /// ***********************************************************************
    /// **                         renderSettings                            **
    /// ** Structure to hold a render setting state kept in gameSettings     **
    /// ** singleton class                                                   **
    /// ** LOTS OF CODE HERE TAKEN FROM XNA CREATORS CLUB:                   **
    /// ** Non Photo Realistic Rendering example (I like the effects)        **
    /// ***********************************************************************
    /// </summary>
    public struct renderSettings
    {
        #region Local Variables

        // Name of a preset setting, for display to the user.
        public readonly string Name;

        // Is the cartoon lighting shader enabled?
        public readonly bool EnableToonShading;

        // Settings for the edge detect filter.
        public readonly bool EnableEdgeDetect;
        public readonly float EdgeWidth;
        public readonly float EdgeIntensity;

        // Settings for the pencil sketch effect.
        public readonly bool EnableSketch;
        public readonly bool SketchInColor;
        public readonly float SketchThreshold;
        public readonly float SketchBrightness;
        public readonly float SketchJitterSpeed;

        #endregion

        #region Constructor - renderSettings(...)
        /// Constructor
        /// ***********************************************************************
        public renderSettings(string name, bool enableToonShading,
                                         bool enableEdgeDetect,
                                         float edgeWidth, float edgeIntensity,
                                         bool enableSketch, bool sketchInColor,
                                         float sketchThreshold, float sketchBrightness,
                                         float sketchJitterSpeed)
        {
            Name = name;
            EnableToonShading = enableToonShading;
            EnableEdgeDetect = enableEdgeDetect;
            EdgeWidth = edgeWidth;
            EdgeIntensity = edgeIntensity;
            EnableSketch = enableSketch;
            SketchInColor = sketchInColor;
            SketchThreshold = sketchThreshold;
            SketchBrightness = sketchBrightness;
            SketchJitterSpeed = sketchJitterSpeed;
        }
        #endregion

    }
}
