using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// A part of an entity that handles some type of input, output and interaction. Components of related types and duties may interact indirectly through 
    /// their interfaces, even if they do not belong to the same entity.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Gets the entity this component belongs to.
        /// </summary>
        public Entity Owner
        {
            get
            {
                this._Owner = Owner;
            }
        }

        /// <summary>
        /// Called when a component is linked to an entity.
        /// </summary>
        internal protected virtual void OnLink(Entity Entity)
        {
            
        }

        /// <summary>
        /// Called when a component is unlinked from its owner entity. This is the place to free any resources specific to the component.
        /// </summary>
        internal protected virtual void OnUnlink()
        {

        }

        private Entity _Owner;
    }
}