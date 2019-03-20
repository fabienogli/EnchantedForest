using System;
using System.Collections.Generic;
using System.Threading;
using Action = EnchantedForest.Agent.Action;

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
            Rand = new Random();

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
            Map = new Map(currentSize + 1);
            GenerateLevel();
        }

        private void GenerateLevel()
        {
            InitAgent();
            InitPortal();

            for (int i = 0; i < Map.Size; i++)
            {
                if (!Map.GetEntityAt(i).Equals(Entity.Nothing))
                {
                    continue;
                }

                var proba = Rand.Next(100);
                Entity entity;
                if (proba < 15)
                {
                    entity = Entity.Monster;
                }
                else if (proba >= 15 && proba < 30)
                {
                    entity = Entity.Pit;
                }
                else
                {
                    continue;
                }

                Map.AddEntityAtPos(entity, i);

                try
                {
                    var up = Map.GetUpFrom(i);
                    AddEntityAssets(entity, up);
                }
                catch (IndexOutOfRangeException)
                {
                    //
                }

                try
                {
                    var down = Map.GetDownFrom(i);
                    AddEntityAssets(entity, down);
                }
                catch (IndexOutOfRangeException)
                {
                    //
                }

                try
                {
                    var left = Map.GetLeftFrom(i);
                    AddEntityAssets(entity, left);
                }
                catch (IndexOutOfRangeException)
                {
                    //
                }


                try
                {
                    var right = Map.GetRightFrom(i);
                    AddEntityAssets(entity, right);
                }
                catch (IndexOutOfRangeException)
                {
                    //
                }
            }
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

        public Entity ObserveCell()
        {
            return Map.GetEntityAt(Map.AgentPos);
        }

        public void HandleAction(Action action)
        {
            switch (action)
            {
                case Action.Leave:
                    if (Map.ContainsEntityAtPos(Entity.Portal, Map.AgentPos))
                    {
                        NextLevel();
                    }

                    return;
            }

            Map.ApplyAction(action);
        }

        public void HandleThrow(Action action)
        {
            try
            {
                int shoutedPos;
                switch (action)
                {
                    case Action.ThrowUp:
                        shoutedPos = Map.GetUpFrom(Map.AgentPos);
                        break;
                    case Action.ThrowDown:
                        shoutedPos = Map.GetUpFrom(Map.AgentPos);
                        break;
                    case Action.ThrowRight:
                        shoutedPos = Map.GetRightFrom(Map.AgentPos);
                        break;
                    case Action.ThrowLeft:
                        shoutedPos = Map.GetLeftFrom(Map.AgentPos);
                        break;
                    default:
                        return;
                }

                RippleEffect(shoutedPos);
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        private void RippleEffect(int shoutedPos)
        {
            //Remove monster (we shot it)  
            Map.RemoveEntityAtPos(Entity.Monster, shoutedPos);
            
            foreach (var cell in Map.GetSurroundingCells(shoutedPos))
            {
                if (!Map.ContainsEntityAtPos(Entity.Poop, cell)) continue;
                Map.RemoveEntityAtPos(Entity.Poop, cell);
                SanityCheck(cell);
            }

    }

        private void SanityCheck(int pos)
        {
            foreach (var cell in Map.GetSurroundingCells(pos))
            {
                if (Map.ContainsEntityAtPos(Entity.Monster, cell))
                {
                    Map.AddEntityAtPos(Entity.Monster, pos);
                }
            }
        }
    }

}