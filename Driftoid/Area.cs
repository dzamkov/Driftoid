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
            this._Effects = new List<Effect>();
            this._Reactions = new List<Reaction>();
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

        private class _EffectInterface : IEffectInterface
        {
            public void Delete()
            {
                this.ToDelete.Add(Effect);
            }

            public void Spawn(Effect Effect)
            {
                this.ToAdd.Add(Effect);
            }

            public Effect Effect;
            public List<Effect> ToDelete;
            public List<Effect> ToAdd;
        }

        /// <summary>
        /// Updates the effects in the area. This is implicitly done by Update.
        /// </summary>
        public void UpdateEffects(double Time)
        {
            List<Effect> todelete = new List<Effect>();
            List<Effect> toadd = new List<Effect>();
            _EffectInterface i = new _EffectInterface()
            {
                ToAdd = toadd,
                ToDelete = todelete
            };

            foreach (Effect e in this._Effects)
            {
                i.Effect = e;
                e.Update(Time, i);
            }
            foreach (Effect e in todelete)
            {
                this._Effects.Remove(e);
            }
            foreach (Effect e in toadd)
            {
                this._Effects.Add(e);
            }
        }

        private class _DriftoidInterface : IDriftoidInterface
        {
            public void ChangeMass(double Mass)
            {
                this.Driftoid._Mass = Mass;
            }

            public void ChangeRadius(double Radius)
            {
                this.Driftoid._Radius = Radius;
            }

            public void Delete()
            {
                this.ToDelete.Add(this.Driftoid);
            }

            public LinkedDriftoid Driftoid;
            public List<LinkedDriftoid> ToDelete;
        }

        /// <summary>
        /// Updates the area by the specified time.
        /// </summary>
        public void Update(double Time)
        {
            // Reactions
            List<LinkedDriftoid> toremove = new List<LinkedDriftoid>();
            foreach (Reaction r in this._Reactions)
            {
                r._Update(Time, toremove, this._Effects);
            }

            // Drift commands
            foreach (LinkedDriftoid dr in this._Driftoids)
            {
                NucleusDriftoid ndr = dr as NucleusDriftoid;
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
            foreach (LinkedDriftoid ldr in this._Driftoids)
            {
                ldr.OnUpdate(Time, new _DriftoidInterface() { Driftoid = ldr, ToDelete = toremove });
            }
            foreach (LinkedDriftoid ldr in toremove)
            {
                ldr._Delete();
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

            this.UpdateEffects(Time);
        }

        /// <summary>
        /// Indicates that a player is trying to manually delink a driftoid from its parent.
        /// </summary>
        public void TryDelink(Player Player, LinkedDriftoid Target)
        {
            if (Target.LinkedParent != null && Target.Reaction == null)
            {
                if (this.HasLinkControl(Player, Target))
                {
                    LinkedDriftoid._Delink(Target.LinkedParent, Target);
                }
            }
        }

        /// <summary>
        /// Gets the visuals in this area in the order they should be rendered.
        /// </summary>
        public IEnumerable<Visual> GetVisuals()
        {
            // Driftoids
            foreach (LinkedDriftoid ld in this._Driftoids)
            {
                Visual v = ld.Visual;
                if (v != null)
                {
                    yield return v;
                }
            }

            // Driftoid linkers
            foreach (LinkedDriftoid ld in this._Driftoids)
            {
                if (ld._LinkedParent != null)
                {
                    yield return LinkedDriftoid.GetLinkerVisual(ld._LinkedParent, ld);
                }
            }
            
            // Effects
            foreach (Effect e in this._Effects)
            {
                yield return e;
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
                NucleusDriftoid ndr = cur as NucleusDriftoid;
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
        /// Begins a reaction in the area.
        /// </summary>
        public void BeginReaction(Reaction Reaction)
        {
            Reaction._Initialize(this._Effects);
            this._Reactions.Add(Reaction);
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

        /// <summary>
        /// The effects in the area.
        /// </summary>
        private List<Effect> _Effects;

        /// <summary>
        /// The reactions in the area.
        /// </summary>
        private List<Reaction> _Reactions;
    }

    /// <summary>
    /// An interface that allows a driftoid to interact with the environment during update. These commands disallow
    /// conflicting changes by different driftoids.
    /// </summary>
    public interface IDriftoidInterface
    {
        /// <summary>
        /// Changes the mass of the current driftoid.
        /// </summary>
        void ChangeMass(double Mass);

        /// <summary>
        /// Changes the radius of the current driftoid.
        /// </summary>
        void ChangeRadius(double Radius);

        /// <summary>
        /// Removes the current driftoid.
        /// </summary>
        void Delete();
    }
}