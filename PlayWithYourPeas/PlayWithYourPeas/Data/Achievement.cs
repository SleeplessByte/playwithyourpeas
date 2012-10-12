using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Data
{
    internal class Achievement
    {
        /// <summary>
        /// 
        /// </summary>
        public Identifier Id { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public String Name { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public String Description { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCompleted { get { return this.CompletionTime.HasValue; } set { if (value) { this.CompletionTime = DateTime.Now; } } }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<DateTime> CompletionTime { get; protected set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Times Scope { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public ProgressType Type { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Object Progress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Object InitialValue { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Achievement(Identifier id, String name, String description, 
            Times times, ProgressType type, Object init = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;

            this.Scope = times;
            this.Type = type;
            this.InitialValue = init;
            this.Progress = init;
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
            return Id.GetHashCode() * 
                (this.Type != Achievement.ProgressType.AccumulationStack ? 1 : 71 ^ this.InitialValue.GetHashCode());
        }

        /// <summary>
        /// 
        /// </summary>
        internal enum Identifier
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

        internal enum Times
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
        /// 
        /// </summary>
        internal enum ProgressType
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
        /// <returns></returns>
        internal Achievement GenerateNew()
        {
            return new Achievement(this.Id, this.Name, this.Description, this.Scope, this.Type, this.InitialValue);
        }
    }
}
