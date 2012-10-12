using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Engine.Services;
using PlayWithYourPeas.Engine.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using FarseerPhysics.Dynamics;
#if !SILVERLIGHT
    using FarseerPhysics.DebugViews;
#endif
using FarseerPhysics.Factories;
using PlayWithYourPeas.Logic;
using PlayWithYourPeas.Drawing;
using Microsoft.Xna.Framework.Input;

namespace PlayWithYourPeas.Screens
{

    /// <summary>
    /// This contains everything processed and drawn during the game.
    /// For menu's and such there are different screens.
    /// </summary>
    internal class PeaScreen : GameScreen
    {
        protected Scene _scene;
        protected Camera2D _camera;
        
        protected SceneLayer _sceneryLayer;
        protected SceneLayer _levelLayer;
        protected SceneLayer _peasLayer;
        protected SceneLayer _hudLayer;

        protected DataGrid _dataGrid;
        protected SpritesetGrid _spriteGrid;
        protected PeaController _peaController;
        protected InputController _inputController;
        protected PointsController _pointsController;
        protected TimeController _timeController;
        protected AchievementController _achievementController;

        protected World _fx;
        protected Body _fxStaticBody;
        protected PauseOverlay _overlay;

        // Debugging
        #if DEBUG && !SILVERLIGHT
            public DebugViewXNA view;
        #endif

