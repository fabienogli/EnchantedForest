using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnchantedForest.Agent.Effectors;
using EnchantedForest.Environment;
using Entity = EnchantedForest.Environment.Entity;

namespace EnchantedForest.Agent
{
    public class Agent
    {
        private Forest Environment { get; }
        private DeathSensor DeathSensor;
        private CellSensor CellSensor { get; }
        private Dictionary<Action, Effector> Effectors { get; }
        private HashSet<int> Frontier { get; set; }
        private HashSet<int> AlreadyVisited { get; set; }
        private Queue<Action> Intents { get; set; }
        private ProbabilityMatrix Proba { get; set; }
        private GraphInferer Inferer { get; set; }
        private int MyPos => Environment.Map.AgentPos;
        private IEnumerable<int> Surrounding => Environment.Map.GetSurroundingCells(MyPos);

        private const double ShootingThreshold = 0.7;

        public Agent(Forest environment)
        {
            Environment = environment;
            CellSensor = new CellSensor();
            DeathSensor = new DeathSensor();
            EffectorFactory.Forest = environment;
            Effectors = EffectorFactory.GetEffectors();
            ResetAgent();
        }

        private void ResetAgent()
        {
            Inferer = new GraphInferer(new ProbabilityMatrix(Environment.Map.Size), Environment.Map);
            Intents = new Queue<Action>();
            Frontier = new HashSet<int>();
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
            bool IsDead = DeathSensor.Observe(Environment);
            
            Entity observe = CellSensor.Observe(Environment);


            Infere(observe);

            if (IsDead)
            {
                Die();
                return;
            }
            
            if (Intents.Any())
            {
                DealWithIntent();
            }
            else
            {
                PlanIntents(observe);
            }
        }

        private void Infere(Entity observe)
        {
            Inferer.MyPos = MyPos;
            Proba = Inferer.Infere(observe);
        }

        private void DealWithIntent()
        {
            var intent = Intents.Dequeue();

            var effector = Effectors[intent];
            var done = effector.DoIt();

            if (intent.Equals(Action.Leave) && done)
            {
                ResetAgent();
            }
        }
        
        private void Die()
        {
            Environment.ResetAgent();
            Intents = new Queue<Action>();
        }

        private void PlanIntents(Entity observe)
        {
            if (HandleEvidentCases(observe))
            {
                return;
            }

            double maxWin = -1;
            var maxCell = 0;
            var theoreticalOfBest = MyPos;
            var surroundingsNotVisited = Surrounding.Where(cell => !AlreadyVisited.Contains(cell));

            foreach (var surrounding in surroundingsNotVisited)
            {
                Frontier.Add(surrounding);
            }

            Frontier.Remove(MyPos);
            AlreadyVisited.Add(MyPos);

            var intentsOfBest = new Queue<Action>();
            var throwed = false;

            foreach (var cellAvailable in Frontier)
            {
                var isBestSoFar = Explore(cellAvailable, maxWin, ref throwed);

                if (!isBestSoFar)
                    continue;

                maxWin = PortalOutcome(cellAvailable);
                maxCell = cellAvailable;
                intentsOfBest = Intents;
                theoreticalOfBest = ComputeTheoretical();
                Intents = new Queue<Action>();
            }

            Intents = intentsOfBest;

            if (throwed)
            {
                Intents.Enqueue(Environment.Map.GetThrowed(theoreticalOfBest, maxCell));
            }

            Intents.Enqueue(Environment.Map.MoveToward(theoreticalOfBest, maxCell));
        }

        private bool HandleEvidentCases(Entity observe)
        {
            if (!observe.HasFlag(Entity.Portal))
                return false;

            Intents = new Queue<Action>();
            Intents.Enqueue(Action.Leave);
            return true;
        }

        private bool Explore(int cell, double bestProbaSoFar, ref bool thrown)
        {
            var theoretical = MyPos;
            var rememberedOldProbas = new List<Tuple<int, double>>();

            if (Surrounding.Contains(cell))
            {
                ShortestPath(cell);
                theoretical = ComputeTheoretical();
            }

            if (ShouldShoot(cell))
            {
                ThrowRock(theoretical, cell, rememberedOldProbas);
                thrown = true;
            }

            if (PortalOutcome(cell) >= bestProbaSoFar)
            {
                return true;
            }

            Rollback(ref thrown, rememberedOldProbas);
            return false;
        }

        private bool ShouldShoot(int cell)
        {
            return Proba.GetProbaFor(cell, Entity.Monster) > ShootingThreshold;
        }

        private void ThrowRock(int src, int cellAvailable, List<Tuple<int, double>> memory)
        {
            Action thrower = Environment.Map.GetThrowed(src, cellAvailable);
            var target = cellAvailable;
            Environment.HandleThrow(thrower);
            memory.Add(new Tuple<int, double>(target, Proba.GetProbaFor(target, Entity.Monster)));
            Proba.UpdateEntity(Entity.Monster, target, 0);
            // todo Irindul March 22, 2019 : Check if still updated in graph
        }

        private void Rollback(ref bool thrown, List<Tuple<int, double>> memory)
        {
            if (thrown)
            {
                RollbackThrow(memory);
                thrown = false;
            }

            Intents = new Queue<Action>();
        }

        private double PortalOutcome(int cell)
        {
            return Proba.GetProbaFor(cell, Entity.Portal);
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
                var action = Environment.Map.MoveToward(prev, dest);
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

        private void RollbackThrow(IEnumerable<Tuple<int, double>> memory)
        {
            Environment.RollBackRipple();
            foreach (var (key, oldValue) in memory)
            {
                Proba.UpdateEntity(Entity.Monster, key, oldValue);
            }
        }
    }
}