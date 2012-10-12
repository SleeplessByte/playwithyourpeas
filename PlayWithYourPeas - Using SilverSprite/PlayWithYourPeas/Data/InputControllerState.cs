using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Keeps track of the input state for the cursor and toolbox
    /// </summary>
    internal class InputControllerState
    {
        protected InputManager _inputManager;

        /// <summary>
        /// Position of the cursor
        /// </summary>
        public Vector2 Position { get; protected set; }

        /// <summary>
        /// Offset for the cursor
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 MaxPos { get; set; }

        /// <summary>
        /// Blocktype holding
        /// </summary>
        public BlockType Type { get; set; } 
        
        /// <summary>
        /// GridPosition
        /// </summary>
        public Point GridPosition { get { return new Point((Int32)MathHelper.Clamp((Int32)Math.Floor((this.Position.X - Offset.X)  / 70f), 0, DataGrid.Width - 1), 
            (Int32)MathHelper.Clamp((Int32)Math.Floor((this.Position.Y - Offset.Y) / 48f), 0, DataGrid.Height - 1)); } }

        /// <summary>
        /// Creates a new state
        /// </summary>
        /// <param name="manager"></param>
        public InputControllerState(InputManager manager, Int32 width, Int32 height)
        {
            _inputManager = manager;
            if (_inputManager == null)
                throw new InvalidOperationException("Inputmanager not found");

            this.Type = BlockType.Normal;
            this.MaxPos = new Vector2(width, height);
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime, Single runningSpeed, Single zoom)
        {
            // Sets the cursor position
            this.Position = Vector2.Clamp(_inputManager.Mouse.Position * (1f / zoom), Vector2.Zero,
                MaxPos);
                //Offset + Vector2.UnitX * DataGrid.WidthInPixels + Vector2.UnitY * DataGrid.HeightInPixels);
        }
    }
}
