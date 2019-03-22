using System.Collections.Generic;
using System.Text;
using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class ProbabilityMatrix
    {
        private Dictionary<int, Dictionary<Entity, double>> Probabilities { get; }

        public ProbabilityMatrix(int size)
        {
            
            Probabilities = new Dictionary<int, Dictionary<Entity, double>>();
            const double pit = 0.15;
            const double monster = 0.15;
            var portal = (double) 1 / (size - 1);
            var nothing = 1 - portal - pit - monster;
            for (var i = 0; i < size; i++)
            {
                Probabilities.Add(i, new Dictionary<Entity, double>());
                Probabilities[i].Add(Entity.Portal, portal);
                Probabilities[i].Add(Entity.Pit, pit);
                Probabilities[i].Add(Entity.Monster, monster);
                Probabilities[i].Add(Entity.Nothing, nothing);
            }
        }

        public void UpdateEntity(Entity entity, int cell, double proba)
        {
            Probabilities[cell][entity] = proba;
            UpdatePortal(cell);
        }

        private void UpdatePortal(int cell)
        {
            Probabilities[cell][Entity.Portal] =
                1 - Probabilities[cell][Entity.Monster] - Probabilities[cell][Entity.Pit] - Probabilities[cell][Entity.Nothing];
        }

        public double GetProbaFor(int cell, Entity entity)
        {
            return Probabilities[cell][entity];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (key, probabilityDict) in Probabilities)
            {
                sb.Append($"probas[{key}] : ");
                foreach (var (entity, proba) in probabilityDict)
                {
                    sb.Append($"[{entity}]={proba}");
                }

                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}