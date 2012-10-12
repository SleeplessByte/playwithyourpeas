using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayWithYourPeas.Data;

namespace PlayWithYourPeas.Logic
{
    /// <summary>
    /// 
    /// </summary>
    internal class JumpEventArgs : EventArgs
    {
        public DataJumpSpot Spot { get; protected set; }
        public DataPea Pea { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="pea"></param>
        public JumpEventArgs(DataJumpSpot spot, DataPea pea)
            : base()
        {
            this.Spot = spot;
            this.Pea = pea;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class BlockStateArgs : EventArgs {
    
        public DataBlock Block { get; protected set; }
        public BlockState State { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <param name="state"></param>
        public BlockStateArgs(DataBlock block, BlockState state) : base() 
        {
            this.Block = block;
            this.State = state;
        }
    }

    internal delegate void JumpEventHandler(JumpEventArgs args);
    internal delegate void AchievementCompletedHandler(Achievement e);

    internal delegate void BlockStateChangedHandler(BlockStateArgs args);
    internal delegate void OnDataGridChangedHandler(DataGrid grid, BlockStateArgs args);
}
