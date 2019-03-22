using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EnchantedForest.Environment;
using Action = EnchantedForest.Agent.Action;
using Entity = EnchantedForest.Environment.Entity;
using Probabilite = EnchantedForest.Agent.Probabilite;

namespace EnchantedForest.Agent
{
    public class Agent
    {
        private CellSensor CellSensor;
        private HashSet<int> Available;

        private Dictionary<Action, Effector> Effectors { get; }
        private Dictionary<int, Dictionary<Entity, double>> Probs;
        private HashSet<int> AlreadyVisited { get; set; }

        private Forest Environment { get; }

        private Map Beliefs { get; set; }

        private int Performance { get; set; }
        private Queue<Action> Intents { get; set; }

        private List<Tuple<int, double>> Memory { get; set; }
        private int ActionDone { get; set; }

        private int MyPos => Environment.Map.AgentPos;

        private Graph Graph { get; set; }

        public Agent(Forest environment)
        {
            Environment = environment;
            CellSensor = new CellSensor();
            EffectorFactory.Forest = environment;
            Effectors = EffectorFactory.GetEffectors();
            AlreadyVisited = new HashSet<int>();
            ResetAgent();
        }

        private void ResetAgent()
        {
            Graph = new Graph();
            Intents = new Queue<Action>();
            ActionDone = 0;
            Available = new HashSet<int>();
            AlreadyVisited = new HashSet<int>();
            Probs = new Dictionary<int, Dictionary<Entity, double>>();
            for (int i = 0; i < Environment.Map.Size; i++)
            {
                Probs.Add(i, new Dictionary<Entity, double>());
                if (i == MyPos)
                {
                    Probs[i].Add(Entity.Portal, 0);
                    Probs[i].Add(Entity.Pit, 0);
                    Probs[i].Add(Entity.Monster, 0);
                    Probs[i].Add(Entity.Nothing, 1);
                }
                else
                {
                    var portal = (double) 1 / (Environment.Map.Size - 1);
                    var pit = 0.15;
                    var monster = 0.15;
                    var nothing = 1 - portal - pit - monster;
                    Probs[i].Add(Entity.Portal, portal);
                    Probs[i].Add(Entity.Pit, pit);
                    Probs[i].Add(Entity.Monster, monster);
                    Probs[i].Add(Entity.Nothing, nothing);
                }
            }
        }

        private void UpdateEntity(Entity entity, int cell, double proba)
        {
            Probs[cell][entity] = proba;
            UpdatePortal(cell);
        }

        private void UpdatePortal(int cell)
        {
            Probs[cell][Entity.Portal] =
                1 - Probs[cell][Entity.Monster] - Probs[cell][Entity.Pit] - Probs[cell][Entity.Nothing];
        }

        public void Run()
        {
            while (true)
            {
                if (IsGoalReached())
                {
                    break;
                }

                Step();

                Thread.Sleep(900);
            }
        }

        private bool IsGoalReached()
        {
            return false;
        }

        private void Step()
        {
//            Beliefs = RoomSensor.Observe(Environment);
//            Performance = PerformanceSensor.Observe(Environment);
            Entity observe = CellSensor.Observe(Environment);


            Infere(observe);

            if (Intents.Any())
            {
                var intent = Intents.Dequeue();

                var effector = Effectors[intent];
                var done = effector.DoIt();

                if (intent.Equals(Action.Leave) && done)
                {
                    ResetAgent();
                }
            }
            else
            {
                PlanIntents(observe);
            }
        }

        private void Infere(Entity observe)
        {
            if (AlreadyVisited.Contains(MyPos))
            {
                return;
            }

            AlreadyVisited.Add(MyPos);

            var surrounding = Environment.Map.GetSurroundingCells(MyPos).Where(child => !AlreadyVisited.Contains(child))
                .ToHashSet();
            foreach (var child in surrounding)
            {
                Graph.AddEdge(MyPos, child);
            }


            Graph.Emancipate(MyPos);
            UpdateSelf(observe);

            Graph.AddCluster(surrounding);
            PropagateInfoToNewNodes(observe, surrounding);
            Graph.RemoveNode(MyPos);
        }

        
        private void PropagateInfoToNewNodes(Entity entity, HashSet<int> surrounding)
        {
            if (entity.HasFlag(Entity.Poop))
            {
                Forward(Entity.Monster, 1, new HashSet<int>(), surrounding);
            }

            if (entity.HasFlag(Entity.Cloud))
            {
                Forward(Entity.Pit, 1, new HashSet<int>(), surrounding);
            }
        }
        
        private void UpdateSelf(Entity observe)
        {
            bool monster = observe.HasFlag(Entity.Monster);
            bool pit = observe.HasFlag(Entity.Pit);
            bool hasNothing = (!monster || !pit) && !observe.HasFlag(Entity.Portal);

            UpdateEntity(Entity.Monster, MyPos, monster ? 1 : 0);
            UpdateEntity(Entity.Pit, MyPos, pit ? 1 : 0);

            if (hasNothing)
            {
               UpdateEntity(Entity.Nothing, MyPos, 1);
            }
        }

        private void Forward(Entity entity, int value, HashSet<int> alreadyVisited, HashSet<int> cluster)
        {
            int sum = 0;
            var cellCount = new Dictionary<int, int>();
            foreach (var elem in cluster)
            {
                if (alreadyVisited.Contains(elem))
                {
                    continue;
                }

                int n = Graph.CountClusters(elem);
                sum += n;
                cellCount.Add(elem, n);
            }

            foreach (var elem in cluster)
            {
                if (alreadyVisited.Contains(elem))
                {
                    continue;
                }

                double proba = (double) cellCount[elem] / sum;
                proba *= value;
                UpdateEntity(entity, elem, proba);
                alreadyVisited.Add(elem);

                foreach (var clust in Graph.GetClustersFor(elem))
                {
                    Forward(entity, value, alreadyVisited, clust);
                }
            }
        }

