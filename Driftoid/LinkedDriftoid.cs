using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// A driftoid that can accept and be involved in tree-like links.
    /// </summary>
    public abstract class LinkedDriftoid : Driftoid
    {
        public LinkedDriftoid(DriftoidConstructorInfo Info) : base(Info)
        {
            this._LinkedChildren = new List<LinkedDriftoid>();
        }

        /// <summary>
        /// Gets the driftoid this driftoid has a child-parent relation to. Parents difer from children in that
        /// parents control reactions and linking for their chain.
        /// </summary>
        public LinkedDriftoid LinkedParent
        {
            get
            {
                return this._LinkedParent;
            }
        }

        /// <summary>
        /// Gets the children driftoids of this driftoid.
        /// </summary>
        public IEnumerable<LinkedDriftoid> LinkedChildren
        {
            get
            {
                return this._LinkedChildren;
            }
        }

        /// <summary>
        /// Gets if the specified driftoid is an ancestor of this.
        /// </summary>
        public bool IsAncestor(Driftoid Driftoid)
        {
            if (this == Driftoid)
            {
                return true;
            }
            if (this._LinkedParent == null)
            {
                return false;   
            }
            return this._LinkedParent.IsAncestor(Driftoid);
        }

        /// <summary>
        /// Gets if the requested child-parent link is allowed. The index indicates where, in terms of children, the
        /// requested link is.
        /// </summary>
        public virtual bool AllowLink(int Index, LinkedDriftoid PossibleChild)
        {
            if (this._LinkedParent != null)
            {
                return this._LinkedParent.AllowChildLink(Index, PossibleChild, this);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets if the requested parent-child link is allowed. Links are initiated by parents and should in most
        /// cases be accepted.
        /// </summary>
        public virtual bool AllowLink(LinkedDriftoid PossibleParent)
        {
            return true;
        }

        /// <summary>
        /// Gets if the possible child should link as a child to a descendant of this driftoid.
        /// </summary>
        public virtual bool AllowChildLink(int Index, LinkedDriftoid PossibleChild, LinkedDriftoid Descendant)
        {
            if (this._LinkedParent != null)
            {
                return this._LinkedParent.AllowChildLink(Index, PossibleChild, Descendant);
            }
            return false;
        }

        /// <summary>
        /// Draws a "linker" between two linked driftoids. Driftoids and view should already be set up.
        /// </summary>
        public static void DrawLinker(LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            _LinkerKey lk = new _LinkerKey()
            {
                ParentRadius = Parent.Radius,
                ChildRadius = Child.Radius,
                ChildBorderWidth = Child.BorderWidth
            };
            _LinkerTexture lt;
            if (!_LinkerTextures.TryGetValue(lk, out lt))
            {
                _LinkerTextures[lk] = lt = _CreateTexture(lk);
            }

            Vector dif = Parent.Position - Child.Position;

            Texture.Bind2D(lt.Texture);
            GL.PushMatrix();
            GL.Translate(Child.Position.X, Child.Position.Y, 0.0);
            GL.Rotate(dif.Angle * 180 / Math.PI, 0.0, 0.0, 1.0);
            GL.Translate(-lt.ChildCenterOffset, 0.0, 0.0);
            GL.Scale(lt.Size.X / 2.0, lt.Size.Y / 2.0, 1.0);
            GL.Translate(1.0, 0.0, 0.0);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(-1.0f, -1.0f); GL.TexCoord2(0f, 0f);
            GL.Vertex2(-1.0f, 1.0f); GL.TexCoord2(1f, 0f);
            GL.Vertex2(1.0f, 1.0f); GL.TexCoord2(1f, 1f);
            GL.Vertex2(1.0f, -1.0f); GL.TexCoord2(0f, 1f);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// A key used to get a linker texture.
        /// </summary>
        private struct _LinkerKey
        {
            public double ParentRadius;
            public double ChildRadius;
            public double ChildBorderWidth;
        }
        private struct _LinkerTexture
        {
            public int Texture;
            public Vector Size;
            public double ChildCenterOffset;
        }

        /// <summary>
        /// Creates a linker texture for the given key.
        /// </summary>
        private static _LinkerTexture _CreateTexture(_LinkerKey Key)
        {
            double grabberlength = 1.0;
            double grabberwidth = Key.ChildBorderWidth;
            double handlelength = 0.4;
            double handlewidth = 0.1;
            double grabbermaxangle = grabberlength / (Key.ChildRadius * 2.0);

            double maxheight = handlewidth / 2.0;
            double maxgrabberwidth = Key.ChildBorderWidth;
            if (grabbermaxangle > Math.PI / 2.0)
            {
                maxheight = Key.ChildRadius;
                maxgrabberwidth = Key.ChildRadius * (1.0 - Math.Cos(grabbermaxangle));
            }
            else
            {
                maxgrabberwidth = Key.ChildRadius * (1.0 + Math.Cos(grabbermaxangle) * grabberwidth);
                double posheight = Math.Sin(grabbermaxangle) * Key.ChildRadius;
                if (posheight > maxheight)
                {
                    maxheight = posheight;
                }
            }

            Vector size = new Vector(maxgrabberwidth + handlelength, maxheight * 2.0);
            double grabbercenter = -Key.ChildRadius + maxgrabberwidth;

            double trans = Key.ChildRadius - grabberwidth;

            double midcenter = 0.5 * size.Y;
            int tex = Drawer.Create(delegate(Vector Point)
            {
                Point.X *= size.X;
                Point.Y *= size.Y;

                Vector grabberdif = (Point - new Vector(grabbercenter, midcenter));
                double grabberdis = grabberdif.Length;
                double grabberang = grabberdif.Angle;

                if (grabberdis < trans)
                {
                    return Color.Transparent;
                }
                if (grabberdis < Key.ChildRadius)
                {
                    double angdif = Math.Abs(grabberang);
                    if (angdif < grabbermaxangle)
                    {
                        return Color.RGBA(1.0, 1.0, 1.0, 1.0);
                    }
                }
                if (Math.Abs(Point.Y - midcenter) < handlewidth / 2.0)
                {
                    double a = (Point.X - maxgrabberwidth) / handlelength;
                    return Color.RGBA(1.0, 1.0, 1.0, 1.0 - a * a);
                }
                return Color.Transparent;
            }).CreateTexture(256);
            return new _LinkerTexture()
            {
                Size = size,
                ChildCenterOffset = grabbercenter,
                Texture = tex
            };
        }

        private static readonly Dictionary<_LinkerKey, _LinkerTexture> _LinkerTextures = new Dictionary<_LinkerKey, _LinkerTexture>();

        /// <summary>
        /// Gets if a link to the specified child is possible. Returns the index of the candidate. 
        /// </summary>
        internal bool _CanLink(LinkedDriftoid Child, out int Index)
        {
            Index = 0;
            Vector pos = this.Position;
            double tarangle = (Child.Position - pos).Angle;
            if (this._LinkedParent != null)
            {
                double lowangle = (this._LinkedParent.Position - pos).Angle;
                foreach (LinkedDriftoid ld in this._LinkedChildren)
                {
                    double highangle = (ld.Position - pos).Angle;
                    if (Vector.AngleBetween(tarangle, lowangle, highangle))
                    {
                        break;
                    }
                    lowangle = highangle;
                    Index++;
                }
            }
            else
            {
                if (this._LinkedChildren.Count == 0)
                {
                    Index = 0;
                }
                else
                {
                    if (this._LinkedChildren.Count == 1)
                    {
                        Index = 1;
                    }
                    else
                    {
                        Index = 0;
                        double lowangle = (this._LinkedChildren[0].Position - pos).Angle;
                        for (int t = 1; t < this._LinkedChildren.Count; t++)
                        {
                            double highangle = (this._LinkedChildren[t].Position - pos).Angle;
                            if (Vector.AngleBetween(tarangle, lowangle, highangle))
                            {
                                break;
                            }
                            lowangle = highangle;
                            Index++;
                        }
                    }
                }
            }

            return Child.AllowLink(this) && this.AllowLink(Index, Child);
        }

        /// <summary>
        /// Corrects (repositions participants) the link between a parent and child driftoid.
        /// </summary>
        internal static void _CorrectLink(LinkedDriftoid Parent, LinkedDriftoid Child, int Iter, Vector Difference, double Distance)
        {
            // Is there a problem
            if (Math.Abs(Distance - Parent.Radius - Child.Radius) > 0.01)
            {
                // Equivalent to another problem...
                Driftoid._CollisionResponse(Parent, Child, Difference, Distance, 1.0);
            }
        }

        /// <summary>
        /// Tries linking two driftoids.
        /// </summary>
        internal static void _Link(LinkedDriftoid A, LinkedDriftoid B)
        {
            int ind;
            if (!A.IsAncestor(B) && !B.IsAncestor(A))
            {
                if (A._LinkedParent == null)
                {
                    if (B._CanLink(A, out ind))
                    {
                        if (B._LinkedParent == null)
                        {
                            int dummy;
                            if (A._CanLink(B, out dummy))
                            {
                                return;
                            }
                        }
                        B._Link(A, ind);
                    }
                }
                else
                {
                    if (B._LinkedParent == null)
                    {
                        if (A._CanLink(B, out ind))
                        {
                            A._Link(B, ind);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Links a child driftoid to this driftoid.
        /// </summary>
        private void _Link(LinkedDriftoid Child, int Index)
        {
            // Make link
            this._LinkedChildren.Add(null);
            for (int t = this._LinkedChildren.Count - 1; t > Index; t--)
            {
                this._LinkedChildren[t] = this._LinkedChildren[t - 1];
            }
            this._LinkedChildren[Index] = Child;
            Child._LinkedParent = this;
        }

        private LinkedDriftoid _LinkedParent;
        private List<LinkedDriftoid> _LinkedChildren;
    }
}