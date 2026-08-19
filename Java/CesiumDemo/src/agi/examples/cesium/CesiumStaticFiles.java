package agi.examples.cesium;

import static javax.servlet.http.HttpServletResponse.*;
import static spark.Spark.*;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Locale;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import spark.Request;
import spark.Response;

/**
 * Helper class to serve static Cesium files directly from the
 * Cesium zip file, to make running this simple demo application easier.
 */
public final class CesiumStaticFiles {
    // Relative paths within the Cesium zip file to search for the requested file.
    private static final String[] zipPrefixes = {
            "cesium/Build/Cesium",
            "packages/ion-sdk-geometry/Build/IonSdkGeometry",
            "packages/ion-sdk-measurements/Build/IonSdkMeasurements",
            "packages/ion-sdk-sensors/Build/IonSdkSensors",
    };

    public static HttpServletResponse processRequest(Request request, Response response) throws IOException {
        String relativePath = getRelativePath(request);
        if (relativePath == null || relativePath.isEmpty()) {
            halt(SC_NOT_FOUND);
            return null;
        }

        // Locate Cesium zip file. Should be in the Data directory.
        Path cesiumZipPath = getCesiumZipPath();
        if (cesiumZipPath == null || !Files.exists(cesiumZipPath)) {
            halt(SC_NOT_FOUND);
            return null;
        }

        try (ZipFile zip = new ZipFile(cesiumZipPath.toFile())) {
            ZipEntry entry = findEntry(relativePath, zip);
            if (entry == null) {
                // Not found under any prefix.
                halt(SC_NOT_FOUND);
                return null;
            }

            final HttpServletRequest rawRequest = request.raw();
            final HttpServletResponse rawResponse = response.raw();

            rawResponse.setHeader("Cache-Control", "public, max-age=0");

            final long lastModified = entry.getTime();
            if (lastModified >= 0) {
                rawResponse.setDateHeader("Last-Modified", lastModified);

                final long ifModifiedSince = rawRequest.getDateHeader("If-Modified-Since");
                if (ifModifiedSince != -1 && lastModified <= ifModifiedSince) {
                    rawResponse.setStatus(SC_NOT_MODIFIED);
                    return rawResponse;
                }
            }

            final String contentType = determineContentType(relativePath);
            if (contentType != null) {
                rawResponse.setContentType(contentType);
            }

            final long contentLength = entry.getSize();
            if (contentLength >= 0) {
                rawResponse.setContentLengthLong(contentLength);
            }

            try (InputStream input = zip.getInputStream(entry)) {
                try (OutputStream output = rawResponse.getOutputStream()) {
                    copyStream(input, output);
                }
            }

            return rawResponse;
        }
    }

    private static String getRelativePath(Request request) {
        String[] splat = request.splat();
        if (splat == null || splat.length == 0) {
            return null;
        }

        return splat[0];
    }

    private static Path getCesiumZipPath() throws IOException {
        try (DirectoryStream<Path> stream = Files.newDirectoryStream(Paths.get("Data"), "Cesium-*.zip")) {
            for (Path p : stream) {
                return p;
            }
        }
        return null;
    }

    private static ZipEntry findEntry(String relativePath, ZipFile zipFile) {
        for (String zipPrefix : zipPrefixes) {
            String entryPath = zipPrefix + "/" + relativePath;
            ZipEntry entry = zipFile.getEntry(entryPath);
            if (entry != null) {
                return entry;
            }
        }
        return null;
    }

    private static String determineContentType(String pathString) throws IOException {
        Path path = Paths.get(pathString);

        String fileName = path.getFileName().toString();
        String extension = "";
        int dotIndex = fileName.lastIndexOf('.');
        if (dotIndex >= 0 && dotIndex < fileName.length() - 1) {
            extension = fileName.substring(dotIndex + 1).toLowerCase(Locale.ROOT);
        }

        String override = Application.mimeTypeOverrides.get(extension);
        if (override != null) {
            return override;
        }

        return Files.probeContentType(path);
    }

    private static void copyStream(InputStream input, OutputStream output) throws IOException {
        byte[] buffer = new byte[8192];
        int bytesRead;
        while ((bytesRead = input.read(buffer)) != -1) {
            output.write(buffer, 0, bytesRead);
        }
    }
}
