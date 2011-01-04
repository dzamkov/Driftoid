using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// Represents a transformation of a driftoid by absorbing others.
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// The driftoid to be transformed. The root in the reaction sequence.
        /// </summary>
        public LinkedDriftoid Target;

        /// <summary>
        /// A constructor for the product of the transformation.
        /// </summary>
        public DriftoidConstructor Product;

        /// <summary>
        /// The amount of time in seconds it takes for a transitional state to be absorbed or transformed.
        /// </summary>
        public const double CooldownTime = 0.2;
    }
}