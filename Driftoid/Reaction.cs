using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// Represents a transformation of a driftoid by absorbing others.
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// The driftoid to be transformed. The root in the reaction sequence.
        /// </summary>
        public LinkedDriftoid Target;

        /// <summary>
        /// A constructor for the product of the transformation.
        /// </summary>
        public DriftoidConstructor Product;

        /// <summary>
        /// Tries starting the reaction in an area.
        /// </summary>
        internal void _Initialize(List<Effect> Effects)
        {
            // Create schedule
            this._ReactantSchedule = new List<List<LinkedDriftoid>>();
            List<LinkedDriftoid> cur = new List<LinkedDriftoid>();
            cur.Add(this.Target);
            while (cur.Count > 0)
            {
                this._ReactantSchedule.Add(cur);
                List<LinkedDriftoid> next = new List<LinkedDriftoid>();
                foreach (LinkedDriftoid ld in cur)
                {
                    next.AddRange(ld.LinkedChildren);
                }
                cur = next;
            }

            // Initialize state
            this._CurrentPhaseTime = 0.0;
            this._Complete = false;

            this._Phase = this._ReactantSchedule.Count;
            this._AdvancePhase(Effects, null);
        }

        /// <summary>
        /// Advances the phase of the reaction.
        /// </summary>
        private void _AdvancePhase(List<Effect> Effects, List<LinkedDriftoid> ToDelete)
        {
            this._Phase--;
            if (this._Phase >= 0)
            {
                List<LinkedDriftoid> absorbing = this._ReactantSchedule[this._ReactantSchedule.Count - 1];
                this._ReactantSchedule.RemoveAt(this._ReactantSchedule.Count - 1);

                if (this._Absorbing != null)
                {
                    ToDelete.AddRange(this._Absorbing);
                }
                this._Absorbing = absorbing;

                // Add effects
                foreach (LinkedDriftoid dr in absorbing)
                {
                    Effects.Add(new _ReactionGlowEffect()
                    {
                        Phase = this._Phase,
                        Target = dr,
                        Reaction = this
                    });
                }
            }
            else
            {
                // TODO: Spawn product here
                ToDelete.Add(Target);
                this._Complete = true;
            }
        }

        /// <summary>
        /// Updates the state of the reaction.
        /// </summary>
        internal void _Update(double Time, List<LinkedDriftoid> ToDelete, List<Effect> Effects)
        {
            this._CurrentPhaseTime += Time;
            while (this._CurrentPhaseTime > PhaseTime)
            {
                this._CurrentPhaseTime -= PhaseTime;
                this._AdvancePhase(Effects, ToDelete);
            }
        }

        /// <summary>
        /// An effect given to reactants as they start glowing.
        /// </summary>
        private class _ReactionGlowEffect : Effect
        {
            public override void Draw()
            {
                double phasetime = Reaction._CurrentPhaseTime / PhaseTime;
                phasetime = Math.Sqrt(phasetime);
                GL.Color4(1.0, 1.0, 1.0, phasetime);
                Visual.DrawTexture(Driftoid.Mask.ID, this.Target.Position, this.Target.Radius, 0.0);
                GL.Color4(1.0, 1.0, 1.0, 1.0);
            }

            public override void Update(double Time, IEffectInterface Interface)
            {
                if (this.Reaction._Phase < this.Phase)
                {
                    if (this.Reaction.Target != this.Target)
                    {
                        // Send an orb to the next reactant.
                        Interface.Spawn(new _ReactionOrbEffect()
                        {
                            Phase = this.Phase - 1,
                            StartPos = this.Target.Position,
                            StartRadius = this.Target.Radius,
                            Target = this.Target.LinkedParent,
                            Reaction = this.Reaction
                        });
                    }
                    Interface.Delete();
                }
            }

            public int Phase;
            public LinkedDriftoid Target;
            public Reaction Reaction;
        }

        /// <summary>
        /// An effect given as the reactant absorbtion propagates to the target.
        /// </summary>
        private class _ReactionOrbEffect : Effect
        {
            public override void Draw()
            {
                double phasetime = this.Reaction._CurrentPhaseTime / PhaseTime;
                phasetime = phasetime * phasetime * (3 - 2 * phasetime);
                Vector pos = this.StartPos + (this.Target.Position - this.StartPos) * phasetime;
                double rad = this.StartRadius + (this.Target.Radius - this.StartRadius) * phasetime;
                Visual.DrawTexture(Driftoid.Mask.ID, pos, rad, 0.0);
            }

            public override void Update(double Time, IEffectInterface Interface)
            {
                if (this.Reaction._Phase < this.Phase)
                {
                    Interface.Delete();
                }
            }

            public int Phase;
            public Vector StartPos;
            public double StartRadius;
            public LinkedDriftoid Target;
            public Reaction Reaction;
        }

        private bool _Complete;
        private List<LinkedDriftoid> _Absorbing;
        private List<List<LinkedDriftoid>> _ReactantSchedule;
        private double _CurrentPhaseTime;
        private int _Phase;

        /// <summary>
        /// The time it takes to complete one reaction phase. Each reaction phase, one level of depth of the reactants is absorbed.
        /// </summary>
        public const double PhaseTime = 0.7;
    }

}