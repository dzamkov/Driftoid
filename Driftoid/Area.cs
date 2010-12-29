using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A dynamic area in which driftoids occupy.
    /// </summary>
    public class Area
    {
        public Area()
        {
            this._Driftoids = new List<Driftoid>();
        }

        /// <summary>
        /// Gets the driftoids in the area. They should not be modified.
        /// </summary>
        public IEnumerable<Driftoid> Driftoids
        {
            get
            {
                return this._Driftoids;
            }
        }

        /// <summary>
        /// Updates the area by the specified time.
        /// </summary>
        public void Update(double Time)
        {

        }

        /// <summary>
        /// Spawns the specified driftoid, out of nowhere. This should not be called during an update.
        /// </summary>
        public void Spawn(Driftoid Driftoid)
        {
            this._Driftoids.Add(Driftoid);
        }

        /// <summary>
        /// The driftoids that occupy the area.
        /// </summary>
        private List<Driftoid> _Driftoids;
    }
}