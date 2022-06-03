package com.agi.satellitetracker;

import java.awt.Container;
import java.awt.Font;
import java.io.File;

import javax.swing.BorderFactory;
import javax.swing.DefaultComboBoxModel;
import javax.swing.GroupLayout;
import javax.swing.GroupLayout.Alignment;
import javax.swing.GroupLayout.Group;
import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JComponent;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.event.DocumentEvent;

import agi.foundation.stk.StkSatelliteDatabase;
import agi.foundation.time.GregorianDate;

/**
 * The main application window. Collects information from user, then creates a
 * ResultWindow with the results of the calculation.
 */
public class SatelliteTracker extends JFrame {
    /**
     * Create the UI for the window. After construction this object will have
     * all of the controls built, added to the Swing hierarchy, and all event
     * handlers wired up.
     */
    public SatelliteTracker() {
        super("Satellite Tracker");
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

        m_database = new Database();
        m_userInput = new UserInput(m_database.getLastUpdateDate());

        JPanel locationPanel = createLocationPanel();
        JPanel spacecraftPanel = createSpacecraftPanel();
        JPanel dateRangePanel = createDateRangePanel();
        m_calculateButton = new JButton("Calculate");

        m_calculateButton.addActionListener(e -> {
            Computation computation = new Computation(m_userInput);
            ResultsWindow resultsWindow = new ResultsWindow(m_userInput, computation);
            resultsWindow.setVisible(true);
        });

        Container contentPane = getContentPane();
        GroupLayout layout = new GroupLayout(contentPane);
        contentPane.setLayout(layout);

        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        {
            // Horizontal group
            Group group = layout.createParallelGroup(Alignment.TRAILING);
            group.addComponent(locationPanel, 0, GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE);
            group.addComponent(spacecraftPanel, 0, GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE);
            group.addComponent(dateRangePanel, 0, GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE);
            group.addComponent(m_calculateButton);
            layout.setHorizontalGroup(group);
        }

        {
            // Vertical group
            Group group = layout.createSequentialGroup();
            group.addComponent(locationPanel);
            group.addComponent(spacecraftPanel);
            group.addComponent(dateRangePanel);
            group.addComponent(m_calculateButton);
            layout.setVerticalGroup(group);
        }

        pack();
    }

    public static String getDataPath() {
        return "Data";
    }

    public static String getDataFilePath(String path) {
        return new File(getDataPath(), path).getAbsolutePath();
    }

    private JPanel createLocationPanel() {
        JPanel panel = new JPanel();
        panel.setBorder(BorderFactory.createTitledBorder(LOCATION_BORDER_LABEL));

        JPanel latitudeLongitudePanel = createLatitudeLongitudePanel();
        JPanel elevationPanel = createElevationPanel();

        GroupLayout layout = new GroupLayout(panel);
        panel.setLayout(layout);

        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        JLabel locationLabel = new JLabel(LOCATION_LABEL);
        JLabel elevationLabel1 = new JLabel(ELEVATION_LABEL_1);
        JLabel elevationLabel2 = new JLabel(ELEVATION_LABEL_2);

        {
            // Horizontal group
            Group group = layout.createParallelGroup();
            group.addComponent(locationLabel);
            group.addComponent(latitudeLongitudePanel);
            group.addComponent(elevationLabel1);
            group.addComponent(elevationLabel2);
            group.addComponent(elevationPanel);
            layout.setHorizontalGroup(group);
        }

        {
            // Vertical group
            Group group = layout.createSequentialGroup();
            group.addComponent(locationLabel);
            group.addComponent(latitudeLongitudePanel);
            group.addComponent(elevationLabel1);
            group.addComponent(elevationLabel2);
            group.addComponent(elevationPanel);
            layout.setVerticalGroup(group);
        }

        return panel;
    }

