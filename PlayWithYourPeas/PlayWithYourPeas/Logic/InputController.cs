using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Engine.Services;
using PlayWithYourPeas.Drawing;

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

        /// <summary>
        /// Current input state
        /// </summary>
        public Data.InputControllerState State { get { return _state; } }

        /// <summary>
        /// Creates a new controller
        /// </summary>
        /// <param name="game">Game to bind to</param>
        /// <param name="grid">Grid to bind to</param>
        public InputController(Game game, DataGrid grid)
            : base(game)
        {
            _gridData = grid;
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
        /// Initializes input controller
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _inputManager = this.Game.Services.GetService(typeof(InputManager)) as InputManager;
            if (_inputManager == null)
                throw new InvalidOperationException("Inputmanager nog found");

            _state = new InputControllerState(_inputManager);
            _timeController = this.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// Update the controller
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _state.Update(gameTime, _timeController != null ? _timeController.Speed : 1);
        }

        /// <summary>
        /// Handle the input
        /// </summary>
        public void HandleInput()
        {
            //_timeController.TargetSpeed = _inputManager.Mouse.IsButtonDown(MouseButton.Right) ? 2 : 1;

            if (_inputManager.Mouse.IsButtonReleased(MouseButton.Left) && 
                
                _toolBox != null && _inputManager.Mouse.IsOverObj(_toolBox.Position, _toolBox.Size))
            {
                _state.Type = _toolBox.Select(_inputManager.Mouse.Position - _toolBox.Position);
                return;

            } else if (_inputManager.Mouse.IsButtonDown(MouseButton.Left) &&
                _grid != null && _gridData != null && _inputManager.Mouse.IsOverObj(_grid.Position, _grid.Size))
            {
                _gridData.Add(_grid.GridPosition(_inputManager.Mouse.Position - _grid.Position), _state.Type);
                return;
            }
            
        }
    }
}
