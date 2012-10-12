using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Services
{

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    sealed class UnusedAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        /// <summary>
        /// 
        /// </summary>
        public UnusedAttribute()
        {

        }

        public Int32 VersionExpected
        {
            get;
            set;
        }
    }
}
