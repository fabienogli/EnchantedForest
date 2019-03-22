using System.Collections.Generic;
using EnchantedForest.Environment;

namespace EnchantedForest.Search
{
    public class Tree
    {
        public class Node
        {
            private List<Node> Children { get; } 
            public Node Parent { get; }
            public State State { get; }
            
            public int Depth { get; }
            
            public int Cost { get; }
            
            private IEnvironment Environment { get; }

            public Node(State state, IEnvironment env)
            {
                Depth = 0;
                State = state;
                Environment = env;
                Cost = Environment.GetCostForAction(state.Action);
                Children = new List<Node>();
                
            }

            public Node(Node parent, State state, IEnvironment env) : this(state, env)
            {
                Parent = parent;
                Cost = parent.Cost + Environment.GetCostForAction(state.Action);
                Depth = parent.Depth + 1;
            }
           
            public void AddChild(Node child)
            {
                Children.Add(child);
            }
        }

        public Node Root { get; }

        public Tree(Node root)
        {
            Root = root;
        }
            
    }
}