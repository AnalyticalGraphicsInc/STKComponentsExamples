package com.agi.satellitetracker;

import javax.swing.event.DocumentEvent;
import javax.swing.event.DocumentListener;

public abstract class DocumentListenerAdapter implements DocumentListener {
    @Override
    public void changedUpdate(DocumentEvent e) {
        documentChanged(e);
    }

    @Override
    public void insertUpdate(DocumentEvent e) {
        documentChanged(e);
    }

    @Override
    public void removeUpdate(DocumentEvent e) {
        documentChanged(e);
    }

    protected abstract void documentChanged(DocumentEvent e);
}
