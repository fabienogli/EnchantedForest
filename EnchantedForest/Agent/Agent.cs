using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnchantedForest.Environment;
using Entity = EnchantedForest.Environment.Entity;

namespace EnchantedForest.Agent
{
    public class Agent
    {
        private Forest Environment { get; }
        private CellSensor CellSensor { get; }
        private Dictionary<Action, Effector> Effectors { get; }
        private HashSet<int> Available { get; set; }
        private HashSet<int> AlreadyVisited { get; set; }
        private Queue<Action> Intents { get; set; }
        private List<Tuple<int, double>> Memory { get; set; }
        private int MyPos => Environment.Map.AgentPos;
        private ProbabilityMatrix Proba { get; set; }
        
        private IInference Inferer { get; set; }
        
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
            Inferer = new GraphInference(new ProbabilityMatrix(Environment.Map.Size), Environment.Map);
            Intents = new Queue<Action>();
            Available = new HashSet<int>();
            AlreadyVisited = new HashSet<int>();
            Proba = new ProbabilityMatrix(Environment.Map.Size);
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
//            Performance = PerformanceSensor.Observe(Environment);
            var observe = CellSensor.Observe(Environment);

            Inferer.Infere(observe);

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

                if (Proba.GetProbaFor(cellAvailable, Entity.Monster) > treshold)
                {
                    // todo Irindul March 21, 2019 : check if working properly !
                    ThrowRock(theoretical, cellAvailable);
                    throwed = true;
                }

                if (!(Proba.GetProbaFor(cellAvailable, Entity.Portal) > maxWin))
                {
                    RollbackThrow();
                    Intents = mem;
                    throwed = false;
                    continue;
                }

                maxWin = Proba.GetProbaFor(cellAvailable, Entity.Portal);
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
                Proba.UpdateEntity(Entity.Monster, key, oldValue);
            }
        }

        private void ThrowRock(int src, int cellAvailable)
        {
            Action thrower = GetThrowed(src, cellAvailable);
            var target = cellAvailable;

            Environment.HandleThrow(thrower);
            Memory.Add(new Tuple<int, double>(target, Proba.GetProbaFor(target, Entity.Monster)));
            Proba.UpdateEntity(Entity.Monster, target, 0);
        }
    }
}