    private JPanel createLatitudeLongitudePanel() {
        JPanel panel = new JPanel();

        m_latitudeField = new JTextField(String.valueOf(m_userInput.getLatitude()), 20);
        m_latitudeField.getDocument().addDocumentListener(new DocumentListenerAdapter() {
            @Override
            protected void documentChanged(DocumentEvent e) {
                try {
                    // try to parse the current value as a double. If
                    // unsuccessful, leave the old value as-is.
                    double value = Double.parseDouble(m_latitudeField.getText());
                    m_userInput.setLatitude(value);
                } catch (NumberFormatException ex) {}
            }
        });

        m_longitudeField = new JTextField(String.valueOf(m_userInput.getLongitude()), 20);
        m_longitudeField.getDocument().addDocumentListener(new DocumentListenerAdapter() {
            @Override
            protected void documentChanged(DocumentEvent e) {
                try {
                    // try to parse the current value as a double. If
                    // unsuccessful, leave the old value as-is.
                    double value = Double.parseDouble(m_longitudeField.getText());
                    m_userInput.setLongitude(value);
                } catch (NumberFormatException ex) {}
            }
        });

        GroupLayout layout = new GroupLayout(panel);
        panel.setLayout(layout);

        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        JLabel latitudeLabel = makeBold(new JLabel(LATITUDE_LABEL));
        latitudeLabel.setLabelFor(m_latitudeField);

        JLabel longitudeLabel = makeBold(new JLabel(LONGITUDE_LABEL));
        longitudeLabel.setLabelFor(m_longitudeField);

        {
            // Horizontal group
            Group group = layout.createSequentialGroup();
            group.addComponent(latitudeLabel);
            group.addComponent(m_latitudeField);
            group.addComponent(longitudeLabel);
            group.addComponent(m_longitudeField);
            layout.setHorizontalGroup(group);
        }

        {
            // Vertical group
            Group group = layout.createParallelGroup(Alignment.BASELINE);
            group.addComponent(latitudeLabel);
            group.addComponent(m_latitudeField);
            group.addComponent(longitudeLabel);
            group.addComponent(m_longitudeField);
            layout.setVerticalGroup(group);
        }

        return panel;
    }

    private JPanel createElevationPanel() {
        JPanel panel = new JPanel();

        m_elevationField = new JTextField(String.valueOf(m_userInput.getElevation()), 30);
        m_elevationField.getDocument().addDocumentListener(new DocumentListenerAdapter() {
            @Override
            protected void documentChanged(DocumentEvent e) {
                try {
                    // try to parse the current value as a double. If
                    // unsuccessful, leave the old value as-is.
                    double value = Double.parseDouble(m_elevationField.getText());
                    m_userInput.setElevation(value);
                } catch (NumberFormatException ex) {}
            }
        });

        GroupLayout layout = new GroupLayout(panel);
        panel.setLayout(layout);

        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        JLabel elevationLabel = makeBold(new JLabel(ELEVATION_LABEL));
        elevationLabel.setLabelFor(m_elevationField);

        {
            // Horizontal group
            Group group = layout.createSequentialGroup();
            group.addComponent(elevationLabel);
            group.addComponent(m_elevationField);
            layout.setHorizontalGroup(group);
        }

        {
            // Vertical group
            Group group = layout.createParallelGroup(Alignment.BASELINE);
            group.addComponent(elevationLabel);
            group.addComponent(m_elevationField);
            layout.setVerticalGroup(group);
        }

        return panel;
    }

