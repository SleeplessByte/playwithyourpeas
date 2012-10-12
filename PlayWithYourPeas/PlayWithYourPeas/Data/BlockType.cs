using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Data
{
    /// <summary>
    /// Block (data) type
    /// </summary>
    internal enum BlockType
    {
        /// <summary>
        /// Clear
        /// </summary>
        None = 0,

        /// <summary>
        /// Normal type (side walk, normal landing)
        /// </summary>
        Normal,
        
        /// <summary>
        /// Gel type (no side walk, but safe landing)
        /// </summary>
        Gel,
        
        /// <summary>
        /// Spring type (no side walk, but bounce landing)
        /// </summary>
        Spring,

        /// <summary>
        /// Ramp left bottom to right top
        /// </summary>
        LeftRamp,

        /// <summary>
        /// Ramp right bottom to left top
        /// </summary>
        RightRamp,

        /// <summary>
        /// Delete underlying type
        /// </summary>
        Delete,


    }
}
