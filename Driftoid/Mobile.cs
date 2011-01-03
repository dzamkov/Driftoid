using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// A kind of driftoid that can propel itself.
    /// </summary>
    public class MobileKind : Kind
    {
        public MobileKind(int EngineLevel)
        {
            this._EngineLevel = EngineLevel;
        }

        /// <summary>
        /// A constructor for a driftoid of this kind.
        /// </summary>
        public DriftoidConstructor Constructor
        {
            get
            {
                return new DriftoidConstructor(this, 1.0, 1.0);
            }
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
                        return new MobileKind(el).Constructor;
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