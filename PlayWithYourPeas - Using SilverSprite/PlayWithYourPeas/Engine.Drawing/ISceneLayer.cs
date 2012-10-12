using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace PlayWithYourPeas.Engine.Drawing
{
    public interface ISceneLayer
    {
        /// <summary>
        /// The texture to draw to the scene
        /// </summary>
        Texture2D Layer { get; }

        /// <summary>
        /// The distance from the center
        /// Range -1 to 1, 1 is background, -1 is foreground
        /// </summary>
        Single Distance { get; set; }

        /// <summary>
        /// The speed which this layer moves in respect to the center (Distance = 0)
        /// </summary>
        Single MoveSpeed { get; set; }

        /// <summary>
        /// Defines whether this layer should be motion blurred when moving
        /// </summary>
        Boolean IsMotionBlurred { get; set; }

        /// <summary>
        /// Defines whether this layer should be depth blurred based on the distance from the center
        /// </summary>
        Boolean IsDepthBlurred { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Single Opacity { get; set; }

        /// <summary>
        /// Game reference
        /// </summary>
        Game Game { get; set; }

        /// <summary>
        /// Initializes layer
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates layer
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Draws layer
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        void Draw(GameTime gameTime);

        /// <summary>
        /// Loads all content on layer
        /// </summary>
        /// <param name="manager">Managed loader</param>
        void LoadContent(ContentManager manager);

        /// <summary>
        /// Unloads all unmanaged content
        /// </summary>
        void UnloadContent();
    }
}
