using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnchantedForest.Agent.Effectors;
using EnchantedForest.Environment;
using EnchantedForest.Search;
using Entity = EnchantedForest.Environment.Entity;

namespace EnchantedForest.Agent
{
    public class Agent
    {
        private Forest Environment { get; }
        private DeathSensor DeathSensor;
        private CellSensor CellSensor { get; }
        private Dictionary<Action, Effector> Effectors { get; }
        private Queue<Action> Intents { get; set; }
        private ProbabilityMatrix Proba { get; set; }
        private GraphInferer Inferer { get; set; }
        private int MyPos => Environment.Map.AgentPos;
        private FakeEnvironment Fake { get; set; }

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
            Proba = new ProbabilityMatrix(Environment.Map.Size);
            Fake = new FakeEnvironment(Environment, Proba);
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
            var isDead = DeathSensor.Observe(Environment);
            var observe = CellSensor.Observe(Environment);

            Infere(observe);

            if (isDead)
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
            if (observe.HasFlag(Entity.Portal))
            {
                Intents = new Queue<Action>();
                Intents.Enqueue(Action.Leave);
                return;
            }
            
            var state = new State(Environment.Map, Action.Idle, Environment);
            var tree = new Tree(new Tree.Node(state, Fake));
            Fake.Visit(MyPos, Proba);
            
            AStarIterator strategy = new AStarIterator(tree, Fake);
            var node = strategy.GetBestExplored();
            BacktrackAndBuildIntents(node);
        }

        private void BacktrackAndBuildIntents(Tree.Node node)
        {
            if (node.Parent == null)
            {
                return;
            }

            BacktrackAndBuildIntents(node.Parent);
            EnqueueIntentFromNode(node);
        }

        private void EnqueueIntentFromNode(Tree.Node node)
        {
            var intent = node.State.Action;
            Intents.Enqueue(intent);
        }
    }
}