using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PlayWithYourPeas.Data;
using PlayWithYourPeas.Engine.Services;
//using Microsoft.Xna.Framework.GamerServices;

namespace PlayWithYourPeas.Logic
{
    /// <summary>
    /// Controls the achievements. Has a complex structure to keep track of what achievements 
    /// are currently possible to get. See the Achievement Data Structure for more information.
    /// All the achievements are event based.
    /// </summary>
    internal class AchievementController : GameComponent
    {
        protected Dictionary<Achievement.Identifier, Achievement> _activeSingle;
        protected Dictionary<Achievement.Identifier, Dictionary<DataPea, Achievement>> _activePea;
        protected List<Achievement> _completed;

        protected Dictionary<DataPea, List<Achievement>> _jumpDisabled;
        protected Dictionary<DataPea, List<Achievement>> _peaDisabled;
        protected List<Achievement> _sessionDisabled;
        protected HashSet<DataPea> _peas;

        /// <summary>
        /// Fires when achievement is obtained
        /// </summary>
        public event AchievementCompletedHandler OnCompletedAchievement = delegate { };

        /// <summary>
        /// Creates a new controller
        /// </summary>
        /// <param name="game">Game to bind to</param>
        public AchievementController(Game game) : base(game)
        {
            // all achievements
            _activeSingle = new Dictionary<Achievement.Identifier, Achievement>();
            _activePea = new Dictionary<Achievement.Identifier, Dictionary<DataPea, Achievement>>();

            // Add
            foreach (var achievement in Achievement.BluePrint)
            {
                _activeSingle.Add(achievement.Id, achievement);

                if (achievement.Scope == Achievement.Times.SingleJump || achievement.Scope == Achievement.Times.SinglePea || 
                    achievement.Scope == Achievement.Times.MultiplePea)
                    _activePea.Add(achievement.Id, new Dictionary<DataPea, Achievement>());
            }

            // Remove obtained
            foreach (var achievement in PlayerProgress.Current.Achievements)
                if (achievement.Scope == Achievement.Times.Single)
                    _activeSingle[achievement.Id] = achievement;
        }

        /// <summary>
        /// Registers a pea
        /// </summary>
        /// <param name="pea"></param>
        public void Register(DataPea pea)
        {
            if (!_peas.Add(pea))
                return;
            
            pea.OnJumpStarted += new JumpEventHandler(pea_OnJumpStarted);
            pea.OnJumpCompleted += new JumpEventHandler(pea_OnJumpCompleted);
            pea.OnJumpFailed += new JumpEventHandler(pea_OnJumpCompleted);
            pea.OnRevive += new EventHandler(pea_OnRevive);

            var peabound = _activeSingle.Where(a => a.Value.Scope == Achievement.Times.SingleJump || a.Value.Scope == Achievement.Times.SinglePea ||
                a.Value.Scope == Achievement.Times.MultiplePea);

            foreach (var achievement in peabound)
                _activePea[achievement.Key].Add(pea, achievement.Value.GenerateNew());

            _peaDisabled.Add(pea, new List<Achievement>());
            _jumpDisabled.Add(pea, new List<Achievement>());
        }

        /// <summary>
        /// Registers grid
        /// </summary>
        /// <param name="grid"></param>
        public void Register(DataGrid grid)
        {
            grid.OnGridChanged += new OnDataGridChangedHandler(grid_OnGridChanged);
        }

        /// <summary>
        /// Starts the session (releases once per session achievements)
        /// </summary>
        public void StartSession()
        {
            _peas.Clear();

            _jumpDisabled.Clear();
            _sessionDisabled.Clear();
            _peaDisabled.Clear();
        }

        /// <summary>
        /// End the session
        /// </summary>
        public void EndSession(Int32 points)
        {
            Check(Achievement.Identifier.NoTraps, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));
            Check(Achievement.Identifier.NoDeaths, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));