    private JPanel createSpacecraftPanel() {
        JPanel panel = new JPanel();

        panel.setBorder(BorderFactory.createTitledBorder(SPACECRAFT_PANEL_BORDER_LABEL));

        m_categoryBox = new JComboBox<>(m_database.getCategories());
        m_categoryBox.addActionListener(e -> {
            String category = (String) m_categoryBox.getSelectedItem();

            m_userInput.setSpacecraftCategory(category);

            // when changing the category, query for the satellites in that
            // category, and re-populate the spacecraft box.
            String[] satellites;
            if (Database.USER_ENTERED_TLE.equals(category)) {
                satellites = new String[] {
                        Database.USER_ENTERED_TLE
                };
            } else {
                satellites = m_database.getSatellitesInCategory(category);
            }

            m_spacecraftBox.setModel(new DefaultComboBoxModel<String>(satellites));
            if (satellites.length > 0) {
                m_spacecraftBox.setSelectedIndex(0);
            }
        });

        m_spacecraftBox = new JComboBox<>();
        m_spacecraftBox.addActionListener(e -> {
            String spacecraft = (String) m_spacecraftBox.getSelectedItem();
            m_userInput.setSpacecraft(spacecraft);

            // when changing the spacecraft, fill in the TLE box, or if the
            // user selected User-entered TLE, blank it out and allow editing.
            if (Database.USER_ENTERED_TLE.equals(spacecraft)) {
                m_tleBox.setEnabled(true);
                m_tleBox.setText("");
            } else {
                m_tleBox.setEnabled(false);
                m_tleBox.setText(m_database.getTLEText((String) m_categoryBox.getSelectedItem(), spacecraft));
            }
        });

        m_tleBox = new JTextArea(3, 40);
        m_tleBox.getDocument().addDocumentListener(new DocumentListenerAdapter() {
            @Override
            protected void documentChanged(DocumentEvent e) {
                m_userInput.setTle(m_tleBox.getText());
            }
        });
        JScrollPane tleBoxScrollPane = new JScrollPane(m_tleBox);

        m_categoryBox.setSelectedItem(m_userInput.getSpacecraftCategory());
        m_spacecraftBox.setSelectedItem(m_userInput.getSpacecraft());

        GroupLayout layout = new GroupLayout(panel);
        panel.setLayout(layout);

        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        JLabel label1 = new JLabel(SPACECRAFT_PANEL_LABEL_1);
        JLabel label2 = new JLabel(SPACECRAFT_PANEL_LABEL_2);

        JLabel categoryLabel = makeBold(new JLabel(CATEGORY_LABEL));
        categoryLabel.setLabelFor(m_categoryBox);

        JLabel spacecraftLabel = makeBold(new JLabel(SPACECRAFT_LABEL));
        spacecraftLabel.setLabelFor(m_spacecraftBox);

        JLabel tleLabel = makeBold(new JLabel(TLE_LABEL));
        tleLabel.setLabelFor(m_tleBox);

        {
            // Horizontal group
            Group group = layout.createParallelGroup();
            group.addComponent(label1);
            group.addComponent(label2);

            Group subGroup = layout.createSequentialGroup();
            subGroup.addComponent(categoryLabel);
            subGroup.addComponent(m_categoryBox);
            subGroup.addComponent(spacecraftLabel);
            subGroup.addComponent(m_spacecraftBox);
            group.addGroup(subGroup);

            subGroup = layout.createSequentialGroup();
            subGroup.addComponent(tleLabel);
            subGroup.addComponent(tleBoxScrollPane);
            group.addGroup(subGroup);

            layout.setHorizontalGroup(group);
        }

        {
            // Vertical group
            Group group = layout.createSequentialGroup();
            group.addComponent(label1);
            group.addComponent(label2);

            Group subGroup = layout.createParallelGroup(Alignment.BASELINE);
            subGroup.addComponent(categoryLabel);
            subGroup.addComponent(m_categoryBox);
            subGroup.addComponent(spacecraftLabel);
            subGroup.addComponent(m_spacecraftBox);
            group.addGroup(subGroup);

            subGroup = layout.createParallelGroup();
            subGroup.addComponent(tleLabel);
            subGroup.addComponent(tleBoxScrollPane);
            group.addGroup(subGroup);

            layout.setVerticalGroup(group);
        }
        return panel;
    }

