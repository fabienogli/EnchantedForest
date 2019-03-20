using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Action = EnchantedForest.Agent.Action;

namespace EnchantedForest.Environment
{
    public class Map
    {
        //todo count Poop and Wind amount
        //todo subenum Asset etc..
        private List<Entity> Cells { get; }
        
        public int Size => Cells.Capacity;

        public int SquaredSize => (int) Math.Sqrt(Size);
        
        //Storing AgentPos to avoid computation
        public int AgentPos { get; set; }
        public int PortalPos { get; set; }

        private object _lock = new object();  
        
        public Map(int size)
        {
            Cells = new List<Entity>(size);
            InitEntities(size);
        }
        
        private void InitEntities(int size)
        {
            for (var i = 0; i < size; i++)
            {
                Cells.Add(Entity.Nothing);
            }
        }
        
        public Map(Map other)
        {
            Cells = new List<Entity>(other.Size);
            lock (other._lock)
            {
                foreach (var room in other.Cells)
                {
                    Cells.Add(room);
                }

                AgentPos = other.AgentPos;
                PortalPos = other.PortalPos;
            }
           
        }

        public void AddEntityAtPos(Entity flag, int pos)
        {
            lock (_lock)
            {
                if (Cells[pos].Equals(Entity.Nothing))
                {
                    Cells[pos] = flag;
                }
                else
                {
                    Cells[pos] |= flag;    
                }

                
            }
        }

        public void RemoveEntityAtPos(Entity flag, int pos)
        {
            lock (_lock)
            {
                if (ContainsEntityAtPos(flag,pos))
                {
                    Cells[pos] &= ~flag;
                }
            }
        }

        public bool ContainsEntityAtPos(Entity flag, int pos)
        {
            return Cells[pos].HasFlag(flag);
        }

        public void MoveAgentTo(int pos)
        {
            var agent = Entity.Agent;
            lock (_lock)
            {
                RemoveEntityAtPos(agent, AgentPos);
                AddEntityAtPos(agent, pos);    
            }
        }

        public Entity GetEntityAt(int pos)
        {
            lock (_lock)
            {
                return Cells[pos];    
            }
        }

        public void ApplyAction(Action action)
        {
            var newPos = AgentPos;
            switch (action)
            {
                case Action.Idle:
                    return;
                case Action.Up:
                    newPos -= SquaredSize;
                    break;
                case Action.Down:
                    newPos += SquaredSize;
                    break;
                case Action.Left:
                    if (newPos % SquaredSize == 0)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    newPos--;
                    
                    break;
                case Action.Right:
                    
                    if (newPos % SquaredSize == SquaredSize-1)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    newPos++;
                    break;
                default:
                    throw new InvalidDataException("No such action was implemented !");
            }

            if (newPos < 0 || newPos >= Size)
            {
                throw new IndexOutOfRangeException();
            }

            MoveAgentTo(newPos);
            AgentPos = newPos;
        }

        public int GetUpFrom(int pos)
        {
            var newPos = pos - SquaredSize;
            CheckBoundaries(newPos);
            return newPos;
        }

        public int GetDownFrom(int pos)
        {
            var newPos = pos + SquaredSize;
            CheckBoundaries(newPos);
            return newPos;
        }

        public int GetLeftFrom(int pos)
        {
            if (pos % SquaredSize == 0)
            {
                throw new IndexOutOfRangeException();
            }
            var newPos = pos - 1;
            CheckBoundaries(newPos);
            return newPos;
        }

        public int GetRightFrom(int pos)
        {
            if (pos % SquaredSize == SquaredSize-1)
            {
                throw new IndexOutOfRangeException();
            }
            var newPos = pos + 1;
            CheckBoundaries(newPos);
            return newPos;
        }

        private void CheckBoundaries(int pos)
        {
            if (pos < 0 || pos >= Size)
            {
                throw new IndexOutOfRangeException();
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                sb.Append(EntityStringer.ObjectToString(GetEntityAt(i)));
            }

            return sb.ToString();
        }
    }
}