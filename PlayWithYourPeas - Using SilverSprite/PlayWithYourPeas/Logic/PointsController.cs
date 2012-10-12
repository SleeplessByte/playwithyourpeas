using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Data;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using PlayWithYourPeas.Engine.Services;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Logic
{
#if SILVERLIGHT
    using Color = ColorHelper;
#endif

    /// <summary>
    /// Scoring
    ///
    /// When a peas finally comes to a stop and it survives it's journey, it gets a happy score.
    ///     Happy score = Total bounces * (Spring Bounces * Spring Value + 
    ///         Normal Bounces * Normal value + Gel bounces * Gel Value + Successful Landing)
    /// The pea's happy score is added to the player's total score. This is a moment of grand 
    /// celebration. If you have a particle system, this is a great time to use it. I've provided
    /// graphics so that a happy pea can yell out "Ninja!", his career aspirations finally complete.
    /// Once the peas is scored, he moves on and starts looking for a new jumping spot to leap from.
    /// </summary>
    internal class PointsController : GameComponent
    {
        protected TimeController _timeController;
        protected List<PointsControllerState> _active;
        protected Int32 _points;

        /// <summary>
        /// Session score
        /// </summary>
        public Int32 PlayingScore
        {
            get
            {
                return _points;
            }
            protected set
            {
                _points = value;
                if (_points > 20000000)
                    this.OnReachedGoal.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event Action OnReachedGoal = delegate { };

        /// <summary>
        /// Session time
        /// </summary>
        public TimeSpan PlayingTime { get; protected set; }

        /// <summary>
        /// Current speed
        /// </summary>
        public Single RunningSpeed { get { return (_timeController != null ? _timeController.Speed : 1); } }

        /// <summary>
        /// Constructor for the pointscontroller
        /// </summary>
        public PointsController(Game game ):  base(game)
        {
            _active = new List<PointsControllerState>();
            
            this.PlayingTime = TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Registers a pea
        /// </summary>
        /// <param name="pea"></param>
        public void Register(DataPea pea)
        {
            pea.OnSeparation += new OnSeparationEventHandler(pea_OnSeparation);
            pea.OnJumpStarted += new JumpEventHandler(pea_OnJumpStarted);
            pea.OnJumpFailed += new JumpEventHandler(pea_OnJumpFailed);
            pea.OnJumpCompleted += new JumpEventHandler(pea_OnJumpCompleted);
        }

        /// <summary>
        /// Used to determine the active jump info
        /// </summary>
        /// <param name="pea"></param>
        /// <returns></returns>
        public Dictionary<BlockType, Int32> ActiveTimes(DataPea pea)
        {
            return _active.FirstOrDefault(s => s.Pea == pea).Times;
        }   

        /// <summary>
        /// Runs when jump is started
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnJumpStarted(JumpEventArgs e)
        {
            _active.Add(new PointsControllerState(e.Pea));
        }

        /// <summary>
        /// Runs when jump is failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnJumpFailed(JumpEventArgs e)
        {
            _active.RemoveAll(a => a.Pea == e.Pea);
        }

        /// <summary>
        /// Runs when jump is completed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnJumpCompleted(JumpEventArgs e)
        {
            // Update score
            foreach (var state in _active.Where(a => a.Pea == e.Pea))
                if (!state.Pea.IsNotHappy && !state.Pea.IsTrapped && state.Pea.IsAlive) // last check shouldn't be needed
                {
                    this.PlayingScore += state.Points;
                    PlayerProgress.Current.Points(state.Points);
                }
                        // multiplier
                        //(Int32)Math.Ceiling(state.Pea.JumpSpot.Completion * DataJumpSpot.MaxCompletion);

            _active.RemoveAll(a => a.Pea == e.Pea);
        }


        /// <summary>
        /// Runs when pea is no longer touching fixture
        /// </summary>
        /// <param name="fixtureA"></param>
        /// <param name="fixtureB"></param>
        private void pea_OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            var pea = fixtureA.UserData as DataPea;
            var bouncePea = fixtureB.UserData as DataPea;
            var speed = fixtureA.Body.LinearVelocity;

            var block = fixtureB.UserData as DataBlock;

            if (block != null && pea.IsNinja)
                DoHappyCollision(pea, speed, block);

            if (block == null && pea.IsNinja)
                if ((bouncePea) == null)
                    DoHappyCollision(pea, speed, block);
        }

        /// <summary>
        /// Process collision
        /// </summary>
        /// <param name="pea"></param>
        /// <param name="speed"></param>
        /// <param name="block"></param>
        private void DoHappyCollision(DataPea pea, Vector2 speed, DataBlock block)
        {
            PointsControllerState state;
            if (block != null && !block.IsPlaced)
                return;

            if ((state = _active.FirstOrDefault(p => p.Pea == pea)) != null)
                if (!state.IncreaseHappiness(speed, block))
                    pea.Trap();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _timeController = this.Game.Services.GetService(typeof(TimeController)) as TimeController;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var passed = TimeSpan.FromTicks((Int64)Math.Ceiling(gameTime.ElapsedGameTime.Ticks * this.RunningSpeed));
            this.PlayingTime = PlayingTime.Add(passed);
            PlayerProgress.Current.TimePassed(passed);

        }

#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public void Draw(ScreenManager sm)
        {
            sm.SpriteBatch.Begin();
            foreach(var s in _active)
            {
                sm.SpriteBatch.DrawString(sm.SpriteFonts["Small"], String.Format("P: {0}", s.Points.ToString()), s.Pea.Position, 
                    s.Pea.IsNotHappy ? Color.Red : Color.Blue);
            }
            sm.SpriteBatch.End();
        }
#endif
    }
}
