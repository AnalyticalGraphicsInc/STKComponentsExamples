using System.Drawing;
using AGI.Foundation;
using AGI.Foundation.Celestial;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;
using AGI.Foundation.Graphics;
using AGI.Foundation.Graphics.Advanced;

namespace AGI.Examples
{
    /// <summary>
    /// Contains various helper methods used in multiple demos that use Insight3D.
    /// </summary>
    public static class Insight3DHelper
    {
        /// <summary>
        /// Measure the size of a string rendered in a given font.
        /// </summary>
        public static Size MeasureString(string text, Font font)
        {
            //Graphics.MeasureString() is more accurate than TextRenderer.MeasureText, but it requires a Graphics object
            using (var tempBitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(tempBitmap))
            {
                return Size.Ceiling(graphics.MeasureString(text, font));
            }
        }

        /// <summary>
        /// Positions the camera to view a given bounding sphere.
        /// </summary>
        public static void ViewBoundingSphere(Insight3D insight3D, CentralBody centralBody, BoundingSphere sphere)
        {
            ViewBoundingSphere(insight3D, centralBody, sphere, Trig.DegreesToRadians(-90.0), Trig.DegreesToRadians(-30.0));
        }

        /// <summary>
        /// Positions the camera to view a given bounding sphere from a given azimuth and elevation angle.
        /// </summary>
        public static void ViewBoundingSphere(Insight3D insight3D, CentralBody centralBody, BoundingSphere sphere,
                                              double azimuthAngle, double elevationAngle)
        {
            var boundingSphereCenter = new PointFixedOffset(centralBody.FixedFrame, sphere.Center);
            var boundingSphereAxes = new AxesEastNorthUp(centralBody, boundingSphereCenter);

            var camera = insight3D.Scene.Camera;
            var offset = new Cartesian(new AzimuthElevationRange(azimuthAngle, elevationAngle, camera.DistancePerRadius * sphere.Radius));
            camera.ViewOffset(boundingSphereAxes, boundingSphereCenter, offset);
        }

        /// <summary>
        /// Positions the camera to view a given extent on the surface from a given azimuth and elevation angle.
        /// </summary>
        public static void ViewExtent(Insight3D insight3D, CentralBody centralBody,
                                      double west, double south, double east, double north,
                                      double azimuthAngle, double elevationAngle)
        {
            var camera = insight3D.Scene.Camera;
            camera.ViewExtent(centralBody, west, south, east, north);
            var offset = new Cartesian(new AzimuthElevationRange(azimuthAngle, elevationAngle, camera.Distance));
            camera.Position = camera.ReferencePoint + offset;
        }

        /// <summary>
        /// Positions the camera to view a given extent on the surface from a given azimuth and elevation angle.
        /// </summary>
        public static void ViewExtent(Insight3D insight3D, CentralBody centralBody, CartographicExtent extent,
                                      double azimuthAngle, double elevationAngle)
        {
            ViewExtent(insight3D, centralBody, extent.WestLongitude, extent.SouthLatitude, extent.EastLongitude, extent.NorthLatitude, azimuthAngle, elevationAngle);
        }
    }
}