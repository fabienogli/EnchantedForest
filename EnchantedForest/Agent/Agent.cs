using System;
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
         private const string OptimalFile = "optimal";
        public const int MaxDepth = 5;
        private CellSensor CellSensor;
        private List<int> Available;

        private Dictionary<Action, Effector> Effectors { get; }
        private Dictionary<int, Dictionary<Entity, double>> Probs;

        private Forest Environment { get; }

        private Map Beliefs { get; set; }

        private int Performance { get; set; }
        private Queue<Action> Intents { get; }


        private int ActionDone { get; set; }

        public Agent(Forest environment)
        {
            Environment = environment;
            CellSensor = new CellSensor();
            Intents = new Queue<Action>();
            ActionDone = 0;
            EffectorFactory.Forest = environment;
            Effectors = EffectorFactory.GetEffectors();
                
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
            
            if (Intents.Any())
            {
                var intent = Intents.Dequeue();
                var effector = Effectors[intent];
                effector.DoIt();
            }
            else
            {
                PlanIntents(observe);
            }
        }

        private void PlanIntents(Entity observe)
        {
            double treshold = 0.56;
            double max_win = -1;
            int max_i = 0;
            Action action = Action.Idle;
            foreach (int cellAvailable in Available)
            {
                if (Probs[cellAvailable][Entity.Monster] > treshold)
                {
                    Intents.Enqueue(Action.ThrowRock);
                    InfereAction(cellAvailable, Action.ThrowRock);
                }
                if (Probs[cellAvailable][Entity.Portal] > max_win)
                {
                    max_win = Probs[cellAvailable][Entity.Portal];
                    max_i = cellAvailable;
                    action = convertPosToAction(cellAvailable);
                }
            }
            Intents.Enqueue(action);
            throw new NotImplementedException();
        }

        private Action convertPosToAction(int cellAvailable)
        {
            throw new NotImplementedException();
        }

        private void InfereAction(int pos, Action action)
        {
            
        }

//        private void PlanIntents(Map actual)
//        {
//            var state = new State(actual, Action.Idle);
//            var tree = new Tree(new Tree.Node(state));
//            var strategy = SetStrategy(tree);
//            Tree.Node node = null;
//            while (strategy.HasNext())
//            {
//                node = strategy.GetNext();
//
//                if (IsGoalNode(node, strategy) || node.Depth == MaxDepth)
//                {
//                    break;
//                }
//
//                strategy.Expand();
//            }
//
//            BacktrackAndBuildIntents(node);
//        }

//        private Iterator<Tree.Node> SetStrategy(Tree tree)
//        {
//            if (IsInformed)
//            {
//                return new AStarIterator(tree);
//            }
//            
//            return new BFSIterator(tree);
//        }

//        private void BacktrackAndBuildIntents(Tree.Node node)
//        {
//            if (node.Parent == null)
//            {
//                return;
//            }
//
//            BacktrackAndBuildIntents(node.Parent);
//            EnqueueIntentFromNode(node);
//        }
//
//        private void EnqueueIntentFromNode(Tree.Node node)
//        {
//            var intent = node.State.Action;
//            Intents.Enqueue(intent);
//        }
//
        private bool IsGoalNode()
        {
            return false;
        }
    }
}