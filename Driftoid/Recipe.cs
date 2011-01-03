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
    /// A direction a recipe can be matched in. (Recipes are rotationally symetrical).
    /// </summary>
    public enum MatchDirection
    {
        Left,
        Right
    }

    /// <summary>
    /// Represents a possible conversion of reactants to a product during a reaction.
    /// </summary>
    public abstract class Recipe
    {
        /// <summary>
        /// Matches a chain of primitives. The pattern string should include one variable indicating where it should continue. Gets the amount of patterns that appear
        /// before the termination string.
        /// </summary>
        public static bool MatchTerminatedChain(MatchDirection Direction, string PatternString, string TerminationString, Structure Structure, out int Amount)
        {
            Amount = 0;
            while (true)
            {
                Structure[] vars = new Structure[1];
                if (Match(Direction, PatternString, Structure, vars))
                {
                    Amount++;
                    Structure = vars[0];
                }
                else
                {
                    return Match(Direction, TerminationString, Structure, null);
                }
            }
        }

        /// <summary>
        /// Matches the specified structure to a pattern string in either direction.
        /// </summary>
        public static bool Match(string PatternString, Structure Structure, Structure[] Variables, out MatchDirection Direction)
        {
            int si = 0;
            int vi = 0;
            if (Match(PatternString, ref si, Structure, false, Variables, ref vi))
            {
                Direction = MatchDirection.Left;
                return true;
            }
            si = 0;
            vi = 0;
            if (Match(PatternString, ref si, Structure, true, Variables, ref vi))
            {
                Direction = MatchDirection.Right;
                return true;
            }
            Direction = MatchDirection.Left;
            return false;
        }

        /// <summary>
        /// Matches the specified structure to a pattern string in the specified direction.
        /// </summary>
        public static bool Match(MatchDirection Direction, string PatternString, Structure Structure, Structure[] Variables)
        {
            int si = 0;
            int vi = 0;
            if (Direction == MatchDirection.Left)
            {
                return Match(PatternString, ref si, Structure, false, Variables, ref vi);
            }
            else
            {
                return Match(PatternString, ref si, Structure, true, Variables, ref vi);
            }
        }

        /// <summary>
        /// Gets if the specified structure matches the pattern string (which defines a relation of primitive kinds in a structure).
        /// </summary>
        public static bool Match(string PatternString, ref int StringIndex, Structure Structure, bool ReverseStructure, Structure[] Variables, ref int VariableIndex)
        {
            PrimitiveKind pk = Structure.RootKind as PrimitiveKind;
            if (pk != null)
            {
                PrimitiveType type = pk.Type;
                string sym = PrimitiveKind.GetSymbol(type);
                if (PatternString[StringIndex] == '?')
                {
                    Variables[VariableIndex] = Structure;
                    VariableIndex++;
                    StringIndex++;
                    return true;
                }
                for (int t = 0; t < sym.Length; t++)
                {
                    if (StringIndex >=  PatternString.Length || PatternString[StringIndex] != sym[t])
                    {
                        return false;
                    }
                    StringIndex++;
                }
                if (StringIndex < PatternString.Length && PatternString[StringIndex] == '(')
                {
                    StringIndex++;
                    Structure[] subs = Structure.Substructures;
                    if (ReverseStructure)
                    {
                        for (int t = subs.Length - 1; t >= 0; t--)
                        {
                            if (!Match(PatternString, ref StringIndex, subs[t], true, Variables, ref VariableIndex))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        for (int t = 0; t < subs.Length; t++)
                        {
                            if (!Match(PatternString, ref StringIndex, subs[t], false, Variables, ref VariableIndex))
                            {
                                return false;
                            }
                        }
                    }
                    if (PatternString[StringIndex] != ')')
                    {
                        return false;
                    }
                    StringIndex++;
                    return true;
                }
                return Structure.SubstructureAmount == 0;
            }
            else
            {
                return false;
            }
        }

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