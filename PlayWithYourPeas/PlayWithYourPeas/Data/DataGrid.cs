using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using PlayWithYourPeas.Logic;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// This is the datagrid which holds all the datablocks on the screen. Initially each block is
    /// created and during the game just the physics and the type/state of the datablock are altered.
    /// Sort to speak, the grid and game therefore are warm started. We don't like the GC that much.
    /// </summary>
    internal class DataGrid : GameComponent
    {
        /// <summary>
        /// Height of the grid
        /// </summary>
        public const Int32 Height = 10;

        /// <summary>
        /// Width of the grid
        /// </summary>
        public const Int32 Width = 17;

        /// <summary>
        /// Visual Width
        /// </summary>
        public const Int32 WidthInPixels = 70 * (Width);

        /// <summary>
        /// Visual Height
        /// </summary>
        public const Int32 HeightInPixels = 49 * (Height);

        protected TimeController _timeController;
        protected DataBlock[][] _grid;
        protected Body _fx;
        protected List<PeaController> _controllers;

        /// <summary>
        /// Fires when a block is placed or removing
        /// </summary>
        public event OnDataGridChangedHandler OnGridChanged = delegate { };

        /// <summary>
        /// 
        /// </summary>
        public Single RunningSpeed { get { return _timeController != null ? _timeController.Speed : 1f; } } 

        /// <summary>
        /// Constructs a new grid
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="fx">Static physics body</param>
        public DataGrid(Game game, Body fx) : base(game)
        {
            _fx = fx;

            // Create grid
            _grid = new DataBlock[Width][];
            for (int i = 0; i < Width; i++)
            {
                _grid[i] = new DataBlock[Height];
                for (int j = 0; j < Height; j++)
                {
                    _grid[i][j] = new DataBlock(new Point(i, j), BlockType.None, _fx);
                }
            }

            _controllers = new List<PeaController>();
        }

        /// <summary>
        /// Initializes grid
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    _grid[i][j].OnStateChanged += new BlockStateChangedHandler(DataGrid_OnStateChanged);
                }

            _timeController = this.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// Method when event fires (a block changed state)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_OnStateChanged(BlockStateArgs args)
        {
            // Bubble up
            this.OnGridChanged.Invoke(this, args);
        }

        /// <summary>
        /// Adds a block (changes to type)
        /// </summary>
        /// <param name="position">at position</param>
        /// <param name="type">to type</param>
        public DataBlock Add(Point position, BlockType type)
        {
            var block = _grid[position.X][position.Y];
            var blockdown = position.Y < DataGrid.Height - 1 ? _grid[position.X][position.Y + 1] : null;
            var blockdowndown = position.Y < DataGrid.Height - 2 ? _grid[position.X][position.Y + 2] : null;
            var blockup = position.Y > 0 ? _grid[position.X][position.Y - 1] : null;

            // Can't add on transit block
            if (block.IsTransitioning)
                return block;
            // Can't remove below jumpspot
            if (block.JumpLeft != null || block.JumpRight != null)
                return block;
            // Can't add on jumpspot
            if (blockdown != null && (blockdown.JumpLeft != null || blockdown.JumpRight != null))
                return block;
            // Can't add above jumpspot
            if (blockdowndown != null && (blockdowndown.JumpLeft != null || blockdowndown.JumpRight != null))
                return block;

            if (type == BlockType.Delete)
            {
                StartRemovingAt(position);

                if (blockup != null)
                    blockup.HasBlockBelow = false;

                return block;
            }
            block.HasBlockBelow = false;

            if (blockdown != null)
                block.HasBlockBelow = (blockdown.BlockType != BlockType.None);

            if (blockup != null)
                blockup.HasBlockBelow = (type != BlockType.None);

            block.Place(type); // TODO settings: overwrite/delete only

            return block;
        }

        /// <summary>
        /// Removes a block (changes to clear)
        /// </summary>
        /// <param name="block"></param>
        private void Remove(DataBlock block)
        {
            block.Place(BlockType.None);
        }

        /// <summary>
        /// Register the pea controller
        /// </summary>
        /// <param name="controller"></param>
        public void Register(PeaController controller)
        {
            _controllers.Add(controller);
        }

        /// <summary>
        /// Start removing (animation)
        /// </summary>
        /// <param name="position">block at position</param>
        public Boolean StartRemovingAt(Point position)
        {
            DataBlock removable = _grid[position.X][position.Y];

            if (removable != null)
            {
                // Can't remove below jumpspot
                if (removable.JumpLeft != null || removable.JumpRight != null)
                    return false;

                removable.Remove();

                var blockup = position.Y > 0 ? _grid[position.X][position.Y - 1] : null;
                if (blockup != null)
                    blockup.HasBlockBelow = false;

                return true;
            }

            return true;
        }

        /// <summary>
        /// Updates a block
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            List<DataBlock> blocksToRemove = new List<DataBlock>();

            foreach (DataBlock[] blocks in _grid)
            {
                foreach (DataBlock block in blocks) 
                {
                    block.Update(gameTime, RunningSpeed, IsTouching);

                    if (block.BlockState == BlockState.Removed)
                        blocksToRemove.Add(block);
                }
            }

            foreach (DataBlock block in blocksToRemove)
                Remove(block);
        }

        /// <summary>
        /// Returns wheather a pea is touching a fixture
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        protected Boolean IsTouching(Fixture fixture)
        {
            return _controllers.Any(controller => controller.IsTouching(fixture));
        }

        /// <summary>
        /// Reference to the grid
        /// </summary>
        public DataBlock[][] Grid { get { return _grid; } }
        
    }
}
