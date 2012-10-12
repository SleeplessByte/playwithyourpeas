using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Drawing
{
    interface ITransitionable
    {
        Single TransitionPosition { get; set; }
        TimeSpan TransitionOnTime { get; set; }
        TimeSpan TransitionOffTime { get; set; }
        SByte TransitionDirection { get; }

        void OffTransition();
        void OnTransition();
        void ToggleTransition();
    }
}
