using System.Collections.Generic;
using System.Linq;
using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class GraphInferer : IInferer
    {
        private Graph Graph { get; }
        private ProbabilityMatrix Proba { get; }
        private HashSet<int> AlreadyVisited { get; }
        
        private Map Map { get; }
        
        public int MyPos { private get; set; }
        
        private IEnumerable<int> Surrounding => Map.GetSurroundingCells(MyPos);

        public GraphInferer(ProbabilityMatrix proba, Map map)
        {
            Proba = proba;
            Map = map;
            AlreadyVisited = new HashSet<int>();
            Graph = new Graph();
        }

        public ProbabilityMatrix Infere(Entity observe)
        {
            if (AlreadyVisited.Contains(MyPos))
            {
                return Proba;
            }

            AlreadyVisited.Add(MyPos);

            var surrounding = Surrounding.Where(child => !AlreadyVisited.Contains(child)).ToHashSet();
            foreach (var child in surrounding)
            {
                Graph.AddEdge(MyPos, child);
            }

            UpdateSelf(observe);
            Graph.Emancipate(MyPos);

            Graph.AddCluster(surrounding);
            PropagateInfoToNewNodes(observe, surrounding);
            Graph.RemoveNode(MyPos);
            return Proba;
        }
        
        private void UpdateSelf(Entity observe)
        {
            var hasMonster = observe.HasFlag(Entity.Monster);
            var hasPit = observe.HasFlag(Entity.Pit);
            var hasNothing = (!hasMonster || !hasPit) && !observe.HasFlag(Entity.Portal);

            Proba.UpdateEntity(Entity.Monster, MyPos, hasMonster ? 1 : 0);
            Proba.UpdateEntity(Entity.Pit, MyPos, hasPit ? 1 : 0);

            if (hasNothing)
            {
                Proba.UpdateEntity(Entity.Nothing, MyPos, 1);
            }
        }
        
        private void PropagateInfoToNewNodes(Entity observe, HashSet<int> surrounding)
        {
            var hasPoop = observe.HasFlag(Entity.Poop);
            var hasCloud = observe.HasFlag(Entity.Cloud);
            var hasMonster = observe.HasFlag(Entity.Monster);
            var hasPit = observe.HasFlag(Entity.Pit);
            var hasSomething = hasMonster || hasPit;

            Forward(Entity.Monster, hasPoop ? 1 : 0, new HashSet<int>(), surrounding);
            Forward(Entity.Pit, hasCloud ? 1 : 0, new HashSet<int>(), surrounding);

            if (hasSomething)
            {
                return;
            }
            
            Forward(Entity.Nothing, 0, new HashSet<int>(), surrounding);

        }
        
        private void Forward(Entity entity, int value, HashSet<int> alreadyVisited, HashSet<int> cluster)
        {
            var totalChildClusters = 0;
            var cellCount = new Dictionary<int, int>();

            var elemNotVisited = cluster.Where(elem => !alreadyVisited.Contains(elem)).ToHashSet();
            
            foreach (var elem in elemNotVisited)
            {
                var clusterCount = Graph.CountClusters(elem);
                totalChildClusters += clusterCount;
                cellCount.Add(elem, clusterCount);
            }

            foreach (var elem in elemNotVisited)
            {

                var proba = (double) cellCount[elem] / totalChildClusters;
                proba *= value;
                
                Proba.UpdateEntity(entity, elem, proba);
                alreadyVisited.Add(elem);
                
                foreach (var clust in Graph.GetClustersFor(elem))
                {
                    Forward(entity, value, alreadyVisited, clust);
                }
            }
        }
    }
}