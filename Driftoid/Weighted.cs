using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// The kind for a very heavy driftoid.
    /// </summary>
    public class WeightedDriftoid : LinkedDriftoid
    {
        private WeightedDriftoid(int Level, MotionState MotionState, double Mass, double Radius) : base(MotionState, Mass, Radius)
        {
            this._Level = Level;
        }

        public override Visual Visual
        {
            get
            {
                return new SimpleVisual(Texture.ID, this);
            }
        }

        /// <summary>
        /// Gets the radius of a weighted driftoid at the specified level. Levels start at 0.
        /// </summary>
        public static double GetLevelRadius(int Level)
        {
            return Math.Sqrt(GetLevelMass(Level)) / 2.0;
        }

        /// <summary>
        /// Gets the mass of a weighted driftoid at the specified level.
        /// </summary>
        public static double GetLevelMass(int Level)
        {
            return 8.0 * (double)(Level + 1);
        }

        /// <summary>
        /// Gets a constructor for a weighted driftoid of a certain level.
        /// </summary>
        public static DriftoidConstructor GetConstructor(int Level)
        {
            return new DriftoidConstructor((MotionState MotionState, double Mass, double Radius) => new WeightedDriftoid(Level, MotionState, Mass, Radius),
                GetLevelMass(Level), GetLevelRadius(Level));
        }

        public override double BorderWidth
        {
            get
            {
                return this._Radius * UnitBorderWidth * 2.0;    
            }
        }

        public static double UnitBorderWidth = 0.2;

        private class _Drawer : Drawer
        {
            public static readonly Color BorderColor = Color.RGB(0.3, 0.3, 0.3);
            public static readonly Color InnerColor = Color.RGB(0.6, 0.6, 0.6);

            public override Color AtPoint(Vector Point)
            {
                Vector dif = Point - new Vector(0.5, 0.5);
                double dis = dif.Length * 2.0;
                double ang = dif.Angle;
                if (dis > 1.0)
                {
                    return Color.Transparent;
                }
                if (dis > 1.0 - UnitBorderWidth * 2.0)
                {
                    return BorderColor;
                }
                if (dis < 0.2)
                {
                    return BorderColor;
                }
                return InnerColor;
            }
        }

        private class _Recipe : Recipe
        {
            public override DriftoidConstructor GetProduct(Structure Structure)
            {
                int l;
                if (Recipe.MatchTerminatedChain(MatchDirection.Left, "Fe?Fe", "H", Structure, out l))
                {
                    if (l > 0)
                    {
                        return WeightedDriftoid.GetConstructor(l - 1);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static void RegisterRecipes(List<Recipe> Recipes)
        {
            Recipes.Add(new _Recipe());
        }

        public static readonly Texture Texture = Texture.Define(new _Drawer(), 1.0, 1.0);

        private int _Level;
    }
}