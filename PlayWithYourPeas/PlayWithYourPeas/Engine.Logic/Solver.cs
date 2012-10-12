using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWithYourPeas.Engine.Logic.AStar
{
	public static class Solver<T> //where T : IEquatable<T>
	{
        /// <summary>
        /// Event that registers progress
        /// </summary>
		public static event EventHandler<SolverResultEventArgs<T>> SolverProgress;

        /// <summary>
        /// LINQ based solving
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="goal">End node</param>
        /// <param name="domain">Nodes</param>
        /// <param name="getNeighbors">Function to get nodes reachable from anode</param>
        /// <param name="getHeuristic">Function to get the heuristic value</param>
        /// <param name="getCost">Function to get the cost values</param>
        /// <param name="isEqual">Function for equality check between nodes</param>
        /// <param name="showProgress">Flag to push intermediate results to the SolverProgress event</param>
        /// <returns>Solved path</returns>
        public static List<Node<T>> Solve(
                Node<T> start,
                Node<T> goal,
                List<Node<T>> domain,
                // This function is normally not needed since the neighbs should be determined
                // when creating the domain.
                Func<Node<T>, List<Node<T>>> getNeighbors,
                Func<Node<T>, int> getHeuristic,
                Func<Node<T>, int> getCost,
                Func<Node<T>, Node<T>, bool> isEqual,
                bool showProgress = false
            )
        {
            Node<T> currentNode = null;
            bool success = false;
            bool searching = true;
            List<Node<T>> result = new List<Node<T>>();

            start.NodeState = NodeState.Open;	//Add the starting node to the open list

            foreach (Node<T> node in domain)
            {
                node.GetCost = getCost;
                node.GetHeuristic = getHeuristic;
                node.IsEqualFunc = isEqual;
                node.Neighbors = getNeighbors;
                node.GoalValue = goal.Value;
            }

            while (searching)
            {
                currentNode = (
                    from n in domain
                    where n.NodeState == NodeState.Open
                    orderby n.GetTotalCost() ascending
                    select n
                     ).FirstOrDefault();

                if (null == currentNode)
                {
                    success = false;
                    break;
                }

                if (isEqual(currentNode, goal))// found the goal!
                {
                    goal = currentNode;
                    success = true;
                    break;
                }

                currentNode.NodeState = NodeState.Closed;

                // open the adjacent nodes that have not been examined
                var neighborsQuery =
                    from n in currentNode.Neighbors.Invoke(currentNode)
                    where n.NodeState == NodeState.Undetermined && 
                        (!domain.Any(m => m.IsEqual(n)) || domain.Find(m => m.IsEqual(n)).NodeState == NodeState.Undetermined)
                    select n;

                neighborsQuery.ToList().ForEach(n => { 
                    n.NodeState = NodeState.Open; 
                    n.PreviousNode = currentNode;

                    if (!domain.Any(m => m.IsEqual(n))) {
                        n.GetCost = getCost;
                        n.GetHeuristic = getHeuristic;
                        n.IsEqualFunc = isEqual;
                        n.Neighbors = getNeighbors;
                        n.GoalValue = goal.Value;
                        domain.Add(n);
                    }
                });

                // if no more open nodes, stop - if we have not found 
                // it yet, we can't get to the goal state
                searching &= (
                    from n in domain
                    where n.NodeState == NodeState.Open
                    select n).Count() > 0;

                // collect the current results and inform our listeners
                if (showProgress)
                {
                    result = GetResultList(currentNode);
                    if (null != SolverProgress)
                    {
                        SolverProgress(success, new SolverResultEventArgs<T>() { Result = result });
                    }
                }
            }

            if (success)
                result = GetResultList(goal);

            if (SolverProgress != null)
            {
                SolverProgress(success, new SolverResultEventArgs<T>() { Result = result });
            }

           

            return success ? result : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topNode"></param>
        /// <returns></returns>
		private static List<Node<T>> GetResultList(Node<T> topNode)
		{
			Node<T> currentNode = topNode;
			Stack<Node<T>> result = new Stack<Node<T>>();

			// follow the resulting linked list from goal back to start
			do
			{
				result.Push(currentNode);
				currentNode = currentNode.PreviousNode;
			} while (currentNode != null);

			return result.ToList();
		}

	}
}
