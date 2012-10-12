using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PlayWithYourPeas.Engine.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class PositionFocus : IFocusable
    {
        public Vector3 Position
        {
            get;
            private set;
        }

        public PositionFocus(Vector3 focus)
        {
            this.Position = focus;
        }
    }
}
