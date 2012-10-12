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
    public class BackgroundLayer : ISceneLayer
    {
        /// <summary>
        /// Creates a new layer
        /// </summary>
        protected BackgroundLayer()
        {
            this.Managed = false;
        }

        /// <summary>
        /// Creates a new Background layer
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="texture">Layer texture</param>
        /// <param name="distance">Distance</param>
        public BackgroundLayer(Game game, Texture2D texture, Single distance)
        {
            this.Layer = texture;
            this.Distance = distance;
            this.Game = game;
            this.Managed = true;
        }

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
        public Texture2D Layer { get; set; }

        /// <summary>
        /// Defines whether this layer should be depth blurred based on the distance from the center
        /// </summary>
        public Boolean IsMotionBlurred { get; set; }

        /// <summary>
        /// Defines whether this layer should be motion blurred when moving
        /// </summary>
        public Boolean IsDepthBlurred { get; set; }

        /// <summary>
        /// Game reference
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Content Managed
        /// </summary>
        public Boolean Managed { get; set; }

        /// <summary>
        /// Loads all content
        /// </summary>
        /// <param name="manager"></param>
        public void LoadContent(ContentManager manager)
        {
            
        }

        /// <summary>
        /// Unloads all content
        /// </summary>
        public void UnloadContent()
        {
            if (this.Managed == false && this.Layer != null)
                this.Layer.Dispose();
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Update frame
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Initalize layer
        /// </summary>
        public virtual void Initialize() { }

        public float Opacity
        {
            get;
            set;
        }
    }
}
