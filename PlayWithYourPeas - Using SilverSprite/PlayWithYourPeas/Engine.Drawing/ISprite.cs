using System;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Content;
using System.IO;
namespace PlayWithYourPeas.Engine.Drawing
{
    public interface ISprite
    {
        /// <summary>
        /// Position of the sprite
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Sprite display size
        /// </summary>
        Vector2 Size { get; set; }

        /// <summary>
        /// Scale
        /// </summary>
        Single Scale { get; set; }

        /// <summary>
        /// Base Color
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Sprite Origin
        /// </summary>
        Vector2 Origin { get; set; }

        /// <summary>
        /// Texture source rectangle
        /// </summary>
        Rectangle SourceRectangle { get; set; }

        /// <summary>
        /// Rotation
        /// </summary>
        Single Rotation { get; set; }

        /// <summary>
        /// Draw processing flag
        /// </summary>
        Boolean Visible { get; set; }

        /// <summary>
        /// Update processing glag
        /// </summary>
        Boolean Enabled { get; set; }

        /// <summary>
        /// Initializes sprite
        /// </summary>
        void Initialize();
        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        /// <param name="offset">Offset</param>
        void Draw(GameTime gameTime, Vector2 offset);
        /// <summary>
        /// Loads content
        /// </summary>
        /// <param name="manager"></param>
        void LoadContent(ContentManager manager);
        /// <summary>
        /// Unloads content
        /// </summary>
        void UnloadContent();
        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(GameTime gameTime);
        
    }
}