            Check(Achievement.Identifier.NoNormal, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));
            Check(Achievement.Identifier.NoGel, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));
            Check(Achievement.Identifier.NoRamp, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));
            Check(Achievement.Identifier.NoSpring, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));
            Check(Achievement.Identifier.NoDelete, (t) => __IsLessThanOrEqual(t.Value<Int32>(), 0));

            Check(Achievement.Identifier.SessionFinished, (t) => __IsMoreThanOrEqual(points, 20000000));
        }

        /// <summary>
        /// Fires when a pea jumps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnJumpStarted(JumpEventArgs e)
        {
            // Check if actual jump
            if (e.Spot == null)
                return;

            Check(Achievement.Identifier.FirstJump);
            Progress(Achievement.Identifier.AllJumping, __CheckAllPeaIn, (h) => __AddAndReturn<DataPea>(h, e.Pea));
        }


        /// <summary>
        /// Fires when a pea completed jump (or is trapped)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnJumpCompleted(JumpEventArgs e)
        {
            // Check if actual jump
            if (e.Spot == null)
                return;

            // Pea Died
            if (!e.Pea.IsAlive)
            {
                Check(Achievement.Identifier.FirstDeath);
                Modify(Achievement.Identifier.NoDeaths, __Increment);
                Progress(Achievement.Identifier.AllDeath, __CheckAllPeaIn, (h) => __AddAndReturn<DataPea>(h, e.Pea));
            }
            // Pea Trapped
            else if (e.Pea.IsTrapped)
            {
                Check(Achievement.Identifier.FirstTrap);
                Modify(Achievement.Identifier.NoTraps, __Increment);
            }

            // Flag completed
            if ((e.Spot.Completion + e.Spot.FailFloat) >= 1.0f)
            {
                Check(Achievement.Identifier.FirstCompletion);
                Check(Achievement.Identifier.FirstHappyFlag, (a) => __IsLessThanOrEqual(e.Spot.FailFloat, 0.05));
                Check(Achievement.Identifier.FirstDeathFlag, (a) => __IsMoreThanOrEqual(e.Spot.FailFloat, 0.95));
            }
                        

            Modify(Achievement.Identifier.AllJumping, (h) => __RemoveAndReturn<DataPea>(h, e.Pea));

            _jumpDisabled[e.Pea].Clear();
        }

        /// <summary>
        /// Fires when pea revives
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pea_OnRevive(object sender, EventArgs e)
        {
            Modify(Achievement.Identifier.AllDeath, (h) => __RemoveAndReturn<DataPea>(h, sender as DataPea));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grid_OnGridChanged(DataGrid grid, BlockStateArgs cause)
        {
            if (cause.Block.BlockType == BlockType.Delete) // deleted
            {
                Modify(Achievement.Identifier.NoDelete, __Increment);
            } 
            else if (cause.State == BlockState.Placed) // just placed
            {
                switch (cause.Block.BlockType)
                {
                    case BlockType.Normal:
                        Modify(Achievement.Identifier.NoNormal, __Increment);
                        break;

                    case BlockType.Gel:
                        Modify(Achievement.Identifier.NoGel, __Increment);
                        break;

                    case BlockType.LeftRamp:
                    case BlockType.RightRamp:
                        Modify(Achievement.Identifier.NoRamp, __Increment);
                        break;

                    case BlockType.Spring:
                        Modify(Achievement.Identifier.NoSpring, __Increment);
                        break;
                }
            }
        }

        /// <summary>
        /// Check function: more or equal
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        protected Boolean __IsMoreThanOrEqual(dynamic t, dynamic c)
        {
            return t >= c;
        }

        /// <summary>
        /// Check: all peas in arg and unique jumpspots
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="res"></param>
        protected Boolean __CheckAllPeaInAndUniqueSpot(Object arg)
        {
            var jumping = ((HashSet<DataPea>)arg);
            return _peas.All(p => jumping.Contains(p) && !jumping.Any(j => j != p && j.JumpSpot == p.JumpSpot));
        }

        /// <summary>
        /// Check: all peas in arg
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Boolean __CheckAllPeaIn(Achievement.IProgress arg)
        {
            var dead = (HashSet<DataPea>)arg.Value<HashSet<DataPea>>();
            return _peas.All(p => dead.Contains(p));
        }

        /// <summary>
        /// Check function: less or equal
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        protected Boolean __IsLessThanOrEqual(dynamic t, dynamic c)
        {
            return t <= c;
        }

        /// <summary>
        /// Modify function: Increment
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected Achievement.IProgress __Increment(Achievement.IProgress t) //dynamic __Increment(dynamic t)
        {
            return t.Add<Int32>(1);
        }


        /// <summary>
        /// Modify Function: add to set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="h"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        protected Achievement.IProgress __AddAndReturn<T>(Achievement.IProgress h, T p)
        {
            return h.Add<T>(p);
        }

        /// <summary>
        /// Modifty Function: remove from set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="h"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        protected Achievement.IProgress __RemoveAndReturn<T>(Achievement.IProgress h, T p) 
        {
            return h.Remove<T>(p);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // all peas
            _peas = new HashSet<DataPea>();

            // all completed
            _completed = new List<Achievement>();

            // disabled for this jump 
            _jumpDisabled = new Dictionary<DataPea, List<Achievement>>();

            // disabled for this session
            _sessionDisabled = new List<Achievement>();

            // disabled for this session (pea)
            _peaDisabled = new Dictionary<DataPea, List<Achievement>>();

            // There is no update logic
            this.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modification"></param>
        private void Modify(Achievement.Identifier id,
            Func<Achievement.IProgress, Achievement.IProgress> modification, 
            DataPea sender = null)
        {
            Progress(id, sender, null, modification);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modification"></param>
        private Boolean Check(Achievement.Identifier id,
            Func<Achievement.IProgress, Boolean> completion = null, 
            DataPea sender = null)
        {
            return Progress(id, sender, completion);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="completion"></param>
        /// <param name="modification"></param>
        private Boolean Progress(Achievement.Identifier id,
            Func<Achievement.IProgress, Boolean> completion,
            Func<Achievement.IProgress, Achievement.IProgress> modification)
        {
            return Progress(id, null, completion, modification);
        }

        /// <summary>
        /// Progresses an achievement
        /// </summary>
        /// <param name="id"></param>
        private Boolean Progress(Achievement.Identifier id, 
            DataPea sender = null, 
            Func<Achievement.IProgress, Boolean> completion = null,
            Func<Achievement.IProgress, Achievement.IProgress> modification = null)
        {
            var achievement = _activeSingle[id];
            var pea = sender as DataPea;

            // Get correct achievement and cancel if disabled
            switch (achievement.Scope)
            {
                case Achievement.Times.Single:
                    if (_activeSingle[id].IsCompleted)
                        return true;
                    break;
                case Achievement.Times.SinglePea:
                    if (_peaDisabled[pea].Contains(achievement))
                        return true;
                    achievement = _activePea[id][pea];
                    break;
                case Achievement.Times.SingleJump:
                    if (_jumpDisabled[pea].Contains(achievement))
                        return true;
                    achievement = _activePea[id][pea];
                    break;
                case Achievement.Times.SingleSession:
                    if (_sessionDisabled.Contains(achievement))
                        return true;
                    break;
                case Achievement.Times.MultiplePea:
                    achievement = _activePea[id][pea];
                    break;
            }

            // Process
            switch (achievement.Type)
            {
                case Achievement.ProgressType.Event:
                    achievement.IsCompleted = true;
                    break;
                case Achievement.ProgressType.Condition:
                    achievement.IsCompleted = completion.Invoke(null);
                    break;
                case Achievement.ProgressType.Accumulation:
                    if (modification != null) // null if check only
                        achievement.ActualValue = modification.Invoke(achievement.ActualValue);
                    if (completion != null) // null if update only
                        achievement.IsCompleted = completion.Invoke(achievement.ActualValue);
                    break;
            }

            // Determine recreating
            if (achievement.IsCompleted)
            {
                _completed.Add(achievement);
                PlayerProgress.Current.Achievement(achievement);

                OnCompletedAchievement.Invoke(achievement);

                System.Diagnostics.Debug.WriteLine("Achievement {0} completed [{1} - {2}]", achievement.Id, achievement.Name, achievement.Description);

                switch (achievement.Scope)
                {
                    case Achievement.Times.Multiple:
                        _activeSingle[id] = achievement.GenerateNew();
                        break;

                    case Achievement.Times.MultiplePea:
                        _activePea[id][pea] = achievement.GenerateNew();
                        break;

                    case Achievement.Times.SingleJump:
                        _activePea[id][pea] = achievement.GenerateNew();
                        _jumpDisabled[pea].Add(_activePea[id][pea]);
                        break;

                    case Achievement.Times.SingleSession:
                        _activeSingle[id] = achievement.GenerateNew();
                        _sessionDisabled.Add(_activeSingle[id]);
                        break;

                    case Achievement.Times.SinglePea:
                        _activePea[id][pea] = achievement.GenerateNew();
                        _peaDisabled[pea].Add(_activePea[id][pea]);
                        break;
                }
            }

            return achievement.IsCompleted;
        }
    }
}
