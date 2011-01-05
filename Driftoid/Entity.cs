using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// An actor in a world whose properties are defined by components. Components inside the same entity may interact directly by
    /// sharing variables or values. Note that some combination of components may be automatically recognized and used as a combination by a world.
    /// </summary>
    public sealed class Entity
    {
        public Entity()
        {
            this._Components = new List<Component>();
        }

        /// <summary>
        /// Gets a component of the specified type from the entity, if the entity defines it.
        /// </summary>
        public T GetComponent<T>()
            where T : Component
        {
            foreach (Component c in this._Components)
            {
                T tc = c as T;
                if (tc != null)
                {
                    return tc;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a component to the entity.
        /// </summary>
        public void LinkComponent(Component Component)
        {
            this._Components.Add(Component);
            if (this._LinkHandlers != null)
            {
                LinkedListNode<_GenericLinkHandler> cur = this._LinkHandlers.First;
                while (cur != null)
                {
                    if (cur.Value.Handle(Component))
                    {
                        cur = cur.Next;
                        this._LinkHandlers.Remove(cur);
                    }
                    else
                    {
                        cur = cur.Next;
                    }
                }
            }
            Component.OnLink(this);
        }

        public void RegisterLinkHandler<T>(ComponentLinkHandler<T> Handler)
            where T : Component
        {
            T c = this.GetComponent<T>();
            if (c == null)
            {
                if (this._LinkHandlers == null)
                {
                    this._LinkHandlers = new LinkedList<_GenericLinkHandler>();
                }
                this._LinkHandlers.AddLast(new _LinkHandler<T>() { _Handler = Handler });
            }
            else
            {
                Handler(c);
            }
        }

        private abstract class _GenericLinkHandler
        {
            public abstract bool Handle(Component Component);
        }

        private class _LinkHandler<T> : _GenericLinkHandler
            where T : Component
        {
            public override bool Handle(Component Component)
            {
                T x = Component as T;
                if (x != null)
                {
                    this._Handler(x);
                    return true;
                }
                return false;
            }

            public ComponentLinkHandler<T> _Handler;
        }

        private LinkedList<_GenericLinkHandler> _LinkHandlers;
        private List<Component> _Components;
    }

    /// <summary>
    /// Creates an entity based on a parameter.
    /// </summary>
    public delegate Entity EntityConstructor<T>(T Parameter);

    /// <summary>
    /// A handler for when a component of a certain kind is linked (or has been linked) to an entity.
    /// </summary>
    public delegate T ComponentLinkHandler<T>(T Component);
}