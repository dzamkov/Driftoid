using System;
using System.Collections.Generic;
using System.Reflection;

namespace Driftoid
{
    /// <summary>
    /// Represents a linked driftoid structure, for use in recipes.
    /// </summary>
    public class Structure
    {
        public Structure(LinkedDriftoid Driftoid)
        {
            this._Driftoid = Driftoid;
        }

        /// <summary>
        /// Gets the first kind in the structure.
        /// </summary>
        public Kind RootKind
        {
            get
            {
                return this._Driftoid.Kind;
            }
        }

        /// <summary>
        /// Gets an array of child structures that make up this structure. The substructures are given in the order as they are connected.
        /// </summary>
        public Structure[] Substructures
        {
            get
            {
                Structure[] structs = new Structure[this._Driftoid.LinkedChildrenAmount];
                int i = 0;
                foreach (LinkedDriftoid child in this._Driftoid.LinkedChildren)
                {
                    structs[i] = new Structure(child);
                    i++;
                }
                return structs;
            }
        }

        /// <summary>
        /// Gets the amount of substructures this structure has.
        /// </summary>
        public int SubstructureAmount
        {
            get
            {
                return this._Driftoid.LinkedChildrenAmount;
            }
        }

        /// <summary>
        /// Gets if the root has the specified primitive type.
        /// </summary>
        public bool IsPrimitive(PrimitiveType Type)
        {
            PrimitiveKind pk = this.RootKind as PrimitiveKind;
            if (pk != null)
            {
                return pk.Type == Type;
            }
            return false;
        }

        /// <summary>
        /// Gets if the structure has no children.
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return this._Driftoid.LinkedChildrenAmount == 0;
            }
        }

        private LinkedDriftoid _Driftoid;
    }

    /// <summary>
    /// Represents a possible conversion of reactants to a product during a reaction.
    /// </summary>
    public abstract class Recipe
    {
        /// <summary>
        /// Gets the product this recipe would produce given the specified structure. Returns null if this recipe has no product for the structure.
        /// </summary>
        public abstract DriftoidConstructor GetProduct(Structure Structure);

        /// <summary>
        /// Gets a recipe that encompasses all registered recipes.
        /// </summary>
        public static Recipe Master
        {
            get
            {
                return _Master;
            }
        }

        private static readonly _MasterRecipe _Master = new _MasterRecipe();

        private class _MasterRecipe : Recipe
        {
            public override DriftoidConstructor GetProduct(Structure Structure)
            {
                if (this._Recipes == null)
                {
                    this._Recipes = new List<Recipe>();
                    Assembly asm = Assembly.GetAssembly(typeof(Reaction));
                    foreach (Module m in asm.GetModules())
                    {
                        foreach (Type t in m.GetTypes())
                        {
                            MethodInfo me = t.GetMethod("RegisterRecipes", BindingFlags.Static | BindingFlags.Public);
                            if (me != null)
                            {
                                me.Invoke(null, new object[] { this._Recipes });
                            }
                        }
                    }
                }
                foreach (Recipe r in this._Recipes)
                {
                    DriftoidConstructor prod = r.GetProduct(Structure);
                    if (prod != null)
                    {
                        return prod;
                    }
                }
                return null;
            }

            private List<Recipe> _Recipes;
        }
    }
}