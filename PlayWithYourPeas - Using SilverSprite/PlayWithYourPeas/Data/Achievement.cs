using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PlayWithYourPeas.Data
{
    [DataContract]
    public class Achievement
    {
        /// <summary>
        /// Id
        /// </summary>
        [DataMember]
        public Identifier Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember]
        public String Name { get; set; }

        /// <summary>
        /// Descripton
        /// </summary>
        [DataMember]
        public String Description { get; set; }

        /// <summary>
        /// Is completed flag
        /// </summary>
        public Boolean IsCompleted { get { return this.CompletionTime.HasValue; } set { if (value) { this.CompletionTime = DateTime.Now; } } }
        
        /// <summary>
        /// Completion time (unlock time)
        /// </summary>
        [DataMember]
        public Nullable<DateTime> CompletionTime { get; set; }
        
        /// <summary>
        /// Scope in which this is obtainable
        /// </summary>
        [DataMember]
        public Times Scope { get; set; }

        /// <summary>
        /// Type of progressing
        /// </summary>
        [DataMember]
        public ProgressType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Mood Alignment { get; set; }

        /// <summary>
        /// Current value
        /// </summary>
        public IProgress ActualValue { get; set; }

        /// <summary>
        /// Initial value
        /// </summary>
        public IProgress InitialValue { get; set; }
        
        /// <summary>
        /// Open Graph Url
        /// </summary>
        public String Url { get { return "http://peas.derk-jan.org/canvas/achievement/" + ((Int32)this.Id).ToString(); } }

        /// <summary>
        /// Unlocked action id
        /// </summary>
        public String FacebookId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Achievement(Identifier id, String name, String description,
            Times times, ProgressType type, Mood mood, IProgress init = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;

            this.Scope = times;
            this.Type = type;
            this.Alignment = mood;
            this.InitialValue = init;
            this.ActualValue = init;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Boolean Equals(Object obj)
        {
            var other = obj as Achievement;
            if (other == null)
                return false;

            return other.Id == this.Id && (this.Type != Achievement.ProgressType.AccumulationStack || (this.InitialValue == other.InitialValue));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Int32 GetHashCode()
        {
            return (Id.GetHashCode() ^ 127) *
                (this.Type != Achievement.ProgressType.AccumulationStack ? 1 : 71 ^ this.InitialValue.GetHashCode());
        }

        /// <summary>
        /// AchievenetId
        /// </summary>
        public enum Identifier
        {
            None = 0,

            // First

            FirstJump = 1,
            FirstDeath = 2,
            FirstTrap = 3,
            FirstCompletion = 4,
            FirstHappyFlag = 5,
            FirstDeathFlag = 6,

            // Win

            NoDeaths = 7,
            NoNormal = 8,
            NoGel = 9,
            NoSpring = 10,
            NoRamp = 11,
            NoDelete = 12,
            NoTraps = 44,

            // Pea state

            AllJumping = 13,
            AllDeath = 14,
            AllTrapped = 15,

            // Single Jump

            JumpTime = 16,
            JumpBounce = 17,
            JumpBounceType = 18,
            JumpPoints = 19,

            // Single Game

            SessionTime = 24,

            SessionJumpTime = 25,
            SessionJumpBounce = 26,
            SessionJumpBounceType = 27,
            SessionJumpPoints = 28,

            SessionJumpCount = 29,
            SessionTrapCount = 30,
            SessionDeathCount = 31,
            SessionCompletionCount = 32,

            SessionFinished = 45,

            // All Games

            AllTime = 33,
            AllSessionCount = 34,
            AllSessionWin = 35,

            AllJumpTime = 36,
            AllJumpBounce = 37,
            AllJumpBounceType = 38,
            AllJumpPoints = 39,

            AllJumpCount = 40,
            AllTrapCount = 41,
            AllDeathCount = 42,
            AllCompletionCount = 43,

        }

        /// <summary>
        /// Progress times
        /// </summary>
        public enum Times
        {
            None = 0,

            /// <summary>
            /// Can only obtain this once
            /// </summary>
            Single = 1,

            /// <summary>
            /// Can obtain this multiple times
            /// </summary>
            Multiple = 2,

            /// <summary>
            /// Can obtain this once per session
            /// </summary>
            SingleSession = 3,

            /// <summary>
            /// Can obtain this once per jump
            /// </summary>
            SingleJump = 4,

            /// <summary>
            /// Can obtain this once per pea per session
            /// </summary>
            SinglePea = 5,

            /// <summary>
            /// Each pea can obtain this multiple times 
            /// </summary>
            MultiplePea = 6,
        }

        /// <summary>
        /// Progress type
        /// </summary>
        public enum ProgressType
        {
            None = 0,

            /// <summary>
            /// Awarded when on event
            /// </summary>
            Event = 1,

            /// <summary>
            /// Awarded when condition is equal
            /// </summary>
            Condition = 2,

            /// <summary>
            /// Awarded when accumulation exceeds conditions
            /// </summary>
            Accumulation = 3,

            /// <summary>
            /// Awarded when accumulation exceeds conditions, multiple version of same award
            /// </summary>
            AccumulationStack = 4,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Mood
        {
            None = 0,

            Neutral = 1,
            Happy = 2,
            Hurt = 3,
        }

        /// <summary>
        /// 
        /// </summary>
        public interface IProgress
        {
            Achievement.IProgress Add<T1>(T1 p);
            Achievement.IProgress Remove<T2>(T2 p);

            Object Value<T1>();
        }

        /// <summary>
        /// 
        /// </summary>
        internal class PeaProgress : IProgress
        {
            /// <summary>
            /// 
            /// </summary>
            protected HashSet<DataPea> _data;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static PeaProgress From() { return new PeaProgress(); }

            /// <summary>
            /// 
            /// </summary>
            private PeaProgress()
            {
                _data = new HashSet<DataPea>();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T1"></typeparam>
            /// <param name="p"></param>
            /// <returns></returns>
            public IProgress Add<T1>(T1 pea)
            {
                if (pea is DataPea)
                {
                    _data.Add(pea as DataPea);
                    return this;
                }

                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T2"></typeparam>
            /// <param name="pea"></param>
            /// <returns></returns>
            public IProgress Remove<T2>(T2 pea)
            {
                if (pea is DataPea)
                {
                    _data.Remove(pea as DataPea);
                    return this;
                }

                throw new NotImplementedException();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T1"></typeparam>
            /// <returns></returns>
            public Object Value<T1>()
            {
                if (_data is T1)
                    return _data;

                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class CountProgress : IProgress
        {
            dynamic _data;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static CountProgress From(dynamic value) { return new CountProgress(value); }

            /// <summary>
            /// 
            /// </summary>
            private CountProgress(dynamic value)
            {
                _data = value;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T1"></typeparam>
            /// <param name="p"></param>
            /// <returns></returns>
            public IProgress Add<T1>(T1 p)
            {
                _data += p;
                return this;
            }

            public IProgress Remove<T2>(T2 p)
            {
                _data -= p;
                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T1"></typeparam>
            /// <returns></returns>
            public object Value<T1>()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Generate a new achievement from this achievement
        /// </summary>
        /// <returns></returns>
        internal Achievement GenerateNew()
        {
            return new Achievement(this.Id, this.Name, this.Description, this.Scope, this.Type, this.Alignment, this.InitialValue);
        }

        public static IEnumerable<Achievement> BluePrint
        {
            get
            {
                var achievements = new Achievement[] { 
                    new Achievement(Achievement.Identifier.FirstJump, "A leap of faith", "first jump", Achievement.Times.Single, Achievement.ProgressType.Event, Achievement.Mood.Neutral ),
                    new Achievement(Achievement.Identifier.FirstDeath, "Pealiminated", "first death", Achievement.Times.Single, Achievement.ProgressType.Event, Achievement.Mood.Neutral ),
                    new Achievement(Achievement.Identifier.FirstCompletion, "Raise that flag", "first fully raised flag", Achievement.Times.Single, Achievement.ProgressType.Event, Achievement.Mood.Neutral ),
                    new Achievement(Achievement.Identifier.FirstHappyFlag, "The Happy Place", "first fully raised happy flag", Achievement.Times.Single, Achievement.ProgressType.Condition, Achievement.Mood.Happy ),
                    new Achievement(Achievement.Identifier.FirstDeathFlag, "Poor construction", "first fully raised death flag", Achievement.Times.Single, Achievement.ProgressType.Condition, Achievement.Mood.Hurt ), 
                    new Achievement(Achievement.Identifier.FirstTrap, "First Trap", "", Achievement.Times.Single, Achievement.ProgressType.Event, Achievement.Mood.Neutral ),

                    new Achievement(Achievement.Identifier.NoDeaths, "Act of Caring", "completed session without deaths", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation, Achievement.Mood.Happy, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.NoNormal, "Act of Irregularity", "completed session without normal blocks", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation, Achievement.Mood.Neutral, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.NoGel, "Act of Racism", "completed session without gel", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation, Achievement.Mood.Neutral, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.NoRamp, "Act of Planes", "completed session without ramps", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation, Achievement.Mood.Neutral, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.NoSpring, "Act of Gravity", "completed session without springs", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation,  Achievement.Mood.Neutral, Achievement.CountProgress.From(0)), 
                    new Achievement(Achievement.Identifier.NoDelete, "Act of Planning", "completed session without removing blocks", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation,  Achievement.Mood.Neutral, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.NoTraps, "Act of Freedom", "completed session without traps", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation,  Achievement.Mood.Happy, Achievement.CountProgress.From(0)),

                    new Achievement(Achievement.Identifier.AllJumping, "Teenage Mutant Ninja Peas", "all peas jumping", Achievement.Times.Multiple, Achievement.ProgressType.Accumulation, Achievement.Mood.Happy, Achievement.PeaProgress.From()),
                    new Achievement(Achievement.Identifier.AllDeath, "The Ninja Cementary", "all peas dead", Achievement.Times.Multiple, Achievement.ProgressType.Accumulation, Achievement.Mood.Hurt,  Achievement.PeaProgress.From()),
                    new Achievement(Achievement.Identifier.AllTrapped, "Hapiness not allowed", "all peas trapped", Achievement.Times.Multiple, Achievement.ProgressType.Accumulation,Achievement.Mood.Hurt,  Achievement.PeaProgress.From()),

                     new Achievement(Achievement.Identifier.SessionFinished, "Feeling like a Ninja", "Finished Session", Achievement.Times.SingleSession, Achievement.ProgressType.Accumulation, Achievement.Mood.Happy, Achievement.CountProgress.From(0)),
                    

                     // Act as stats. Yield achievements from progress
                    new Achievement(Achievement.Identifier.SessionCompletionCount, "Stat - Completion Count", "Number of Completed flags", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionDeathCount, "Stat - Death Count", "Number of peas that died", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionJumpBounce, "Stat - Bounces", "Number of objects hit by peas", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionJumpBounceType, "Stat - Bounces Type", "Objects hit by peas for each type", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionJumpCount, "Stat - Jumps", "Number of Jumps", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionJumpPoints, "Stat- Points", "Number of Points", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionJumpTime, "Stat - Jump Time", "Number of Seconds of peas as ninja", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionTime, "Stat - Time", "Number of minutes passed", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                    new Achievement(Achievement.Identifier.SessionTrapCount, "Stat - Trap Count", "Number of peas that got trapped", Achievement.Times.SingleSession, Achievement.ProgressType.AccumulationStack, Achievement.Mood.None, Achievement.CountProgress.From(0)),
                
                };

                return achievements;
            }
        }
    }
}
