using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// Represents a type of driftoid. Driftoid kinds give them their properties.
    /// </summary>
    public abstract class Kind
    {
        /// <summary>
        /// Gets how long it takes to process a driftoid of this kind in a reaction.
        /// </summary>
        public virtual double ReactionTime
        {
            get
            {
                return 1.0;
            }
        }

        /// <summary>
        /// Gets if links should be shown on this kind.
        /// </summary>
        public virtual bool ShowLink
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Updates the state of the driftoid as time passes. The returned value indicates the next state of the
        /// kind, if it has changed.
        /// </summary>
        public virtual Kind Update(LinkedDriftoid Driftoid, double Time, IDriftoidInterface Interface)
        {
            return this;
        }

        /// <summary>
        /// Draws a driftoid of this kind.
        /// </summary>
        public virtual void Draw(Driftoid Driftoid)
        {

        }

        /// <summary>
        /// Gets the visual width of the border of a driftoid of this kind.
        /// </summary>
        public virtual double GetBorderWidth(Driftoid Driftoid)
        {
            return Driftoid.Radius * 0.3;
        }

        /// <summary>
        /// Gets if an other driftoid can link to a driftoid of this kind as a child.
        /// </summary>
        public virtual bool AllowLink(int Index, LinkedDriftoid This, LinkedDriftoid Other)
        {
            if (This.LinkedParent != null)
            {
                return This.LinkedParent.Kind.AllowChildLink(Index, This.LinkedParent, This, Other);
            }
            return false;
        }

        /// <summary>
        /// Gets if an other driftoid can link to a driftoid that is a descendant of a driftoid of this kind.
        /// </summary>
        public virtual bool AllowChildLink(int Index, LinkedDriftoid This, LinkedDriftoid Child, LinkedDriftoid Other)
        {
            if (This.LinkedParent != null)
            {
                return This.LinkedParent.Kind.AllowChildLink(Index, This.LinkedParent, Child, Other);
            }
            return false;
        }

        /// <summary>
        /// Gets if this driftoid can be linked to another as a child.
        /// </summary>
        public virtual bool AllowLink(LinkedDriftoid This, LinkedDriftoid Other)
        {
            return true;
        }

        /// <summary>
        /// Gets the player that "owns" this driftoid, or null if this driftoid does not have an alliance.
        /// </summary>
        public virtual Player Owner
        {
            get
            {
                return null;
            }
        }
    }
}