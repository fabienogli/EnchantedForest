using System.Collections.Generic;
using System.Text;
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

            protected bool Equals(Node other)
            {
                return Equals(Parent, other.Parent) && Equals(State, other.State) && Depth == other.Depth && Cost == other.Cost;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Node) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Parent != null ? Parent.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (State != null ? State.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Depth;
                    hashCode = (hashCode * 397) ^ Cost;
                    return hashCode;
                }
            }

            public bool IsLeaf()
            {
                return Children.Count == 0;
            }

            public override string ToString()
            {
                return RecursiveToString();
            }

            private string RecursiveToString()
            {
                if (Parent == null)
                {
                    return State.Action.ToString();
                }

                return Parent.RecursiveToString() + "->" + State.Action;
            }
        }

        public Node Root { get; }

        public Tree(Node root)
        {
            Root = root;
        }
            
    }
}