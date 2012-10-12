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
    [Serializable]
    public class SceneLayer : ISceneLayer
    {
        /// <summary>
        /// The distance from the center
        /// <remarks>Range -1 to 1, -1 is background, 1 is foreground</remarks>
        /// </summary>
        public Single Distance { get; set; }

        /// <summary>
        /// The speed which this layer moves in respect to the center (Distance = 0)
        /// </summary>
        public Single MoveSpeed { get; set; }

        /// <summary>
        /// The texture to draw to the scene
        /// </summary>
        public Texture2D Layer { get { return _target; } }

        /// <summary>
        /// Defines whether this layer should be depth blurred based on the distance from the center
        /// </summary>
        public Boolean IsMotionBlurred { get; set; }

        /// <summary>
        /// Defines whether this layer should be motion blurred when moving
        /// </summary>
        public Boolean IsDepthBlurred { get; set; }

        /// <summary>
        /// Spritebatch reference to draw too
        /// </summary>
        public SpriteBatch SpriteBatch { get; protected set; }

        /// <summary>
        /// Camera used for this scene layer
        /// </summary>
        public Camera2D Camera { get; protected set; }

        /// <summary>
        /// Game Reference
        /// </summary>
        public Game Game { get; set; }

        
        protected RenderTarget2D _target;
        protected List<ISprite> _components;

        /// <summary>
        /// Layer bounds
        /// </summary>
        public Rectangle Bounds { get { return (_target ?? new RenderTarget2D(this.Game.GraphicsDevice, 0, 0)).Bounds; } }

        /// <summary>
        /// Constructor 
        /// </summary>
        public SceneLayer()
        {
           
            _components = new List<ISprite>();
        }

        /// <summary>
        /// Creates a new Scene Layer
        /// </summary>
        /// <param name="game">Game to bind</param>
        public SceneLayer(Game game)
        {
            this.Game = game;
            _components = new List<ISprite>();
        }

        /// <summary>
        /// Creates a new Scene Layer
        /// </summary>
        /// <param name="game"></param>
        /// <param name="camera"></param>
        public SceneLayer(Game game, Camera2D camera)
        {
            this.Game = game;
            this.Camera = camera;
            _components = new List<ISprite>();
        }

        /// <summary>
        /// Adds a Sprite to this Scene Layer
        /// </summary>
        /// <param name="sprite">Sprite to add</param>
        public void Add(Sprite sprite)
        {
            _components.Add(sprite);
        }

        /// <summary>
        /// Removes a Sprite from this Scene Layer
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public bool Remove(Sprite sprite)
        {
            return _components.Remove(sprite);
        }

        /// <summary>
        /// Initializes the component
        /// </summary>
        public void Initialize()
        {
            this.SpriteBatch = new SpriteBatch(this.Game.GraphicsDevice);

            _target = new RenderTarget2D(this.Game.GraphicsDevice, 1280, 720); // TODO arguments

            for (Int32 i = 0; i < _components.Count; i++ )
                    _components[i].Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        public virtual void LoadContent(ContentManager manager)
        {
            for (Int32 i = 0; i < _components.Count; i++)
                _components[i].LoadContent(manager);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void UnloadContent()
        {
            for (Int32 i = 0; i < _components.Count; i++)
                _components[i].UnloadContent();
        }

        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            for (Int32 i = 0; i < _components.Count; i++)
                if (_components[i].Enabled)
                    _components[i].Update(gameTime);
        }

        /// <summary>
        /// Draws the component
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public void Draw(GameTime gameTime)
        {
            Vector2 offset = this.Camera != null ? this.Camera.Position * this.MoveSpeed : Vector2.Zero;
            //Vector2 offset = Vector2.UnitX * offset3d.X + Vector2.UnitY * offset3d.Y;

            this.Game.GraphicsDevice.SetRenderTarget(_target);
            this.Game.GraphicsDevice.Clear(Color.Transparent);
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);

            for (Int32 i = 0; i < _components.Count; i++)
                if (_components[i].Visible && Sprite.IsVisible(_components[i], offset))
                    _components[i].Draw(gameTime, offset);

            this.SpriteBatch.End();
            this.Game.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
