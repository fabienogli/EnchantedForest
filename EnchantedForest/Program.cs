using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnchantedForest.Agent;
using EnchantedForest.Environment;
using EnchantedForest.View;

namespace EnchantedForest
{
    internal class Program
    {
        private static Dictionary<int, double> probas = new Dictionary<int, double>();
        private static Map Map = new Map(3*3);
        private static Graph Graph = new Graph();
        private static HashSet<int> AlreadyVisited = new HashSet<int>();
        public static void Main(string[] args)
        {
            Map.AddEntityAtPos(Entity.Poop,1);
            Map.AddEntityAtPos(Entity.Poop,3);
            Map.AddEntityAtPos(Entity.Poop, 5);
            Map.AddEntityAtPos(Entity.Poop, 7);
            Map.AddEntityAtPos(Entity.Monster, 4);
            Run();
        }

        private static void Run()
        {
            Infere(Map.GetEntityAt(3), 3);
            Print();
            Infere(Map.GetEntityAt(0), 0);
            Print();
            Infere(Map.GetEntityAt(1), 1);
            Print();
            Infere(Map.GetEntityAt(4), 4);
            Print();
            Infere(Map.GetEntityAt(5), 5);
            Print();
        }


        private static void Infere(Entity entity, int pos)
        {
            if (AlreadyVisited.Contains(pos))
            {
                return;
            }

            AlreadyVisited.Add(pos);
            
            var surrouding = GetSurrounding(pos).Where(child => !AlreadyVisited.Contains(child)).ToHashSet();
            foreach (var child in surrouding)
            {
                Graph.AddEdge(pos, child);
            }
            
            
            Graph.Emancipate(pos);
            UpdateSelf(entity, pos);
            Graph.RemoveFromEachCluster(pos);
            Graph.AddCluster(surrouding);

            
            PropagateInfoToNewNodes(entity, pos);
            
        }

        private static void UpdateSelf(Entity entity, int pos)
        {
            if (!entity.HasFlag(Entity.Monster))
            {
                UpdateProbaMonster(Graph, new HashSet<int>(), pos, 0);
            }
            else
            {
                UpdateProbaMonster(Graph, new HashSet<int>(), pos, 1);
            }
        }
        private static void PropagateInfoToNewNodes(Entity entity, int pos)
        {
            if (entity.HasFlag(Entity.Poop))
            {
                PropagateProbaMonster(Graph, new HashSet<int>(), pos);
            }
        }
        private static HashSet<int> GetSurrounding(int pos)
        {
            return Map.GetSurroundingCells(pos).ToHashSet();
        }

        private static void UpdateProbaMonster(Graph graph, HashSet<int> alreadyVisited, int cell, double proba)
        {
            alreadyVisited.Add(cell);
            if (!probas.ContainsKey(cell))
            {
                probas.Add(cell, 0.15);
            }

            probas[cell] = proba;

            PropagateProbaMonster(graph, alreadyVisited, cell);
        }

        private static void PropagateProbaMonster(Graph graph, HashSet<int> alreadyVisited, int cell)
        {
            var children = graph.GetChildren(cell).Where(child => !alreadyVisited.Contains(child)).ToList();
            var childCount = new Dictionary<int, int>();
            var total = 0;
            
            foreach (var child in children)
            {
                int count = graph.CountClusters(child);
                childCount.Add(child, count);
                total += count;
            }

            foreach (var child in children)
            {
                var childProba = (double) childCount[child] / total;
                UpdateProbaMonster(graph, alreadyVisited, child, childProba);
            }
        }

        private static void Print()
        {
            for (int i = 0; i < Map.Size; i++)
            {
                if (probas.ContainsKey(i))
                {
                    Console.WriteLine($"proba[{i}]={probas[i]}");
                }
            }
            Console.WriteLine();
        }
    }
    
  
}