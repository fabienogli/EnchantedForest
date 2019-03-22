using System;
using System.Collections.Generic;
using System.Linq;

namespace EnchantedForest.Agent
{
    public class Graph
    {
        private HashSet<int> Nodes = new HashSet<int>();
        private HashSet<Tuple<int, int>> Edges = new HashSet<Tuple<int, int>>();
        private HashSet<HashSet<int>> Clusters = new HashSet<HashSet<int>>();

        public void AddNode(int i)
        {
            Nodes.Add(i);
        }

        public void AddEdge(int parent, int child)
        {
            AddNode(parent);
            AddNode(child);
            Edges.Add(new Tuple<int, int>(parent, child));
        }

        public void Emancipate(int child)
        {
            var remaining = new HashSet<Tuple<int, int>>();
            foreach (var edge in Edges)
            {
                if(edge.Item2 == child)
                {
                    continue;
                }

                remaining.Add(edge);
            }

            Edges = remaining;
        }


        public int DeepCountChildren(int node)
        {
            return DeepCountChildren(node, new HashSet<int>());
        }
        
        public int DeepCountChildren(int node, HashSet<int> explored)
        {
            explored.Add(node);
            int sum = 0;
            var children = Edges.Where(parent => parent.Item1 == node).ToList();
            
            foreach (var (parent, child) in children)
            {
                if (explored.Contains(child))
                {
                    continue;
                } 
                if (parent == node)
                {
                    sum += DeepCountChildren(child, explored);
                }
            }

            explored.Remove(node);
            sum += children.Count;
            return sum;
        }

        public int CountClusters(int node)
        {
            return Clusters.Count(cluster => cluster.Contains(node));
        }

        public void AddCluster(HashSet<int> nodes)
        {
            Clusters.Add(nodes);
            foreach (var parent in nodes)
            {
                foreach (var child in nodes)
                {
                    if (parent == child)
                    {
                        continue;
                    }
                    
                    AddEdge(parent, child);
                }
            }
        }

        public void RemoveNode(int node)
        {
            Edges.RemoveWhere(tuple => tuple.Item1 == node || tuple.Item2 == node);
            
            foreach (var cluster in Clusters)
            {
                cluster.RemoveWhere(cell => cell == node);
            }

            Nodes.Remove(node);
        }

        public IEnumerable<HashSet<int>> GetClustersFor(int node)
        {
            var clustersOfNode = new HashSet<HashSet<int>>();

            foreach (var cluster in Clusters)
            {
                if (cluster.Contains(node))
                {
                    clustersOfNode.Add(cluster);
                }
            }

            return clustersOfNode;
        }

        public HashSet<int> GetChildren(int node)
        {
            var children = new HashSet<int>();
            foreach (var (parent, child) in Edges)
            {
                if (parent == node)
                {
                    children.Add(child);
                }
            }

            return children;
        }
    }
}