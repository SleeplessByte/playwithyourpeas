using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using PlayWithYourPeas.Data;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Logic;
using Microsoft.Xna.Framework.Graphics;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// Draws the entire hud 
    /// TODO draw the toolbox in here
    /// </summary>
    internal class SpritesetHud : Sprite
    {
        protected InputControllerState _source;
        protected PointsController _controller;
        protected TimeController _timeController;

        // Placing sprites
        protected SpriteBlock _placingSprite;
        protected DataBlock _placingBlock;
        protected Sprite _cursorStandard;
        protected Sprite _cursorDelete;

        // Logo, score. time
        protected Sprite _spriteLogo;
        protected Sprite _spriteHappyPoints;
        protected Sprite _spriteTime;
        protected SpriteFont _fontTime;
        protected SpriteFont _fontPoints;

        protected Int32 _displayScore;
        
        /// <summary>
        /// 
        /// </summary>
        public Single RunningSpeed { get { return _timeController != null ? _timeController.Speed : 1f; } }

        /// <summary>
        /// Creates a new hud
        /// </summary>
        /// <param name="layer"></param>
        public SpritesetHud(SceneLayer layer, PointsController controller, InputControllerState source)
            : base(layer)
        {
            _source = source;
            _controller = controller;

            _cursorStandard = new Sprite(layer, "Graphics/Pointer-Standard");
            _cursorDelete = new Sprite(layer, "Graphics/Pointer-Delete");
           
            _placingBlock = new DataBlock(source.GridPosition, source.Type == BlockType.None ? BlockType.Normal : source.Type, null);
            _placingSprite = new SpriteBlock(layer, _placingBlock);

            _spriteLogo = new Sprite(layer, "Graphics/Logo") { Position = Vector2.UnitY * 40 + Vector2.UnitX * 50 };
            _spriteHappyPoints = new Sprite(layer, "Graphics/Icon-HappyPoints") { Position = Vector2.UnitY * 40 + Vector2.UnitX * 520 };
            _spriteTime = new Sprite(layer, "Graphics/Icon-Time") { Position = Vector2.UnitY * 40 + Vector2.UnitX * 1130 };            
        }

        /// <summary>
        /// Initializes all underlying sprites
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _placingSprite.Initialize();
            _cursorStandard.Initialize();
            _cursorDelete.Initialize();
            _spriteLogo.Initialize();
            _spriteTime.Initialize();
            _spriteHappyPoints.Initialize();

            _cursorStandard.Visible = _source.Type != BlockType.Delete;
            _cursorDelete.Visible = _source.Type == BlockType.Delete;

            _timeController = this.SceneLayer.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager manager)
        {
            base.LoadContent(manager);

            _placingSprite.LoadContent(manager);
            _cursorStandard.LoadContent(manager);
            _cursorDelete.LoadContent(manager);
            _spriteHappyPoints.LoadContent(manager);
            _spriteLogo.LoadContent(manager);
            _spriteTime.LoadContent(manager);

            _fontTime = this.ContentManager.Load<SpriteFont>("Fonts/Time");
            _fontPoints = this.ContentManager.Load<SpriteFont>("Fonts/Time");
        }

        /// <summary>
        /// Unloads all unmanaged content
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            _placingSprite.UnloadContent();
            _cursorStandard.UnloadContent();
            _cursorDelete.UnloadContent();
            _spriteHappyPoints.UnloadContent();
            _spriteLogo.UnloadContent();
            _spriteTime.UnloadContent();
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            _placingBlock.GridPosition = _source.GridPosition;
            
            if (_source.Type != _placingBlock.BlockType)
            {
                _placingBlock.Place(_source.Type == BlockType.None ? BlockType.Normal : _source.Type);
                _cursorStandard.Visible = _source.Type != BlockType.Delete;
                _cursorDelete.Visible = _source.Type == BlockType.Delete;

            }

            var targetColor = Color.Lerp(Color.White, Color.Green, Math.Min(1f, (_controller.PlayingScore - _displayScore) / 100f));
            this.Color = Color.Lerp(this.Color, targetColor, (Single)gameTime.ElapsedGameTime.TotalSeconds * 10 * RunningSpeed);

            _displayScore = (Int32)MathHelper.Lerp(_displayScore, _controller.PlayingScore, (Single)gameTime.ElapsedGameTime.TotalSeconds * 10 * RunningSpeed);

            _placingSprite.Update(gameTime);
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Vector2 offset)
        {
            //base.Draw(gameTime, offset);

            _placingSprite.Draw(gameTime, offset - _source.Offset);

            _cursorDelete.Draw(gameTime, offset - _source.Position);
            _cursorStandard.Draw(gameTime, offset - _source.Position);

            _spriteLogo.Draw(gameTime, offset);
            _spriteTime.Draw(gameTime, offset);

            this.SceneLayer.SpriteBatch.DrawString(_fontTime, String.Format("{0}:{1:00}",  Math.Floor((Double)_controller.PlayingTime.TotalMinutes), Math.Floor((Double)_controller.PlayingTime.Seconds)),
                _spriteTime.Position + (_spriteTime.Size.X + 10) * Vector2.UnitX + Vector2.One - Vector2.UnitY * 10, Color.Black);
            this.SceneLayer.SpriteBatch.DrawString(_fontTime, String.Format("{0}:{1:00}",  Math.Floor((Double)_controller.PlayingTime.TotalMinutes), Math.Floor((Double)_controller.PlayingTime.Seconds)),
                _spriteTime.Position + (_spriteTime.Size.X + 10) * Vector2.UnitX - Vector2.UnitY * 10, Color.White);

            _spriteHappyPoints.Draw(gameTime, offset);

            this.SceneLayer.SpriteBatch.DrawString(_fontPoints, String.Format("{0:N0} ({1:##}M)", _displayScore, 20), 
                _spriteHappyPoints.Position + (_spriteHappyPoints.Size.X + 10) * Vector2.UnitX + Vector2.One - Vector2.UnitY * 10, Color.Black);
            this.SceneLayer.SpriteBatch.DrawString(_fontPoints, String.Format("{0:N0} ({1:##}M)", _displayScore, 20),
                _spriteHappyPoints.Position + (_spriteHappyPoints.Size.X + 10) * Vector2.UnitX - Vector2.UnitY * 10, this.Color);
        }
    }
}
