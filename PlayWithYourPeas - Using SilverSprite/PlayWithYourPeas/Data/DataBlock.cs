using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Controllers;
using FarseerPhysics.Common;
using FarseerPhysics;
using PlayWithYourPeas.Engine.Drawing;
using System.Diagnostics;
using PlayWithYourPeas.Logic;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Data container for block. Does not visually represent the blocks, but keeps
    /// track of the state and type and the physics involved. The block is not registered
    /// as a body, but a fixture is added to the static body. This costs less CPU and
    /// memory than adding each block as an individual FX body.
    /// </summary>
    internal class DataBlock
    {
        /// <summary>
        /// Gets/Sets the position in the grid
        /// </summary>
        public Point GridPosition { get; set; }

        /// <summary>
        /// Gets the display position according to the grid
        /// </summary>
        public Vector2 Position { get { return Vector2.UnitX * X * 70 + Vector2.UnitY * Y * 48; } }

        /// <summary>
        /// Gets/Sets the Blocktype
        /// </summary>
        public BlockType BlockType { get; protected set; }

        /// <summary>
        /// Gets/Sets the Blockstate
        /// </summary>
        public BlockState BlockState { get; protected set; }

        /// <summary>
        /// Gets the Grid X position
        /// </summary>
        public Int32 X { get { return GridPosition.X; } }

        /// <summary>
        /// Gets the Grid Y position
        /// </summary>
        public Int32 Y { get { return GridPosition.Y; } }

        /// <summary>
        /// Gets/Sets the Jump spot on the left
        /// </summary>
        public DataJumpSpot JumpLeft { get; protected set; }

        /// <summary>
        /// Gets/Sets the Jump spot on the right
        /// </summary>
        public DataJumpSpot JumpRight { get; protected set; }

        protected Single _stateTime;
        protected const Single PlacingTime = 5;
        protected const Single RemovingTime = 3;
        protected Fixture _fxBlock;
        protected Body _fxBody;

        /// <summary>
        /// Gets the restitution value
        /// </summary>
        public Single PeaRestitution
        {
            get
            {
                switch (this.BlockType)
                {
                    case Data.BlockType.Gel:
                        return 0.3f;
                    case Data.BlockType.LeftRamp:
                    case Data.BlockType.RightRamp:
                    case Data.BlockType.Normal:
                        return 0.7f;
                    case Data.BlockType.Spring:
                        return 1f;

                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Gets the friction value
        /// </summary>
        public Single PeaFriction
        {
            get
            {
                switch (this.BlockType)
                {
                    case Data.BlockType.Gel:
                        return 0.8f;
                    case Data.BlockType.LeftRamp:
                    case Data.BlockType.RightRamp:
                    case Data.BlockType.Normal:
                    case Data.BlockType.Spring:
                        return 0.5f;

                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Fires when block state is changed
        /// </summary>
        public event BlockStateChangedHandler OnStateChanged = delegate { };

        /// <summary>
        /// Fires when a jumpspot is secured on this block
        /// </summary>
        public event EventHandler OnJumpSpotBound = delegate { };

        /// <summary>
        /// Fires when a jumpspot is created
        /// </summary>
        public event EventHandler OnJumpSpotCreated = delegate { };

        /// <summary>
        /// Fires when a jumpspot is completed
        /// </summary>
        public event EventHandler OnJumpSpotCompleted = delegate { };

        /// <summary>
        /// Creates new datablock
        /// </summary>
        /// <param name="gridPosition">the position</param>
        /// <param name="blockType">the type</param>
        /// <param name="fx">the physics reference</param>
        public DataBlock(Point gridPosition, BlockType blockType, Body fx)
        {
            this.GridPosition = gridPosition;
            this.BlockType = blockType;
            this.BlockState = blockType == Data.BlockType.None ? BlockState.None : BlockState.Placing;
            _fxBody = fx;
            

            if (fx != null)
                UpdateFx();

            _stateTime = 0;
        }

        /// <summary>
        /// Frame Renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime, Single speed, Func<Fixture, Boolean> isTouching)
        {
            if (!this.IsPlaced)
                UpdateState(gameTime, speed, isTouching);
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateState(GameTime gameTime, Single speed, Func<Fixture, Boolean> isTouching)
        {
           
            if (this.BlockState == BlockState.Placing)
            {
                // This is how function call works
                var touch = isTouching(_fxBlock);
                _stateTime += (Single)gameTime.ElapsedGameTime.TotalSeconds * speed;

                if (_stateTime > PlacingTime)
                {
                    if (!touch)
                        ChangeState(BlockState.Placed);
                    else
                        _stateTime = Math.Max(0, PlacingTime - 1.8f);
                }
            }
            else if (this.BlockState == BlockState.Removing)
            {
                _stateTime += (Single)gameTime.ElapsedGameTime.TotalSeconds * speed;
                if (_stateTime > RemovingTime)
                {
                    this.BlockType = Data.BlockType.None;
                    ChangeState(BlockState.Removed);
                }
            }
        }

        /// <summary>
        /// Starts placing this block
        /// </summary>
        public void Place(BlockType type, BlockState newState = Data.BlockState.Placing)
        {
            this.BlockType = type;
            ChangeState(newState);
        }

        /// <summary>
        /// Starts removing this block
        /// </summary>
        public void Remove()
        {
            ChangeState(BlockState.Removing);
        }

        /// <summary>
        /// Changes the state
        /// </summary>
        /// <param name="newState"></param>
        private void ChangeState(BlockState newState)
        {
            this.BlockState = newState;
            
            if (_fxBody != null)
                UpdateFx();

            _stateTime = 0;

            OnStateChanged.Invoke(new BlockStateArgs(this, newState));
        }

        /// <summary>
        /// Create jumps pot
        /// </summary>
        /// <param name="spot"></param>
        public void CreateJumpSpot(DataJumpSpot spot)
        {
            this.OnJumpSpotCreated.Invoke(spot, EventArgs.Empty);
        }
        
        /// <summary>
        /// Binds a jump spot to this block
        /// </summary>
        /// <param name="spot"></param>
        public void BindJumpSpot(DataJumpSpot spot)
        {
            if (spot.Placement == DataJumpSpot.Location.Left)
                this.JumpLeft = spot;
            else if (spot.Placement == DataJumpSpot.Location.Right)
                this.JumpRight = spot;

            this.OnJumpSpotBound.Invoke(spot, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spot"></param>
        public void CompleteJumpSpot(DataJumpSpot spot)
        {
            this.OnJumpSpotCompleted.Invoke(spot, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the physics
        /// </summary>
        public void UpdateFx()
        {
            // Reset blockstate
            if (this.BlockType == Data.BlockType.None)
                this.BlockState = Data.BlockState.None;

            // Destroy old fixture
            if (_fxBlock != null && (this.BlockState == Data.BlockState.Placing || 
                this.BlockState == Data.BlockState.Removed || this.BlockState == Data.BlockState.None))
            {
                _fxBody.DestroyFixture(_fxBlock);
                _fxBlock = null;
            }

            if (this.BlockType == Data.BlockType.None)
                return;

            if (_fxBlock == null)
            {
                if (this.BlockType != Data.BlockType.LeftRamp && this.BlockType != Data.BlockType.RightRamp)
                {
                    // Create rectangular fixture
                    _fxBlock = FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(68), ConvertUnits.ToSimUnits(50), 1f,
                        ConvertUnits.ToSimUnits(this.Position + Vector2.UnitY * (8 + 23) + Vector2.UnitX * 38), _fxBody, this);
                }
                else
                {
                    // Create ramp
                    var vertices = new List<Vector2>();

                    if (this.BlockType == Data.BlockType.LeftRamp)
                        // right top corner
                        vertices.Add(ConvertUnits.ToSimUnits(Position + Vector2.UnitY * (6) + Vector2.UnitX * 68));
                    else
                        // left top corner
                        vertices.Add(ConvertUnits.ToSimUnits(Position + Vector2.UnitY * (6)));

                    // right bottom corner
                    vertices.Add(ConvertUnits.ToSimUnits(Position + Vector2.UnitY * (8 + 48) + Vector2.UnitX * 68));
                    // left bottom corner
                    vertices.Add(ConvertUnits.ToSimUnits(Position + Vector2.UnitY * (8 + 48)));

                    _fxBlock = FixtureFactory.AttachPolygon(new Vertices(vertices), 1f, _fxBody, this);
                }
            }

            _fxBlock.Restitution = this.PeaRestitution;
            _fxBlock.Friction = this.PeaFriction;
            _fxBlock.CollidesWith = Category.None; // This line is to prevent manifold to become 0 (fx)
            //_fxBlock.CollisionCategories = Category.Cat1;
            _fxBlock.IsSensor = !this.IsPlaced;
            _fxBlock.CollidesWith = (this.BlockType != BlockType.None) ? Category.All : Category.None; // Cat2
        }

        /// <summary>
        /// Gets the hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GridPosition.GetHashCode() ^ 63 * BlockType.GetHashCode() ^ 127;
        }

        /// <summary>
        /// Equality comparere
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var that = obj as DataBlock;
            if (that == null)
                return false;

            return that.GridPosition == this.GridPosition && that.BlockType == this.BlockType;
        }

        /// <summary>
        /// Block is placed
        /// </summary>
        public Boolean IsPlaced { get { return this.BlockState == Data.BlockState.Placed; } }

        /// <summary>
        /// Block is being placed or removed
        /// </summary>
        public Boolean IsTransitioning { get { return this.BlockState == Data.BlockState.Placing || this.BlockState == Data.BlockState.Removing; } }

        /// <summary>
        /// Block has no physics and graphics (empty space)
        /// </summary>
        public Boolean IsClear { get { return !this.IsPlaced || this.BlockType == Data.BlockType.None; } }

        /// <summary>
        /// Block is a normal typed block
        /// </summary>
        public Boolean IsNormal { get { return this.IsPlaced && this.BlockType == Data.BlockType.Normal; } }

        /// <summary>
        /// Block is a gel typed block
        /// </summary>
        public Boolean IsGel { get { return this.IsPlaced && this.BlockType == Data.BlockType.Gel; } }

        /// <summary>
        /// Block is a spring typed block
        /// </summary>
        public Boolean IsSpring { get { return this.IsPlaced && this.BlockType == Data.BlockType.Spring; } }
        
        /// <summary>
        /// Block is has a flat top
        /// </summary>
        public Boolean IsFlatTop { get { return IsNormal || IsGel || IsSpring; } }

        /// <summary>
        /// Block is Ramp from the left
        /// </summary>
        public Boolean IsRampLeft { get { return this.IsPlaced && this.BlockType == Data.BlockType.LeftRamp; } }

        /// <summary>
        /// Block is Ramp from the right
        /// </summary>
        public Boolean IsRampRight { get { return this.IsPlaced && this.BlockType == Data.BlockType.RightRamp; } }

        /// <summary>
        /// Block has a solid right side
        /// </summary>
        public Boolean IsSolidRight { get { return IsNormal || IsRampLeft; } }

        /// <summary>
        /// Block has a solid left side
        /// </summary>
        public Boolean IsSolidLeft { get { return IsNormal || IsRampRight; } }

        /// <summary>
        /// Block has another (solid) block below this
        /// </summary>
        public Boolean HasBlockBelow { get; set; }
    }
}
