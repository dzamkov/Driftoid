using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A high-mass player-controlled driftoid that can apply force to other driftoids.
    /// </summary>
    public class NucleusDriftoid : LinkedDriftoid
    {
        private NucleusDriftoid(Player Owner, MotionState MotionState, double Mass, double Radius) : base(MotionState, Mass, Radius)
        {
            this._Owner = Owner;
        }

        /// <summary>
        /// Gets a constructor for a nucleus driftoid owned by the specified player.
        /// </summary>
        public static DriftoidConstructor GetConstructor(Player Owner)
        {
            return new DriftoidConstructor((MotionState MotionState, double Mass, double Radius) => new NucleusDriftoid(Owner, MotionState, Mass, Radius),
                20.0, 3.0);
        }

        public override void Draw()
        {
            int texid;
            if (!_Textures.TryGetValue(this._Owner, out texid))
            {
                _Textures[this._Owner] = texid = _CreateTexture(this._Owner);
            }
            this.DrawTexture(texid, 1.0, 0.0);
        }

        private static int _CreateTexture(Player Player)
        {
            Color primary = Player.FadedColor;
            Color secondary = Player.SecondaryColor;
            int texsize = 256;
            float actualsize = (float)texsize;
            using (Bitmap bm = new Bitmap(texsize, texsize))
            {
                Driftoid.DrawSolid(bm, 0.15, primary, secondary);
                using (Graphics g = Graphics.FromImage(bm))
                {
                    int dashamount = 9;
                    double dashdelta = Math.PI * 2.0 / (double)dashamount;
                    Vector center = new Vector(actualsize / 2.0f, actualsize / 2.0f);
                    using (Pen p = new Pen(primary, actualsize * 0.05f))
                    {
                        for (int t = 0; t < dashamount; t++)
                        {
                            double dashangle = dashdelta * (double)t;
                            Vector dashunit = Vector.Unit(dashangle) * (double)actualsize * 0.5;

                            g.DrawLine(p, dashunit * 0.3 + center, dashunit * 0.6 + center);
                        }
                    }
                }
                return Texture.Create(bm);
            }
        }

        private static readonly Dictionary<Player, int> _Textures = new Dictionary<Player, int>();

        /// <summary>
        /// Gets the maximum force magnitude the nucleus can apply.
        /// </summary>
        public double MaxForce
        {
            get
            {
                return 18.0;
            }
        }

        /// <summary>
        /// Gets the radius of the area of influence of this nucleus.
        /// </summary>
        public double MaxDistance
        {
            get
            {
                return 30.0;
            }
        }

        public override bool AllowLink(int Index, LinkedDriftoid Parent, LinkedDriftoid Child)
        {
            return true;
        }

        internal void _Pull(double Time, Driftoid Driftoid, Driftoid Target, Vector Position)
        {
            double dis = (Target.Position - Driftoid.Position).Length;
            if (dis < this.MaxDistance)
            {
                Vector vel = Target.MotionState.Velocity;
                double vellen = vel.Length;
                vel = vel * vellen;

                Vector gpos = Target.Position + vel * (Target.Mass / this.MaxForce);
                Vector vec = (Position - gpos);
                double mdis = vec.Length;
                Vector nvec = vec * (1.0 / mdis);
                vec = nvec * this.MaxForce;
                Target._ApplyForce(Time, vec);
                Driftoid._ApplyForce(Time, -vec);
            }
        }

        public Player Owner
        {
            get
            {
                return this._Owner;
            }
        }

        private Player _Owner;
    }

    /// <summary>
    /// A player-wide command that causes all the players nuclei to position (by applying force) another driftoid.
    /// </summary>
    public struct DriftCommand
    {
        /// <summary>
        /// The driftoid to be moved.
        /// </summary>
        public Driftoid TargetDriftoid;

        /// <summary>
        /// The position the driftoid should be moved to.
        /// </summary>
        public Vector TargetPosition;
    }
}