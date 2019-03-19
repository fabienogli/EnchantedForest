using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnchantedForest.Environment
{
    public class Map
    {
        private List<Entity> Rooms { get; }
        
        public int Size => Rooms.Capacity;

        public int SquaredSize => (int) Math.Sqrt(Size);
        
        //Storing AgentPos to avoid computation
        public int AgentPos { get; set; }

        private object _lock = new object();  
        
        public Map(int size)
        {
            Rooms = new List<Entity>(size);
            InitEntities(size);
        }
        
        private void InitEntities(int size)
        {
            for (var i = 0; i < size; i++)
            {
                Rooms.Add(Entity.Nothing);
            }
        }
        
        public Map(Map other)
        {
            Rooms = new List<Entity>(other.Size);
            lock (other._lock)
            {
                foreach (var room in other.Rooms)
                {
                    Rooms.Add(room);
                }

                AgentPos = other.AgentPos;
            }
           
        }

        public void AddEntityAtPos(Entity flag, int pos)
        {
            lock (_lock)
            {
                if (Rooms[pos].Equals(Entity.Nothing))
                {
                    Rooms[pos] = flag;
                }
                else
                {
                    Rooms[pos] |= flag;    
                }

                
            }
        }

        public void RemoveEntityAtPos(Entity flag, int pos)
        {
            lock (_lock)
            {
                if (ContainsEntityAtPos(flag,pos))
                {
                    Rooms[pos] &= ~flag;
                }
            }
        }

        public bool ContainsEntityAtPos(Entity flag, int pos)
        {
            return Rooms[pos].HasFlag(flag);
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
                return Rooms[pos];    
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