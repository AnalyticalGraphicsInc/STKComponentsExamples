package com.agi.satellitetracker;

import java.net.URL;
import java.net.URLEncoder;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathFactory;

import org.w3c.dom.Document;

/**
 * An extremely basic geocoding client for the Google Maps Geocoding API.
 * In a real application, use Google's Java client library, which is much more robust.
 */
public final class GoogleGeocoder {
    private GoogleGeocoder() {}

    public static GeocoderResult geocode(String address) {
        try {
            StringBuilder builder = new StringBuilder("https://maps.google.com/maps/api/geocode/xml?address=");
            builder.append(URLEncoder.encode(address, "UTF-8"));
            builder.append("&key=AIzaSyBBmvXIjzF59K6Yay-8WFxQoAFivIN_hBo");

            URL url = new URL(builder.toString());

            DocumentBuilder documentBuilder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
            Document document = documentBuilder.parse(url.openStream());

            XPath xpath = XPathFactory.newInstance().newXPath();
            String longitudeString = xpath.evaluate("/GeocodeResponse/result/geometry/location/lng", document);
            String latitudeString = xpath.evaluate("/GeocodeResponse/result/geometry/location/lat", document);

            double longitudeDegrees = Double.parseDouble(longitudeString);
            double latitudeDegrees = Double.parseDouble(latitudeString);

            return new GeocoderResult(longitudeDegrees, latitudeDegrees);
        } catch (Exception e) {
            // rethrow and let the application-level UncaughtExceptionHandler show
            // the message in a dialog box
            throw new RuntimeException(e);
        }
    }

    public static final class GeocoderResult {
        public GeocoderResult(double longitudeDegrees, double latitudeDegrees) {
            this.longitudeDegrees = longitudeDegrees;
            this.latitudeDegrees = latitudeDegrees;
        }

        public final double longitudeDegrees;
        public final double latitudeDegrees;
    }
}
