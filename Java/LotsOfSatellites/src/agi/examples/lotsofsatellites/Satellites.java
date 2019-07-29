package agi.examples.lotsofsatellites;

import java.util.ArrayList;

import agi.foundation.MotionEvaluator1;
import agi.foundation.coordinates.Cartesian;
import agi.foundation.graphics.MarkerBatchPrimitive;
import agi.foundation.graphics.SceneManager;
import agi.foundation.time.JulianDate;

public class Satellites {
    public Satellites() {
        m_satellites = new ArrayList<>();
        m_accessPositions = new ArrayList<>();
        m_noAccessPositions = new ArrayList<>();

        m_accessBatch = new MarkerBatchPrimitive();
        m_accessBatch.setTexture(SceneManager.getTextures().fromUri(LotsOfSatellites.getDataFilePath("Markers/Satellite-Red.png")));

        m_noAccessBatch = new MarkerBatchPrimitive();
        m_noAccessBatch.setTexture(SceneManager.getTextures().fromUri(LotsOfSatellites.getDataFilePath("Markers/Satellite.png")));

        SceneManager.getPrimitives().add(m_accessBatch);
        SceneManager.getPrimitives().add(m_noAccessBatch);
    }

    public void add(MotionEvaluator1<Cartesian> evaluator, JulianDate epoch) {
        Cartesian position = evaluator.evaluate(epoch);
        m_noAccessPositions.add(position);
        m_satellites.add(evaluator);
    }

    public void appendPosition(Cartesian position, boolean showAccess) {
        if (showAccess) {
            m_accessPositions.add(position);
        } else {
            m_noAccessPositions.add(position);
        }
    }

    public void setMarkerBatches() {
        if (m_accessPositions.size() == 0) {
            m_accessPositions.add(Cartesian.getZero());
        }

        if (m_noAccessPositions.size() == 0) {
            m_noAccessPositions.add(Cartesian.getZero());
        }

        m_accessBatch.set(m_accessPositions);
        m_noAccessBatch.set(m_noAccessPositions);
    }

    public final void clearPositions() {
        m_accessPositions.clear();
        m_noAccessPositions.clear();
    }

    public final MotionEvaluator1<Cartesian> getSatellite(int index) {
        return m_satellites.get(index);
    }

    public final int getCount() {
        return m_satellites.size();
    }

    public final void removeUsingIndexList(ArrayList<Integer> satellitesToRemove) {
        for (int i = satellitesToRemove.size() - 1; i >= 0; --i) {
            m_satellites.remove((int)satellitesToRemove.get(i));
        }
    }

    public final void clear() {
        m_satellites.clear();
        m_accessPositions.clear();
        m_noAccessPositions.clear();
    }

    public final void clearAccesses() {
        for (Cartesian position : m_accessPositions) {
            m_noAccessPositions.add(position);
        }

        m_accessPositions.clear();
    }

    private final ArrayList<MotionEvaluator1<Cartesian>> m_satellites;
    private final ArrayList<Cartesian> m_accessPositions;
    private final ArrayList<Cartesian> m_noAccessPositions;
    private final MarkerBatchPrimitive m_accessBatch;
    private final MarkerBatchPrimitive m_noAccessBatch;
}