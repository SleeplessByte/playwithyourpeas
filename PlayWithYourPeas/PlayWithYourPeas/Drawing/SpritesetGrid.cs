using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Drawing;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;

namespace PlayWithYourPeas.Drawing
{
    /// <summary>
    /// Contains all the SpriteBlocks in the grid so they can be drawn and maintained
    /// in one single object. Read the class comments for SpriteBlock if you want to
    /// know why some methods are obsolete.
    /// </summary>
    internal class SpritesetGrid : Sprite
    {
        protected SortedList<Int32, SpriteBlock> _grid;

        /// <summary>
        /// Creates a new spriteset
        /// </summary>
        /// <param name="layer"></param>
        public SpritesetGrid(SceneLayer layer) : base(layer)
        {
            _grid = new SortedList<Int32, SpriteBlock>();
        }

        /// <summary>
        /// Initilizesall blocks
        /// </summary>
        /// <param name="manager"></param>
        public override void Initialize()
        {
            base.Initialize();

            foreach (SpriteBlock block in _grid.Values)
            {
                block.Initialize();
            }
        }

        /// <summary>
        /// Adds a block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="position"></param>
        public void Add(SpriteBlock block)
        {
            _grid.Add(KeyFromPosition(block.GridPosition), block);
        }

        /// <summary>
        /// Removes a block
        /// </summary>
        /// <param name="key"></param>
        [Obsolete]
        public void Remove(Int32 key)
        {
            SpriteBlock block;
            if (_grid.TryGetValue(key, out block))
            {
                _grid.Remove(key);
            }
        }

        /// <summary>
        /// Gets a index key (depth key) for a position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected Int32 KeyFromPosition(Point position)
        {
            if (position.Y < 0)
                return -1;

            return (DataGrid.Height - Math.Min(DataGrid.Height, position.Y)) * DataGrid.Width + Math.Min(DataGrid.Width, position.X);
        }

        /// <summary>
        /// Gets the gridposition of a display position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Point GridPosition(Vector2 position)
        {
            return new Point((Int32)Math.Min(DataGrid.Width - 1, position.X / 70f), (Int32)Math.Min(DataGrid.Height - 1, position.Y / 48f));
        }

        /// <summary>
        /// Loads all content of all blocks
        /// </summary>
        /// <param name="manager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager manager)
        {
            base.LoadContent(manager);

            foreach (SpriteBlock block in _grid.Values)
            {
                block.LoadContent(this.ContentManager);
            }

            this.Size = new Vector2(DataGrid.WidthInPixels, DataGrid.HeightInPixels);
        }

        /// <summary>
        /// Draw the blocks
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="offset"></param>
        public override void Draw(GameTime gameTime, Vector2 offset)
        {
            foreach (SpriteBlock block in _grid.Values)
            {
                block.Draw(gameTime, offset - this.Position);
            }
        }

        /// <summary>
        /// Update the block sprites
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            List<Int32> blocksToRemove = new List<Int32>();

            foreach (KeyValuePair<Int32, SpriteBlock> block in _grid)
            {
                block.Value.Update(gameTime);
                
                //if (block.Value.BlockState == BlockState.Removed)
                //    blocksToRemove.Add(block.Key);
            }

            //foreach (Int32 key in blocksToRemove)
                //Remove(key);
        }
    }
}
