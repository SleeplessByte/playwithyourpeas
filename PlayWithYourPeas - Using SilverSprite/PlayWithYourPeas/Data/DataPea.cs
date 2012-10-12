using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using PlayWithYourPeas.Engine.Drawing;
using System.Diagnostics;
using PlayWithYourPeas.Logic;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Data container and processer for peas. Does not visually represent 
    /// a pea but keeps track of physics and other pea state data. 
    /// </summary>
    internal class DataPea : GameComponent
    {
        /// <summary>
        /// Height from which the mood is hurt
        /// </summary>
        protected const Int32 HurtHeight = 3;

        /// <summary>
        /// Maximum fall height for normal blocks
        /// </summary>
        protected const Int32 MaxFallHeight = 4;

        /// <summary>
        /// Maximum fall height for the ground
        /// </summary>
        protected const Int32 MaxFallHeightGround = 4;

        /// <summary>
        /// Maxmimum fall height when on ramps
        /// </summary>
        protected const Int32 MaxFallHeightRamp = 5;

        /// <summary>
        /// Random instance
        /// </summary>
        public static Random Random = new Random();

        /// <summary>
        /// Pea is alive flag
        /// </summary>
        public Boolean IsAlive { get; set; }

        /// <summary>
        /// Pea mood
        /// </summary>
        public Mood Feeling { get; protected set; }
        
        /// <summary>
        /// Pea is currently physics enabled
        /// </summary>
        public Boolean IsNinja { get; protected set; }

        /// <summary>
        /// Pea has hurt mood
        /// </summary>
        public Boolean IsHurt { get { return Feeling == Mood.Hurt; } }

        /// <summary>
        /// Pea has happy mood
        /// </summary>
        public Boolean IsHappy { get { return Feeling == Mood.Happy; } }

        /// <summary>
        /// Pea feels like a ninja
        /// </summary>
        public Boolean IsRehabilitating { get { return Feeling == Mood.Ninja || Feeling == Mood.Trapped; } }

        /// <summary>
        /// Pea is dying
        /// </summary>
        public Boolean IsDying { get { return Feeling == Mood.Dying;  } }

        /// <summary>
        /// Pea feels trapped
        /// </summary>
        public Boolean IsTrapped { get { return Feeling == Mood.Trapped; } }

        /// <summary>
        /// Pea is navigating the environment
        /// </summary>
        public Boolean IsNeutral { get { return Feeling == Mood.Neutral; } }

        /// <summary>
        /// Pea is navigating and therefore jumping. Does not collect points
        /// in this state and not showing the ninja end animation.
        /// </summary>
        public Boolean IsNotHappy { get; protected set; }

        /// <summary>
        /// Moodtime for current mood
        /// </summary>
        public Single MoodTime
        {
            get
            {
                switch(this.Feeling)
                {
                    case Mood.Trapped:
                    case Mood.Ninja:
                        return 2;
                    case Mood.Happy:
                        return 1.5f;
                    case Mood.Dying:
                        return 0.3f;
                    case Mood.Hurt:
                        return 1f;
                    default:
                        return Single.PositiveInfinity;
                }
            }
        }

        /// <summary>
        /// Display position
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Grid position
        /// </summary>
        public Point GridPosition { get { return new Point((Int32)Math.Floor(this.Position.X / 70f), (Int32)Math.Floor(this.Position.Y / 48f)); } }

        /// <summary>
        /// Rotataion
        /// </summary>
        public Single Rotation { get; set; }

        /// <summary>
        /// Current jump spot
        /// </summary>
        public DataJumpSpot JumpSpot { get { return _currentJumpSpot; } }

        /// <summary>
        /// 
        /// </summary>
        public Single RunningSpeed { get { return _timeController != null ? _timeController.Speed : 1; } }

        protected Vector2 _previousPosition;
        protected Single _fallHeight;       
        protected Body _fxPea;
        protected Single _inactiveTime;
        protected Single _moodTime;
        protected PeaController _controller;
        protected TimeController _timeController;
        protected HashSet<Fixture> _collisions;
        protected DataJumpSpot _currentJumpSpot;

        /// <summary>
        /// Fires when pea jumps
        /// </summary>
        public event JumpEventHandler OnJumpStarted = delegate { };

        /// <summary>
        /// Fires when pea completes a jump
        /// </summary>
        public event JumpEventHandler OnJumpCompleted = delegate { };

        /// <summary>
        /// Fires when the completed jump was a failure
        /// </summary>
        public event JumpEventHandler OnJumpFailed = delegate { };

        /// <summary>
        /// Fires when pea is separated from another fixture
        /// </summary>
        public event OnSeparationEventHandler OnSeparation = delegate { };

        /// <summary>
        /// Fires when pea revives on the grid
        /// </summary>
        public event EventHandler OnRevive = delegate { };

        /// <summary>
        /// Creates a new pea
        /// </summary>
        /// <param name="fx">Physics world</param>
        /// <param name="game">Game to bind to</param>
        /// <param name="controller">Controller</param>
        public DataPea(Game game, World fx, PeaController controller)
            : base(game)
        {
            // Put on floor
            Revive();

            // Create physics
            _fxPea = BodyFactory.CreateCircle(fx, ConvertUnits.ToSimUnits(16), 1f,
                ConvertUnits.ToSimUnits(this.Position) + Vector2.UnitX * ConvertUnits.ToSimUnits(18) + Vector2.UnitY * ConvertUnits.ToSimUnits(18), this);
            
            _fxPea.BodyType = BodyType.Dynamic;
            _fxPea.Mass = 1f;
            _fxPea.Restitution = 0.7f;
            _fxPea.Friction = 0.5f;
            _fxPea.LinearDamping = 0.2f;
            _fxPea.OnCollision += new OnCollisionEventHandler(_fxPea_OnCollision);
            _fxPea.OnSeparation += new OnSeparationEventHandler(_fxPea_OnSeparation);
            //_fxPea.CollisionCategories = Category.Cat2;
            //_fxPea.CollidesWith = Category.All;
            Jump(null);
           
            _collisions = new HashSet<Fixture>();
            _controller = controller;
            _controller.Register(this);

            ChangeMood(Mood.Happy);
        }

        /// <summary>
        /// Initializes Pea
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _timeController = this.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// Fires on separation
        /// </summary>
        /// <param name="fixtureA"></param>
        /// <param name="fixtureB"></param>
        private void _fxPea_OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            if (_collisions.Remove(fixtureB))
                #if VERBOSE
                    Debug.WriteLine("Separation from fixture " + fixtureB.FixtureId);
                #else           
                    {}
                #endif

            this.OnSeparation.Invoke(fixtureA, fixtureB);
        }

        /// <summary>
        /// Fires on collision (narrowphase)
        /// </summary>
        /// <param name="fixtureA"></param>
        /// <param name="fixtureB"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        private Boolean _fxPea_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            var pea = (fixtureA.UserData as DataPea) ?? (fixtureB.UserData as DataPea);
            var block = (fixtureB.UserData as DataBlock) ?? (fixtureA.UserData as DataBlock);

            // Falling on top face
            if (block == null || (block.IsPlaced && (block.Position.X < pea.Position.X && block.Position.X + 70 > pea.Position.X))) // &&
                //block.Position.Y  pea.Position.Y))
            {
                #if VERBOSE
                    Debug.WriteLine("Fall: " + _fallHeight.ToString());
                #endif

                // If high fallen
                if ((block == null && _fallHeight >= MaxFallHeightGround) ||
                    ((block != null && (block.IsRampLeft || block.IsRampRight)) && _fallHeight >= MaxFallHeightRamp) ||
                     (block != null && !block.IsSpring && !block.IsGel) && _fallHeight >= MaxFallHeight)
                {
                    // TODO: option for kill on collision with pea
                    if (block == null || (fixtureB.UserData is DataPea) == false)
                    {
                        // Kills the pea
                        Die();

                        return false;
                    }
                }

                // Register hurt
                if (_fallHeight >= HurtHeight)
                    ChangeMood(Mood.Hurt);

                _fallHeight = 0;
            }

            // If there is a block, didn't collide with ground
            if (block != null)
            {
                if (block.IsPlaced)
                {
                    _fxPea.Restitution = block.PeaRestitution;
                    _fxPea.Friction = block.PeaFriction;
                }
                _collisions.Add(fixtureB);
                 #if VERBOSE
                    Debug.WriteLine("Collision with fixture " + fixtureB.FixtureId);
                #endif
            }
            else
            {
                // Collision with ground
                _fxPea.Restitution = 0.7f;
                _fxPea.Friction = 0.5f;
            }

            return true;
        }

        /// <summary>
        /// Changes mood
        /// </summary>
        /// <param name="mood">New mood</param>
        private void ChangeMood(Mood mood)
        {
            this.Feeling = mood;
            _moodTime = 0;
        }        
                
        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Progress mood
            if (!this.IsNeutral)
                UpdateMood(gameTime);

            // Progress physics
            if (this.IsNinja)
            {
                this._previousPosition = this.Position;
                this.Position = ConvertUnits.ToDisplayUnits(_fxPea.Position);
                this.Rotation = _fxPea.Rotation;

                // Keep track of fall height
                if (_fxPea.LinearVelocity.Y < 0)
                    _fallHeight = 0;
                else
                    _fallHeight += (this.Position.Y - this._previousPosition.Y) / 49f;

                // Keep track of in activity
                if (ConvertUnits.ToDisplayUnits(_fxPea.LinearVelocity).Length() < 50)
                    _inactiveTime += (Single)gameTime.ElapsedGameTime.TotalSeconds * RunningSpeed;
                else
                    _inactiveTime = 0;

                // Reset if inactive 
                if (!_fxPea.Awake || _inactiveTime > 1)
                    EndJump();
            }
            else if (!this.IsDying)
            {
                _fxPea.Awake = true;

                // Update navigation
                _controller.Update(this, gameTime);

                // Update position to navigation
                _fxPea.Position = ConvertUnits.ToSimUnits(this.Position);
                _fxPea.Rotation = this.Rotation;
            }

            if (this.IsAlive)
                UpdateBounds();
        }

        /// <summary>
        /// Progresses mood
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values</param>
        protected void UpdateMood(GameTime gameTime) {
            _moodTime += (Single)gameTime.ElapsedGameTime.TotalSeconds * RunningSpeed;

            if (_moodTime > MoodTime)
                ChangeMood(Mood.Neutral);
        }

        /// <summary>
        /// Kill when outside of the grid
        /// </summary>
        protected void UpdateBounds()
        {
            if (this.GridPosition.X < -1 || this.GridPosition.X > DataGrid.Width || this.GridPosition.Y > DataGrid.Height)
                Die();
        }

        /// <summary>
        /// Jumps this DataJumpSpot
        /// </summary>
        /// <param name="location"></param>
        internal void Jump(DataJumpSpot jumpspot)
        {
            this.IsNinja = true;
            this.IsNotHappy = jumpspot == null;

            _fxPea.CollidesWith = Category.None; // This line is to prevent manifold to become 0 (fx)
            _fxPea.ResetDynamics();
            _fxPea.IsSensor = false;
            _fxPea.IgnoreGravity = false;
            _fxPea.CollidesWith = Category.All;

            if (jumpspot == null)
            {
                _fxPea.ApplyLinearImpulse(ConvertUnits.ToSimUnits(Vector2.UnitX * 120 * (Random.Next(100) >= 50 ? -1 : 1) + Vector2.UnitY * -220));
                this.OnJumpStarted.Invoke(new JumpEventArgs(jumpspot, this, 0));
                return;
            }
            
            var location = jumpspot.Placement;
            _currentJumpSpot = jumpspot;
            _currentJumpSpot.Start(this);

            _fxPea.ApplyLinearImpulse(ConvertUnits.ToSimUnits(Vector2.UnitX * 120 * (location == DataJumpSpot.Location.Left ? -1 : 1) + Vector2.UnitY * -220));

            ChangeMood(Mood.Happy);

            this.OnJumpStarted.Invoke(new JumpEventArgs(jumpspot, this, 0));
        }

        /// <summary>
        /// End a jump
        /// </summary>
        internal void EndJump()
        {
            if (_currentJumpSpot != null)
                _currentJumpSpot.Complete(this);

            this.IsNinja = false;
            _inactiveTime = 0;
            
            _fxPea.CollidesWith = Category.None;  // This line is to prevent manifold to become 0 (fx)
            _fxPea.ResetDynamics();
            _fxPea.IgnoreGravity = true;
            _fxPea.IsSensor = true;
            _fxPea.CollidesWith = Category.All;

            if (!IsTrapped)
                ChangeMood(Mood.Ninja);

            this.OnJumpCompleted.Invoke(new JumpEventArgs(_currentJumpSpot, this, 0));

            _currentJumpSpot = null;
        }

        /// <summary>
        /// Kills the pea
        /// </summary>
        internal void Die()
        {
            this.IsAlive = false;
            this.IsNinja = false;
            _inactiveTime = 0;
            _fxPea.LinearVelocity = Vector2.Zero;
            _fxPea.CollidesWith = Category.None; // stop colliding
            _fxPea.IgnoreGravity = true;
            _collisions.Clear();

            if (_currentJumpSpot != null)
                _currentJumpSpot.Fail(this);

            ChangeMood(Mood.Dying);

            this.OnJumpFailed.Invoke(new JumpEventArgs(_currentJumpSpot, this, 0));

            _currentJumpSpot = null;
        }

        /// <summary>
        /// Replaces the pea somewhere on the screen
        /// </summary>
        internal void Revive()
        {
            this.IsAlive = true;
            this.Position = Vector2.UnitX * (35 + Random.Next(DataGrid.WidthInPixels - 70)) + Vector2.UnitY * (DataGrid.HeightInPixels - Random.Next(150) - 20);

            this.IsNinja = true;
            this._fallHeight = 0;

            this._previousPosition = this.Position;

            if (_fxPea != null)
            {
                _fxPea.Position = ConvertUnits.ToSimUnits(this.Position);
                _fxPea.CollidesWith = Category.None;
                _fxPea.ResetDynamics();
                _fxPea.IgnoreGravity = false;
                _fxPea.IsSensor = false;
                _fxPea.CollidesWith = Category.All;

                this.OnRevive.Invoke(this, EventArgs.Empty);

                Jump(null);
            }
        }

        /// <summary>
        /// Mark as trapped
        /// </summary>
        internal void Trap()
        {
            this.IsNotHappy = true;
            ChangeMood(Mood.Trapped);
            EndJump();

            this.OnJumpFailed.Invoke(new JumpEventArgs(_currentJumpSpot, this, 0));
        }

        /// <summary>
        /// Checks if fixture is currently in collision with this pea
        /// </summary>
        /// <param name="fix"></param>
        /// <returns></returns>
        internal Boolean IsTouching(Fixture fix)
        {
            return _collisions.Contains(fix);
        }

        /// <summary>
        /// Mood enumeration
        /// </summary>
        public enum Mood
        {
            None = 0,

            /// <summary>
            /// Navigation mood
            /// </summary>
            Neutral,

            /// <summary>
            /// Ninja mood (in happy events, at jump or at happy points)
            /// </summary>
            Happy,

            /// <summary>
            /// Hurt mood (high fallen)
            /// </summary>
            Hurt,

            /// <summary>
            /// Happy Ninja mood (completed jump)
            /// </summary>
            Ninja,

            /// <summary>
            /// Trapped ninja
            /// </summary>
            Trapped,
            
            /// <summary>
            /// Dying pea
            /// </summary>
            Dying,
        }
    }        
}
