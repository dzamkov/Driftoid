using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// A kind of driftoid that can propel itself.
    /// </summary>
    public class MobileDriftoid : LinkedDriftoid
    {
        public MobileDriftoid(int EngineLevel, MotionState MotionState, double Mass, double Radius) : base(MotionState, Mass, Radius)
        {
            this._EngineLevel = EngineLevel;
        }

        /// <summary>
        /// Gets a constructor for a mobile driftoid of a certain engine level.
        /// </summary>
        public static DriftoidConstructor GetConstructor(int EngineLevel)
        {
            return new DriftoidConstructor((MotionState MotionState, double Mass, double Radius) => new MobileDriftoid(EngineLevel, MotionState, Mass, Radius),
                1.0, 1.0);
        }

        private class _Recipe : Recipe
        {
            public override DriftoidConstructor GetProduct(Structure Structure)
            {
                Structure[] vars = new Structure[2];
                MatchDirection dir;
                if (Recipe.Match("C(O(?)?)", Structure, vars, out dir))
                {
                    int el;
                    if (Recipe.MatchTerminatedChain(dir, "C(N?N(OO))", "H", vars[1], out el))
                    {
                        return MobileDriftoid.GetConstructor(el);
                    }
                }
                return null;
            }
        }

        public static void RegisterRecipes(List<Recipe> Recipes)
        {
            Recipes.Add(new _Recipe());
        }

        private int _EngineLevel;
    }
}