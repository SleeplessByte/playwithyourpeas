using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Pea controller state is the movement state for a single pea. It is maintained  by
    /// the PeaController and used as such. See it as the data container for a controller.
    /// </summary>
    internal class PeaControllerState
    {
        protected Queue<MoveNode> _nodeList;

        /// <summary>
        /// Current destination
        /// </summary>
        public DataJumpSpot Target { get; set; }

        /// <summary>
        /// Last reached node
        /// </summary>
        public MoveNode PreviousNode { get; protected set; }

        /// <summary>
        /// Next node
        /// </summary>
        public MoveNode CurrentNode { get { return _nodeList.Peek(); } }

        /// <summary>
        /// Has a next node
        /// </summary>
        public Boolean HasRemainingPath { get { return _nodeList != null && _nodeList.Count > 0; } }

        /// <summary>
        /// Wandering node (no destination)
        /// </summary>
        public MoveNode? WanderingNode { get; protected set; }

        /// <summary>
        /// PathFind search limit
        /// </summary>
        public Single TargetLimit { get; protected set; }

        /// <summary>
        /// Creates a new state
        /// </summary>
        public PeaControllerState()
        {
            this.TargetLimit = 0;
            this.WanderingNode = null;
            _nodeList = new Queue<MoveNode>();

            this.Target = null;
        }

        /// <summary>
        /// Updates the targetlimit
        /// </summary>
        /// <param name="gameTime"></param>
        internal void UpdateTargetLimit(GameTime gameTime, Single runningSpeed)
        {
            this.TargetLimit = Math.Max(0, this.TargetLimit - (Single)gameTime.ElapsedGameTime.TotalSeconds * runningSpeed);
        }

        /// <summary>
        /// Resets the targetlimit
        /// </summary>
        /// <returns></returns>
        internal void ResetTargetLimit()
        {
            this.TargetLimit = 2;
        }

        /// <summary>
        /// Starts following a path
        /// </summary>
        /// <param name="path"></param>
        internal void StartPath(Queue<MoveNode> path)
        {
            _nodeList = path;
            this.PreviousNode = path.Peek();
        }

        /// <summary>
        /// Wanders from a to b
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void Wander(MoveNode from, MoveNode? to)
        {
            this.PreviousNode = from;
            this.WanderingNode = to;
        }

        /// <summary>
        /// Moves to next node on path
        /// </summary>
        internal void ProgressPath()
        {
            this.PreviousNode = (this.HasRemainingPath ? _nodeList.Dequeue() : this.PreviousNode);
            this.WanderingNode = (this.HasRemainingPath ? this.PreviousNode : this.WanderingNode);
        }

        /// <summary>
        /// Stops following a path
        /// </summary>
        internal void EndPath()
        {
            if (_nodeList != null)
                _nodeList.Clear();
        }
    }
}
