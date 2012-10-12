using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace PlayWithYourPeas.Engine.Drawing
{
    public class Scene : DrawableGameComponent, ITransitionable
    {
        /// <summary>
        /// Camera 
        /// </summary>
        public Camera2D Camera { get; protected set; }

        protected SpriteBatch _spriteBatch;
        protected Effect _blurEffect;
        protected List<ISceneLayer> _layers;

        /// <summary>
        /// 
        /// </summary>
        public List<ISceneLayer> Layers { get { return _layers; } }

        /// <summary>
        /// Creates a scene
        /// </summary>
        /// <param name="game">Game to Bind</param>
        public Scene(Game game, Camera2D camera)
            : base(game)
        {
            _layers = new List<ISceneLayer>();
            this.Camera = camera;
        }

        /// <summary>
        /// Add a layer to the scene
        /// </summary>
        /// <param name="layer">layer to add</param>
        public void Add(ISceneLayer layer)
        {
            _layers.Add(layer);
        }

        /// <summary>
        /// Removes a layer from the scene
        /// </summary>
        /// <param name="layer">layer to remove</param>
        /// <returns></returns>
        public bool Remove(ISceneLayer layer)
        {
            return _layers.Remove(layer);
        }

        /// <summary>
        /// Initializes scene
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            this.Enabled = true;
            this.Visible = true;
            foreach (ISceneLayer layer in _layers)
            {
                layer.Initialize();
                layer.Opacity = this.TransitionPosition;
            }
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            if (this.Enabled == false)
                return;

            if (this.TransitionDirection != 0)
            {
                UpdateTransition(gameTime, this.TransitionDirection > 0 ? this.TransitionOnTime : this.TransitionOffTime, this.TransitionDirection);
                foreach (ISceneLayer layer in _layers)
                    layer.Opacity = this.TransitionPosition;
            }

            foreach (ISceneLayer layer in _layers)
                layer.Update(gameTime);
        }

        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>
        private bool UpdateTransition(GameTime gameTime, TimeSpan time, Int32 direction)
        {
            // How much should we move by?
            Single transitionDelta;

            // Update delay

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (Single)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                           time.TotalMilliseconds);

            // Update the transition position.
            this.TransitionPosition += transitionDelta * direction;


            // Did we reach the end of the transition?
            if ((this.TransitionPosition <= 0) || (this.TransitionPosition >= 1))
            {
                this.TransitionPosition = MathHelper.Clamp(this.TransitionPosition, 0, 1);
                this.TransitionDirection = 0;
                return false;
            }

            // Otherwise we are still busy transitioning.
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        public virtual void LoadContent(ContentManager manager)
        {
            try
            {
                _blurEffect = manager.Load<Effect>("Shaders\\SpriteBlur")
#if !SILVERLIGHT
                    .Clone()
#endif
;
            }
            catch (Exception) { }

            foreach (ISceneLayer l in _layers)
                l.LoadContent(manager);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (_blurEffect != null)
                _blurEffect.Dispose();

            foreach (ISceneLayer l in _layers)
                l.UnloadContent();

        }

        /// <summary>
        /// Draw 
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            if (this.Visible == false)
                return;

            var textures = new Dictionary<ISceneLayer, Texture2D>();

            foreach (ISceneLayer layer in _layers)
                layer.Draw(gameTime);

            foreach(ISceneLayer layer in _layers)
                textures.Add(layer, layer.Layer);

            foreach(ISceneLayer layer in _layers)
                if(layer.IsDepthBlurred)
                {
#if !SILVERLIGHT
                    int w = 1280;
                    int h = 720;
                    Vector2 sampleSpacing = new Vector2(2 * (layer.Distance - 0.5f) / w, 2 * (layer.Distance - 0.5f) / h);
                    RenderTarget2D target = new RenderTarget2D(this.GraphicsDevice, w, h);
                    _blurEffect.Parameters["sampleSpacing"].SetValue(sampleSpacing);

                    this.GraphicsDevice.SetRenderTarget(target);
                    _blurEffect.CurrentTechnique = _blurEffect.Techniques[0];
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, _blurEffect);
                    _blurEffect.CurrentTechnique.Passes[0].Apply();
                    _spriteBatch.Draw(textures[layer], new Rectangle(0,0,w,h), Color.White);
                    _spriteBatch.End();
                    var temp = (RenderTarget2D)textures[layer];
                    textures[layer] = target;

                    target = temp;//new RenderTarget2D(this.GraphicsDevice, w, h);
                    this.GraphicsDevice.SetRenderTarget(target);
                    _blurEffect.CurrentTechnique = _blurEffect.Techniques[1];
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, _blurEffect);
                    _blurEffect.CurrentTechnique.Passes[0].Apply();
                    _spriteBatch.Draw(textures[layer], new Rectangle(0, 0, w, h), Color.White);
                    _spriteBatch.End();
                    textures[layer] = target;

                    this.GraphicsDevice.SetRenderTarget(null);
#endif
                }

            this.GraphicsDevice.Clear(Color.Black);

            // Could be very bad for performance, switch to FrontToBack in case of performance issues. (Back to front is on to enable alpha blending)
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, RasterizerState.CullNone, null, this.Camera.View); 

            foreach (ISceneLayer layer in _layers)
            {
                _spriteBatch.Draw(textures[layer], Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, layer.Distance);
            }

            _spriteBatch.End();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OffTransition()
        {
            TransitionDirection = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnTransition()
        {
            TransitionDirection = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToggleTransition()
        {
            TransitionDirection *= -1;

            if (TransitionDirection == 0)
                TransitionDirection = (SByte)(TransitionPosition * -2 + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public float TransitionPosition
        {
            get;
            set;
        }

        public TimeSpan TransitionOnTime
        {
            get;
            set;
        }

        public TimeSpan TransitionOffTime
        {
            get;
            set;
        }

        public SByte TransitionDirection
        {
            get;
            protected set;
        }
    }
}
