using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using Microsoft.Xna.Framework.Graphics;
using PlayWithYourPeas.Data;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Logic;
using PlayWithYourPeas.Engine.Services;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// Graphical representation of a pea
    /// </summary>
    internal class SpritePea : Sprite
    {
        protected Texture2D _ghostTexture;
        protected Texture2D _hurtTexture;
        protected Texture2D _happyTexture;
        protected Texture2D _shadowTexture;

        protected InputManager _inputManager;
        protected PointsController _pointsController;
        protected SpriteJumpInfo _activeState;
        protected SpriteAlert _alert;
        protected DataPea _source;

        /// <summary>
        /// Base Offset is to comply with grid offset
        /// </summary>
        protected readonly Vector2 BaseOffset = Vector2.UnitX * 40 + Vector2.UnitY * 150;

        /// <summary>
        /// Source is alive
        /// </summary>
        public Boolean IsAlive { get { return _source.IsAlive; } }

        /// <summary>
        /// Creates a new sprite pea
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="source"></param>
        /// <param name="pointsController"></param>
        public SpritePea(SceneLayer layer, DataPea source, PointsController pointsController)
            : base(layer)
        {
            _source = source;
            _pointsController = pointsController;
            _source.OnJumpStarted += new JumpEventHandler(_source_OnJumpStarted);
            _source.OnJumpCompleted += new JumpEventHandler(_source_OnJumpCompleted);
            _source.OnJumpFailed += new JumpEventHandler(_source_OnJumpCompleted);
            _source.OnRevive += new EventHandler(_source_OnRevive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _source_OnRevive(Object sender, EventArgs e)
        {
            this.Color = Color.Transparent;
        }

        /// <summary>
        /// Method that is called when pea jumps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _source_OnJumpStarted(JumpEventArgs e)
        {
            // Because the controler is created BEFORE the sprites this works
            if (e.Pea.IsAlive && !e.Pea.IsNotHappy && e.Pea.JumpSpot != null)
            {
                _activeState = new SpriteJumpInfo(this.SceneLayer, _pointsController.ActiveTimes(e.Pea));
                _activeState.Initialize();

                if (this.ContentManager != null)
                    _activeState.LoadContent(this.ContentManager);
            }
        }

        /// <summary>
        /// Method that is called when a pea finishes jumping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _source_OnJumpCompleted(JumpEventArgs e)
        {
            if (_activeState != null)
                _activeState.Exit();

            if (!e.Pea.IsAlive)
                return;

            if (e.Pea.IsTrapped)
                _alert = SpriteAlert.GenerateTrap(this.SceneLayer);
            else if (!e.Pea.IsNotHappy)
                _alert = SpriteAlert.GenerateNinja(this.SceneLayer);

            if (_alert != null)
            {
                _alert.Initialize();

                if (this.ContentManager != null)
                    _alert.LoadContent(this.ContentManager);
            }
        }


        /// <summary>
        /// Initialze sprite
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.TextureName = "Graphics/Pea-Standard";
            this.Color = Color.Transparent;

            _inputManager = this.SceneLayer.Game.Services.GetService(typeof(InputManager)) as InputManager;
        }

        /// <summary>
        /// Load all content
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager manager)
        {
            base.LoadContent(manager);

            _ghostTexture = this.ContentManager.Load<Texture2D>("Graphics/Pea-Ghost");
            _happyTexture = this.ContentManager.Load<Texture2D>("Graphics/Pea-Happy");
            _hurtTexture = this.ContentManager.Load<Texture2D>("Graphics/Pea-Hurt");
            _shadowTexture = this.ContentManager.Load<Texture2D>("Graphics/Pea-Shadow");
            
            if (_alert != null)
                _alert.LoadContent(this.ContentManager);

            if (_activeState != null)
                _activeState.LoadContent(this.ContentManager);

            this.Origin = new Vector2((Int32)Math.Round(_texture.Width / 2f), (Int32)Math.Round(_texture.Height / 2f));
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            base.Position = _source.Position;
            this.Rotation = _source.Rotation;

            if (_source.IsAlive == false)
            {
                if (_source.IsDying)
                    this.Color = Color.Lerp(this.Color, Color.Transparent, (Single)gameTime.ElapsedGameTime.TotalSeconds * 15);
                else
                    this.Color = Color.Lerp(this.Color, Color.White, (Single)gameTime.ElapsedGameTime.TotalSeconds * 5);

                this.Rotation = 0; // (Single)MathHelper.Lerp(this.Rotation, 0, (Single)gameTime.ElapsedGameTime.TotalSeconds * 10);
            }
            else
            {
                this.Color = Color.Lerp(this.Color, Color.White, (Single)gameTime.ElapsedGameTime.TotalSeconds * 5);
            }

            // TODO rotation smoothing
            //var degrees = Math.Round(this.Rotation * 180 / Math.PI, 1);
            //this.Rotation = (Single)(degrees * Math.PI / 180);

            if (_alert != null)
            {
                _alert.Update(gameTime);
                _alert.Position = this.Position;

                if (_alert.Enabled == false)
                    _alert = null;
            }

            if (_activeState != null)
            {
                _activeState.Update(gameTime);
                _activeState.Position = this.Position;
                _activeState.Visible = true; //Settings.Get.ShowJumpInfo;

                if (_activeState.Enabled == false)
                    _activeState = null;
            }
        }

        /// <summary>
        /// Draw sprite
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(GameTime gameTime, Vector2 offset)
        {
            Texture2D temp = _texture;

            // Go for moody texture
            if (!_source.IsAlive && !_source.IsDying)
                _texture = _ghostTexture;
            else if (_source.IsHappy)
                _texture = _happyTexture;
            else if (_source.IsHurt || _source.IsDying)
                _texture = _hurtTexture;
            else if (_source.IsNinja)
                _texture = temp;
 
            this.SourceRectangle = _texture.Bounds;
            base.Draw(gameTime, offset - BaseOffset + (!_source.IsAlive && !_source.IsDying ? Vector2.UnitX * +30 : Vector2.Zero));

            _texture = temp;

            if (_alert != null)
                _alert.Draw(gameTime, offset - Vector2.UnitY * 80);

            if (_activeState != null )
                _activeState.Draw(gameTime, offset - Vector2.UnitY * 90 - Vector2.UnitX * 15);
        }

    }
}
