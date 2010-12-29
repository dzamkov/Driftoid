using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A type of primitive driftoid.
    /// </summary>
    public enum PrimitiveDriftoidType
    {
        Carbon,
        Nitrogen,
        Oxygen,
        Hydrogen,
        Iron,
        Sulfur
    }

    /// <summary>
    /// A primitive (simple, building block) driftoid.
    /// </summary>
    public class PrimitiveDriftoid : Driftoid
    {
        public PrimitiveDriftoid(PrimitiveDriftoidType Type, DriftoidState MotionState)
            : base(1.0, MotionState)
        {
            this._Type = Type;
        }

        public override int TextureID
        {
            get
            {
                int texid;
                if (!_Textures.TryGetValue(this._Type, out texid))
                {
                    _Textures[this._Type] = texid = _Visuals[(int)this._Type].CreateTexture();
                }
                return texid;
            }
        }

        /// <summary>
        /// Really quick way to create a primitive driftoid.
        /// </summary>
        public static PrimitiveDriftoid QuickCreate(PrimitiveDriftoidType Type, double X, double Y)
        {
            return new PrimitiveDriftoid(Type, new DriftoidState(new Vector(X, Y)));
        }

        /// <summary>
        /// Gets the type of this primitive driftoid.
        /// </summary>
        public PrimitiveDriftoidType Type
        {
            get
            {
                return this._Type;
            }
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
            /// The text in the middle of the driftoid.
            /// </summary>
            public string Text;

            /// <summary>
            /// Creates a texture for a driftoid of this type.
            /// </summary>
            public int CreateTexture()
            {
                return Driftoid.CreateSolidTexture(256, 0.15f, this.BorderColor, this.InteriorColor);
            }
        }

        /// <summary>
        /// Visual properties of the primitive types.
        /// </summary>
        private static readonly _Visual[] _Visuals = new _Visual[] { new _Visual() 
            {
                Text = "C",
                BorderColor = Color.FromArgb(150, 50, 50),
                InteriorColor = Color.FromArgb(200, 100, 100),
                TextColor = Color.FromArgb(180, 50, 50)
            }, new _Visual() 
            {
                Text = "N",
                BorderColor = Color.FromArgb(50, 150, 50),
                InteriorColor = Color.FromArgb(100, 200, 100),
                TextColor = Color.FromArgb(50, 180, 50)
            }, new _Visual() 
            {
                Text = "O",
                BorderColor = Color.FromArgb(160, 160, 160),
                InteriorColor = Color.FromArgb(200, 200, 200),
                TextColor = Color.FromArgb(140, 140, 140)
            }, new _Visual() 
            {
                Text = "H",
                BorderColor = Color.FromArgb(0, 50, 200),
                InteriorColor = Color.FromArgb(50, 100, 200),
                TextColor = Color.FromArgb(0, 50, 200)
            }
        };

        /// <summary>
        /// A cache of textures used for primitive driftoids.
        /// </summary>
        private static readonly Dictionary<PrimitiveDriftoidType, int> _Textures = new Dictionary<PrimitiveDriftoidType, int>();

        private PrimitiveDriftoidType _Type;
    }

}