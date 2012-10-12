using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using PlayWithYourPeas.Data;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// Shows the flag for a jumpspot. This Sprite is bound to the SpriteBlock class
    /// where it is utilized to keep flag logic separated from the block logic, while
    /// coupling the sprite right where the data is.
    /// </summary>
    internal class SpriteFlag : Sprite
    {
        protected DataJumpSpot _source;

        protected Texture2D _flagGoodTexture;
        protected Texture2D _flagBadTexture;

        protected Single _displayCompletion;
        protected Single _displayDeathrate;

        /// <summary>
        /// Creates a new flag sprite
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="source"></param>
        public SpriteFlag(SceneLayer layer, DataJumpSpot source) : base(layer, "Graphics/Flag-Pole" )
        {
            _source = source;

            //if (source.Placement == DataJumpSpot.Location.Left)
            //    this.Effects = SpriteEffects.FlipHorizontally;
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(ContentManager manager)
        {
            base.LoadContent(manager);
            _flagBadTexture = this.ContentManager.Load<Texture2D>("Graphics/Flag-Bad");
            _flagGoodTexture = this.ContentManager.Load<Texture2D>("Graphics/Flag-Good");
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _displayCompletion += (Single)((_source.Completion - _displayCompletion) * gameTime.ElapsedGameTime.TotalSeconds * 3);
            _displayDeathrate += (Single)((_source.FailFloat - _displayDeathrate) * gameTime.ElapsedGameTime.TotalSeconds * 3); 
        }

        /// <summary>
        /// Draw flag(s)
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(GameTime gameTime, Vector2 offset)
        {
            // Draw pole
            base.Draw(gameTime, offset + Vector2.UnitY * _texture.Height);
            Texture2D temp = _texture;

            // Draw flag
            _texture = _flagGoodTexture;

            var totalCompletion = (_displayCompletion + _displayDeathrate);
            var deathPartial = totalCompletion > 0 ? _displayDeathrate / totalCompletion : 0;
            var succesPartial = 1 - deathPartial;

            this.SourceRectangle = new Rectangle(0, 0, _texture.Width, (Int32)(_texture.Height * succesPartial));
            base.Draw(gameTime, offset + Vector2.UnitY * ((temp.Height - 36) * totalCompletion + _texture.Height) + Vector2.UnitX * 30); //(_source.Placement == DataJumpSpot.Location.Left ? -5 : 30));

            // Draw deaths
            if (_displayDeathrate > 0)
            {
                _texture = _flagBadTexture;
                this.SourceRectangle = new Rectangle(0, (Int32)(_texture.Height * succesPartial), _texture.Width, (Int32)(_texture.Height * deathPartial));
                base.Draw(gameTime, offset + Vector2.UnitY * ((temp.Height - 36) * totalCompletion - this.SourceRectangle.Y + _texture.Height) + Vector2.UnitX * 30);//(_source.Placement == DataJumpSpot.Location.Left ? -5 : 30));
            }

            // Reset texture
            _texture = temp;
            this.SourceRectangle = _texture.Bounds;
        }
    }
}
