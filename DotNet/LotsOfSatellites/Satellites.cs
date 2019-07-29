using System.Collections.Generic;
using AGI.Foundation;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Graphics;
using AGI.Foundation.Time;

namespace AGI.Examples.LotsOfSatellites
{
    public class Satellites
    {
        public Satellites()
        {
            m_satellites = new List<MotionEvaluator<Cartesian>>();
            m_accessPositions = new List<Cartesian>();
            m_noAccessPositions = new List<Cartesian>();

            m_accessBatch = new MarkerBatchPrimitive
            {
                Texture = SceneManager.Textures.FromUri(LotsOfSatellites.GetDataFilePath("Markers/Satellite-Red.png"))
            };

            m_noAccessBatch = new MarkerBatchPrimitive
            {
                Texture = SceneManager.Textures.FromUri(LotsOfSatellites.GetDataFilePath("Markers/Satellite.png"))
            };

            SceneManager.Primitives.Add(m_accessBatch);
            SceneManager.Primitives.Add(m_noAccessBatch);
        }

        public void Add(MotionEvaluator<Cartesian> evaluator, JulianDate epoch)
        {
            Cartesian position = evaluator.Evaluate(epoch);
            m_noAccessPositions.Add(position);

            m_satellites.Add(evaluator);
        }

        public void AppendPosition(Cartesian position, bool showAccess)
        {
            if (showAccess)
            {
                m_accessPositions.Add(position);
            }
            else
            {
                m_noAccessPositions.Add(position);
            }
        }

        public void SetMarkerBatches()
        {
            if (m_accessPositions.Count == 0)
            {
                m_accessPositions.Add(Cartesian.Zero);
            }

            if (m_noAccessPositions.Count == 0)
            {
                m_noAccessPositions.Add(Cartesian.Zero);
            }

            m_accessBatch.Set(m_accessPositions);
            m_noAccessBatch.Set(m_noAccessPositions);
        }

        public void ClearPositions()
        {
            m_accessPositions.Clear();
            m_noAccessPositions.Clear();
        }

        public MotionEvaluator<Cartesian> GetSatellite(int index)
        {
            return m_satellites[index];
        }

        public int Count
        {
            get { return m_satellites.Count; }
        }

        public void RemoveUsingIndexList(List<int> satellitesToRemove)
        {
            for (int i = satellitesToRemove.Count - 1; i >= 0; --i)
            {
                m_satellites.RemoveAt(satellitesToRemove[i]);
            }
        }

        public void Clear()
        {
            m_satellites.Clear();
            m_accessPositions.Clear();
            m_noAccessPositions.Clear();
        }

        public void ClearAccesses()
        {
            foreach (Cartesian position in m_accessPositions)
            {
                m_noAccessPositions.Add(position);
            }

            m_accessPositions.Clear();
        }

        private readonly List<MotionEvaluator<Cartesian>> m_satellites;
        private readonly List<Cartesian> m_accessPositions;
        private readonly List<Cartesian> m_noAccessPositions;
        private readonly MarkerBatchPrimitive m_accessBatch;
        private readonly MarkerBatchPrimitive m_noAccessBatch;
    }
}