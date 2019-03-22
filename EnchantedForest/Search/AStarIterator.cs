using System;
using System.Collections.Generic;
using EnchantedForest.Environment;
using Action = EnchantedForest.Agent.Action;

namespace EnchantedForest.Search
{
    public class AStarIterator : IIterator<Tree.Node>
    {
        private Tree.Node Current { get; set; }
        private PriorityQueue<Tree.Node> Frontier { get; }
        private PriorityQueue<Tree.Node> Explored { get; }

        private HashSet<int> Visited { get; }

        private IEnvironment Environment;
        
        private State Initial { get; }

        public AStarIterator(Tree tree, IEnvironment environment)
        {
            Environment = environment;
            Frontier = new PriorityQueue<Tree.Node>();
            Frontier.Enqueue(tree.Root, 0);
            Explored = new PriorityQueue<Tree.Node>();
            Visited = new HashSet<int>();
            Initial = tree.Root.State;
            Visited.Add(Initial.Map.AgentPos);
            Expand();
        }
        public bool HasNext()
        {
            return Frontier.Any();
        }

        public Tree.Node GetNext()
        {
            if (!HasNext())
            {
                throw new InvalidOperationException("No element in queue");
            }

            Current = Frontier.Dequeue();
            return Current;
        }
        
        public void Expand()
        {
            
            while (HasNext())
            {
                
                GetNext();
                CreateAndEnqueueNodes(Current, Environment.GetSuccessors(Current.State));
                
                if (!Current.IsLeaf()) 
                    continue;
                
                if (IsThrow(Current.State.Action))
                {
                    continue;
                }

                if (Initial.Map.AgentPos == Current.State.Map.AgentPos)
                {
                    continue;
                }
                
                Explored.Enqueue(Current, GetPriority(Current));

            }
        }

        private bool IsThrow(Action stateAction)
        {
            switch (stateAction)
            {
                case Action.Idle:
                case Action.Left:
                case Action.Right:
                case Action.Up:
                case Action.Leave:
                case Action.Down:
                    return false;
                case Action.ThrowLeft:
                case Action.ThrowRight:
                case Action.ThrowUp:
                case Action.ThrowDown:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateAction), stateAction, null);
            }
        }

        private void CreateAndEnqueueNodes(Tree.Node parent, List<State> nextPossibleStates)
        {
            nextPossibleStates.ForEach(state =>
            {
                if (!IsThrow(state.Action))
                {
                    if (Visited.Contains(state.Forest.Map.AgentPos))
                    {
                        return;
                    }
                }
                var node = new Tree.Node(parent, state, Environment);
                parent.AddChild(node);
                var prio = GetPriority(node);

                if(!CheckLoop(node, new HashSet<int>()))
                {
                    Frontier.Enqueue(node, prio);    
                }
            });
        }

        private bool CheckLoop(Tree.Node node, HashSet<int> visited)
        {
            if (node.Parent == null)
            {
                return false;
            }

            if (visited.Contains(node.State.Map.AgentPos))
            {
                return true;
            }
            

            if (!IsThrow(node.State.Action))
            {
                visited.Add(node.State.Map.AgentPos);    
            }
            
            return CheckLoop(node.Parent, visited);

        }
        
        

        private double GetPriority(Tree.Node current)
        {
            var h = Environment.GetHeuristicForState(current.State);
            var g = current.Cost;
            return h;
        }

        public Tree.Node GetBestExplored()
        {
            return Explored.Dequeue();
        }
    }
}