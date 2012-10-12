using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Logic.AStar
{
	public class SolverResultEventArgs<T> : EventArgs
	{
		public List<Node<T>> Result { get; set; }
	}
}
