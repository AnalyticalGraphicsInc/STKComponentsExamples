using AGI.Foundation.Access;
using AGI.Foundation.Access.Constraints;
using AGI.Foundation.Celestial;
using AGI.Foundation.Geometry;
using AGI.Foundation.Time;

namespace AGI.Examples
{
    /// <summary>
    /// Contains helper methods used in multiple demos to calculate viewing times.
    /// </summary>
    public static class VisibilityHelper
    {
        /// <summary>
        /// Finds the first time in a given interval that a given location is in sunlight.
        /// This is used in multiple demos to set the animation time.
        /// </summary>
        /// <param name="target">The target location to view in sunlight.</param>
        /// <param name="consideredInterval">The interval to consider.</param>
        /// <returns>
        /// The first time within the given date range where the target location is in sunlight,
        /// or the start of the given interval if it is never in sunlight.
        /// </returns>
        public static JulianDate ViewPointInSunlight(Point target, TimeInterval consideredInterval)
        {
            var earth = CentralBodiesFacet.GetFromContext().Earth;
            var sun = CentralBodiesFacet.GetFromContext().Sun;

            var sunlight = new LinkSpeedOfLight(sun, target, earth.InertialFrame);
            var sunlightConstraint = new CentralBodyObstructionConstraint(sunlight, earth);

            // The target is in sunlight when the sunlight link is not obstructed by the Earth.
            using (var accessEvaluator = sunlightConstraint.GetEvaluator(target))
            {
                var accessResult = accessEvaluator.Evaluate(consideredInterval);
                var satisfactionIntervals = accessResult.SatisfactionIntervals;
                if (satisfactionIntervals.IsEmpty)
                {
                    // No valid times (unlikely).  Just use the start date.
                    return consideredInterval.Start;
                }

                return satisfactionIntervals.Start;
            }
        }
    }
}
