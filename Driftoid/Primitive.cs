using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A type of primitive driftoid.
    /// </summary>
    public enum PrimitiveType
    {
        Carbon,
        Nitrogen,
        Oxygen,
        Hydrogen,
        Iron,
        Sulfur
    }

    /// <summary>
    /// A kind for a primitive/element driftoid. Primitives have very little special properties.
    /// </summary>
    public sealed class PrimitiveDriftoid : LinkedDriftoid
    {
        private PrimitiveDriftoid(PrimitiveType Type, MotionState MotionState, double Mass, double Radius) : base(MotionState, Mass, Radius)
        {
            this._Type = Type;
        }

        public override Visual Visual
        {
            get
            {
                int texid;
                if (!_Textures.TryGetValue(this._Type, out texid))
                {
                    _Textures[this._Type] = texid = _Visuals[(int)this._Type].CreateTexture((int)this._Type);
                }
                return new SimpleVisual(texid, this);
            }
        }

        /// <summary>
        /// Gets a constructor for a certain type of primitive driftoid.
        /// </summary>
        public static DriftoidConstructor GetConstructor(PrimitiveType Type)
        {
            return new DriftoidConstructor((MotionState MotionState, double Mass, double Radius) => new PrimitiveDriftoid(Type, MotionState, Mass, Radius),
                _Masses[(int)Type], 1.0);
        }

        /// <summary>
        /// Visual properties of a primitive driftoid with a certain type.
        /// </summary>
        private struct _Visual
        {
            /// <summary>
            /// The color of the interior area of the driftoid.
            /// </summary>
            public Color InteriorColor;

            /// <summary>
            /// The color of the border of the driftoid.
            /// </summary>
            public Color BorderColor;

            /// <summary>
            /// The color of the text of the driftoid.
            /// </summary>
            public Color TextColor;

            /// <summary>
            /// Creates a texture for a driftoid of this type.
            /// </summary>
            public int CreateTexture(int TypeIndex)
            {
                int texsize = 256;
                using (Bitmap bm = new Bitmap(texsize, texsize))
                {
                    Driftoid.DrawSolid(bm, 0.15, this.BorderColor, this.InteriorColor);
                    using (Graphics g = Graphics.FromImage(bm))
                    {
                        float actualsize = (float)texsize;
                        using (Font f = new Font(_GetFont(), actualsize * 0.4f, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            using (Brush b = new SolidBrush(this.TextColor))
                            {
                                string text = _Symbols[TypeIndex];
                                SizeF strsize = g.MeasureString(text, f);

                                // Try centering the text in the bubble
                                float x = actualsize * 0.5f - strsize.Width * 0.5f;
                                float y = actualsize * 0.5f - strsize.Height * 0.5f;
                                g.DrawString(text, f, b, x, y);

                            }
                        }
                    }
                    return Texture.Create(bm);
                }
            }
        }

        /// <summary>
        /// Gets the font family with the specified name.
        /// </summary>
        public static FontFamily GetFont(string Name)
        {
            foreach (FontFamily ff in FontFamily.Families)
            {
                if (ff.Name == Name)
                {
                    return ff;
                }
            }
            return null;
        }

        private static FontFamily _GetFont()
        {
            if (_Font == null)
            {
                _Font = GetFont("Verdana");
            }
            return _Font;
        }

        /// <summary>
        /// The prefered font for primitive driftoids.
        /// </summary>
        private static FontFamily _Font;

        /// <summary>
        /// Gets the maximum amount of links a driftoid of a primitive type can support.
        /// </summary>
        public static int GetMaxLinks(PrimitiveType Type)
        {
            return _MaxLinks[(int)Type];
        }

        public override bool AllowLink(int Index, LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            if (Parent == this)
            {
                if (this.LinkedChildrenAmount < GetMaxLinks(this.Type))
                {
                    return base.AllowLink(Index, Parent, Child);
                }
                else
                {
                    return false;
                }
            }
            return base.AllowLink(Index, Parent, Child);
        }

        /// <summary>
        /// Gets the symbol for the specified primitive type.
        /// </summary>
        public static string GetSymbol(PrimitiveType Type)
        {
            return _Symbols[(int)Type];
        }

        private static readonly string[] _Symbols = new string[] {
            "C",
            "N",
            "O",
            "H",
            "Fe",
            "S"
        };

        private static readonly double[] _Masses = new double[] {
            0.3,
            0.4,
            0.4,
            0.2,
            3.0,
            1.0
        };

        private static readonly int[] _MaxLinks = new int[] {
            3,
            2,
            1,
            0,
            0,
            0,
        };

        /// <summary>
        /// Visual properties of the primitive types.
        /// </summary>
        private static readonly _Visual[] _Visuals = new _Visual[] { new _Visual() 
            {
                BorderColor = Color.RGB(0.5, 0.2, 0.2),
                InteriorColor = Color.RGB(0.7, 0.4, 0.4),
                TextColor = Color.RGB(0.6, 0.2, 0.2)
            }, new _Visual() 
            {
                BorderColor = Color.RGB(0.2, 0.5, 0.2),
                InteriorColor = Color.RGB(0.4, 0.7, 0.4),
                TextColor = Color.RGB(0.2, 0.6, 0.2)
            }, new _Visual() 
            {
                BorderColor = Color.RGB(0.55, 0.55, 0.55),
                InteriorColor = Color.RGB(0.7, 0.7, 0.7),
                TextColor = Color.RGB(0.45, 0.45, 0.45)
            }, new _Visual() 
            {
                BorderColor = Color.RGB(0.0, 0.2, 0.7),
                InteriorColor = Color.RGB(0.2, 0.4, 0.7),
                TextColor = Color.RGB(0.0, 0.2, 0.7)
            }, new _Visual() 
            {
                BorderColor = Color.RGB(0.7, 0.7, 0.7),
                InteriorColor = Color.RGB(0.4, 0.4, 0.4),
                TextColor = Color.RGB(0.8, 0.8, 0.8)
            }, new _Visual()
            {
                BorderColor = Color.RGB(0.7, 0.7, 0.0),
                InteriorColor = Color.RGB(0.8, 0.8, 0.4),
                TextColor = Color.RGB(0.6, 0.6, 0)
            }
        };

        /// <summary>
        /// A cache of textures used for primitive driftoids.
        /// </summary>
        private static readonly Dictionary<PrimitiveType, int> _Textures = new Dictionary<PrimitiveType, int>();

        /// <summary>
        /// Gets the primitive type of this kind.
        /// </summary>
        public PrimitiveType Type
        {
            get
            {
                return this._Type;
            }
        }

        private PrimitiveType _Type;
    }
}