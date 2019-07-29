package agi.examples.numericalpropagationdemo;

import java.awt.Dimension;
import java.io.PrintWriter;
import java.io.StringWriter;

import javax.swing.JOptionPane;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;

import agi.examples.EarthOrientationParametersHelper;
import agi.examples.LeapSecondsFacetHelper;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.EarthCentralBody;

public class Main {
    public static void main(String[] args) throws Exception {
        UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
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

        NumericalPropagationDemo app = new NumericalPropagationDemo();
        app.setVisible(true);
    }
}