        /// <summary>
        /// Initializes screen
        /// </summary>
        public override void Initialize()
        {
            // Create the scene and add it
            _camera = new Camera2D(this.Game);
            _camera.Initialize();

            _scene = new Scene(this.Game, _camera);
            _fx = new World(Vector2.UnitY * 10);
            _fxStaticBody = new Body(_fx) { Restitution = 0, Mass = 0, BodyType = BodyType.Static, Friction = 0.5f };
           
            // Create controllers
            _timeController = new TimeController(this.Game);
            _pointsController = new PointsController(this.Game);
            _dataGrid = new DataGrid(this.Game, _fxStaticBody);
            _peaController = new PeaController(this.Game, _dataGrid);
            _inputController = new InputController(this.Game, _dataGrid);
            _achievementController = new AchievementController(this.Game);

            // Initialize controllers
            _timeController.Initialize();
            _pointsController.Initialize();
            _dataGrid.Initialize();
            _peaController.Initialize();
            _inputController.Initialize();
            _achievementController.Initialize();

            // Register controllers
            _dataGrid.Register(_peaController);

#if DEBUG && TEST
            
            DataBlock testBlock = _dataGrid.Add(new Point(0, 0), BlockType.Normal);
            DataBlock testBlock2 = _dataGrid.Add(new Point(0, 1), BlockType.Gel);
            DataBlock testBlock3 = _dataGrid.Add(new Point(1, 1), BlockType.Normal);
            DataBlock testBlock4 = _dataGrid.Add(new Point(2, 1), BlockType.RightRamp);
            DataBlock testBlock5 = _dataGrid.Add(new Point(3, 2), BlockType.Normal);
            DataBlock testBlock6 = _dataGrid.Add(new Point(DataGrid.Width - 1, DataGrid.Height - 1), BlockType.Normal);
#endif
            // Create the peas
            DataPea pea1 = new DataPea(this.Game, _fx, _peaController);
            _pointsController.Register(pea1);
            _achievementController.Register(pea1);

            pea1.Initialize();

            this.Game.Components.Add(pea1);

            DataPea pea2 = new DataPea(this.Game, _fx, _peaController);
            DataPea pea3 = new DataPea(this.Game, _fx, _peaController);
            DataPea pea4 = new DataPea(this.Game, _fx, _peaController);
            DataPea pea5 = new DataPea(this.Game, _fx, _peaController);

            // This needs to be done BEFORE sprites of pea
            _pointsController.Register(pea2);
            _pointsController.Register(pea3);
            _pointsController.Register(pea4);
            _pointsController.Register(pea5);

            _achievementController.Register(pea2);
            _achievementController.Register(pea3);
            _achievementController.Register(pea4);
            _achievementController.Register(pea5);

            pea2.Initialize();
            pea3.Initialize();
            pea4.Initialize();
            pea5.Initialize();

            this.Game.Components.Add(pea2);
            this.Game.Components.Add(pea3);
            this.Game.Components.Add(pea4);
            this.Game.Components.Add(pea5);

            // Create layers
            _sceneryLayer = new SceneLayer(this.Game, _scene.Camera) { MoveSpeed = 0f, Distance = 0.9f };
            _levelLayer = new SceneLayer(this.Game, _scene.Camera) { MoveSpeed = 0f, Distance = 0.6f };
            _peasLayer = new SceneLayer(this.Game, _scene.Camera) { MoveSpeed = 0f, Distance = 0.3f };
            _hudLayer = new SceneLayer(this.Game, _scene.Camera) { MoveSpeed = 0f, Distance = 0.1f };

            // Create sprites
            _spriteGrid = new SpritesetGrid(_levelLayer) { Position = Vector2.UnitX * 40 + Vector2.UnitY * 150 };

            _peasLayer.Add(new SpritePea(_peasLayer, pea1, _pointsController));

#if !DEBUG || !TEST
            _peasLayer.Add(new SpritePea(_peasLayer, pea2, _pointsController));
            _peasLayer.Add(new SpritePea(_peasLayer, pea3, _pointsController));
            _peasLayer.Add(new SpritePea(_peasLayer, pea4, _pointsController));
            _peasLayer.Add(new SpritePea(_peasLayer, pea5, _pointsController));
#endif

#if DEBUG && TEST
            for (int i = 0; i < DataGrid.Width; i++)
            {
                if (new int[] { 3, 4, 5, 10, 12, 14 }.Contains(i) == false)
                {
                    DataBlock b = _dataGrid.Add(new Point(i, DataGrid.Height - 1), BlockType.Normal);
                    //_spriteGrid.Add(new SpriteBlock(_levelLayer, b));

                    if (new int[] { 0 , 1, 2, 5, 8, 15 }.Contains(i))
                    {
                        b = _dataGrid.Add(new Point(i, DataGrid.Height - 2), BlockType.Normal);
                        //_spriteGrid.Add(new SpriteBlock(_levelLayer, b));
                    }

                    if (new int[] { 0, 15 }.Contains(i))
                    {
                        b = _dataGrid.Add(new Point(i, DataGrid.Height - 3), BlockType.Gel);
                        //_spriteGrid.Add(new SpriteBlock(_levelLayer, b));
                    }

                    if (new int[] { 0, 15 }.Contains(i))
                    {
                        b = _dataGrid.Add(new Point(i, DataGrid.Height - 4), BlockType.Gel);
                        //_spriteGrid.Add(new SpriteBlock(_levelLayer, b));
                    }
                }
            }


            DataBlock jump = _dataGrid.Add(new Point(3, 7), BlockType.Normal);
            //_spriteGrid.Add(new SpriteBlock(_levelLayer, jump));

            DataBlock ramp = _dataGrid.Add(new Point(1, 8), BlockType.RightRamp);
            //_spriteGrid.Add(new SpriteBlock(_levelLayer, ramp));

            //DataBlock gel = _dataGrid.Add(new Point(5, 10), BlockType.LeftRamp);
            //_spriteGrid.Add(new SpriteBlock(_levelLayer, gel));
#else
            // Some boundary blocks
            _dataGrid.Add(new Point(0, DataGrid.Height - 1), BlockType.Gel);
            _dataGrid.Add(new Point(DataGrid.Width - 1, DataGrid.Height - 1), BlockType.Gel);
            _dataGrid.Add(new Point(0, DataGrid.Height - 2), BlockType.Gel);
            _dataGrid.Add(new Point(DataGrid.Width - 1, DataGrid.Height - 2), BlockType.Gel);
#endif

            foreach (var blockRow in _dataGrid.Grid)
                foreach (var block in blockRow)
                {
                    block.Place(block.BlockType, BlockState.Placed); // Direct placing;
                    _spriteGrid.Add(new SpriteBlock(_levelLayer, block));
                }

            SpriteToolbox toolbox = new SpriteToolbox(_hudLayer) { Position = new Vector2(1220, 150) };
            SpritesetHud hud = new SpritesetHud(_hudLayer, _pointsController, _inputController.State);

            // Register sprites at inputControllers
            _inputController.Register(_spriteGrid);
            _inputController.Register(toolbox);

            // Add the layers
            _scene.Add(_sceneryLayer);
            _scene.Add(_levelLayer);
            _scene.Add(_peasLayer);
            _scene.Add(_hudLayer);

            // Add the content
            _sceneryLayer.Add(new Sprite(_sceneryLayer, "Graphics/Background"));
            _sceneryLayer.Add(new Sprite(_sceneryLayer, "Graphics/Plate"));
            _levelLayer.Add(_spriteGrid);
            _hudLayer.Add(toolbox);
            _hudLayer.Add(hud);

            // Bottom plate physics
            var plate = FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(1280), ConvertUnits.ToSimUnits(20), 1f, 
                ConvertUnits.ToSimUnits(Vector2.UnitY * (49 * (DataGrid.Height - 3) + _spriteGrid.Position.Y + 5) + (1280 / 2 - _spriteGrid.Position.X) * Vector2.UnitX), _fxStaticBody);
            plate.Restitution = 0;
            plate.Friction = 0.5f;

            // Initializes scene and so on
            base.Initialize();
            _scene.Initialize();
            _camera.Position = new Vector2(1280 / 2, 720 / 2) - _spriteGrid.Position;
            _camera.Jump2Target();
            
