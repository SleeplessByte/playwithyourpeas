using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Services
{
    public class MessageEventArgs : EventArgs
    {
        public String Message { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MessageEventArgs(String message)
            : base()
        {
            this.Message = message;
        }
    }
}
