using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A dynamic area in which driftoids occupy. Note that at any time, each point in the area may be occupied
    /// by at most one driftoid.
    /// </summary>
    public class Area
    {
        public Area()
        {
            this._DriftCommands = new Dictionary<Player, DriftCommand>();
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
        /// Gets the driftoid at the specified position. Returns null if the point is unoccupied.
        /// </summary>
        public Driftoid Pick(Vector Position)
        {
            foreach (Driftoid dr in this._Driftoids)
            {
                double radius = dr.Radius;
                if ((dr.MotionState.Position - Position).SquareLength <= radius * radius)
                {
                    return dr;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the area by the specified time.
        /// </summary>
        public void Update(double Time)
        {
            // Drift commands
            foreach (Driftoid dr in this._Driftoids)
            {
                NucleusDriftoid ndr = dr as NucleusDriftoid;
                if (ndr != null)
                {
                    DriftCommand dc;
                    if (this._DriftCommands.TryGetValue(ndr.Player, out dc))
                    {
                        ndr._Pull(Time, dc.TargetDriftoid, dc.TargetPosition);
                    }
                }
            }

            // Update positions
            foreach (Driftoid dr in this._Driftoids)
            {
                dr._MotionState.Update(Time, 0.7, 0.8);
            }

            // Collision handling
            for (int idra = 0; idra < this._Driftoids.Count; idra++)
            {
                for (int idrb = idra + 1; idrb < this._Driftoids.Count; idrb++)
                {
                    Driftoid dra = this._Driftoids[idra];
                    Driftoid drb = this._Driftoids[idrb];
                    double trad = dra.Radius + drb.Radius;
                    Vector dif = drb.Position - dra.Position;
                    double dis = dif.Length;
                    LinkedDriftoid ldra = dra as LinkedDriftoid;
                    LinkedDriftoid ldrb = drb as LinkedDriftoid;
                    if (ldra != null && ldrb != null)
                    {
                        if (ldra.LinkedParent == ldrb)
                        {
                            LinkedDriftoid._CorrectLink(ldra, ldrb, 0, dif, dis);
                            continue;
                        }
                        if (ldrb.LinkedParent == ldra)
                        {
                            LinkedDriftoid._CorrectLink(ldrb, ldra, 0, -dif, dis);
                            continue;
                        }
                    }

                    if (dis <= trad)
                    {
                        // Try link
                        if (ldra != null && ldrb != null)
                        {
                            LinkedDriftoid._Link(ldra, ldrb);
                            if (ldra.LinkedParent == ldrb)
                            {
                                LinkedDriftoid._CorrectLink(ldra, ldrb, 0, dif, dis);
                                continue;
                            }
                            if (ldrb.LinkedParent == ldra)
                            {
                                LinkedDriftoid._CorrectLink(ldrb, ldra, 0, dif, dis);
                                continue;
                            }
                        }

                        Driftoid._CollisionResponse(dra, drb, dif, dis, 1.0);
                    }
                }
            }
        }

        /// <summary>
        /// Spawns the specified driftoid, out of nowhere. This should not be called during an update.
        /// </summary>
        public void Spawn(Driftoid Driftoid)
        {
            this._Driftoids.Add(Driftoid);
        }

        /// <summary>
        /// Issues or updates a drift command for the specified player.
        /// </summary>
        public void IssueCommand(Player Player, DriftCommand Command)
        {
            this._DriftCommands[Player] = Command;
        }

        /// <summary>
        /// Cancels all drift commands for the specified player.
        /// </summary>
        public void CancelDriftCommand(Player Player)
        {
            this._DriftCommands.Remove(Player);
        }

        /// <summary>
        /// Currently active drift commands.
        /// </summary>
        private Dictionary<Player, DriftCommand> _DriftCommands;

        /// <summary>
        /// The driftoids that occupy the area.
        /// </summary>
        private List<Driftoid> _Driftoids;
    }
}