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
        /// The radius of the product to be created.
        /// </summary>
        public double ProductRadius;

        /// <summary>
        /// The amount of time in seconds it takes to convert a normal reactant into a transitional state.
        /// </summary>
        public const double WarmupRate = 1.0;

        /// <summary>
        /// The amount of time in seconds it takes for a transitional state to be absorbed or transformed.
        /// </summary>
        public const double CooldownRate = 1.0;
    }

    public enum DriftoidReactionState
    {
        /// <summary>
        /// The state of a driftoid as it begins being absorbed. A driftoid in this state retains its properties.
        /// </summary>
        Warmup,

        /// <summary>
        /// The state of a driftoid as it waits for some part of the reaction to take place before cooling down. A driftoid
        /// in this state is inactive.
        /// </summary>
        Transitional,

        /// <summary>
        /// The state of a driftoid as it is absorbed.
        /// </summary>
        Cooldown,

        /// <summary>
        /// The state of a driftoid as it morphs into its final state. This is assigned to the product of the reaction, once it
        /// is created.
        /// </summary>
        TransformCooldown
    }
}