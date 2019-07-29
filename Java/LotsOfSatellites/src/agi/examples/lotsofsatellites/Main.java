package agi.examples.lotsofsatellites;

import java.awt.Dimension;
import java.io.PrintWriter;
import java.io.StringWriter;

import javax.swing.JOptionPane;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.SwingUtilities;

import agi.examples.EarthOrientationParametersHelper;
import agi.examples.LeapSecondsFacetHelper;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.JplDE430;

public class Main {
    public static void main(String[] args) {
        SwingUtilities.invokeLater(Main::createAndShowGUI);
    }

    private static void createAndShowGUI() {
        // install an exception handler that will create and display a dialog
        // containing the exception stack trace
        Thread.currentThread().setUncaughtExceptionHandler((t, e) -> {
            e.printStackTrace();

            StringWriter sw = new StringWriter();
            e.printStackTrace(new PrintWriter(sw));

            JTextArea textArea = new JTextArea(sw.toString());
            JScrollPane scrollPane = new JScrollPane(textArea);
            scrollPane.setPreferredSize(new Dimension(700, 300));
            textArea.setEditable(false);
            JOptionPane.showMessageDialog(null, scrollPane, "Unhandled Exception", JOptionPane.ERROR_MESSAGE);
        });

        // startup data configuration

        // Update LeapSecond.dat, and use it in the current calculation context.
        LeapSecondsFacetHelper.getLeapSeconds().useInCurrentContext();

        EarthCentralBody earth = CentralBodiesFacet.getFromContext().getEarth();

        // Load EOP Data - For fixed to inertial transformations
        earth.setOrientationParameters(EarthOrientationParametersHelper.getEarthOrientationParameters());

        // Load JPL data
        // Optional - Without this an analytic model is used to position central bodies
        JplDE430 jpl = new JplDE430("Data/plneph.430");
        jpl.useForCentralBodyPositions(CentralBodiesFacet.getFromContext());

        LotsOfSatellites app = new LotsOfSatellites();
        app.setVisible(true);
    }
}
