package agi.examples.numericalpropagationdemo;

import java.awt.BorderLayout;
import java.awt.FlowLayout;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;

import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JTextField;
import javax.swing.border.EmptyBorder;

import agi.foundation.Constants;
import agi.foundation.numericalmethods.IntegrationSense;
import agi.foundation.numericalmethods.KindOfStepSize;
import agi.foundation.numericalmethods.NumericalIntegrator;
import agi.foundation.numericalmethods.RungeKutta4Integrator;
import agi.foundation.numericalmethods.RungeKuttaFehlberg78Integrator;

public class IntegratorSettings extends JDialog {
    /**
     * Create the dialog.
     */
    public IntegratorSettings() {
        setTitle("Integrator Settings");
        setBounds(100, 100, 280, 233);
        getContentPane().setLayout(new BorderLayout());
        contentPanel.setBorder(new EmptyBorder(5, 5, 5, 5));
        getContentPane().add(contentPanel, BorderLayout.CENTER);
        GridBagLayout gbl_contentPanel = new GridBagLayout();
        gbl_contentPanel.columnWidths = new int[] {
                84,
                11,
                72,
                52,
                0
        };
        gbl_contentPanel.rowHeights = new int[] {
                20,
                20,
                20,
                20,
                20,
                20,
                0
        };
        gbl_contentPanel.columnWeights = new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        gbl_contentPanel.rowWeights = new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        contentPanel.setLayout(gbl_contentPanel);

        m_integratorType = new JComboBox<>();

        JLabel lblIntegrator = new JLabel("Integrator:");
        GridBagConstraints gbc_lblIntegrator = new GridBagConstraints();
        gbc_lblIntegrator.anchor = GridBagConstraints.EAST;
        gbc_lblIntegrator.insets = new Insets(0, 0, 5, 5);
        gbc_lblIntegrator.gridx = 0;
        gbc_lblIntegrator.gridy = 0;
        contentPanel.add(lblIntegrator, gbc_lblIntegrator);
        GridBagConstraints gbc_m_integratorType = new GridBagConstraints();
        gbc_m_integratorType.anchor = GridBagConstraints.NORTH;
        gbc_m_integratorType.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_integratorType.insets = new Insets(0, 0, 5, 0);
        gbc_m_integratorType.gridwidth = 3;
        gbc_m_integratorType.gridx = 1;
        gbc_m_integratorType.gridy = 0;
        contentPanel.add(m_integratorType, gbc_m_integratorType);
        m_integratorType.addItem(RKF78);
        m_integratorType.addItem(RK4);

        JLabel lblStepSize = new JLabel("Step Size (sec):");
        GridBagConstraints gbc_lblStepSize = new GridBagConstraints();
        gbc_lblStepSize.anchor = GridBagConstraints.EAST;
        gbc_lblStepSize.insets = new Insets(0, 0, 5, 5);
        gbc_lblStepSize.gridwidth = 2;
        gbc_lblStepSize.gridx = 0;
        gbc_lblStepSize.gridy = 1;
        contentPanel.add(lblStepSize, gbc_lblStepSize);

        m_stepSize = new JTextField();
        GridBagConstraints gbc_m_stepSize = new GridBagConstraints();
        gbc_m_stepSize.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_stepSize.anchor = GridBagConstraints.NORTH;
        gbc_m_stepSize.insets = new Insets(0, 0, 5, 5);
        gbc_m_stepSize.gridx = 2;
        gbc_m_stepSize.gridy = 1;
        contentPanel.add(m_stepSize, gbc_m_stepSize);
        m_stepSize.setColumns(10);

        m_stepSize.setText("60");

        JLabel lblMaxStep = new JLabel("Min Step (sec):");
        GridBagConstraints gbc_lblMaxStep = new GridBagConstraints();
        gbc_lblMaxStep.anchor = GridBagConstraints.EAST;
        gbc_lblMaxStep.insets = new Insets(0, 0, 5, 5);
        gbc_lblMaxStep.gridwidth = 2;
        gbc_lblMaxStep.gridx = 0;
        gbc_lblMaxStep.gridy = 2;
        contentPanel.add(lblMaxStep, gbc_lblMaxStep);

        m_minStep = new JTextField();
        m_minStep.setColumns(10);
        GridBagConstraints gbc_m_minStep = new GridBagConstraints();
        gbc_m_minStep.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_minStep.anchor = GridBagConstraints.NORTH;
        gbc_m_minStep.insets = new Insets(0, 0, 5, 5);
        gbc_m_minStep.gridx = 2;
        gbc_m_minStep.gridy = 2;
        contentPanel.add(m_minStep, gbc_m_minStep);
        m_minStep.setText("1");

        JLabel lblMaxStep_1 = new JLabel("Max Step (sec):");
        GridBagConstraints gbc_lblMaxStep_1 = new GridBagConstraints();
        gbc_lblMaxStep_1.anchor = GridBagConstraints.EAST;
        gbc_lblMaxStep_1.insets = new Insets(0, 0, 5, 5);
        gbc_lblMaxStep_1.gridwidth = 2;
        gbc_lblMaxStep_1.gridx = 0;
        gbc_lblMaxStep_1.gridy = 3;
        contentPanel.add(lblMaxStep_1, gbc_lblMaxStep_1);

        m_maxStep = new JTextField();
        m_maxStep.setColumns(10);
        GridBagConstraints gbc_m_maxStep = new GridBagConstraints();
        gbc_m_maxStep.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_maxStep.anchor = GridBagConstraints.NORTH;
        gbc_m_maxStep.insets = new Insets(0, 0, 5, 5);
        gbc_m_maxStep.gridx = 2;
        gbc_m_maxStep.gridy = 3;
        contentPanel.add(m_maxStep, gbc_m_maxStep);
        m_maxStep.setText("86400");

        JLabel lblMaxError = new JLabel("Max Error:");
        GridBagConstraints gbc_lblMaxError = new GridBagConstraints();
        gbc_lblMaxError.gridwidth = 2;
        gbc_lblMaxError.anchor = GridBagConstraints.EAST;
        gbc_lblMaxError.insets = new Insets(0, 0, 5, 5);
        gbc_lblMaxError.gridx = 0;
        gbc_lblMaxError.gridy = 4;
        contentPanel.add(lblMaxError, gbc_lblMaxError);

        m_maxError = new JTextField();
        m_maxError.setColumns(10);
        GridBagConstraints gbc_m_maxError = new GridBagConstraints();
        gbc_m_maxError.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_maxError.anchor = GridBagConstraints.NORTH;
        gbc_m_maxError.insets = new Insets(0, 0, 5, 5);
        gbc_m_maxError.gridx = 2;
        gbc_m_maxError.gridy = 4;
        contentPanel.add(m_maxError, gbc_m_maxError);
        m_maxError.setText(Double.toString(Constants.Epsilon13));

        JLabel lblStepAdjustemnt = new JLabel("Step Adjustemnt:");
        GridBagConstraints gbc_lblStepAdjustemnt = new GridBagConstraints();
        gbc_lblStepAdjustemnt.anchor = GridBagConstraints.EAST;
        gbc_lblStepAdjustemnt.insets = new Insets(0, 0, 0, 5);
        gbc_lblStepAdjustemnt.gridwidth = 2;
        gbc_lblStepAdjustemnt.gridx = 0;
        gbc_lblStepAdjustemnt.gridy = 5;
        contentPanel.add(lblStepAdjustemnt, gbc_lblStepAdjustemnt);

        m_fixedOrRelative = new JComboBox<>();
        GridBagConstraints gbc_m_fixedOrRelative = new GridBagConstraints();
        gbc_m_fixedOrRelative.insets = new Insets(0, 0, 0, 5);
        gbc_m_fixedOrRelative.anchor = GridBagConstraints.NORTH;
        gbc_m_fixedOrRelative.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_fixedOrRelative.gridx = 2;
        gbc_m_fixedOrRelative.gridy = 5;
        contentPanel.add(m_fixedOrRelative, gbc_m_fixedOrRelative);

        m_fixedOrRelative.addItem(RELATIVE);
        m_fixedOrRelative.addItem(FIXED);
        {
            JPanel buttonPane = new JPanel();
            buttonPane.setLayout(new FlowLayout(FlowLayout.RIGHT));
            getContentPane().add(buttonPane, BorderLayout.SOUTH);
            {
                JButton okButton = new JButton("OK");
                okButton.setSize(75, 23);
                okButton.addActionListener(e -> setVisible(false));
                okButton.setActionCommand("OK");
                buttonPane.add(okButton);
                getRootPane().setDefaultButton(okButton);
            }
        }

        m_integratorType.addItemListener(e -> {
            if (RK4.equals(m_integratorType.getSelectedItem().toString())) {
                m_maxError.setEnabled(false);
                m_maxStep.setEnabled(false);
                m_minStep.setEnabled(false);
                m_fixedOrRelative.setEnabled(false);
            }
            if (RKF78.equals(m_integratorType.getSelectedItem().toString())) {
                m_maxError.setEnabled(true);
                m_maxStep.setEnabled(true);
                m_minStep.setEnabled(true);
                m_fixedOrRelative.setEnabled(true);
            }
        });
    }

