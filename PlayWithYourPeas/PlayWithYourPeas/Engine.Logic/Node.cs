using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Logic.AStar
{
	public class Node<T> //where T : IEquatable<T>
	{
        public Func<Node<T>, List<Node<T>>> Neighbors { get; set; }
		public Func<Node<T>, int> GetHeuristic { get; set; }
		public Func<Node<T>, int> GetCost { get; set; }
		public Func<Node<T>, Node<T>, bool> IsEqualFunc { get; set; }
		public NodeState NodeState { get; set; }
		public T Value { get; set; }
		public T GoalValue { get; set; }
		public Node<T> PreviousNode { get; set; }

		public Node()
		{
			NodeState = NodeState.Undetermined;
            Neighbors = null;
		}

		public int GetTotalCost()
		{
			int totalCost = GetHeuristic(this) + GetCost(this);

			return totalCost;
		}

		public bool IsGoalValue()
		{
			return (Value as IComparable).CompareTo(GoalValue as IComparable) == 0;
		}

		public bool IsEqual(Node<T> otherNode)
		{
			return IsEqualFunc(this, otherNode);
		}
	}
}