        private void PlanIntents(Entity observe)
        {
            if (observe.HasFlag(Entity.Portal))
            {
                Intents = new Queue<Action>();
                Intents.Enqueue(Action.Leave);
                return;
            }

            var treshold = 5;
            double maxWin = -1;
            var maxI = 0;
            var throwed = false;

            Memory = new List<Tuple<int, double>>();
            var theoreticalOfBest = MyPos;

            var surroundingsNotVisited = Environment.Map.GetSurroundingCells(MyPos)
                .Where(cell => !AlreadyVisited.Contains(cell));

            foreach (var surrounding in surroundingsNotVisited)
            {
                Available.Add(surrounding);
            }

            Available.Remove(MyPos);
            AlreadyVisited.Add(MyPos);

            var intentOfBest = new Queue<Action>();
            var surroundings = Environment.Map.GetSurroundingCells(MyPos).ToList();

            foreach (var cellAvailable in Available)
            {
                var theoretical = MyPos;

                var mem = new Queue<Action>();


                if (!surroundings.Contains(cellAvailable))
                {
                    foreach (var intent in Intents)
                    {
                        mem.Enqueue(intent);
                    }

                    ShortestPath(cellAvailable);
                    theoretical = ComputeTheoretical();
                }

                if (Probs[cellAvailable][Entity.Monster] > treshold)
                {
                    // todo Irindul March 21, 2019 : check if working properly !
                    ThrowRock(theoretical, cellAvailable);
                    throwed = true;
                }

                if (!(Probs[cellAvailable][Entity.Portal] > maxWin))
                {
                    RollbackThrow();
                    Intents = mem;
                    throwed = false;
                    continue;
                }

                maxWin = Probs[cellAvailable][Entity.Portal];
                maxI = cellAvailable;
                intentOfBest = Intents;
                theoreticalOfBest = theoretical;
                Intents = new Queue<Action>();
            }

            Intents = intentOfBest;
            if (throwed)
            {
                Intents.Enqueue(GetThrowed(theoreticalOfBest, maxI));
            }

            Intents.Enqueue(MoveToward(theoreticalOfBest, maxI));
        }

        private int ComputeTheoretical()
        {
            var theoretical = MyPos;
            foreach (var intent in Intents)
            {
                switch (intent)
                {
                    case Action.Up:
                        theoretical = Environment.Map.GetUpFrom(theoretical);
                        break;
                    case Action.Down:
                        theoretical = Environment.Map.GetDownFrom(theoretical);
                        break;
                    case Action.Left:
                        theoretical = Environment.Map.GetLeftFrom(theoretical);
                        break;
                    case Action.Right:
                        theoretical = Environment.Map.GetRightFrom(theoretical);
                        break;
                }
            }

            return theoretical;
        }

        private void ShortestPath(int cell)
        {
            var cells = new Queue<int>();
            if (!ShortestPath(cells, new HashSet<int>(), MyPos, cell)) return;

            var dest = cells.Dequeue();

            while (cells.Any())
            {
                var prev = cells.Dequeue();
                var action = MoveToward(prev, dest);
                Intents.Enqueue(action);
                dest = prev;
            }
        }

        private bool ShortestPath(Queue<int> cells, ISet<int> explored, int current, int end)
        {
            explored.Add(current);
            var surroundings = Environment.Map.GetSurroundingCells(current).ToList();

            if (surroundings.Contains(end))
            {
                cells.Enqueue(current);
                explored.Remove(current);
                return true;
            }

            var children = surroundings.Where(cell => AlreadyVisited.Contains(cell) && !explored.Contains(cell));

            if (children.Select(child => ShortestPath(cells, explored, child, end)).Any(path => path))
            {
                cells.Enqueue(current);
                explored.Remove(current);
                return true;
            }

            explored.Remove(current);
            return false;
        }

        private Action MoveToward(int src, int dest)
        {
            var up = Environment.Map.GetUpFrom(src);
            var down = Environment.Map.GetDownFrom(src);
            var left = Environment.Map.GetLeftFrom(src);
            var right = Environment.Map.GetRightFrom(src);

            if (up == dest)
            {
                return Action.Up;
            }

            if (down == dest)
            {
                return Action.Down;
            }

            if (left == dest)
            {
                return Action.Left;
            }

            if (right == dest)
            {
                return Action.Right;
            }

            return Action.Idle;
        }

        private Action GetThrowed(int src, int dest)
        {
            var up = Environment.Map.GetUpFrom(src);
            var down = Environment.Map.GetDownFrom(src);
            var left = Environment.Map.GetLeftFrom(src);

            if (up == dest)
            {
                return Action.ThrowUp;
            }

            if (down == dest)
            {
                return Action.ThrowDown;
            }

            return left == dest ? Action.ThrowLeft : Action.ThrowRight;
        }

        private void RollbackThrow()
        {
            Environment.RollBackRipple();
            foreach (var (key, oldValue) in Memory)
            {
                Probs[key][Entity.Monster] = oldValue;
            }
        }

        private void ThrowRock(int src, int cellAvailable)
        {
            Action thrower = GetThrowed(src, cellAvailable);
            var target = cellAvailable;

            Environment.HandleThrow(thrower);
            Memory.Add(new Tuple<int, double>(target, Probs[target][Entity.Monster]));
            Probs[target][Entity.Monster] = 0;
        }
    }
}