    /**
     * Creates a NumericalIntegrator from the entered settings.
     *
     * @return A fully configured NumericalIntegrator based on the current
     *         settings.
     * @throws Exception
     *             If a value in the UI is not properly formated an exception
     *             will be thrown.
     */
    public NumericalIntegrator getIntegrator() {
        if (RK4.equals(m_integratorType.getSelectedItem().toString())) {
            RungeKutta4Integrator integrator = new RungeKutta4Integrator();
            integrator.setInitialStepSize(Double.parseDouble(m_stepSize.getText()));
            return integrator;
        } else if (RKF78.equals(m_integratorType.getSelectedItem().toString())) {
            RungeKuttaFehlberg78Integrator integrator = new RungeKuttaFehlberg78Integrator();
            integrator.setDirection(IntegrationSense.INCREASING);
            integrator.setInitialStepSize(Double.parseDouble(m_stepSize.getText()));
            integrator.setMaximumStepSize(Double.parseDouble(m_maxStep.getText()));
            integrator.setMinimumStepSize(Double.parseDouble(m_minStep.getText()));
            if (RELATIVE.equals(m_fixedOrRelative.getSelectedItem().toString())) {
                integrator.setStepSizeBehavior(KindOfStepSize.RELATIVE);
            } else if (FIXED.equals(m_fixedOrRelative.getSelectedItem().toString())) {
                integrator.setStepSizeBehavior(KindOfStepSize.FIXED);
            }
            return integrator;
        } else {
            throw new RuntimeException("Unknown integrator");
        }
    }

    private static final long serialVersionUID = 1L;
    private final JPanel contentPanel = new JPanel();
    private final JTextField m_stepSize;
    private final JTextField m_minStep;
    private final JTextField m_maxStep;
    private final JTextField m_maxError;

    private final String RKF78 = "Runge-Kutta-Fehlberg 7/8";
    private final String RK4 = "Runge-Kutta 4";
    private final String FIXED = "Fixed";
    private final String RELATIVE = "Relative";
    private final JComboBox<String> m_fixedOrRelative;
    private final JComboBox<String> m_integratorType;
}
