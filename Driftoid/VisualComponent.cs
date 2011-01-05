using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// A component that defines the appearance of an entity.
    /// </summary>
    public abstract class VisualComponent : Component
    {
        /// <summary>
        /// Gets a visual that can create a visual representation of the entity.
        /// </summary>
        public abstract Visual Visual { get; }
    }

    /// <summary>
    /// A visual component that draws a texture in place of a driftoid defined by a physics component.
    /// </summary>
    public class SimpleDriftoidVisualComponent : VisualComponent
    {
        public SimpleDriftoidVisualComponent(int TextureID)
        {
            this._TextureID = TextureID;
        }

        public override Visual Visual
        {
            get
            {
                return new SimpleVisual(this._TextureID, this._Physics.Position, this._Physics.Radius, this._Physics.Angle);
            }
        }

        public override void OnLink(Entity Entity)
        {
            Entity.RegisterLinkHandler<PhysicsComponent>(delegate(PhysicsComponent Physics)
            {
                this._Physics = Physics;
            });
        }

        private PhysicsComponent _Physics;
        private int _TextureID;
    }
}