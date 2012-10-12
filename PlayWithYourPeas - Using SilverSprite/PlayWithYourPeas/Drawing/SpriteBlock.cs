using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using PlayWithYourPeas.Data;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// Shows a block on the screen (when type and state allows that). I used warmloading for
    /// the block sprites. So although the spritesetGrid has methods to actually remove the
    /// sprite from the grid, instead it is made invisible. This is to keep the drawing cycle
    /// lenght more consisitent.
    /// </summary>
    internal class SpriteBlock : Sprite
    {

        protected String _intermediateTextureName;
        protected Texture2D _intermediateTexture;
        protected Texture2D _shadowTexture;
        protected DataBlock _source;

        protected SpriteFlag _jumpSpotLeft;
        protected SpriteFlag _jumpSpotRight;
        protected SpriteAlert _jumpSpotAlert;

        /// <summary>
        /// Block on Grid Position
        /// </summary>
        public Point GridPosition { get { return _source.GridPosition; } }

        /// <summary>
        /// Block State
        /// </summary>
        public BlockState BlockState { get { return _source.BlockState; } }

        /// <summary>
        /// Block Type
        /// </summary>
        public BlockType DisplayingType { get; protected set; }

        /// <summary>
        /// Block is being placed/removed flag
        /// </summary>
        public Boolean IsTransitioning { get { return BlockState == Data.BlockState.Placing || BlockState == Data.BlockState.Removing;  } }

        /// <summary>
        /// Block shadow should be drawn
        /// </summary>
        public Boolean IsShadowVisible { get { return !_source.HasBlockBelow; } }

        /// <summary>
        /// Creates new SpriteBlock
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="source"></param>
        public SpriteBlock(SceneLayer layer, DataBlock source) : base(layer)
        {
            _source = source;
        }

        /// <summary>
        /// Initializes block
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            SetAssetNames();
            this.Position = Vector2.UnitX * 70 * _source.X + Vector2.UnitY * 60 * _source.Y;

            _source.OnStateChanged += new Logic.BlockStateChangedHandler(_source_OnStateChanged);
            _source.OnJumpSpotBound += new EventHandler(_source_OnJumpSpotBound);
            _source.OnJumpSpotCreated += new EventHandler(_source_OnJumpSpotMarked);
            _source.OnJumpSpotCompleted += new EventHandler(_source_OnJumpSpotCompleted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void _source_OnStateChanged(Logic.BlockStateArgs args)
        {
            // Update graphic
            if (this.DisplayingType != args.Block.BlockType)
            {
                _jumpSpotAlert = null;

                SetAssetNames();
                LoadContent(this.ContentManager);
            }

            if (args.State == Data.BlockState.Removing)
                _jumpSpotAlert = null;

        }

        /// <summary>
        /// Sets asset names for current displaytype
        /// </summary>
        public void SetAssetNames()
        {
            this.DisplayingType = _source.BlockType;

            _intermediateTextureName = "Graphics/Block-Place-" + SpriteBlock.AssetName(this.DisplayingType);
            this.TextureName = "Graphics/Block-" + SpriteBlock.AssetName(this.DisplayingType);
            _texture = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _source_OnJumpSpotMarked(object sender, EventArgs e)
        {
            var spot = sender as DataJumpSpot;

            if (_jumpSpotAlert == null)
            {
                _jumpSpotAlert = SpriteAlert.GenerateJumpSpot(this.SceneLayer);

                _jumpSpotAlert.Initialize();
                _jumpSpotAlert.LoadContent(this.ContentManager);
                _jumpSpotAlert.OnFinished += new EventHandler(_jumpSpotAlert_OnFinished);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _source_OnJumpSpotCompleted(object sender, EventArgs e)
        {
            var spot = sender as DataJumpSpot;

            _jumpSpotAlert = SpriteAlert.GenerateCompleted(this.SceneLayer);

            _jumpSpotAlert.Initialize();
            _jumpSpotAlert.LoadContent(this.ContentManager);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _jumpSpotAlert_OnFinished(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Method to run when a jumpspot is bound
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _source_OnJumpSpotBound(object sender, EventArgs e)
        {
            var spot = sender as DataJumpSpot;

            if (spot.Placement == DataJumpSpot.Location.Left)
            {
                _jumpSpotLeft = new SpriteFlag(this.SceneLayer, spot);
                _jumpSpotLeft.Initialize();
                _jumpSpotLeft.LoadContent(this.ContentManager);
            }
            else {
                _jumpSpotRight = new SpriteFlag(this.SceneLayer, spot);
                _jumpSpotRight.Initialize();
                _jumpSpotRight.LoadContent(this.ContentManager);
            }
        }

        /// <summary>
        /// Get asset name for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static String AssetName(BlockType type)
        {
            switch (type)
            {
                case BlockType.Normal:
                    return "Normal";
                case BlockType.Gel:
                    return "Gel";
                case BlockType.Spring:
                    return "Spring";
                case BlockType.LeftRamp:
                    return "LeftRamp";
                case BlockType.RightRamp:
                    return "RightRamp";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager manager)
        {
            this.ContentManager = manager;

            _jumpSpotAlert = null;

            if (this.DisplayingType == BlockType.None || this.DisplayingType == BlockType.Delete)
                return;

            base.LoadContent(manager);

            // Duplicates are shared in memory per GameScreen
            _shadowTexture = this.ContentManager.Load<Texture2D>("Graphics/Block-Shadow");
            _intermediateTexture = this.ContentManager.Load<Texture2D>(_intermediateTextureName);
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            base.Position = _source.Position;

            // Update jump spots
            if (_jumpSpotLeft != null)
                _jumpSpotLeft.Update(gameTime);

            if (_jumpSpotRight != null)
                _jumpSpotRight.Update(gameTime);

            if (_jumpSpotAlert != null)
                _jumpSpotAlert.Update(gameTime);
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Vector2 offset)
        {
            if (this.DisplayingType == BlockType.Delete || this.DisplayingType == BlockType.None)
                return;

            Texture2D temp = _texture;
            var ramp = _source.BlockType == BlockType.LeftRamp || _source.BlockType == BlockType.RightRamp;

            if (this.IsTransitioning)
            {
                _texture = _intermediateTexture;
                this.SourceRectangle = _texture.Bounds;
                base.Draw(gameTime, offset);
            }
            else
            {
                if (this.IsShadowVisible && !this.IsTransitioning)
                {
                    _texture = _shadowTexture;
                    this.SourceRectangle = _texture.Bounds;
                    base.Draw(gameTime, offset - Vector2.UnitY * 50);
                }
                
                _texture = temp;
                this.SourceRectangle = _texture.Bounds;
                base.Draw(gameTime, offset - (ramp ? Vector2.UnitY * 1 : Vector2.Zero));

                if (_jumpSpotLeft != null)
                    _jumpSpotLeft.Draw(gameTime, offset - Vector2.UnitX * 5 - this.Position - Vector2.UnitY * 7);
                if (_jumpSpotRight != null)
                    _jumpSpotRight.Draw(gameTime, offset - Vector2.UnitX * 50 - this.Position - Vector2.UnitY * 7);
                if (_jumpSpotAlert != null)
                    _jumpSpotAlert.Draw(gameTime, offset - this.Position + Vector2.UnitX * 25 + Vector2.UnitY * 60);
            }
            
            _texture = temp;
        }
    }
}