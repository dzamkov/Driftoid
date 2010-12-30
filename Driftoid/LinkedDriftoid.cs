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