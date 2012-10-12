using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayWithYourPeas.Data;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// The Sprite Jump Info shows information about the current jump. At this
    /// moment the info consists of a graphical and textual representation of 
    /// how many times each blocktype was hit.
    /// </summary>
    internal class SpriteJumpInfo : Sprite
    {
        protected Dictionary<Data.BlockType, Int32> _data;
        protected Sprite[] _blocks;
        protected SpriteFont _font;

        /// <summary>
        /// 
        /// </summary>
        public Boolean PointsInfoVisible { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean BlocksInfoVisible { get; set; }

        /// <summary>
        /// Type to index
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected Int32 SpriteIndex(Data.BlockType type) {
            switch (type)
            {
                case Data.BlockType.Normal:
                    return 0;
                case Data.BlockType.Gel:
                    return 1;
                case Data.BlockType.LeftRamp:
                    return 2;
                case Data.BlockType.RightRamp:
                    return 3;
                case Data.BlockType.Spring:
                    return 4;
                default:
                    return 5;
            }
        }

        /// <summary>
        /// Index to Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected Data.BlockType DataIndex(Int32 index)
        {
            switch (index)
            {
                case 0:
                    return Data.BlockType.Normal;
                case 1:
                    return Data.BlockType.Gel;
                case 2:
                    return Data.BlockType.LeftRamp;
                case 3:
                    return Data.BlockType.RightRamp;
                case 4:
                    return Data.BlockType.Spring;

                default:
                    return Data.BlockType.None;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Rectangle SpriteSourceRectangle(Int32 index)
        {
            switch (index)
            {
                case 0:
                    return new Rectangle(3, 3, 17, 17);
                case 1:
                    return new Rectangle(22, 3, 17, 17);
                case 2:
                    return new Rectangle(3, 20, 18, 18);
                case 3:
                    return new Rectangle(21, 20, 18, 18);
                case 4:
                    return new Rectangle(2, 41, 18, 18);
                
                default:
                    return new Rectangle(21, 41, 18, 16); ;
            }
        }

        /// <summary>
        /// Current Display state
        /// </summary>
        protected SpriteState CurrentState { get; set; }

        /// <summary>
        /// Creates a new jump info sprite
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="data"></param>
        public SpriteJumpInfo(SceneLayer layer, Dictionary<Data.BlockType, Int32> data)
            : base(layer, "")
        {
            _data = data;

            _blocks = new Sprite[] { 
                new Sprite(layer, "Graphics/Icon-Tools"),  
                new Sprite(layer, "Graphics/Icon-Tools"),
                new Sprite(layer, "Graphics/Icon-Tools"),
                new Sprite(layer, "Graphics/Icon-Tools"),
                new Sprite(layer, "Graphics/Icon-Tools"),
                new Sprite(layer, "Graphics/Icon-Tools"),
            };

            this.Color = Color.Transparent;
        }

        /// <summary>
        /// Initialzes jup info
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            foreach (var block in _blocks)
                block.Initialize();

            this.CurrentState = SpriteState.Active;
            this.PointsInfoVisible = !true;
            this.BlocksInfoVisible = !false;
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager manager)
        {
            base.LoadContent(manager);

            _font = this.ContentManager.Load<SpriteFont>("Fonts/Small");
            for (Int32 i = 0; i < _blocks.Length; i++)
            {
                _blocks[i].LoadContent(manager);
                _blocks[i].SourceRectangle = SpriteSourceRectangle(i);
                _blocks[i].Size = new Vector2(_blocks[i].SourceRectangle.Width, _blocks[i].SourceRectangle.Height);
            }

            //_blocks[SpriteIndex(Data.BlockType.None)].Scale = 0.01f; // todo icon

            this.Size = Vector2.UnitX * (_blocks.Length + 1) * 40 + Vector2.UnitY * _blocks[0].Size.Y;
        }

        /// <summary>
        /// Unloads all content
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            foreach (var block in _blocks)
                block.UnloadContent();
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            for (Int32 i = 0; i < _blocks.Length; i++)
            {
                _blocks[i].Update(gameTime);
                _blocks[i].Position = this.Position + Vector2.UnitX * 20 * (i % 3) + Vector2.UnitY * (20 * (i / 3));
                _blocks[i].Color = this.Color;
            }

            // Update opacity
            this.Color = Color.Lerp(this.Color, this.CurrentState == SpriteState.TransitionOff ? 
                Color.Transparent : Color.White, (Single)gameTime.ElapsedGameTime.TotalSeconds * 5);

            if (this.CurrentState == SpriteState.TransitionOff)
            {
                if (this.Color.A < 5)
                {
                    this.CurrentState = SpriteState.Hidden;
                    this.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Draw jump inf
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Vector2 offset)
        {
            //base.Draw(gameTime, offset);
            if (this.Visible == false)
                return;

            Color tcolor = new Color(0, 0, 0, this.Color.A);

            // Block hit info
            if (this.BlocksInfoVisible)
            {
                for (Int32 i = 0; i < _blocks.Length; i++)
                {
                    _blocks[i].Draw(gameTime, offset);
                    this.SceneLayer.SpriteBatch.DrawString(_font, _data[DataIndex(i)].ToString(),
                        _blocks[i].Position - offset + Vector2.UnitX * 3 - Vector2.UnitY * 15 + Vector2.One, tcolor);
                    this.SceneLayer.SpriteBatch.DrawString(_font, _data[DataIndex(i)].ToString(),
                        _blocks[i].Position - offset + Vector2.UnitX * 3 - Vector2.UnitY * 15, this.Color);
                }
            }

            // Points info
            if (this.PointsInfoVisible)
            {
                var points = PointsControllerState.CalculatePoints(_data);

                this.SceneLayer.SpriteBatch.DrawString(_font, points.ToString(), this.Position - offset + Vector2.One, tcolor);
                this.SceneLayer.SpriteBatch.DrawString(_font, points.ToString(), this.Position - offset + Vector2.Zero, this.Color);
            }
        }

        /// <summary>
        /// Exit sprite (go to off transit)
        /// </summary>
        public void Exit()
        {
            this.CurrentState = SpriteState.TransitionOff;
        }
    }
}