    private JPanel createDateRangePanel() {
        JPanel panel = new JPanel();

        String dateRangeTitle = String.format(DATE_RANGE_BORDER_LABEL, m_database.getLastUpdateDate().toShortDateString());
        panel.setBorder(BorderFactory.createTitledBorder(dateRangeTitle));

        StkSatelliteDatabase temporarySatDb = new StkSatelliteDatabase("Data/SatelliteDatabase", "stkSatDb");
        m_startDateField = new JTextField(temporarySatDb.getLastUpdateDate().toString(), 30);
        m_startDateField.getDocument().addDocumentListener(new DocumentListenerAdapter() {
            GregorianDate[] date = new GregorianDate[1];

            @Override
            protected void documentChanged(DocumentEvent e) {
                // try to parse the current text as a GregorianDate, if
                // unsuccessful, leave the old value as-is
                boolean success = GregorianDate.tryParse(m_startDateField.getText(), date);
                if (success) {
                    m_userInput.setStartDate(date[0]);
                }
            }
        });
        m_endDateField = new JTextField(temporarySatDb.getLastUpdateDate().toJulianDate().addDays(1).toGregorianDate().toString(), 30);
        m_endDateField.getDocument().addDocumentListener(new DocumentListenerAdapter() {
            GregorianDate[] date = new GregorianDate[1];

            @Override
            protected void documentChanged(DocumentEvent e) {
                // try to parse the current text as a GregorianDate, if
                // unsuccessful, leave the old value as-is
                boolean success = GregorianDate.tryParse(m_endDateField.getText(), date);
                if (success) {
                    m_userInput.setEndDate(date[0]);
                }
            }
        });

        GroupLayout layout = new GroupLayout(panel);
        panel.setLayout(layout);

        layout.setAutoCreateGaps(true);
        layout.setAutoCreateContainerGaps(true);

        JLabel startDateLabel = makeBold(new JLabel(START_DATE_LABEL));
        startDateLabel.setLabelFor(m_startDateField);

        JLabel endDateLabel = makeBold(new JLabel(END_DATE_LABEL));
        endDateLabel.setLabelFor(m_endDateField);

        {
            // Horizontal group
            Group group = layout.createSequentialGroup();

            Group subGroup = layout.createParallelGroup(Alignment.LEADING, false);
            subGroup.addComponent(startDateLabel, GroupLayout.DEFAULT_SIZE, GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE);
            subGroup.addComponent(endDateLabel, GroupLayout.DEFAULT_SIZE, GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE);
            group.addGroup(subGroup);

            subGroup = layout.createParallelGroup();
            subGroup.addComponent(m_startDateField);
            subGroup.addComponent(m_endDateField);
            group.addGroup(subGroup);

            layout.setHorizontalGroup(group);
        }

        {
            // Vertical group
            Group group = layout.createSequentialGroup();

            Group subGroup = layout.createParallelGroup(Alignment.BASELINE);
            subGroup.addComponent(startDateLabel);
            subGroup.addComponent(m_startDateField);
            group.addGroup(subGroup);

            subGroup = layout.createParallelGroup(Alignment.BASELINE);
            subGroup.addComponent(endDateLabel);
            subGroup.addComponent(m_endDateField);
            group.addGroup(subGroup);

            layout.setVerticalGroup(group);
        }

        return panel;
    }

    public static <T extends JComponent> T makeBold(T c) {
        Font font = c.getFont();
        c.setFont(font.deriveFont(font.getStyle() | Font.BOLD));
        return c;
    }

    private static final long serialVersionUID = 1L;

    private static final String LOCATION_BORDER_LABEL = "Select a viewing location";
    private static final String LOCATION_LABEL = "Where are you? Enter latitude and longitude.";

    private static final String LATITUDE_LABEL = "Latitude (deg):";
    private static final String LONGITUDE_LABEL = "Longitude (deg):";

    private static final String ELEVATION_LABEL_1 = "What is the minimum elevation angle above the horizon that you can easily see?";
    private static final String ELEVATION_LABEL_2 = "In order to be considered visible, an object must be above this elevation angle.";

    private static final String ELEVATION_LABEL = "Min. Elevation (deg):";

    private static final String SPACECRAFT_PANEL_BORDER_LABEL = "Select a spacecraft to view";
    private static final String SPACECRAFT_PANEL_LABEL_1 = "What spacecraft would you like to view?";
    private static final String SPACECRAFT_PANEL_LABEL_2 = "Select from the list, or select \"User-entered TLE\" and enter a Two Line Element Set for the spacecraft.";

    private static final String CATEGORY_LABEL = "Spacecraft Category:";
    private static final String SPACECRAFT_LABEL = "Spacecraft:";
    private static final String TLE_LABEL = "Two Line Element Set:";

    private static final String DATE_RANGE_BORDER_LABEL = "Select a range of dates to consider. For valid results, enter dates within 1 week of %s.";
    private static final String START_DATE_LABEL = "Start Date (UTC):";
    private static final String END_DATE_LABEL = "End Date (UTC):";

    private final UserInput m_userInput;
    private final Database m_database;

    private JTextField m_longitudeField;
    private JTextField m_latitudeField;
    private JTextField m_elevationField;
    private JComboBox<String> m_categoryBox;
    private JComboBox<String> m_spacecraftBox;
    private JTextArea m_tleBox;
    private JTextField m_startDateField;
    private JTextField m_endDateField;
    private final JButton m_calculateButton;
}
