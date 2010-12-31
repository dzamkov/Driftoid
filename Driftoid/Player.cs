using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A player in a game of driftoids. Players are able to control driftoids of their own alliance. They may not
    /// directly affect the game.
    /// </summary>
    public class Player
    {
        public Player(Color Color)
        {
            this._Color = Color;
        }

        /// <summary>
        /// Gets the color associated with this player.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        /// <summary>
        /// Gets a color very slightly faded from the player color.
        /// </summary>
        public Color FadedColor
        {
            get
            {
                return this._Color.Desaturate(0.2, 0.5);
            }
        }

        /// <summary>
        /// Gets a color complementary to the player color. This color is usually less intense, and distinct, to
        /// the player color.
        /// </summary>
        public Color SecondaryColor
        {
            get
            {
                return this._Color.Desaturate(0.4, 0.7);
            }
        }

        private Color _Color;
    }

}