using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Move node for navigating. Is the basis for movement when navigating 
    /// the environment. See PeaController for usuage
    /// </summary>
    internal struct MoveNode
    {
        /// <summary>
        /// Position (grid) of the node
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Visual position (for movement update)
        /// </summary>
        public Vector2 ScreenPosition { get { return Vector2.UnitX * (70 * Position.X + 35) + Vector2.UnitY * (48 * Position.Y + 40) + ActionOffset; } }

        /// <summary>
        /// Action offset (displacement for action type, so you can move in
        /// different positions on the same grid postion according to action)
        /// </summary>
        public Vector2 ActionOffset
        {
            get
            {
                switch (Action)
                {
                    case Type.Jump:
                    case Type.Round:
                    case Type.Climb:
                    case Type.Wander:
                        return Vector2.UnitX * (35 - 16) * (Dir == Direction.None ? 0 : (Dir == Direction.Left ? -1 : 1));
                    case Type.Ramp:
                        return (Vector2.UnitX * (35 - 1) * (Dir == Direction.None ? 0 : (Dir == Direction.Left ? 1 : -1)) + Vector2.UnitY * - 5);
                }

                return Vector2.Zero;

            }
        }
        
        /// <summary>
        /// Action of the move node (determines reachable nodes)
        /// </summary>
        public Type Action { get; set; }

        /// <summary>
        /// Direction of the move node 
        /// </summary>
        public Direction Dir { get; set; }

        /// <summary>
        /// Equality comparer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as MoveNode?;
            if (other == null)
                return false;

            return Position.X == other.Value.Position.X && 
                Position.Y == other.Value.Position.Y && 
                Action == other.Value.Action && 
                Dir == other.Value.Dir;
        }

        /// <summary>
        /// Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Position.GetHashCode() ^ 63) * (Action.GetHashCode() ^ 127) * (Dir.GetHashCode() * 255);
        }

        /// <summary>
        /// Move type
        /// </summary>
        internal enum Type
        {
            None = 0,

            /// <summary>
            /// Normal walk
            /// </summary>
            Walk = 1,

            /// <summary>
            /// Jump (Dir = none is linear, else is wall jump)
            /// </summary>
            Jump = 2,

            /// <summary>
            /// Climbing
            /// </summary>
            Climb = 3,

            /// <summary>
            /// Round the top
            /// </summary>
            Round = 4,

            /// <summary>
            /// Move on a ramp
            /// </summary>
            Ramp = 5,

            /// <summary>
            /// Unused
            /// </summary>
            Wander,
        }

        /// <summary>
        /// Can't remember why these are flags...
        /// </summary>
        [Flags]
        internal enum Direction
        {
            None = 0,

            /// <summary>
            /// 
            /// </summary>
            Left = (1 << 0),
            
            /// <summary>
            /// 
            /// </summary>
            Right = (1 << 1),
        }
    }

    
}