            #if DEBUG && !SILVERLIGHT
                view = new DebugViewXNA(_fx);
                view.AppendFlags(FarseerPhysics.DebugViewFlags.CenterOfMass | FarseerPhysics.DebugViewFlags.DebugPanel | FarseerPhysics.DebugViewFlags.PerformanceGraph | FarseerPhysics.DebugViewFlags.PolygonPoints);
            #endif
            // Add components
            this.Game.Components.Add(_peaController);
            this.Game.Components.Add(_timeController);
            this.Game.Components.Add(_inputController);
            this.Game.Components.Add(_pointsController);
            this.Game.Components.Add(_achievementController);
            this.Game.Components.Add(_camera);
            this.Game.Components.Add(_dataGrid);
            this.Game.Components.Add(_scene);

            // Add popups
            _achievementController.OnCompletedAchievement += new AchievementCompletedHandler(_achievementController_OnCompletedAchievement);

            // Add overlay
            _overlay = new PauseOverlay();
            _overlay.Exiting += new EventHandler(_overlay_Exiting);

            this.Exited += new EventHandler(PeaScreen_Exited);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void _achievementController_OnCompletedAchievement(Achievement e)
        {
            this.ScreenManager.AddScreen(new AchievementsPopup(e));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _overlay_Exiting(object sender, EventArgs e)
        {
            _timeController.Resume();
        }

        /// <summary>
        /// Removes global components
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeaScreen_Exited(object sender, EventArgs e)
        {
            this.Game.Components.Remove(_scene);
            this.Game.Components.Remove(_dataGrid);
            this.Game.Components.Remove(_pointsController);
            this.Game.Components.Remove(_peaController);
            this.Game.Components.Remove(_inputController);
            this.Game.Components.Remove(_achievementController);
            this.Game.Components.Remove(_timeController);

            this.Game.Services.RemoveService(typeof(TimeController));

            var peas = _peaController.DeRegisterAll();
            foreach(var pea in peas)
                this.Game.Components.Remove(pea);
        }

        /// <summary>
        /// Load content
        /// </summary>
        /// <param name="contentManager"></param>
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            // Loads the scene and so on
            base.LoadContent(contentManager);
            _scene.LoadContent(contentManager);
            
            #if DEBUG && !SILVERLIGHT
                view.LoadContent(this.Game.GraphicsDevice, contentManager);
            #endif
        }

        /// <summary>
        /// Draw 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // Draw scene
            base.Draw(gameTime);

#if DEBUG
            _camera.Position = new Vector2(1280 / 2, 720 / 2) - _spriteGrid.Position;
            Matrix mprojection = _camera.SimProjection;
            Matrix mview = _camera.SimView;

            #if !SILVERLIGHT
            if (_inputManager.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F12))
                view.RenderDebugData(ref mprojection, ref mview);
            #endif

            if (PeaController.DebugNodes != null)
            {
                this.ScreenManager.SpriteBatch.Begin();
                var font = this.ScreenManager.SpriteFonts["Default"];
                var goal = PeaController.DebugNodes[0].GoalValue;
                var posg = _spriteGrid.Position + new Vector2(goal.Position.X * 70, goal.Position.Y * 49);
                this.ScreenManager.SpriteBatch.DrawString(font, String.Format("A{0}-D{1}",
                   goal.Action.ToString()[0].ToString(), goal.Dir.ToString()[0].ToString()), posg, Color.Red);

                foreach (var node in PeaController.DebugNodes)
                {
                    var pos = _spriteGrid.Position + new Vector2(node.Value.Position.X * 70, node.Value.Position.Y * 49);
                    pos += Vector2.UnitY * 10 * (Int32)node.Value.Action;
                    this.ScreenManager.SpriteBatch.DrawString(font, String.Format("S{0}: A{1}-D{2}",
                        node.NodeState.ToString()[0].ToString(), node.Value.Action.ToString()[0].ToString(), node.Value.Dir.ToString()[0].ToString()
                            //(node.PreviousNode ?? new ArtificialIntelligence.AStar.Node<MoveNode>() { Value = new MoveNode() }).Value.Position), 
                            ),pos, Color.Black);
                }
                this.ScreenManager.SpriteBatch.End();
            }
#endif
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (_overlay.ScreenState != ScreenState.Active && _overlay.ScreenState != ScreenState.TransitionOn && 
                !_overlay.IsExiting && otherScreenHasFocus)
            {
                this.ScreenManager.AddScreen(_overlay);
                _timeController.Stop();
            }

            // Update physics
            _fx.Step((Single)gameTime.ElapsedGameTime.TotalSeconds * _timeController.Speed);

            // Update scene
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
            _inputController.HandleInput();

            // Go to paused
            if (this.InputManager.Keyboard.IsKeyPressed(Keys.Escape))
            {
                this.ScreenManager.AddScreen(_overlay);
                _timeController.Stop();
            }
        }
    }
}
