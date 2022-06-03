package agi.examples.numericalpropagationdemo;

import java.awt.BorderLayout;
import java.awt.FlowLayout;
import java.awt.Font;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.swing.ButtonGroup;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JComboBox;
import javax.swing.JDialog;
import javax.swing.JFileChooser;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JRadioButton;
import javax.swing.JTextField;
import javax.swing.border.EmptyBorder;
import javax.swing.border.EtchedBorder;

import com.jgoodies.forms.factories.FormFactory;
import com.jgoodies.forms.layout.ColumnSpec;
import com.jgoodies.forms.layout.FormLayout;
import com.jgoodies.forms.layout.RowSpec;

import agi.foundation.celestial.AtmosphericDragForce;
import agi.foundation.celestial.CentralBodiesFacet;
import agi.foundation.celestial.CentralBody;
import agi.foundation.celestial.ConstantSolarGeophysicalData;
import agi.foundation.celestial.EarthCentralBody;
import agi.foundation.celestial.Iers2003SolidTideModel;
import agi.foundation.celestial.JplDE;
import agi.foundation.celestial.JplDECentralBody;
import agi.foundation.celestial.PermanentSolidTideModel;
import agi.foundation.celestial.ScalarAtmosphericDensity;
import agi.foundation.celestial.ScalarDensityJacchiaRoberts;
import agi.foundation.celestial.ScalarDensityMsis2000;
import agi.foundation.celestial.ScalarDensityMsis86;
import agi.foundation.celestial.ScalarDensityMsis90;
import agi.foundation.celestial.ScalarOccultationCylindrical;
import agi.foundation.celestial.ScalarOccultationDualCone;
import agi.foundation.celestial.SimpleSolarRadiationForce;
import agi.foundation.celestial.SolarGeophysicalData;
import agi.foundation.celestial.SolidTideModel;
import agi.foundation.celestial.SphericalHarmonicGravity;
import agi.foundation.celestial.SphericalHarmonicGravityField;
import agi.foundation.celestial.SphericalHarmonicGravityModel;
import agi.foundation.celestial.SphericalHarmonicsTideType;
import agi.foundation.celestial.ThirdBodyGravity;
import agi.foundation.celestial.TwoBodyGravity;
import agi.foundation.geometry.Scalar;
import agi.foundation.geometry.ScalarFixed;
import agi.foundation.propagators.PropagationNewtonianPoint;

public class ForceModelSettings extends JDialog {

