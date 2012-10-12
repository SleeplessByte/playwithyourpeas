using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Engine.Services;
using PlayWithYourPeas.Drawing;
using PlayWithYourPeas.Engine.Drawing;

namespace PlayWithYourPeas.Logic
{
    /// <summary>
    /// Handles the input on the grid and the toolbox
    /// </summary>
    internal class InputController : GameComponent
    {
        protected InputControllerState _state;
        protected InputManager _inputManager;
        protected TimeController _timeController;

        protected DataGrid _gridData;
        protected SpritesetGrid _grid;
        protected SpriteToolbox _toolBox;
        protected Camera2D _camera;

        /// <summary>
        /// Current input state
        /// </summary>
        public Data.InputControllerState State { get { return _state; } }

        /// <summary>
        /// Creates a new controller
        /// </summary>
        /// <param name="game">Game to bind to</param>
        public InputController(Game game)
            : base(game)
        {
            _gridData = DataGrid.Singleton;
        }

        /// <summary>
        /// Registers the sprite toolbox
        /// </summary>
        /// <param name="toolBox"></param>
        public void Register(SpriteToolbox toolBox)
        {
            _toolBox = toolBox;
        }

        /// <summary>
        /// Registers the sprite grid
        /// </summary>
        /// <param name="grid"></param>
        public void Register(SpritesetGrid grid)
        {
            _grid = grid;
            _state.Offset = grid.Position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        public void Register(Camera2D camera, Int32 width, Int32 height)
        {
            _camera = camera;
            _state.MaxPos = new Vector2(width, height);
        }

        /// <summary>
        /// Initializes input controller
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _inputManager = this.Game.Services.GetService(typeof(InputManager)) as InputManager;
            if (_inputManager == null)
                throw new InvalidOperationException("Inputmanager nog found");

            _state = new InputControllerState(_inputManager, (Int32)1280, (Int32)720);
            _timeController = this.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// Update the controller
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _state.Update(gameTime, _timeController != null ? _timeController.Speed : 1, _camera.Zoom);
        }

        /// <summary>
        /// Handle the input
        /// </summary>
        public void HandleInput()
        {
            //_timeController.TargetSpeed = _inputManager.Mouse.IsButtonDown(MouseButton.Right) ? 2 : 1;

            if (_inputManager.Mouse.IsButtonReleased(MouseButton.Left) &&

                _toolBox != null && IsOverObj(_toolBox.Position, _toolBox.Size))
            {
                _state.Type = _toolBox.Select(_state.Position - _toolBox.Position);
                return;

            } else if (_inputManager.Mouse.IsButtonDown(MouseButton.Left) &&
                _grid != null && _gridData != null && IsOverObj(_grid.Position, _grid.Size))
            {
                _gridData.Add(_grid.GridPosition(_state.Position - _grid.Position), _state.Type);
                return;
            }
            
        }

             /// <summary>
        /// Check if mouse is over x
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="size">Size</param>
        /// <returns>True if over, false if not over</returns>
        public Boolean IsOverObj(Vector2 pos, Vector2 size)
        {
            if (_state.Position.X < pos.X)
                return false;
            if (_state.Position.Y < pos.Y)
                return false;
            if (_state.Position.X > pos.X + size.X)
                return false;
            if (_state.Position.Y > pos.Y + size.Y)
                return false;

            return true;
        }
        
    }
}
