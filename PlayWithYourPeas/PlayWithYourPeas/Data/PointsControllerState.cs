using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// A PointController State keeps track of the points during a jump. It holds the
    /// exact number of hits and makes sure that a pea is marked as traped when it
    /// it is trapped. Contains the formula for scoring.
    /// </summary>
    internal class PointsControllerState
    {

        /// <summary>
        /// Pea
        /// </summary>
        public DataPea Pea { get; protected set; }


        /// <summary>
        /// Hit list
        /// </summary>
        public Dictionary<BlockType, Int32> Times { get; protected set; }

        /// <summary>
        /// Last few hits
        /// </summary>
        public Queue<DataBlock> RecentHits { get; protected set; }

        /// <summary>
        /// Points for this jump
        /// </summary>
        public Int32 Points
        {
            get
            {
                return CalculatePoints(Times);
            }
        }

        /// <summary>
        /// Calculates the points according to a hitlist
        /// </summary>
        /// <param name="Times"></param>
        /// <returns></returns>
        public static Int32 CalculatePoints(Dictionary<BlockType, Int32> Times)
        {
            return (Int32)Math.Ceiling(Times.Values.Sum() * (Times[BlockType.Spring] * BlockScore(BlockType.Spring) +
                   (Times[BlockType.LeftRamp] + Times[BlockType.RightRamp] + Times[BlockType.Normal]) * BlockScore(BlockType.Normal) +
                   Times[BlockType.Gel] * BlockScore(BlockType.Gel) + 1000));
        }

        /// <summary>
        /// Creates a new state
        /// </summary>
        /// <param name="pea"></param>
        public PointsControllerState(DataPea pea)
        {
            this.Times = new Dictionary<BlockType, Int32>() { 
                { BlockType.Normal, 0 }, 
                { BlockType.LeftRamp, 0 },
                { BlockType.RightRamp, 0 },
                { BlockType.Spring, 0},
                { BlockType.Gel, 0},
                { BlockType.None, 0}, 
            };
            this.Pea = pea;
            this.RecentHits = new Queue<DataBlock>();
        }

        /// <summary>
        /// Hits a block
        /// </summary>
        /// <param name="block"></param>
        public Int32 Hit(DataBlock block, Vector2 speed)
        {
            Int32 saved;
            if (!this.Times.TryGetValue(block == null ? BlockType.None : block.BlockType, out saved))
                saved = 0;

            // Only register if hitting hard enough
            if (speed.Length() > 1)
            {
                this.Times[block == null ? BlockType.None : block.BlockType] = ++saved;

                if (block != null)
                    RecentHit(block);
            }

            return saved;
        }

        /// <summary>
        /// Hits a block (mark as recent)
        /// </summary>
        /// <param name="block"></param>
        private void RecentHit(DataBlock block)
        {
            this.RecentHits.Enqueue(block);
            if (this.RecentHits.Count > 20)
                this.RecentHits.Dequeue();
        }

        /// <summary>
        /// Increase Happiness by processing a collision
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="block"></param>
        internal Boolean IncreaseHappiness(Vector2 speed, DataBlock block)
        {
            var times = Hit(block, speed);
            var recent = this.RecentHits.Count(b => b == block);
            //var newmultiplier

            //System.Diagnostics.Debug.WriteLine("speed: {0}, recent = {1}, times = {2}", speed, recent, times);

            return recent < 10 || Math.Abs(speed.Y) < 0.5f;
        } 

        /// <summary>
        /// Gets the score for a block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private static Single BlockScore(DataBlock block)
        {
            return BlockScore(block.BlockType);
        }

        /// <summary>
        /// Gets the score for a blocktype
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Single BlockScore(BlockType type)
        {
            switch (type)
            {
                case BlockType.Gel:
                    return 175;
                case BlockType.LeftRamp:
                    return 100;
                case BlockType.RightRamp:
                    return 100;
                case BlockType.Spring:
                    return 500;
                case BlockType.Normal:
                    return 250;
                default:
                    return 125;
            }
        }
    }
}
