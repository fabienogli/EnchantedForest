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
        private List<int> Available;

        private Dictionary<Action, Effector> Effectors { get; }
        private Dictionary<int, Dictionary<Entity, double>> Probs;
        private HashSet<int> AlreadyVisited { get; }

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
            Intents = new Queue<Action>();
            ActionDone = 0;
            EffectorFactory.Forest = environment;
            Effectors = EffectorFactory.GetEffectors();
            AlreadyVisited = new HashSet<int>();
            Available = new List<int>();
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

                Thread.Sleep(200);
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
                effector.DoIt();
            }
            else
            {
                PlanIntents();
            }
        }

        private void Infere(Entity observe)
        {
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

            //Si y a rien d'autres
            //On met la proba de monstre à 0
        }

        private void PlanIntents()
        {
            var treshold = 0.56;
            double max_win = -1;
            var max_i = 0;
            var action = Action.Idle;
            var throwed = false;

            Memory = new List<Tuple<int, double>>();
            var theoretical = MyPos;

            var surroundingsNotVisited = Environment.Map.GetSurroundingCells(MyPos)
                .Where(cell => !AlreadyVisited.Contains(cell));

            foreach (var surrounding in surroundingsNotVisited)
            {
                Available.Add(surrounding);
            }

            Available.Remove(MyPos);
            AlreadyVisited.Add(MyPos);

            foreach (var cellAvailable in Available)
            {
                var surroundings = Environment.Map.GetSurroundingCells(MyPos);
                var mem = new Queue<Action>();
                foreach (var intent in Intents)
                {
                    mem.Enqueue(intent);
                }

                if (!surroundings.Contains(cellAvailable))
                {
                    // fixme Irindul March 20, 2019 : Bug here
                    //Whene we add to the intents we will rollback 
                    //Only if are not the best *so far* 
                    //Not if we are the best *total* /!\ 
                    //Causes theoreticalPos to be outDated and Intents filled with actions not executed
                    ShortestPath(cellAvailable);
                    theoretical = cellAvailable;
                }

                if (Probs[cellAvailable][Entity.Monster] > treshold)
                {
                    ThrowRock(theoretical, cellAvailable);
                    throwed = true;
                }

                if (!(Probs[cellAvailable][Entity.Portal] > max_win))
                {
                    RollbackThrow();
                    Intents = mem;
                    theoretical = MyPos;
                    throwed = false;
                    continue;
                }

                max_win = Probs[cellAvailable][Entity.Portal];
                max_i = cellAvailable;
            }

            if (throwed)
            {
                Intents.Enqueue(GetThrowed(theoretical, max_i));
            }

            Intents.Enqueue(MoveToward(theoretical, max_i));
        }

        private int ComputeTheoretical(int myPos)
        {
            var theoretical = myPos;
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

        private bool ShortestPath(Queue<int> cell, HashSet<int> explored, int current, int end)
        {
            explored.Add(current);
            if (Environment.Map.GetUpFrom(current) == end || Environment.Map.GetDownFrom(current) == end ||
                Environment.Map.GetLeftFrom(current) == end || Environment.Map.GetRightFrom(current) == end)
            {
                cell.Enqueue(current);
                return true;
            }

            var children = Environment.Map.GetSurroundingCells(MyPos)
                .Where(tile => AlreadyVisited.Contains(tile) && !explored.Contains(tile));

            if (!children.Any(child => ShortestPath(cell, explored, child, end)))
                return false;

            cell.Enqueue(current);
            return true;
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