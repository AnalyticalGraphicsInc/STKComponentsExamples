    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using AGI.Foundation;
    using AGI.Foundation.Celestial;
    using AGI.Foundation.Coordinates;
    using AGI.Foundation.Geometry.Shapes;

namespace Spatial_Library_Exercise
{
        public static class STKUtil
        {
            /// <summary>
            /// Reads an STK area target file (*.at) and returns the points defining
            /// the area target's boundary as a list of Cartographic points.
            /// </summary>
            public static IList<Cartographic> ReadAreaTargetCartographic(String fileName)
            {
                //
                // Open the file and read everything between "BEGIN PolygonPoints"
                // and "END PolygonPoints"
                //
                String areaTarget = File.ReadAllText(fileName);
                String startToken = "BEGIN PolygonPoints";
                String points = areaTarget.Substring(areaTarget.IndexOf(startToken, StringComparison.Ordinal) + startToken.Length);
                points = points.Substring(0, points.IndexOf("END PolygonPoints", StringComparison.Ordinal));

                String[] splitPoints = points.Split(new char[] { '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                List<Cartographic> targetPoints = new List<Cartographic>();
                for (int i = 0; i < splitPoints.Length; i += 3)
                {
                    //
                    // Each line is [Latitude][Longitude][Altitude].  In the file,
                    // latitude and longitude are in degrees and altitude is in
                    // meters.
                    //
                    double longitude = Trig.DegreesToRadians(Double.Parse(splitPoints[i + 1], CultureInfo.InvariantCulture));
                    double latitude = Trig.DegreesToRadians(Double.Parse(splitPoints[i], CultureInfo.InvariantCulture));
                    double height = Double.Parse(splitPoints[i + 2], CultureInfo.InvariantCulture);
                    Cartographic cartographicPoint = new Cartographic(longitude, latitude, height);

                    targetPoints.Add(cartographicPoint);
                }

                return targetPoints;
            }



            /// <summary>
            /// Reads an STK area target file (*.at) and returns the points defining
            /// the area target's boundary as a list Cartesian points in the
            /// earth's fixed frame.
            /// This method assumes the file exists, that it is a valid area target 
            /// file, and the area target is on earth.
            /// </summary>
            public static IList<Cartesian> ReadAreaTargetPoints(String fileName)
            {
                //
                // Open the file and read everything between "BEGIN PolygonPoints"
                // and "END PolygonPoints"
                //
                String areaTarget = File.ReadAllText(fileName);
                String startToken = "BEGIN PolygonPoints";
                String points = areaTarget.Substring(areaTarget.IndexOf(startToken, StringComparison.Ordinal) + startToken.Length);
                points = points.Substring(0, points.IndexOf("END PolygonPoints", StringComparison.Ordinal));

                String[] splitPoints = points.Split(new char[] { '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                List<Cartesian> targetPoints = new List<Cartesian>();

                Ellipsoid ellipsoid = CentralBodiesFacet.GetFromContext().Earth.Shape;
                for (int i = 0; i < splitPoints.Length; i += 3)
                {
                    //
                    // Each line is [Latitude][Longitude][Altitude].  In the file,
                    // latitude and longitude are in degrees and altitude is in
                    // meters.
                    //
                    double longitude = Trig.DegreesToRadians(Double.Parse(splitPoints[i + 1], CultureInfo.InvariantCulture));
                    double latitude = Trig.DegreesToRadians(Double.Parse(splitPoints[i], CultureInfo.InvariantCulture));
                    double height = Double.Parse(splitPoints[i + 2], CultureInfo.InvariantCulture);
                    Cartographic cartographicPoint = new Cartographic(longitude, latitude, height);

                    Cartesian point = ellipsoid.CartographicToCartesian(cartographicPoint);
                    targetPoints.Add(point);
                }

                return targetPoints;
            }

            public static IList<Cartesian> ReadLineTargetPoints(String fileName)
            {
                String areaTarget = File.ReadAllText(fileName);
                String startToken = "BEGIN PolylinePoints";
                String points = areaTarget.Substring(areaTarget.IndexOf(startToken, StringComparison.Ordinal) + startToken.Length);
                points = points.Substring(0, points.IndexOf("END PolylinePoints", StringComparison.Ordinal));

                String[] splitPoints = points.Split(new char[] { '\t', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<Cartesian> targetPoints = new List<Cartesian>();

                Ellipsoid ellipsoid = CentralBodiesFacet.GetFromContext().Earth.Shape;
                for (int i = 0; i < splitPoints.Length; i += 3)
                {
                    double longitude = Trig.DegreesToRadians(Double.Parse(splitPoints[i + 1], CultureInfo.InvariantCulture));
                    double latitude = Trig.DegreesToRadians(Double.Parse(splitPoints[i], CultureInfo.InvariantCulture));
                    double height = Double.Parse(splitPoints[i + 2], CultureInfo.InvariantCulture);
                    Cartographic cartographicPoint = new Cartographic(longitude, latitude, height);

                    Cartesian point = ellipsoid.CartographicToCartesian(cartographicPoint);
                    targetPoints.Add(point);
                }

                return targetPoints;
            }
        }
    
}