    /**
     * Create the dialog.
     *
     * @param jplInfo
     *            The JPL Ephemeris object.
     * @param gravityFile
     *            The spherical harmonic gravity file.
     */
    public ForceModelSettings(JplDE jplInfo, String gravityFile) {
        m_gravConstants.put(JplDECentralBody.EARTH.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.EARTH));
        m_gravConstants.put(JplDECentralBody.SUN.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.SUN));
        m_gravConstants.put(JplDECentralBody.MOON.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.MOON));
        m_gravConstants.put(JplDECentralBody.JUPITER.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.JUPITER));
        m_gravConstants.put(JplDECentralBody.MARS.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.MARS));
        m_gravConstants.put(JplDECentralBody.MERCURY.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.MERCURY));
        m_gravConstants.put(JplDECentralBody.NEPTUNE.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.NEPTUNE));
        m_gravConstants.put(JplDECentralBody.PLUTO.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.PLUTO));
        m_gravConstants.put(JplDECentralBody.SATURN.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.SATURN));
        m_gravConstants.put(JplDECentralBody.URANUS.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.URANUS));
        m_gravConstants.put(JplDECentralBody.VENUS.toString(), jplInfo.getGravitationalParameter(JplDECentralBody.VENUS));

        m_nameToJplMapping.put(EARTH, JplDECentralBody.EARTH);
        m_nameToJplMapping.put(SUN, JplDECentralBody.SUN);
        m_nameToJplMapping.put(MOON, JplDECentralBody.MOON);
        m_nameToJplMapping.put("Jupiter", JplDECentralBody.JUPITER);
        m_nameToJplMapping.put("Mars", JplDECentralBody.MARS);
        m_nameToJplMapping.put("Mercury", JplDECentralBody.MERCURY);
        m_nameToJplMapping.put("Neptune", JplDECentralBody.NEPTUNE);
        m_nameToJplMapping.put("Pluto", JplDECentralBody.PLUTO);
        m_nameToJplMapping.put("Saturn", JplDECentralBody.SATURN);
        m_nameToJplMapping.put("Uranus", JplDECentralBody.URANUS);
        m_nameToJplMapping.put("Venus", JplDECentralBody.VENUS);

        m_jplInfo = jplInfo;

        setBounds(100, 100, 546, 594);
        getContentPane().setLayout(new BorderLayout());
        contentPanel.setBorder(new EmptyBorder(5, 5, 5, 5));
        getContentPane().add(contentPanel, BorderLayout.CENTER);
        contentPanel.setLayout(new FormLayout(new ColumnSpec[] {
                FormFactory.GLUE_COLSPEC,
                FormFactory.GLUE_COLSPEC,
        }, new RowSpec[] {
                FormFactory.GLUE_ROWSPEC,
                FormFactory.GLUE_ROWSPEC,
        }));
        {
            JPanel panel = new JPanel();
            panel.setBorder(new EtchedBorder(EtchedBorder.LOWERED, null, null));
            contentPanel.add(panel, "1, 1, fill, fill");
            {
                GridBagLayout gbl_panel = new GridBagLayout();
                gbl_panel.columnWidths = new int[] {
                        5,
                        70,
                        40,
                        30,
                        50,
                        33,
                        0
                };
                gbl_panel.rowHeights = new int[] {
                        28,
                        20,
                        23,
                        23,
                        14,
                        23,
                        20,
                        20,
                        20,
                        0
                };
                gbl_panel.columnWeights = new double[] {
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        Double.MIN_VALUE
                };
                gbl_panel.rowWeights = new double[] {
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        Double.MIN_VALUE
                };
                panel.setLayout(gbl_panel);

                JLabel lblCentralBodyGravity = new JLabel("Central Body Gravity");
                lblCentralBodyGravity.setFont(new Font("Tahoma", Font.PLAIN, 11));
                GridBagConstraints gbc_lblCentralBodyGravity = new GridBagConstraints();
                gbc_lblCentralBodyGravity.anchor = GridBagConstraints.EAST;
                gbc_lblCentralBodyGravity.insets = new Insets(0, 0, 5, 5);
                gbc_lblCentralBodyGravity.gridwidth = 2;
                gbc_lblCentralBodyGravity.gridx = 1;
                gbc_lblCentralBodyGravity.gridy = 0;
                panel.add(lblCentralBodyGravity, gbc_lblCentralBodyGravity);
                {
                    JLabel lblNewLabel = new JLabel("Central Body:");
                    GridBagConstraints gbc_lblNewLabel = new GridBagConstraints();
                    gbc_lblNewLabel.anchor = GridBagConstraints.EAST;
                    gbc_lblNewLabel.insets = new Insets(0, 0, 5, 5);
                    gbc_lblNewLabel.gridwidth = 2;
                    gbc_lblNewLabel.gridx = 1;
                    gbc_lblNewLabel.gridy = 1;
                    panel.add(lblNewLabel, gbc_lblNewLabel);
                }
                m_centralBodySelection = new JComboBox<>();

                GridBagConstraints gbc_m_centralBodySelection = new GridBagConstraints();
                gbc_m_centralBodySelection.anchor = GridBagConstraints.NORTH;
                gbc_m_centralBodySelection.fill = GridBagConstraints.HORIZONTAL;
                gbc_m_centralBodySelection.insets = new Insets(0, 0, 5, 5);
                gbc_m_centralBodySelection.gridwidth = 2;
                gbc_m_centralBodySelection.gridx = 3;
                gbc_m_centralBodySelection.gridy = 1;
                panel.add(m_centralBodySelection, gbc_m_centralBodySelection);
                m_centralBodySelection.addItem(EARTH);
                m_centralBodySelection.addItem(MOON);

                m_twoBodyGravity = new JRadioButton("Two Body Gravity");

                ButtonGroup group = new ButtonGroup();
                GridBagConstraints gbc_m_twoBodyGravity = new GridBagConstraints();
                gbc_m_twoBodyGravity.anchor = GridBagConstraints.NORTHWEST;
                gbc_m_twoBodyGravity.insets = new Insets(0, 0, 5, 5);
                gbc_m_twoBodyGravity.gridwidth = 2;
                gbc_m_twoBodyGravity.gridx = 1;
                gbc_m_twoBodyGravity.gridy = 2;
                panel.add(m_twoBodyGravity, gbc_m_twoBodyGravity);
                group.add(m_twoBodyGravity);

                m_gravityField = new JRadioButton("Gravity Field");
                m_gravityField.setSelected(true);
                GridBagConstraints gbc_m_gravityField = new GridBagConstraints();
                gbc_m_gravityField.anchor = GridBagConstraints.NORTHWEST;
                gbc_m_gravityField.insets = new Insets(0, 0, 5, 5);
                gbc_m_gravityField.gridwidth = 2;
                gbc_m_gravityField.gridx = 1;
                gbc_m_gravityField.gridy = 3;
                panel.add(m_gravityField, gbc_m_gravityField);
                group.add(m_gravityField);
                {
                    JLabel lblNewLabel_1 = new JLabel("Gravity File:");
                    GridBagConstraints gbc_lblNewLabel_1 = new GridBagConstraints();
                    gbc_lblNewLabel_1.anchor = GridBagConstraints.EAST;
                    gbc_lblNewLabel_1.insets = new Insets(0, 0, 5, 5);
                    gbc_lblNewLabel_1.gridx = 1;
                    gbc_lblNewLabel_1.gridy = 4;
                    panel.add(lblNewLabel_1, gbc_lblNewLabel_1);
                }

                m_centralBodySelection.addItemListener(e -> {
                    String newCBName = m_centralBodySelection.getSelectedItem().toString();
                    if (newCBName.equals(EARTH)) {
                        m_earth3B.setEnabled(false);
                        m_moon3B.setEnabled(true);
                        m_dragUse.setEnabled(true);
                        m_dragUse.setSelected(m_lastDragSelection);
                        m_tides.setEnabled(true);
                        m_centralBody = CentralBodiesFacet.getFromContext().getEarth();
                    }
                    if (newCBName.equals(MOON)) {
                        m_earth3B.setEnabled(true);
                        m_lastDragSelection = m_dragUse.isSelected() || m_lastDragSelection;
                        m_dragUse.setSelected(false);
                        m_dragUse.setEnabled(false);
                        m_moon3B.setEnabled(false);
                        m_tides.setEnabled(false);
                        m_centralBody = CentralBodiesFacet.getFromContext().getMoon();
                    }
                    m_gravitationalParameter = m_gravConstants.get(newCBName.toUpperCase());
                });

                final JButton m_findGravFile = new JButton("Browse");
                m_findGravFile.setSize(75, 23);
                m_findGravFile.addActionListener(e -> {
                    JFileChooser fileChooser = new JFileChooser();
                    fileChooser.setCurrentDirectory(new File(m_gravityFieldFile.getText()).getAbsoluteFile().getParentFile());
                    int returnVal = fileChooser.showOpenDialog(null);
                    if (returnVal == JFileChooser.APPROVE_OPTION) {
                        m_gravityFieldFile.setText(fileChooser.getSelectedFile().getAbsolutePath());
                    }
                });

                m_twoBodyGravity.addChangeListener(e -> {
                    if (m_twoBodyGravity.isSelected()) {
                        m_gravityFieldFile.setEnabled(false);
                        m_degree.setEnabled(false);
                        m_order.setEnabled(false);
                        m_tides.setEnabled(false);
                        m_findGravFile.setEnabled(false);
                    } else {
                        m_gravityFieldFile.setEnabled(true);
                        m_degree.setEnabled(true);
                        m_order.setEnabled(true);
                        m_tides.setEnabled(true);
                        m_findGravFile.setEnabled(true);
                    }
                });

                GridBagConstraints gbc_m_findGravFile = new GridBagConstraints();
                gbc_m_findGravFile.gridwidth = 2;
                gbc_m_findGravFile.anchor = GridBagConstraints.NORTH;
                gbc_m_findGravFile.fill = GridBagConstraints.HORIZONTAL;
                gbc_m_findGravFile.insets = new Insets(0, 0, 5, 5);
                gbc_m_findGravFile.gridx = 2;
                gbc_m_findGravFile.gridy = 4;
                panel.add(m_findGravFile, gbc_m_findGravFile);
                {
                    m_gravityFieldFile = new JTextField();
                    m_gravityFieldFile.setEditable(false);
                    GridBagConstraints gbc_m_gravityFieldFile = new GridBagConstraints();
                    gbc_m_gravityFieldFile.fill = GridBagConstraints.BOTH;
                    gbc_m_gravityFieldFile.insets = new Insets(0, 0, 5, 0);
                    gbc_m_gravityFieldFile.gridwidth = 5;
                    gbc_m_gravityFieldFile.gridx = 1;
                    gbc_m_gravityFieldFile.gridy = 5;
                    panel.add(m_gravityFieldFile, gbc_m_gravityFieldFile);
                    m_gravityFieldFile.setColumns(10);
                    m_gravityFieldFile.setText(gravityFile);
                }
                {
                    JLabel lblNewLabel_2 = new JLabel("Order:");
                    GridBagConstraints gbc_lblNewLabel_2 = new GridBagConstraints();
                    gbc_lblNewLabel_2.gridwidth = 2;
                    gbc_lblNewLabel_2.anchor = GridBagConstraints.EAST;
                    gbc_lblNewLabel_2.insets = new Insets(0, 0, 5, 5);
                    gbc_lblNewLabel_2.gridx = 1;
                    gbc_lblNewLabel_2.gridy = 6;
                    panel.add(lblNewLabel_2, gbc_lblNewLabel_2);
                }
                {
                    m_order = new JTextField();
                    GridBagConstraints gbc_m_order = new GridBagConstraints();
                    gbc_m_order.anchor = GridBagConstraints.NORTH;
                    gbc_m_order.fill = GridBagConstraints.HORIZONTAL;
                    gbc_m_order.insets = new Insets(0, 0, 5, 5);
                    gbc_m_order.gridwidth = 2;
                    gbc_m_order.gridx = 3;
                    gbc_m_order.gridy = 6;
                    panel.add(m_order, gbc_m_order);
                    m_order.setColumns(10);
                }

                m_order.setText("21");
                {
                    JLabel lblNewLabel_3 = new JLabel("Degree:");
                    GridBagConstraints gbc_lblNewLabel_3 = new GridBagConstraints();
                    gbc_lblNewLabel_3.gridwidth = 2;
                    gbc_lblNewLabel_3.anchor = GridBagConstraints.EAST;
                    gbc_lblNewLabel_3.insets = new Insets(0, 0, 5, 5);
                    gbc_lblNewLabel_3.gridx = 1;
                    gbc_lblNewLabel_3.gridy = 7;
                    panel.add(lblNewLabel_3, gbc_lblNewLabel_3);
                }
                {
                    m_degree = new JTextField();
                    GridBagConstraints gbc_m_degree = new GridBagConstraints();
                    gbc_m_degree.anchor = GridBagConstraints.NORTH;
                    gbc_m_degree.fill = GridBagConstraints.HORIZONTAL;
                    gbc_m_degree.insets = new Insets(0, 0, 5, 5);
                    gbc_m_degree.gridwidth = 2;
                    gbc_m_degree.gridx = 3;
                    gbc_m_degree.gridy = 7;
                    panel.add(m_degree, gbc_m_degree);
                    m_degree.setColumns(10);
                }
                m_degree.setText("21");
                {
                    JLabel lblTides = new JLabel("Tides:");
                    GridBagConstraints gbc_lblTides = new GridBagConstraints();
                    gbc_lblTides.gridwidth = 2;
                    gbc_lblTides.anchor = GridBagConstraints.EAST;
                    gbc_lblTides.insets = new Insets(0, 0, 0, 5);
                    gbc_lblTides.gridx = 1;
                    gbc_lblTides.gridy = 8;
                    panel.add(lblTides, gbc_lblTides);
                }
                {
                    m_tides = new JComboBox<>();
                    GridBagConstraints gbc_m_tides = new GridBagConstraints();
                    gbc_m_tides.anchor = GridBagConstraints.NORTH;
                    gbc_m_tides.fill = GridBagConstraints.HORIZONTAL;
                    gbc_m_tides.insets = new Insets(0, 0, 0, 5);
                    gbc_m_tides.gridwidth = 2;
                    gbc_m_tides.gridx = 3;
                    gbc_m_tides.gridy = 8;
                    panel.add(m_tides, gbc_m_tides);
                }
                m_tides.addItem(PERMANENTTIDES);
                m_tides.addItem(NOTIDES);
                m_tides.addItem(TIMEVARYINGTIDES);
            }

        }

        JPanel panel_1 = new JPanel();
        panel_1.setBorder(new EtchedBorder(EtchedBorder.LOWERED, null, null));
        contentPanel.add(panel_1, "2, 1, fill, fill");
        GridBagLayout gbl_panel_1 = new GridBagLayout();
        gbl_panel_1.columnWidths = new int[] {
                5,
                65,
                19,
                95,
                0
        };
        gbl_panel_1.rowHeights = new int[] {
                23,
                20,
                20,
                14,
                23,
                23,
                23,
                23,
                23,
                0
        };
        gbl_panel_1.columnWeights = new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        gbl_panel_1.rowWeights = new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        panel_1.setLayout(gbl_panel_1);

        JLabel lblSolarRadiationForce = new JLabel("Solar Radiation Force");
        GridBagConstraints gbc_lblSolarRadiationForce = new GridBagConstraints();
        gbc_lblSolarRadiationForce.anchor = GridBagConstraints.EAST;
        gbc_lblSolarRadiationForce.insets = new Insets(0, 0, 5, 5);
        gbc_lblSolarRadiationForce.gridwidth = 2;
        gbc_lblSolarRadiationForce.gridx = 1;
        gbc_lblSolarRadiationForce.gridy = 0;
        panel_1.add(lblSolarRadiationForce, gbc_lblSolarRadiationForce);

        m_srpUse = new JCheckBox("Use");
        m_srpUse.addChangeListener(e -> {
            if (m_srpUse.isSelected()) {
                m_shadowModel.setEnabled(true);
                m_srpReflectivity.setEnabled(true);
                for (JCheckBox box1 : m_srpOcculingCheckBoxes) {
                    box1.setEnabled(true);
                }
            } else {
                m_shadowModel.setEnabled(false);
                m_srpReflectivity.setEnabled(false);
                for (JCheckBox box2 : m_srpOcculingCheckBoxes) {
                    box2.setEnabled(false);
                }
            }
        });
        GridBagConstraints gbc_m_srpUse = new GridBagConstraints();
        gbc_m_srpUse.anchor = GridBagConstraints.NORTH;
        gbc_m_srpUse.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_srpUse.insets = new Insets(0, 0, 5, 0);
        gbc_m_srpUse.gridx = 3;
        gbc_m_srpUse.gridy = 0;
        panel_1.add(m_srpUse, gbc_m_srpUse);

        JLabel lblShadowModel = new JLabel("Shadow Model:");
        GridBagConstraints gbc_lblShadowModel = new GridBagConstraints();
        gbc_lblShadowModel.anchor = GridBagConstraints.EAST;
        gbc_lblShadowModel.insets = new Insets(0, 0, 5, 5);
        gbc_lblShadowModel.gridwidth = 2;
        gbc_lblShadowModel.gridx = 1;
        gbc_lblShadowModel.gridy = 1;
        panel_1.add(lblShadowModel, gbc_lblShadowModel);

        m_shadowModel = new JComboBox<>();
        GridBagConstraints gbc_m_shadowModel = new GridBagConstraints();
        gbc_m_shadowModel.anchor = GridBagConstraints.NORTH;
        gbc_m_shadowModel.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_shadowModel.insets = new Insets(0, 0, 5, 0);
        gbc_m_shadowModel.gridx = 3;
        gbc_m_shadowModel.gridy = 1;
        panel_1.add(m_shadowModel, gbc_m_shadowModel);

        m_shadowModel.addItem(DUALCONE);
        m_shadowModel.addItem(CYLINDRICAL);

        JLabel lblReflectivity = new JLabel("Reflectivity:");
        GridBagConstraints gbc_lblReflectivity = new GridBagConstraints();
        gbc_lblReflectivity.gridwidth = 2;
        gbc_lblReflectivity.anchor = GridBagConstraints.EAST;
        gbc_lblReflectivity.insets = new Insets(0, 0, 5, 5);
        gbc_lblReflectivity.gridx = 1;
        gbc_lblReflectivity.gridy = 2;
        panel_1.add(lblReflectivity, gbc_lblReflectivity);

        m_srpReflectivity = new JTextField();
        GridBagConstraints gbc_m_srpReflectivity = new GridBagConstraints();
        gbc_m_srpReflectivity.anchor = GridBagConstraints.NORTH;
        gbc_m_srpReflectivity.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_srpReflectivity.insets = new Insets(0, 0, 5, 0);
        gbc_m_srpReflectivity.gridx = 3;
        gbc_m_srpReflectivity.gridy = 2;
        panel_1.add(m_srpReflectivity, gbc_m_srpReflectivity);
        m_srpReflectivity.setColumns(10);
        m_srpReflectivity.setText("1");

        JLabel lblOccludingBodies = new JLabel("Occluding Bodies:");
        GridBagConstraints gbc_lblOccludingBodies = new GridBagConstraints();
        gbc_lblOccludingBodies.anchor = GridBagConstraints.NORTHEAST;
        gbc_lblOccludingBodies.insets = new Insets(0, 0, 5, 5);
        gbc_lblOccludingBodies.gridwidth = 2;
        gbc_lblOccludingBodies.gridx = 1;
        gbc_lblOccludingBodies.gridy = 3;
        panel_1.add(lblOccludingBodies, gbc_lblOccludingBodies);

        m_earthSRP = new JCheckBox("Earth");
        GridBagConstraints gbc_m_earthSRP = new GridBagConstraints();
        gbc_m_earthSRP.anchor = GridBagConstraints.NORTH;
        gbc_m_earthSRP.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_earthSRP.insets = new Insets(0, 0, 5, 5);
        gbc_m_earthSRP.gridx = 1;
        gbc_m_earthSRP.gridy = 4;
        panel_1.add(m_earthSRP, gbc_m_earthSRP);

        m_srpOcculingCheckBoxes.add(m_earthSRP);

        m_moonSRP = new JCheckBox("Moon");
        GridBagConstraints gbc_m_moonSRP = new GridBagConstraints();
        gbc_m_moonSRP.anchor = GridBagConstraints.NORTH;
        gbc_m_moonSRP.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_moonSRP.insets = new Insets(0, 0, 5, 5);
        gbc_m_moonSRP.gridx = 1;
        gbc_m_moonSRP.gridy = 5;
        panel_1.add(m_moonSRP, gbc_m_moonSRP);
        m_srpOcculingCheckBoxes.add(m_moonSRP);
        {
            JPanel panel = new JPanel();
            panel.setBorder(new EtchedBorder(EtchedBorder.LOWERED, null, null));
            contentPanel.add(panel, "1, 2, fill, fill");
            GridBagLayout gbl_panel = new GridBagLayout();
            gbl_panel.columnWidths = new int[] {
                    2,
                    72,
                    97,
                    0
            };
            gbl_panel.rowHeights = new int[] {
                    27,
                    23,
                    23,
                    23,
                    23,
                    23,
                    23,
                    0
            };
            gbl_panel.columnWeights = new double[] {
                    0.0,
                    0.0,
                    0.0,
                    Double.MIN_VALUE
            };
            gbl_panel.rowWeights = new double[] {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    Double.MIN_VALUE
            };
            panel.setLayout(gbl_panel);

            JLabel lblNewLabel_4 = new JLabel("Third Body Gravity");
            GridBagConstraints gbc_lblNewLabel_4 = new GridBagConstraints();
            gbc_lblNewLabel_4.gridwidth = 2;
            gbc_lblNewLabel_4.anchor = GridBagConstraints.WEST;
            gbc_lblNewLabel_4.insets = new Insets(0, 0, 5, 0);
            gbc_lblNewLabel_4.gridx = 1;
            gbc_lblNewLabel_4.gridy = 0;
            panel.add(lblNewLabel_4, gbc_lblNewLabel_4);

            m_earth3B = new JCheckBox("Earth");
            GridBagConstraints gbc_m_earth3B = new GridBagConstraints();
            gbc_m_earth3B.anchor = GridBagConstraints.NORTHWEST;
            gbc_m_earth3B.insets = new Insets(0, 0, 5, 5);
            gbc_m_earth3B.gridx = 1;
            gbc_m_earth3B.gridy = 1;
            panel.add(m_earth3B, gbc_m_earth3B);

            m_thirdBodyCheckBoxes.add(m_earth3B);

            m_moon3B = new JCheckBox("Moon");
            GridBagConstraints gbc_m_moon3B = new GridBagConstraints();
            gbc_m_moon3B.anchor = GridBagConstraints.NORTHWEST;
            gbc_m_moon3B.insets = new Insets(0, 0, 5, 5);
            gbc_m_moon3B.gridx = 1;
            gbc_m_moon3B.gridy = 2;
            panel.add(m_moon3B, gbc_m_moon3B);
            m_thirdBodyCheckBoxes.add(m_moon3B);

            m_jupiter3B = new JCheckBox("Jupiter");
            GridBagConstraints gbc_m_jupiter3B = new GridBagConstraints();
            gbc_m_jupiter3B.anchor = GridBagConstraints.NORTH;
            gbc_m_jupiter3B.fill = GridBagConstraints.HORIZONTAL;
            gbc_m_jupiter3B.insets = new Insets(0, 0, 5, 0);
            gbc_m_jupiter3B.gridx = 2;
            gbc_m_jupiter3B.gridy = 2;
            panel.add(m_jupiter3B, gbc_m_jupiter3B);
            m_thirdBodyCheckBoxes.add(m_jupiter3B);

            m_sun3B = new JCheckBox("Sun");
            GridBagConstraints gbc_m_sun3B = new GridBagConstraints();
            gbc_m_sun3B.anchor = GridBagConstraints.NORTHWEST;
            gbc_m_sun3B.insets = new Insets(0, 0, 5, 5);
            gbc_m_sun3B.gridx = 1;
            gbc_m_sun3B.gridy = 3;
            panel.add(m_sun3B, gbc_m_sun3B);
            m_thirdBodyCheckBoxes.add(m_sun3B);

            m_saturn3B = new JCheckBox("Saturn");
            GridBagConstraints gbc_m_saturn3B = new GridBagConstraints();
            gbc_m_saturn3B.anchor = GridBagConstraints.NORTH;
            gbc_m_saturn3B.fill = GridBagConstraints.HORIZONTAL;
            gbc_m_saturn3B.insets = new Insets(0, 0, 5, 0);
            gbc_m_saturn3B.gridx = 2;
            gbc_m_saturn3B.gridy = 3;
            panel.add(m_saturn3B, gbc_m_saturn3B);
            m_thirdBodyCheckBoxes.add(m_saturn3B);

            m_mercury3B = new JCheckBox("Mercury");
            GridBagConstraints gbc_m_mercury3B = new GridBagConstraints();
            gbc_m_mercury3B.anchor = GridBagConstraints.NORTHWEST;
            gbc_m_mercury3B.insets = new Insets(0, 0, 5, 5);
            gbc_m_mercury3B.gridx = 1;
            gbc_m_mercury3B.gridy = 4;
            panel.add(m_mercury3B, gbc_m_mercury3B);
            m_thirdBodyCheckBoxes.add(m_mercury3B);

            m_uranus3B = new JCheckBox("Uranus");
            GridBagConstraints gbc_m_uranus3B = new GridBagConstraints();
            gbc_m_uranus3B.anchor = GridBagConstraints.NORTH;
            gbc_m_uranus3B.fill = GridBagConstraints.HORIZONTAL;
            gbc_m_uranus3B.insets = new Insets(0, 0, 5, 0);
            gbc_m_uranus3B.gridx = 2;
            gbc_m_uranus3B.gridy = 4;
            panel.add(m_uranus3B, gbc_m_uranus3B);
            m_thirdBodyCheckBoxes.add(m_uranus3B);

            m_venus3B = new JCheckBox("Venus");
            GridBagConstraints gbc_m_venus3B = new GridBagConstraints();
            gbc_m_venus3B.anchor = GridBagConstraints.NORTHWEST;
            gbc_m_venus3B.insets = new Insets(0, 0, 5, 5);
            gbc_m_venus3B.gridx = 1;
            gbc_m_venus3B.gridy = 5;
            panel.add(m_venus3B, gbc_m_venus3B);
            m_thirdBodyCheckBoxes.add(m_venus3B);

            m_neptune3B = new JCheckBox("Neptune");
            GridBagConstraints gbc_m_neptune3B = new GridBagConstraints();
            gbc_m_neptune3B.anchor = GridBagConstraints.NORTH;
            gbc_m_neptune3B.fill = GridBagConstraints.HORIZONTAL;
            gbc_m_neptune3B.insets = new Insets(0, 0, 5, 0);
            gbc_m_neptune3B.gridx = 2;
            gbc_m_neptune3B.gridy = 5;
            panel.add(m_neptune3B, gbc_m_neptune3B);
            m_thirdBodyCheckBoxes.add(m_neptune3B);

            m_mars3B = new JCheckBox("Mars");
            GridBagConstraints gbc_m_mars3B = new GridBagConstraints();
            gbc_m_mars3B.anchor = GridBagConstraints.NORTHWEST;
            gbc_m_mars3B.insets = new Insets(0, 0, 0, 5);
            gbc_m_mars3B.gridx = 1;
            gbc_m_mars3B.gridy = 6;
            panel.add(m_mars3B, gbc_m_mars3B);
            m_thirdBodyCheckBoxes.add(m_mars3B);

            m_pluto3B = new JCheckBox("Pluto");
            GridBagConstraints gbc_m_pluto3B = new GridBagConstraints();
            gbc_m_pluto3B.anchor = GridBagConstraints.NORTH;
            gbc_m_pluto3B.fill = GridBagConstraints.HORIZONTAL;
            gbc_m_pluto3B.gridx = 2;
            gbc_m_pluto3B.gridy = 6;
            panel.add(m_pluto3B, gbc_m_pluto3B);
            m_thirdBodyCheckBoxes.add(m_pluto3B);
        }

        JPanel panel = new JPanel();
        panel.setBorder(new EtchedBorder(EtchedBorder.LOWERED, null, null));
        contentPanel.add(panel, "2, 2, fill, fill");
        GridBagLayout gbl_panel = new GridBagLayout();
        gbl_panel.columnWidths = new int[] {
                86,
                123,
                0
        };
        gbl_panel.rowHeights = new int[] {
                23,
                20,
                20,
                20,
                20,
                20,
                0
        };
        gbl_panel.columnWeights = new double[] {
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        gbl_panel.rowWeights = new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                Double.MIN_VALUE
        };
        panel.setLayout(gbl_panel);

        JLabel lblNewLabel_5 = new JLabel("Drag:");
        GridBagConstraints gbc_lblNewLabel_5 = new GridBagConstraints();
        gbc_lblNewLabel_5.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_5.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_5.gridx = 0;
        gbc_lblNewLabel_5.gridy = 0;
        panel.add(lblNewLabel_5, gbc_lblNewLabel_5);

        m_dragUse = new JCheckBox("Use");
        m_dragUse.addItemListener(e -> {
            if (m_dragUse.isSelected()) {
                m_density.setEnabled(true);
                m_geomagKP.setEnabled(true);
                m_solarFlux.setEnabled(true);
                m_avgSolarFlux.setEnabled(true);
                m_dragCoef.setEnabled(true);
                m_lastDragSelection = true;
            } else {
                m_density.setEnabled(false);
                m_geomagKP.setEnabled(false);
                m_solarFlux.setEnabled(false);
                m_avgSolarFlux.setEnabled(false);
                m_dragCoef.setEnabled(false);
                if (m_centralBodySelection.getSelectedItem().toString().contentEquals(EARTH)) {
                    m_lastDragSelection = false;
                }
            }
        });

        GridBagConstraints gbc_m_dragUse = new GridBagConstraints();
        gbc_m_dragUse.anchor = GridBagConstraints.NORTH;
        gbc_m_dragUse.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_dragUse.insets = new Insets(0, 0, 5, 0);
        gbc_m_dragUse.gridx = 1;
        gbc_m_dragUse.gridy = 0;
        panel.add(m_dragUse, gbc_m_dragUse);

        JLabel lblDensity = new JLabel("Density:");
        GridBagConstraints gbc_lblDensity = new GridBagConstraints();
        gbc_lblDensity.anchor = GridBagConstraints.EAST;
        gbc_lblDensity.insets = new Insets(0, 0, 5, 5);
        gbc_lblDensity.gridx = 0;
        gbc_lblDensity.gridy = 1;
        panel.add(lblDensity, gbc_lblDensity);

        m_density = new JComboBox<>();
        GridBagConstraints gbc_m_density = new GridBagConstraints();
        gbc_m_density.anchor = GridBagConstraints.NORTH;
        gbc_m_density.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_density.insets = new Insets(0, 0, 5, 0);
        gbc_m_density.gridx = 1;
        gbc_m_density.gridy = 1;
        panel.add(m_density, gbc_m_density);

        m_density.addItem(JR);
        m_density.addItem(MSIS86);
        m_density.addItem(MSIS90);
        m_density.addItem(MSIS2000);

        JLabel lblNewLabel_6 = new JLabel("Drag Coeff:");
        GridBagConstraints gbc_lblNewLabel_6 = new GridBagConstraints();
        gbc_lblNewLabel_6.anchor = GridBagConstraints.EAST;
        gbc_lblNewLabel_6.insets = new Insets(0, 0, 5, 5);
        gbc_lblNewLabel_6.gridx = 0;
        gbc_lblNewLabel_6.gridy = 2;
        panel.add(lblNewLabel_6, gbc_lblNewLabel_6);

        m_dragCoef = new JTextField();
        GridBagConstraints gbc_m_dragCoef = new GridBagConstraints();
        gbc_m_dragCoef.anchor = GridBagConstraints.NORTH;
        gbc_m_dragCoef.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_dragCoef.insets = new Insets(0, 0, 5, 0);
        gbc_m_dragCoef.gridx = 1;
        gbc_m_dragCoef.gridy = 2;
        panel.add(m_dragCoef, gbc_m_dragCoef);
        m_dragCoef.setColumns(10);
        m_dragCoef.setText("2.2");

        JLabel lblAvgSolarFlux = new JLabel("Avg Solar Flux:");
        GridBagConstraints gbc_lblAvgSolarFlux = new GridBagConstraints();
        gbc_lblAvgSolarFlux.anchor = GridBagConstraints.EAST;
        gbc_lblAvgSolarFlux.insets = new Insets(0, 0, 5, 5);
        gbc_lblAvgSolarFlux.gridx = 0;
        gbc_lblAvgSolarFlux.gridy = 3;
        panel.add(lblAvgSolarFlux, gbc_lblAvgSolarFlux);

        m_avgSolarFlux = new JTextField();
        m_avgSolarFlux.setColumns(10);
        GridBagConstraints gbc_m_avgSolarFlux = new GridBagConstraints();
        gbc_m_avgSolarFlux.anchor = GridBagConstraints.NORTH;
        gbc_m_avgSolarFlux.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_avgSolarFlux.insets = new Insets(0, 0, 5, 0);
        gbc_m_avgSolarFlux.gridx = 1;
        gbc_m_avgSolarFlux.gridy = 3;
        panel.add(m_avgSolarFlux, gbc_m_avgSolarFlux);
        m_avgSolarFlux.setText("150");

        JLabel lblSolarFlux = new JLabel("Solar Flux:");
        GridBagConstraints gbc_lblSolarFlux = new GridBagConstraints();
        gbc_lblSolarFlux.anchor = GridBagConstraints.EAST;
        gbc_lblSolarFlux.insets = new Insets(0, 0, 5, 5);
        gbc_lblSolarFlux.gridx = 0;
        gbc_lblSolarFlux.gridy = 4;
        panel.add(lblSolarFlux, gbc_lblSolarFlux);

        m_solarFlux = new JTextField();
        m_solarFlux.setColumns(10);
        GridBagConstraints gbc_m_solarFlux = new GridBagConstraints();
        gbc_m_solarFlux.anchor = GridBagConstraints.NORTH;
        gbc_m_solarFlux.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_solarFlux.insets = new Insets(0, 0, 5, 0);
        gbc_m_solarFlux.gridx = 1;
        gbc_m_solarFlux.gridy = 4;
        panel.add(m_solarFlux, gbc_m_solarFlux);
        m_solarFlux.setText("150");

        JLabel lblGeomagKp = new JLabel("Geomag. KP:");
        GridBagConstraints gbc_lblGeomagKp = new GridBagConstraints();
        gbc_lblGeomagKp.anchor = GridBagConstraints.EAST;
        gbc_lblGeomagKp.insets = new Insets(0, 0, 0, 5);
        gbc_lblGeomagKp.gridx = 0;
        gbc_lblGeomagKp.gridy = 5;
        panel.add(lblGeomagKp, gbc_lblGeomagKp);

        m_geomagKP = new JTextField();
        m_geomagKP.setColumns(10);
        GridBagConstraints gbc_m_geomagKP = new GridBagConstraints();
        gbc_m_geomagKP.anchor = GridBagConstraints.NORTH;
        gbc_m_geomagKP.fill = GridBagConstraints.HORIZONTAL;
        gbc_m_geomagKP.gridx = 1;
        gbc_m_geomagKP.gridy = 5;
        panel.add(m_geomagKP, gbc_m_geomagKP);
        m_geomagKP.setText("3.0");
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

        m_dragUse.setSelected(true);
        m_srpUse.setSelected(true);
        m_centralBody = CentralBodiesFacet.getFromContext().getEarth();
        m_gravitationalParameter = m_gravConstants.get(EARTH.toUpperCase());
        m_earthSRP.setSelected(true);
        m_moonSRP.setSelected(true);
        m_earth3B.setEnabled(false);
    }

    /**
     * This will read in everything in this UI, generate the force models and
     * apply those force models to the entered point.
     *
     * @param point
     *            The point to have the force models applied.
     * @param area
     *            The cross sectional area to use for Drag and Solar Radiation
     *            Pressure.
     * @throws Exception
     *             Will be thrown if something is not configured properly.
     */
    public void setForceModelsOnPoint(PropagationNewtonianPoint point, Scalar area) {
        point.getAppliedForces().clear();
        if (m_srpUse.isSelected()) {
            SimpleSolarRadiationForce SRP = new SimpleSolarRadiationForce(point.getIntegrationPoint(),
                    new ScalarFixed(Double.parseDouble(m_srpReflectivity.getText())), area);
            if (DUALCONE.equals(m_shadowModel.getSelectedItem().toString())) {
                SRP.setOccultationFactor(
                        new ScalarOccultationDualCone(CentralBodiesFacet.getFromContext().getSun(), point.getIntegrationPoint()));
            } else if (CYLINDRICAL.equals(m_shadowModel.getSelectedItem().toString())) {
                SRP.setOccultationFactor(
                        new ScalarOccultationCylindrical(CentralBodiesFacet.getFromContext().getSun(), point.getIntegrationPoint()));
            }
            for (JCheckBox box : m_srpOcculingCheckBoxes) {
                if (box.isEnabled() && box.isSelected()) {
                    SRP.getOccultationFactor().getOccludingBodies().add(CentralBodiesFacet.getFromContext().getByName(box.getText()));
                }
            }

            point.getAppliedForces().add(SRP);
        }

        if (m_dragUse.isEnabled() && m_dragUse.isSelected()) {
            ScalarAtmosphericDensity density = null;
            SolarGeophysicalData geoData = new ConstantSolarGeophysicalData(Double.parseDouble(m_avgSolarFlux.getText()),
                    Double.parseDouble(m_solarFlux.getText()), Double.parseDouble(m_geomagKP.getText()));
            if (JR.equals(m_density.getSelectedItem().toString())) {
                density = new ScalarDensityJacchiaRoberts(point.getIntegrationPoint(), geoData);
            } else if (MSIS86.equals(m_density.getSelectedItem().toString())) {

                density = new ScalarDensityMsis86(point.getIntegrationPoint(), geoData);
            } else if (MSIS90.equals(m_density.getSelectedItem().toString())) {
                density = new ScalarDensityMsis90(point.getIntegrationPoint(), geoData);
            } else if (MSIS2000.equals(m_density.getSelectedItem().toString())) {
                density = new ScalarDensityMsis2000(point.getIntegrationPoint(), geoData);
            }
            AtmosphericDragForce atmo = new AtmosphericDragForce(density, new ScalarFixed(Double.parseDouble(m_dragCoef.getText())), area);
            point.getAppliedForces().add(atmo);
        }

        ThirdBodyGravity thirdGravity = new ThirdBodyGravity(point.getIntegrationPoint());
        boolean anyThirdBodies = false;
        for (JCheckBox box : m_thirdBodyCheckBoxes) {
            if (box.isEnabled() && box.isSelected()) {
                anyThirdBodies = true;
                String thirdBodyName = box.getText();
                if (thirdBodyName.contentEquals(EARTH) || thirdBodyName.contentEquals(SUN) || thirdBodyName.contentEquals(MOON)) {
                    thirdGravity.addThirdBody(thirdBodyName,
                            CentralBodiesFacet.getFromContext().getByName(thirdBodyName).getCenterOfMassPoint(),
                            m_gravConstants.get(thirdBodyName.toUpperCase()));
                } else {
                    thirdGravity.addThirdBody(thirdBodyName, m_jplInfo.getCenterOfMassPoint(m_nameToJplMapping.get(thirdBodyName)),
                            m_gravConstants.get(thirdBodyName.toUpperCase()));
                }
            }
        }
        thirdGravity.setCentralBody(m_centralBody);
        if (anyThirdBodies) {
            point.getAppliedForces().add(thirdGravity);
        }

        String primaryCB = m_centralBodySelection.getSelectedItem().toString();
        if (m_twoBodyGravity.isSelected()) {
            TwoBodyGravity gravity = new TwoBodyGravity(point.getIntegrationPoint(),
                    CentralBodiesFacet.getFromContext().getByName(primaryCB), m_gravConstants.get(primaryCB.toUpperCase()));
            point.getAppliedForces().add(gravity);
        } else {
            SphericalHarmonicGravityModel model = SphericalHarmonicGravityModel.readFrom(m_gravityFieldFile.getText());
            SolidTideModel tideModel;
            if (m_tides.isEnabled()) {
                // Only enabled if Earth is selected as central body.
                if (NOTIDES.equals(m_tides.getSelectedItem().toString())) {
                    model = model.getIncludesPermanentTides() ? model.withoutEarthPermanentTides() : model;
                    tideModel = null;
                } else if (PERMANENTTIDES.equals(m_tides.getSelectedItem().toString())) {
                    tideModel = model.getIncludesPermanentTides() ? null : new PermanentSolidTideModel();
                } else {
                    model = model.getIncludesPermanentTides() ? model.withoutEarthPermanentTides() : model;
                    tideModel = new Iers2003SolidTideModel(((EarthCentralBody)m_centralBody).getOrientationParameters());
                }
            } else {
                tideModel = null;
            }

            int order = Integer.parseInt(m_order.getText());
            int degree = Integer.parseInt(m_degree.getText());
            SphericalHarmonicGravity gravity = new SphericalHarmonicGravity(point.getIntegrationPoint(),
                    new SphericalHarmonicGravityField(model, degree, order, true, tideModel));
            point.getAppliedForces().add(gravity);

            if (!gravity.getGravityField().getCentralBody().getName().toUpperCase().contentEquals(primaryCB.toUpperCase())) {
                throw new RuntimeException("The gravity file entered does not match the central body you selected.");
            }
        }
        if (EARTH.equals(primaryCB)) {
            point.setIntegrationFrame(CentralBodiesFacet.getFromContext().getEarth().getInternationalCelestialReferenceFrame());
        } else if (MOON.equals(primaryCB)) {
            point.setIntegrationFrame(CentralBodiesFacet.getFromContext().getMoon().getInertialFrame());
        }
    }

    /**
     * Gets the gravitational parameter of the central body currently selected.
     *
     * @return The gravitational parameter of the central body currently
     *         selected.
     */
    public double getCurrentGravitationalParameter() {
        return m_gravitationalParameter;
    }

    /**
     * Gets the central body currently selected.
     *
     * @return The currently selected central body.
     */
    public CentralBody getCurrentCentralBody() {
        return m_centralBody;
    }

    /**
     * Members
     */
    private static final long serialVersionUID = 1L;
    private final JPanel contentPanel = new JPanel();
    private JTextField m_gravityFieldFile;
    private JTextField m_order;
    private JTextField m_degree;
    private JTextField m_srpReflectivity;
    private JTextField m_dragCoef;
    private JTextField m_avgSolarFlux;
    private JTextField m_solarFlux;
    private JTextField m_geomagKP;
    private JComboBox<String> m_centralBodySelection;
    private JComboBox<String> m_tides;
    private final JCheckBox m_earthSRP;
    private final JCheckBox m_moonSRP;
    private final JCheckBox m_srpUse;
    private JComboBox<String> m_shadowModel;
    private JCheckBox m_pluto3B;
    private JCheckBox m_neptune3B;
    private JCheckBox m_uranus3B;
    private JCheckBox m_saturn3B;
    private JCheckBox m_jupiter3B;
    private JCheckBox m_mars3B;
    private JCheckBox m_venus3B;
    private JCheckBox m_sun3B;
    private JCheckBox m_mercury3B;
    private JCheckBox m_moon3B;
    private JCheckBox m_earth3B;
    private JComboBox<String> m_density;
    private JCheckBox m_dragUse;
    private JRadioButton m_twoBodyGravity;
    private JRadioButton m_gravityField;

    private final String EARTH = CentralBodiesFacet.getFromContext().getEarth().getName();
    private final String MOON = CentralBodiesFacet.getFromContext().getMoon().getName();
    private final String SUN = CentralBodiesFacet.getFromContext().getSun().getName();

    private final String JR = "Jacchia-Roberts";
    private final String MSIS86 = "MSIS86";
    private final String MSIS90 = "MSIS90";
    private final String MSIS2000 = "MSIS2000";

    private final String DUALCONE = "Dual Cone";
    private final String CYLINDRICAL = "Cylindrical";

    private final String TIMEVARYINGTIDES = "TimeVarying";
    private final String PERMANENTTIDES = "Permanent";
    private final String NOTIDES = "None";

    private final Map<String, Double> m_gravConstants = new HashMap<>();
    private final Map<String, JplDECentralBody> m_nameToJplMapping = new HashMap<>();

    private double m_gravitationalParameter;
    private CentralBody m_centralBody;
    private boolean m_lastDragSelection = true;
    private final JplDE m_jplInfo;

    private final List<JCheckBox> m_thirdBodyCheckBoxes = new ArrayList<>();
    private final List<JCheckBox> m_srpOcculingCheckBoxes = new ArrayList<>();
}
