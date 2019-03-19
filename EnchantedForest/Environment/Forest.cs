using System;
using System.Collections.Generic;
using System.Threading;
using EnchantedForest.Environment;

namespace EnchantementForest.Environment
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
            GenerateLevel();
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

        private void NextLevel()
        {
            var currentSize = Map.SquaredSize;
            Map = new Map(currentSize+1);
            GenerateLevel();
        }

        private void GenerateLevel()
        {
            InitAgent();
            InitPortal();
            var path = GetPath();
            var blacklisted = new HashSet<int>();

            while (blacklisted.Count < Map.Size)
            {
                var i = Rand.Next(Map.Size);
                if (blacklisted.Contains(i))
                {
                    continue;
                }

                if (path.Contains(i))
                {
                    continue;
                }

                var whichShouldGenerate = Rand.Next(3);
                if (whichShouldGenerate == 0)
                {
                    continue;
                }
                
                var entity = whichShouldGenerate == 1 ? Entity.Monster : Entity.Pit;
                Map.AddEntityAtPos(entity, i);
                
                try
                {
                    path = GetPath();
                }
                catch (PathNotFoundException)
                {
                    blacklisted.Add(i);
                    Map.RemoveEntityAtPos(entity, i);
                }
                
            }
            
        }

        private HashSet<int> GetPath()
        {
            /*
             * todo
             * Create AStarIterator with random heuristic
             * Find any path
             * 
             */
            throw new PathNotFoundException();
        }

        private void InitAgent()
        {
            var agentPos = Rand.Next(Map.Size);
            Map.AgentPos = agentPos;
            Map.AddEntityAtPos(Entity.Agent, agentPos);
        }
        
        private void InitPortal()
        {
            int portalPos;
            do
            {
                portalPos = Rand.Next(Map.Size);
            } while (portalPos == Map.AgentPos);

            Map.PortalPos = portalPos;
            Map.AddEntityAtPos(Entity.Portal, portalPos);
        }

        private void AddEntityAssets(Entity entityToPose, int pos)
        {
            var entityAsset = entityToPose.Equals(Entity.Monster) ? Entity.Poop : Entity.Cloud;
            Map.AddEntityAtPos(entityAsset, pos);
        }
    }
}