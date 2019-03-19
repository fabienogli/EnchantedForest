using System;
using System.Collections.Generic;
using System.IO;
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
            var emptyTiles = Map.Size;
            while (emptyTiles > Map.Size/3)
            {
                var i = Rand.Next(Map.Size);
                if (Map.ContainsEntityAtPos(Entity.Pit, i) | Map.ContainsEntityAtPos(Entity.Monster, i))
                {
                    continue;
                }

                var entityToPose = Rand.Next(2) == 0 ? Entity.Monster : Entity.Pit;
                Map.AddEntityAtPos(entityToPose, i);
                emptyTiles--;

                try
                {
                    var up = Map.GetUpFrom(i);
                    AddEntityAssets(entityToPose, up);
                } catch (IndexOutOfRangeException) {}

                try
                {
                    var down = Map.GetDownFrom(i);
                    AddEntityAssets(entityToPose, down);
                } catch(IndexOutOfRangeException) {}

                try
                {
                    var left = Map.GetLeftFrom(i);
                    AddEntityAssets(entityToPose, left);
                } catch(IndexOutOfRangeException) {}

                try
                {
                    var right = Map.GetRightFrom(i);
                    AddEntityAssets(entityToPose, right);
                } catch(IndexOutOfRangeException) {}
            }
            InitAgent();
        }
        
        private void InitAgent()
        {
            HashSet<int> alreadyVisited = new HashSet<int>();
            while (alreadyVisited.Count < Map.Size)
            {
                int i = Rand.Next(Map.Size);
                if (alreadyVisited.Contains(i))
                {
                    continue;
                }

                alreadyVisited.Add(i);
                if (Map.ContainsEntityAtPos(Entity.Monster, i))
                {
                    continue;
                }

                if (Map.ContainsEntityAtPos(Entity.Pit, i))
                {
                    continue;
                }

                Map.AgentPos = i;
                Map.AddEntityAtPos(Entity.Agent, i);
                return;
            }
            throw new InvalidDataException("The map is not correct");
        }

        private void AddEntityAssets(Entity entityToPose, int pos)
        {
            var entityAsset = entityToPose.Equals(Entity.Monster) ? Entity.Poop : Entity.Cloud;
            Map.AddEntityAtPos(entityAsset, pos);
        }
    }
}