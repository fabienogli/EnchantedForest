using System;
using System.Collections.Generic;
using System.Threading;
using EnchantedForest.Search;
using Action = EnchantedForest.Agent.Action;

namespace EnchantedForest.Environment
{
    public class Forest : IObservable<Forest>, IEnvironment
    {
        private IObserver<Forest> Observer { get; set; }
        private bool Running { get; }
        private Random Rand { get; }
        public Map Map { get; set; }
        private Map Memory { get; set; }
        public int Fitness { get; set; }

        public bool AgentDead { get; private set; }
        public int InitPosAgent { get; private set; }

        public Forest(int size)
        {
            //Only once initialization to get uniform result
            //Seeding to reproduce outcomes easily
            Rand = new Random(4);

            Running = true;

            InitMap(size);
        }

        public Forest(Forest forest)
        {
            Running = forest.Running;
            Map = new Map(forest.Map);
            Fitness = forest.Fitness;
            Memory = forest.Memory;
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
            Observer?.OnNext(this);
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
            var newSquaredSize = Map.SquaredSize + 1;
            Map = new Map(newSquaredSize * newSquaredSize);
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

                var up = Map.GetUpFrom(i);
                if (up >= 0)
                {
                    AddEntityAssets(entity, up);    
                }

                var down = Map.GetDownFrom(i);
                if (down >= 0)
                {
                    AddEntityAssets(entity, down);
                }

                var left = Map.GetLeftFrom(i);
                if (left >= 0)
                {
                    AddEntityAssets(entity, left);
                }

                var right = Map.GetRightFrom(i);
                if (right >= 0)
                {
                    AddEntityAssets(entity, right);
                }
            }
        }

        private void InitAgent()
        {
            var agentPos = Rand.Next(Map.Size);
            Map.AgentPos = agentPos;
            InitPosAgent = agentPos;
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

        public bool HandleAction(Action action)
        {
            Fitness += GetFitnessForAction(action);

            switch (action)
            {
                case Action.Leave:
                    Console.WriteLine();
                    if (Map.ContainsEntityAtPos(Entity.Portal, Map.AgentPos))
                    {
                        NextLevel();
                        Notify();
                        return true;
                    }
                    return false;
                case Action.ThrowUp:
                case Action.ThrowDown:
                case Action.ThrowLeft:
                case Action.ThrowRight:
                    HandleThrow(action);
                    Notify();
                    return true;
                    
            }
            
            Map.ApplyAction(action);            
            Notify();
            CheckDie();
            Notify();
            return true;
        }

        private void CheckDie()
        {
            if (Map.ContainsEntityAtPos(Entity.Pit, Map.AgentPos) || Map.ContainsEntityAtPos(Entity.Monster, Map.AgentPos))
            {
                AgentDead = true;
                Fitness -= 10 * Map.Size;
            }
        }

        public void HandleThrow(Action action)
        {
            int shoutedPos;
            switch (action)
            {
                case Action.ThrowUp:
                    shoutedPos = Map.GetUpFrom(Map.AgentPos);
                    break;
                case Action.ThrowDown:
                    shoutedPos = Map.GetDownFrom(Map.AgentPos);
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

            if (shoutedPos >= 0)
            {
                RippleEffect(shoutedPos);    
            }
        }

        private void RippleEffect(int shoutedPos)
        {
            //Remove monster (we shot it)  
            Memory = new Map(Map);
            Map.RemoveEntityAtPos(Entity.Monster, shoutedPos);
            
            foreach (var cell in Map.GetSurroundingCells(shoutedPos))
            {
                if (!Map.ContainsEntityAtPos(Entity.Poop, cell)) continue;
                Map.RemoveEntityAtPos(Entity.Poop, cell);
                SanityCheck(cell);
            }

    }

        public void RollBackRipple()
        {
            if (Memory == null) 
                return;
            
            
            Map = Memory;
            Memory = null;
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

        public int GetCostForAction(Action action)
        {
            switch (action)
            {
                case Action.Idle:
                    return 0;
                case Action.Left:
                case Action.Right:
                case Action.Up:
                case Action.Down:
                    return 1;
                case Action.ThrowLeft:
                case Action.ThrowRight:
                case Action.ThrowUp:
                case Action.ThrowDown:
                    return 10;    
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public List<State> GetSuccessors(State currentState)
        {
            throw new NotImplementedException();
        }

        public double GetHeuristicForState(State state)
        {
            throw new NotImplementedException();
        }
        
        public int GetFitnessForAction(Action action)
        {
            switch (action)
            {
                case Action.Left:
                case Action.Right:
                case Action.Up:
                case Action.Down:
                    return -1;
                case Action.ThrowLeft:
                case Action.ThrowRight:
                case Action.ThrowUp:
                case Action.ThrowDown:
                    return -10;
                case Action.Leave:
                    return 10 * Map.Size;
                default:
                    return 0;
            }
        }
        public void ResetAgent()
        {
            Map.MoveAgentTo(InitPosAgent);
            AgentDead = false;
            Notify();
        }
    }

}