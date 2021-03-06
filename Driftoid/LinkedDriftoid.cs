﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// A kind of driftoid that can accept and be involved in tree-like links.
    /// </summary>
    public abstract class LinkedDriftoid : Driftoid
    {
        public LinkedDriftoid(MotionState MotionState, double Mass, double Radius)
            : base(MotionState, Mass, Radius)
        {
            this._LinkedChildren = new List<LinkedDriftoid>();
        }

        /// <summary>
        /// Updates the state of the driftoid as time passes.
        /// </summary>
        public virtual void OnUpdate(double Time, IDriftoidInterface Interface)
        {

        }

        /// <summary>
        /// Called when the parent of this driftoid delinks from it.
        /// </summary>
        public virtual void OnParentDelink(LinkedDriftoid Parent)
        {

        }

        /// <summary>
        /// Called when a child is delinked from this driftoid.
        /// </summary>
        public virtual void OnChildDelink(LinkedDriftoid Child)
        {

        }

        /// <summary>
        /// Called when this is added as a child of another driftoid.
        /// </summary>
        public virtual void OnParentLink(LinkedDriftoid Parent)
        {

        }

        /// <summary>
        /// Called when this acquires a new child driftoid.
        /// </summary>
        public virtual void OnChildLink(int Index, LinkedDriftoid Child)
        {

        }

        /// <summary>
        /// Gets if a link from the parent (which is a descendant of this driftoid) to the specified child is allowed.
        /// </summary>
        public virtual bool AllowLink(int Index, LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            if (this._LinkedParent != null)
            {
                return this._LinkedParent.AllowLink(Index, Parent, Child);
            }
            return false;
        }

        /// <summary>
        /// Gets if a link to the specified parent as a child is allowed.
        /// </summary>
        public virtual bool AllowParentLink(LinkedDriftoid Parent)
        {
            return true;
        }

        /// <summary>
        /// Gets the width of the border around the driftoid, for linking purposes.
        /// </summary>
        public virtual double BorderWidth
        {
            get
            {
                return 0.3 * this._Radius;
            }
        }

        /// <summary>
        /// Gets the reaction this driftoid is currently in, or null if not in a reaction.
        /// </summary>
        public Reaction Reaction
        {
            get
            {
                return this._CurrentReaction;
            }
        }

        /// <summary>
        /// Returns true if neither this, nor its descendants are currently in reactions.
        /// </summary>
        public bool ReactionClear
        {
            get
            {
                if (this._CurrentReaction != null)
                {
                    return false;
                }
                else
                {
                    foreach (LinkedDriftoid ld in this._LinkedChildren)
                    {
                        if (!ld.ReactionClear)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Completely removes all links from the driftoid, without correcting any links on this driftoid, with the
        /// expectation that this driftoid will never be used again.
        /// </summary>
        internal void _Delete()
        {
            if (this._LinkedParent != null)
            {
                this._LinkedParent._LinkedChildren.Remove(this);
            }
            foreach (LinkedDriftoid ldr in this._LinkedChildren)
            {
                ldr._LinkedParent = null;
            }
        }

        /// <summary>
        /// Gets the final descendants (those without children) of this driftoid.
        /// </summary>
        public IEnumerable<LinkedDriftoid> Leaves
        {
            get
            {
                List<LinkedDriftoid> li = new List<LinkedDriftoid>();
                this._WriteLeaves(li);
                return li;
            }
        }

        private void _WriteLeaves(List<LinkedDriftoid> List)
        {
            if (this._LinkedChildren.Count == 0)
            {
                List.Add(this);
            }
            else
            {
                foreach (LinkedDriftoid child in this._LinkedChildren)
                {
                    child._WriteLeaves(List);
                }
            }
        }

        /// <summary>
        /// Gets all descendants of the driftoid, excluding itself.
        /// </summary>
        public IEnumerable<LinkedDriftoid> Descendants
        {
            get
            {
                List<LinkedDriftoid> li = new List<LinkedDriftoid>();
                this._WriteDescendants(li);
                return li;
            }
        }

        private void _WriteDescendants(List<LinkedDriftoid> List)
        {
            foreach (LinkedDriftoid dr in this._LinkedChildren)
            {
                List.Add(dr);
                dr._WriteDescendants(List);
            }
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
        /// Gets the amount of linked children this driftoid currently has.
        /// </summary>
        public int LinkedChildrenAmount
        {
            get
            {
                return this._LinkedChildren.Count;
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
        /// Draws a "linker" between two linked driftoids. Driftoids and view should already be set up.
        /// </summary>
        public static void DrawLinker(LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            _LinkerKey lk = new _LinkerKey()
            {
                ChildRadius = Child.Radius,
                ChildBorderWidth = Child.BorderWidth
            };
            _LinkerTexture lt;
            if (!_LinkerTextures.TryGetValue(lk, out lt))
            {
                _LinkerTextures[lk] = lt = _CreateTexture(lk);
            }

            Vector dif = Parent.Position - Child.Position;

            Texture.Bind2D(lt.Texture.ID);
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
        /// Gets a visual for a linker between to linked driftoids.
        /// </summary>
        public static Visual GetLinkerVisual(LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            return new _LinkerVisual()
            {
                Parent = Parent,
                Child = Child
            };
        }

        private class _LinkerVisual : Visual
        {
            public override void Draw()
            {
                DrawLinker(this.Parent, this.Child);
            }

            public LinkedDriftoid Parent;
            public LinkedDriftoid Child;
        }

        /// <summary>
        /// A key used to get a linker texture.
        /// </summary>
        private struct _LinkerKey
        {
            public double ChildRadius;
            public double ChildBorderWidth;
        }
        private struct _LinkerTexture
        {
            public Texture Texture;
            public Vector Size;
            public double ChildCenterOffset;
        }

        /// <summary>
        /// Creates a linker texture for the given key.
        /// </summary>
        private static _LinkerTexture _CreateTexture(_LinkerKey Key)
        {
            double grabberwidth = Key.ChildBorderWidth;
            double handlelength = 0.4;
            double handlewidth = 0.1;
            double grabbermaxangle = Math.PI / 8;
            double partangle = grabbermaxangle / 2.0;

            double maxheight = handlewidth / 2.0;

            double innerwidth = Key.ChildRadius - grabberwidth;
            double maxgrabberwidth = Key.ChildRadius - Math.Cos(grabbermaxangle) * innerwidth;
            double posheight = Math.Sin(grabbermaxangle) * Key.ChildRadius;
            if (posheight > maxheight)
            {
                maxheight = posheight;
            }

            Vector size = new Vector(maxgrabberwidth + handlelength, maxheight * 2.0);
            double grabbercenter = -Key.ChildRadius + maxgrabberwidth;

            double midcenter = 0.5 * size.Y;
            Drawer dr = Drawer.Create(delegate(Vector Point)
            {
                Point.X *= size.X;
                Point.Y *= size.Y;

                Vector grabberdif = (Point - new Vector(grabbercenter, midcenter));
                double grabberdis = grabberdif.Length;
                double grabberang = grabberdif.Angle;

                if (grabberdis < innerwidth)
                {
                    return Color.Transparent;
                }
                if (grabberdis < Key.ChildRadius)
                {
                    double angdif = Math.Abs(grabberang);
                    if (angdif < grabbermaxangle - partangle)
                    {
                        return Color.RGBA(0.9, 0.9, 0.9, 1.0);
                    }
                    if (angdif < grabbermaxangle)
                    {
                        return Color.RGBA(0.9, 0.9, 0.9, 0.5);
                    }
                }
                if (Math.Abs(Point.Y - midcenter) < handlewidth / 2.0)
                {
                    double a = (Point.X - maxgrabberwidth) / handlelength;
                    return Color.RGBA(0.9, 0.9, 0.9, 1.0 - a * a);
                }
                return Color.Transparent;
            });
            return new _LinkerTexture()
            {
                Size = size,
                ChildCenterOffset = grabbercenter,
                Texture = Texture.Define(dr, size.X / size.Y, 0.7)
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

            return Child.AllowParentLink(this) && this.AllowLink(Index, this, Child);
        }

        /// <summary>
        /// Corrects (repositions participants) the link between a parent and child driftoid.
        /// </summary>
        internal static void _CorrectLink(LinkedDriftoid Parent, LinkedDriftoid Child, int Iter, Vector Difference, double Distance)
        {
            // Is there a problem
            if (Math.Abs(Distance - Parent.Radius - Child.Radius) > 0.001)
            {
                // Equivalent to another problem...
                Driftoid._CollisionResponse(Parent, Child, Difference, Distance, 1.0);
            }
        }

        /// <summary>
        /// Unlinks the specified child.
        /// </summary>
        internal static void _Delink(LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            _SimpleDelink(Parent, Child);

            // Apply a small impluse to ensure they stay delinked
            Vector dif = Parent.Position - Child.Position;
            dif = dif * (1.0 / dif.Length);
            Parent._ApplyImpulse(dif);
            Child._ApplyImpulse(-dif);
        }

        /// <summary>
        /// Unlinks the specified child, providing no way to ensure they remain delinked.
        /// </summary>
        internal static void _SimpleDelink(LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            Parent._LinkedChildren.Remove(Child);
            Child._LinkedParent = null;
            Parent.OnChildDelink(Child);
            Child.OnParentDelink(Parent);
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
                        return;
                    }
                }

                if (B._LinkedParent == null)
                {
                    if (A._CanLink(B, out ind))
                    {
                        A._Link(B, ind);
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

            // Events
            this.OnChildLink(Index, Child);
            Child.OnParentLink(this);
        }

        internal LinkedDriftoid _LinkedParent;
        internal List<LinkedDriftoid> _LinkedChildren;
        internal Reaction _CurrentReaction;
    }
}