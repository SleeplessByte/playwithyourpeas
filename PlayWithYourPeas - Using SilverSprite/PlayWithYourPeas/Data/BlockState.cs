using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Block (placement) state
    /// </summary>
    public enum BlockState
    {
        None = 0,

        /// <summary>
        /// Block is placing (just placed)
        /// </summary>
        Placing = 1,

        /// <summary>
        /// Block is placed
        /// </summary>
        Placed = 2,

        /// <summary>
        /// Block is removing (just deleted)
        /// </summary>
        Removing = 3,

        /// <summary>
        /// Block is removed
        /// </summary>
        Removed = 4,
    }
}
