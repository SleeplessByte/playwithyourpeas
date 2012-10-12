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
    public class Sprite : ISprite
    {
        /// <summary>
        /// Contentmanager
        /// </summary>
        protected ContentManager ContentManager { get; set; }

        /// <summary>
        /// Sprite Position
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Sprite Size
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Scale
        /// </summary>
        public Single Scale { get; set; }

        /// <summary>
        /// Base Color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Single Opacity { get; set; }

        /// <summary>
        /// Sprite Origin
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// Texture source rectangle
        /// </summary>
        public Rectangle SourceRectangle { get; set; }

        /// <summary>
        /// Rotation
        /// </summary>
        public Single Rotation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SpriteEffects Effects { get; set; }

        /// <summary>
        /// Scene Layer on which sprite is drawn
        /// </summary>
        public SceneLayer SceneLayer { get; set; }

        /// <summary>
        /// Draw processing flag
        /// </summary>
        public Boolean Visible { get; set; }

        /// <summary>
        /// Update processing glag
        /// </summary>
        public Boolean Enabled { get; set; }

        /// <summary>
        /// Texture is Managed
        /// </summary>
        public Boolean Managed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Texture2D _texture;

        /// <summary>
        /// The external texture name
        /// </summary>
        public String TextureName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Sprite() { }

        /// <summary>
        /// Creates a new sprite
        /// </summary>
        /// <param name="layer"></param>
        public Sprite(SceneLayer layer)
        {
            this.SceneLayer = layer;
            this.Rotation = 0;
            this.Scale = 1;
            this.Opacity = 1;
            this.Position = Vector2.Zero;
            this.Color = Color.White;
            this.Origin = Vector2.Zero;
            this.Managed = false;
            this.Effects = SpriteEffects.None;
        }

        /// <summary>
        /// Creates a new Sprite
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="texture"></param>
        public Sprite(SceneLayer layer, Texture2D texture)
            : this(layer)
        {
            if (texture != null)
            {
                _texture = texture;
                this.Size = new Vector2(_texture.Bounds.Width, _texture.Bounds.Height);
                this.SourceRectangle = new Rectangle(0, 0, _texture.Bounds.Width, _texture.Bounds.Height);
                this.Managed = true;
            }
        }

        /// <summary>
        /// Creates a new sprite with scene layer and asset name
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="assetName"></param>
        public Sprite(SceneLayer layer, String assetName)
            : this(layer)
        {
            this.TextureName = assetName;
            this.Managed = false;
        }

        /// <summary>
        /// Initializes Sprite
        /// </summary>
        public virtual void Initialize()
        {
            this.Enabled = true;
            this.Visible = true;
        }

        /// <summary>
        /// Updates sprite
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public virtual void Update(GameTime gameTime) 
        {
            this.Opacity = this.SceneLayer.Opacity;
        }

        /// <summary>
        /// Draws Sprite
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        /// <param name="offset">Camera draw offset</param>
        public virtual void Draw(GameTime gameTime, Vector2 offset)
        {
            if (this.Visible == false)
                return;
            //this.SceneLayer.SpriteBatch.Begin();
            this.SceneLayer.SpriteBatch.Draw(
                _texture, Position - offset,
                this.SourceRectangle,
                this.Color * this.Opacity,
                this.Rotation,
                this.Origin,
                this.Scale,
                this.Effects,
                0);
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public virtual void LoadContent(ContentManager manager)
        {
            this.ContentManager = manager;

            if (_texture == null || _texture.IsDisposed)
            {
                if (!String.IsNullOrWhiteSpace(TextureName))
                {
                    _texture = manager.Load<Texture2D>(TextureName);
                    this.Managed = true;
                }

                if (_texture != null)
                {
                    this.Size = new Vector2(_texture.Bounds.Width, _texture.Bounds.Height);
                    this.SourceRectangle = new Rectangle(0, 0, _texture.Bounds.Width, _texture.Bounds.Height);
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources
        /// </summary>
        public virtual void UnloadContent()
        {
            if (!this.Managed && _texture != null && !_texture.IsDisposed)
                _texture.Dispose();
        }

        /// <summary>
        /// Calculates if a given ISprite is visible
        /// </summary>
        /// <param name="sprite">The sprite</param>
        /// <param name="offset">The camera offset</param>
        /// <returns>Wether the sprite is visible</returns>
        public static Boolean IsVisible(ISprite sprite, Vector2 offset)
        {
            if (!sprite.Visible)
                return false;

            Vector2 screenPosition = sprite.Position - offset;

            if (screenPosition.X < -sprite.Size.X - 100)
                return false;
            if (screenPosition.X > 1280 + sprite.Size.X + 100)
                return false;

            if (screenPosition.Y < -sprite.Size.Y - 200) // TODO doesn't work for manually offsetted sprites
                return false;
            if (screenPosition.Y > 720 + sprite.Size.Y + 100)
                return false;

            return true;
        }

    }
}
