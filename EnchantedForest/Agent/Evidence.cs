using System;
using System.Collections.Generic;
using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class Evidence
    {

        private int position;
        private bool here;
        private bool set;

        private Entity Entity;
        //This is just for binding 

        public Evidence(int position, Entity entity)
        {
            this.position = position;
            Entity = entity;
        }
        

        public bool Invalidate(Evidence evidence)
        {
            return (evidence.Position == position && evidence.GetEntity != Entity);
        }
        
        public void Assert(bool result)
        {
            here = result;
            set = true;
        }
        
        public int Position => position;

        public bool Here => here;
        
        public Entity GetEntity => Entity;

        public override bool Equals(object obj)
        {
            if (obj is Evidence)
            {
                Evidence ev = (Evidence) obj;
                if (ev.Position == position && ev.Entity == Entity)
                {
                    return true;
                } 
            }

            return false;
        }
    }
}