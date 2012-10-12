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
    public class Scene : DrawableGameComponent
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
            foreach (ISceneLayer layer in _layers)
                layer.Initialize();
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            foreach (ISceneLayer layer in _layers)
                layer.Update(gameTime);
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

#if !SILVERLIGHT
            if (_blurEffect != null)
                _blurEffect.Dispose();
#endif

            foreach (ISceneLayer l in _layers)
                l.UnloadContent();

        }

        /// <summary>
        /// Draw 
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
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

            // Could be very bad for performance, switch to FrontToBack in case of performance issues. (Back to front is on to enable alpha blending)
            _spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, RasterizerState.CullNone, null, Matrix.Identity); 

            foreach (ISceneLayer layer in _layers)
            {
                _spriteBatch.Draw(textures[layer], Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, layer.Distance);
            }

            _spriteBatch.End();
        }
    }
}
