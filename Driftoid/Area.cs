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
            this._Driftoids = new List<LinkedDriftoid>();
        }

        /// <summary>
        /// Gets the driftoids in the area. They should not be modified.
        /// </summary>
        public IEnumerable<LinkedDriftoid> Driftoids
        {
            get
            {
                return this._Driftoids;
            }
        }

        /// <summary>
        /// Gets the driftoid at the specified position. Returns null if the point is unoccupied.
        /// </summary>
        public LinkedDriftoid Pick(Vector Position)
        {
            foreach (LinkedDriftoid dr in this._Driftoids)
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
            foreach (LinkedDriftoid dr in this._Driftoids)
            {
                NucleusKind ndr = dr.Kind as NucleusKind;
                if (ndr != null)
                {
                    DriftCommand dc;
                    if (this._DriftCommands.TryGetValue(ndr.Owner, out dc))
                    {
                        ndr._Pull(Time, dr, dc.TargetDriftoid, dc.TargetPosition);
                    }
                }
            }

            // Update state
            List<LinkedDriftoid> toremove = new List<LinkedDriftoid>();
            foreach (LinkedDriftoid ldr in this._Driftoids)
            {
                Kind next = ldr.Kind.Update(Time);
                if (next == null)
                {
                    toremove.Add(ldr);
                }
                else
                {
                    ldr._Kind = next;
                }
            }
            foreach (LinkedDriftoid ldr in toremove)
            {
                this._Driftoids.Remove(ldr);
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
                    LinkedDriftoid dra = this._Driftoids[idra];
                    LinkedDriftoid drb = this._Driftoids[idrb];
                    double trad = dra.Radius + drb.Radius;
                    Vector dif = drb.Position - dra.Position;
                    double dis = dif.Length;
                    if (dra.LinkedParent == drb)
                    {
                        LinkedDriftoid._CorrectLink(dra, drb, 0, dif, dis);
                        continue;
                    }
                    if (drb.LinkedParent == dra)
                    {
                        LinkedDriftoid._CorrectLink(drb, dra, 0, -dif, dis);
                        continue;
                    }

                    if (dis <= trad)
                    {
                        // Try link
                        LinkedDriftoid._Link(dra, drb);
                        if (dra.LinkedParent == drb)
                        {
                            LinkedDriftoid._CorrectLink(dra, drb, 0, dif, dis);
                            continue;
                        }
                        if (drb.LinkedParent == dra)
                        {
                            LinkedDriftoid._CorrectLink(drb, dra, 0, dif, dis);
                            continue;
                        }
                        Driftoid._CollisionResponse(dra, drb, dif, dis, 1.0);
                    }
                }
            }
        }

        /// <summary>
        /// Indicates that a player is trying to manually delink a driftoid from its parent.
        /// </summary>
        public void TryDelink(Player Player, LinkedDriftoid Target)
        {
            if (Target.LinkedParent != null)
            {
                if (this.HasLinkControl(Player, Target))
                {
                    LinkedDriftoid._Delink(Target.LinkedParent, Target);
                }
            }
        }

        /// <summary>
        /// Gets if a player can control the links associated with the target.
        /// </summary>
        public bool HasLinkControl(Player Player, LinkedDriftoid Target)
        {
            LinkedDriftoid cur = Target;
            while (cur != null)
            {
                NucleusKind ndr = cur.Kind as NucleusKind;
                if (ndr != null)
                {
                    if (ndr.Owner == Player)
                    {
                        return true;
                    }
                }

                cur = cur.LinkedParent;
            }
            return false;
        }

        /// <summary>
        /// Spawns the specified driftoid, out of nowhere. This should not be called during an update.
        /// </summary>
        public void Spawn(LinkedDriftoid Driftoid)
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
        private List<LinkedDriftoid> _Driftoids;
    }
}