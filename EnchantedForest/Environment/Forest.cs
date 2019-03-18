using System;
using System.Threading;

namespace EnchantedForest.Environment
{
    public class Forest : IObservable<Forest>
    {
        private IObserver<Forest> Observer { get; set; }
        private bool Running { get; }
        private Random Rand { get; }
        
        private int CurrentSize { get; set; } 

         public Map Map { get; set; }
        public int Fitness { get; set; }

        public Forest(int size)
        {
            //Only once initialization to get uniform result
            //Seeding to reproduce outcomes easily
            Rand = new Random(1);

            Running = true;
            
            InitMap(size);
            InitAgent();
            
        }

        public Forest(Forest forest)
        {
            Running = forest.Running;
            Map = new Map(forest.Map);
            Fitness = forest.Fitness;
        }

        private void InitMap(int size)
        {
            Map = new Map(size);
            //Todo fill map here
        }

        private void InitAgent()
        {
            // todo Irindul March 18, 2019 : Init agent at first empty tile !
            var agentPos = Rand.Next(CurrentSize);
            
            Map.AddEntityAtPos(Entity.Agent, agentPos);
            Map.AgentPos = agentPos;
            Fitness = 0;
        }

        public IDisposable Subscribe(IObserver<Forest> observer)
        {
            Observer = observer;
            Notify();
            return null; //Maybe return an Unsubscriber
        }

        private void Notify()
        {
            Observer.OnNext(this);
        }
        
        public void Run()
        {
            while (Running)
            {
                
                //Since the environment is not changing on its own
                //This loop is empty
                //But every event coming from the environment should be put here
                Thread.Sleep(50);
            }
        }
    }
}