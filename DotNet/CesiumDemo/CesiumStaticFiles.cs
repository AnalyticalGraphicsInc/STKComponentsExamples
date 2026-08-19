using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace CesiumDemo
{
    /// <summary>
    /// HTTP handler to serve static Cesium files directly from the
    /// Cesium zip file, to make running this simple demo application easier.
    /// </summary>
    internal sealed class CesiumStaticFileHandler : IHttpHandler
    {
        private const string Prefix = "~/Cesium/";

        // Relative paths within the Cesium zip file to search for the requested file.
        private static readonly string[] ZipPrefixes =
        {
            "cesium/Build/Cesium",
            "packages/ion-sdk-geometry/Build/IonSdkGeometry",
            "packages/ion-sdk-measurements/Build/IonSdkMeasurements",
            "packages/ion-sdk-sensors/Build/IonSdkSensors",
        };

        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.Request;
            var response = context.Response;

            string relativePath = GetRelativePath(context);
            if (string.IsNullOrEmpty(relativePath))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            // Locate Cesium zip file. Should be in the ~/Cesium/ directory.
            string cesiumZipPath = GetCesiumZipPath(context);
            if (string.IsNullOrEmpty(cesiumZipPath) || !File.Exists(cesiumZipPath))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            // Open zip file.
            using (var zip = new ZipArchive(File.OpenRead(cesiumZipPath), ZipArchiveMode.Read))
            {
                var entry = FindEntry(relativePath, zip);
                if (entry == null)
                {
                    // Not found under any prefix.
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetMaxAge(TimeSpan.Zero);

                var lastModified = entry.LastWriteTime;
                if (lastModified != DateTimeOffset.MinValue)
                {
                    response.Cache.SetLastModified(lastModified.UtcDateTime);

                    DateTime? ifModifiedSince = GetDateHeader(request, "If-Modified-Since");
                    if (ifModifiedSince != null && lastModified.UtcDateTime <= ifModifiedSince.Value)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotModified;
                        response.SuppressContent = true;
                        return;
                    }
                }

                string contentType = DetermineContentType(relativePath);
                response.ContentType = contentType;

                long contentLength = entry.Length;
                if (contentLength >= 0)
                {
                    response.Headers["Content-Length"] = contentLength.ToString(CultureInfo.InvariantCulture);
                }

                response.BufferOutput = false;

                using (var stream = entry.Open())
                {
                    stream.CopyTo(response.OutputStream);
                }
            }
        }

        private static string GetRelativePath(HttpContext context)
        {
            string path = context.Request.AppRelativeCurrentExecutionFilePath;

            // Trim prefix from path.
            if (!string.IsNullOrEmpty(path) &&
                path.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(Prefix.Length);
            }

            return path;
        }

        private static string GetCesiumZipPath(HttpContext context)
        {
            string cesiumDirectoryPath = context.Server.MapPath(Prefix);
            if (Directory.Exists(cesiumDirectoryPath))
            {
                return Directory.EnumerateFiles(cesiumDirectoryPath, "Cesium-*.zip").FirstOrDefault();
            }

            return null;
        }

        private static ZipArchiveEntry FindEntry(string relativePath, ZipArchive archive)
        {
            return ZipPrefixes.Select(zipPrefix => zipPrefix + "/" + relativePath)
                              .Select(archive.GetEntry)
                              .FirstOrDefault(entry => entry != null);
        }

        private static string DetermineContentType(string relativePath)
        {
            return MimeMapping.GetMimeMapping(Path.GetFileName(relativePath));
        }

        private static DateTime? GetDateHeader(HttpRequest request, string name)
        {
            string value = request.Headers[name];
            if (string.IsNullOrEmpty(value))
                return null;
            const DateTimeStyles style = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, style, out DateTime result))
                return result;
            return null;
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            // Map a route so that any path under the prefix is served from the zip file.
            routes.Add(new Route("Cesium/{*path}", new RouteHandler()));
        }

        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Route handler that creates an HTTP handler for serving static files from a zip archive.
        /// </summary>
        internal sealed class RouteHandler : IRouteHandler
        {
            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new CesiumStaticFileHandler();
            }
        }
    }
}
