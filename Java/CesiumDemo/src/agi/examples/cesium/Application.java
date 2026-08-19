package agi.examples.cesium;

import static spark.Spark.*;

import java.io.Writer;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

import javax.servlet.http.HttpServletResponse;

import agi.examples.LeapSecondsFacetHelper;
import agi.foundation.infrastructure.CalculationContext;

public class Application {
    public static void main(String[] args) {
        // Get leap second data, and use it in the current calculation context.
        LeapSecondsFacetHelper.getLeapSeconds().useInCurrentContext();
        CalculationContext.setDefaultForNewContexts(CalculationContext.getInstance());

        staticFiles.location("/public");

        for (Map.Entry<String, String> entry : mimeTypeOverrides.entrySet()) {
            staticFiles.registerMimeType(entry.getKey(), entry.getValue());
        }

        // Serve static Cesium files directly from the
        // Cesium zip file, to make running this simple demo application easier.
        get("/Cesium/*", CesiumStaticFiles::processRequest);

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

    public static final Map<String, String> mimeTypeOverrides = createMimeTypeOverrides();

    private static Map<String, String> createMimeTypeOverrides() {
        Map<String, String> map = new HashMap<>();
        map.put("gltf", "model/gltf+json");
        map.put("glb", "model/gltf-binary");
        map.put("json", "application/json");
        map.put("wasm", "application/wasm");
        map.put("woff", "application/font-woff");
        map.put("woff2", "application/font-woff2");
        map.put("svg", "image/svg+xml");
        return Collections.unmodifiableMap(map);
    }
}
