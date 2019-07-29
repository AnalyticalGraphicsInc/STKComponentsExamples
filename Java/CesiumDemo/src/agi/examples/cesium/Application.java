package agi.examples.cesium;

import static spark.Spark.*;

import java.io.Writer;

import javax.servlet.http.HttpServletResponse;

import agi.examples.LeapSecondsFacetHelper;
import agi.foundation.infrastructure.CalculationContext;

public class Application {
    public static void main(String[] args) {
        // Get leap second data, and use it in the current calculation context.
        LeapSecondsFacetHelper.getLeapSeconds().useInCurrentContext();
        CalculationContext.setDefaultForNewContexts(CalculationContext.getInstance());

        staticFiles.location("/public");
        staticFiles.registerMimeType("gltf", "model/gltf+json");
        staticFiles.registerMimeType("glb", "model/gltf-binary");
        staticFiles.registerMimeType("json", "application/json");
        staticFiles.registerMimeType("wasm", "application/wasm");
        staticFiles.registerMimeType("woff", "application/font-woff");
        staticFiles.registerMimeType("woff2", "application/font-woff2");
        staticFiles.registerMimeType("svg", "image/svg+xml");

        // Define a basic web service. It expects
        // a NORAD satellite identifier to be passed as a query parameter, and
        // writes CZML for the demonstration directly to the response.

        get("/GenerateCzml", (request, response) -> {
            String satelliteIdentifier = request.queryParams("id");

            // construct the objects for the demonstration.
            CesiumDemo demo = new CesiumDemo(satelliteIdentifier, request.url());

            // Write the CZML document directly to the response.
            response.type("application/json");

            HttpServletResponse rawResponse = response.raw();
            try (Writer writer = rawResponse.getWriter()) {
                demo.writeDocument(writer);
            }

            return rawResponse;
        });
    }
}
