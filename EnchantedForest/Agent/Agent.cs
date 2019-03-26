using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EnchantedForest.Agent.Effectors;
using EnchantedForest.Environment;
using Action = EnchantedForest.Agent.Action;
using Entity = EnchantedForest.Environment.Entity;
using Probabilite = EnchantedForest.Agent.Probabilite;

namespace EnchantedForest.Agent
{
    public class Agent
    {
        private CellSensor CellSensor;
        private DeathSensor DeathSensor;
        private HashSet<int> Available;

        private Dictionary<Entity, Dictionary<int, Hypothesis>> dictionary;
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

        public Agent(Forest environment)
        {
            Environment = environment;
            CellSensor = new CellSensor();
            DeathSensor = new DeathSensor();
            EffectorFactory.Forest = environment;
            Effectors = EffectorFactory.GetEffectors();
            AlreadyVisited = new HashSet<int>();
            ResetAgent();
        }

        private void ResetAgent()
        {
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

            dictionary = Hypothesis.GenerateHypothesis(Environment.Map, MyPos);
        }

        public void UpdateMonster(int cell, double newProba)
        {
            Probs[cell][Entity.Monster] = newProba;
            UpdatePortal(cell);
        }

        public void UpdatePit(int cell, double newProba)
        {
            Probs[cell][Entity.Pit] = newProba;
            UpdatePortal(cell);
        }

        public void UpdateNothing(int cell, double newProba)
        {
            Probs[cell][Entity.Pit] = newProba;
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
            bool IsDead = DeathSensor.Observe(Environment);
            
            Entity observe = CellSensor.Observe(Environment);

            Infere(observe);

            if (IsDead)
            {
                Die();
            }
            
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

        private void Die()
        {
            throw new NotImplementedException();
        }

        private void Infere(Entity observe)
        {
            if (AlreadyVisited.Contains(MyPos))
            {
                return;
            }
            if (Environment.Map.ContainsEntityAtPos(Entity.Pit, MyPos))
            {
                Dictionary<int, Hypothesis> hypotheses = dictionary[Entity.Pit];
                Hypothesis trueHypothesis = hypotheses[MyPos];
                Hypothesis.Assert(dictionary, trueHypothesis);
            }
            if (Environment.Map.ContainsEntityAtPos(Entity.Monster, MyPos))
            {
                Dictionary<int, Hypothesis> hypotheses = dictionary[Entity.Monster];
                Hypothesis trueHypothesis = hypotheses[MyPos];
                Hypothesis.Assert(dictionary, trueHypothesis);
            }
            if (Environment.Map.ContainsEntityAtPos(Entity.Poop, MyPos))
            {
                Evidence evidence = new Evidence(MyPos, observe);
                Hypothesis.Assert(dictionary, evidence);
            }
            if (Environment.Map.ContainsEntityAtPos(Entity.Cloud, MyPos))
            {
                Evidence evidence = new Evidence(MyPos, observe);
                Hypothesis.Assert(dictionary, evidence);
            }
            
            var surroundings = Environment.Map.GetSurroundingCells(MyPos)
                .Where(cell => !AlreadyVisited.Contains(cell))
                .ToList();
            var nbSurroundings = surroundings.Count;

            if (observe.HasFlag(Entity.Cloud))
            {
                foreach (var cell in surroundings)
                {
                    var newProba = (double) 1 / nbSurroundings;
                    UpdatePit(cell, Probs[cell][Entity.Pit] + newProba);
                    if (Probs[cell][Entity.Pit] > 1)
                    {
                        UpdatePit(cell, 1);
                        //throw new InvalidDataException("Proba is too high");
                        // todo Irindul March 20, 2019 : Refactor into method   
                    }
                }
            }

            if (observe.HasFlag(Entity.Poop))
            {
                foreach (var cell in surroundings)
                {
                    var newProba = (double) 1 / nbSurroundings;
                    UpdateMonster(cell, Probs[cell][Entity.Monster] + newProba);
                    if (Probs[cell][Entity.Monster] > 1)
                    {
                        UpdateMonster(cell, 1);
                        //throw  new InvalidDataException("Proba is too high");
                    }
                }
            }

            if (observe.HasFlag(Entity.Pit))
            {
                UpdatePit(MyPos, 1);
                //Maybe see if some other cells proba might change
            }

            if (observe.HasFlag(Entity.Monster))
            {
                UpdateMonster(MyPos, 1);
            }

            // todo Irindul March 21, 2019 : Add others proba
            // todo Irindul March 21, 2019 : Check what other tiles are affected when changing a proba

            //Si y a rien d'autres
            //On met la proba de monstre/crevasse à 0
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