using System;
using System.Collections.Generic;
using System.Linq;
using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class Hypothesis
    {
        private int position;
        private List<Evidence> _evidences;
        private double probabilite;
        private Entity Entity; //@TODO should move to another subclass
        private bool found;
        private Evidence OwnEvidence;

        public int Position => position;

        
        /**
         * var portal = (double) 1 / (Environment.Map.Size - 1);
                    var pit = 0.15;
                    var monster = 0.15;
                    var nothing = 1 - portal - pit - monster;
         */
        public Hypothesis(int position, Entity entity)
        {
            this.position = position;
            OwnEvidence = new Evidence(position, entity);
            found = false;
            _evidences = new List<Evidence>();
        }

        public static Dictionary<Entity, Dictionary<int, Hypothesis>> GenerateHypothesis(Map map, int start)
        {
            Dictionary<Entity, Dictionary<int, Hypothesis>> dictionary = new Dictionary<Entity, Dictionary<int, Hypothesis>>();
            Dictionary<int, Hypothesis> pitDictionary = new Dictionary<int, Hypothesis>();
            Dictionary<int, Hypothesis> monstertDictionary = new Dictionary<int, Hypothesis>();
            for (int i = 0; i < map.Size; i++)
            {
                if (i == start)
                {
                    continue;
                }
                Hypothesis monster = new Hypothesis(i, Entity.Monster);
                List<Evidence> poops = new List<Evidence>();

                Hypothesis pit = new Hypothesis(i, Entity.Pit);
                List<Evidence> winds = new List<Evidence>();
               
                List<int> relatedPosition = new List<int>(map.GetSurroundingCells(i));
                foreach (int position in relatedPosition)
                {
                    poops.Add(new Evidence(position, Entity.Poop));
                    winds.Add(new Evidence(position, Entity.Cloud));
                }

                monster.Evidences = poops;
                pit.Evidences = winds;

                pitDictionary.Add(i, pit);
                monstertDictionary.Add(i, monster);
            }
            dictionary.Add(Entity.Monster, monstertDictionary);
            return dictionary;
        }

        public static void Assert(Dictionary<Entity, Dictionary<int, Hypothesis>> _dictionary, Evidence evidence)
        {
            int pos_e = evidence.Position;
            foreach (var pair in _dictionary)
            {
                var dictionary = pair.Value;
                var entity = pair.Key;
                foreach (var hypothesis in dictionary.Values)
                {
                    hypothesis.RemoveEvidence(evidence);
                }
            }
        }        
        
        
        public static void Assert(Dictionary<Entity, Dictionary<int, Hypothesis>> _dictionary, Hypothesis trueHypothesis)
        {
            foreach (var pair in _dictionary)
            {
                var dictionary = pair.Value;
                var entity = pair.Key;
                foreach (var hypothesi in dictionary.Values)
                {
                    hypothesi.Assert(trueHypothesis);
                }
            }
        }

        public static void GetProba(Dictionary<Entity, Dictionary<int, Hypothesis>> _dictionary, Entity entity)
        {
            switch (entity)
            {
                case Entity.Nothing:
                    
                case Entity.Monster:
                case Entity.Poop:
                    break;
                case Entity.Pit:
                    break;
                case Entity.Cloud:
                    break;
                case Entity.Portal:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity), entity, null);
            }
        }
        



        public void Assert(Hypothesis hypothesis)
        {
            foreach (var evidence in hypothesis.Evidences)
            {
                Assert(evidence);
            }
        }

        private void Assert(Evidence evidence)
        {
            for (int i = 0; i < _evidences.Count; i++)
            {
                if (evidence.Equals(_evidences[i]))
                {
                    _evidences[i].Assert(evidence.Here);
                }
            }
        }

        private void Assert(bool result)
        {
            for (int i = 0; i < _evidences.Count; i++)
            {
                _evidences[i].Assert(result);
            }
        }

        public void RemoveEvidence(Evidence evidence)
        {
            for (int i = 0; i < _evidences.Count; i++)
            {
                if (_evidences[i].Invalidate(evidence))
                {
                    
                    _evidences.Remove(_evidences[i]);
                }
            }

            UpdateProba();
        }
        
            

        private void UpdateProba()
        {
            if (!_evidences.Any())
            {
                Sure(false);
            }
        }

        public void Sure(bool result)
        {
            found = true;
            probabilite = result ? 1 : 0;
        }

        public List<Evidence> Evidences
        {
            get => _evidences;
            set => _evidences = value;
        }

        public double Probabilite
        {
            get => probabilite;
            set => probabilite = value;
        }

        public bool Found => found;
    }
}