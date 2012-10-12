using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Data class for a jump spot (if attached to source, generates visuals in SpriteBlock
    /// and generates repercussions for block placement).
    /// </summary>
    internal class DataJumpSpot
    {
        /// <summary>
        /// Number of peas needed for completion
        /// </summary>
        public const Single MaxCompletion = 5f;

        protected HashSet<DataPea> _hasCompleted;
        protected HashSet<DataPea> _hasFailed;
        protected HashSet<DataPea> _hasStarted;

        /// <summary>
        /// Completion of this jumpspot
        /// </summary>
        public Single Completion { get { return _hasCompleted.Count / MaxCompletion; } }

        /// <summary>
        /// Percentage failed
        /// </summary>
        public Single FailFloat { get { return _hasFailed.Count / MaxCompletion;  } }

        /// <summary>
        /// Block where this jump spot resides
        /// </summary>
        public DataBlock Source { get; protected set; }

        /// <summary>
        /// Placement (left or right)
        /// </summary>
        public Location Placement { get; set; }

        /// <summary>
        /// Creates a new jump spot
        /// </summary>
        /// <param name="source"></param>
        public DataJumpSpot(DataBlock source)
        {
            Source = source;

            _hasCompleted = new HashSet<DataPea>();
            _hasStarted = new HashSet<DataPea>();
            _hasFailed  = new HashSet<DataPea>();

            source.CreateJumpSpot(this);
        }

        /// <summary>
        /// Has jumped this for a pea
        /// </summary>
        /// <param name="pea"></param>
        /// <returns></returns>
        public Boolean HasJumped(DataPea pea)
        {
            return _hasStarted.Contains(pea);
        }

        /// <summary>
        /// Starts jumping
        /// </summary>
        public void Start(DataPea pea)
        {
            _hasStarted.Add(pea);

            var getOnSource = (Placement == Location.Left ? Source.JumpLeft : Source.JumpRight);

            // Register
            if (getOnSource == null)
                Source.BindJumpSpot(this);
            else if (getOnSource != this)
                getOnSource.Start(pea);
        }

        /// <summary>
        /// Complete this for a pea
        /// </summary>
        /// <param name="pea"></param>
        public void Complete(DataPea pea)
        {
            _hasCompleted.Add(pea);

            var getOnSource = (Placement == Location.Left ? Source.JumpLeft : Source.JumpRight);

            // Complete on other and kill this (shouldnt be needed)
            if (getOnSource != this)
                getOnSource.Complete(pea);

            if (_hasCompleted.Count == MaxCompletion)
                getOnSource.Source.CompleteJumpSpot(getOnSource);
        }

        /// <summary>
        /// Fail the spot for this pea
        /// </summary>
        /// <param name="dataPea"></param>
        internal void Fail(DataPea pea)
        {
            _hasFailed.Add(pea);

            var getOnSource = (Placement == Location.Left ? Source.JumpLeft : Source.JumpRight);

            // Complete on other and kill this (shouldnt be needed)
            if (getOnSource != this)
                getOnSource.Fail(pea);
        }

        /// <summary>
        /// Placement
        /// </summary>
        public enum Location
        {
            None = 0,

            Left, Right
        }
    }
}
