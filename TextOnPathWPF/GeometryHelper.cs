using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using PhysicsSimulation.Utilities;

namespace TextOnPathWPF
{
    public static class GeometryHelper
    {
        public static List<Point> GetIntersectionPoints(PathGeometry flattenedPath, double[] segmentLengths)
        {
            var intersectionPoints = new List<Point>();

            var pointsOnFlattenedPath = GetPointsOnFlattenedPath(flattenedPath);

            if (pointsOnFlattenedPath == null || pointsOnFlattenedPath.Count < 2)
                return intersectionPoints;

            var currPoint = pointsOnFlattenedPath[0];
            intersectionPoints.Add(currPoint);

            // find point on flattened path that is segment length away from current point

            var flattedPathIndex = 0;

            var segmentIndex = 1;

            while (flattedPathIndex < pointsOnFlattenedPath.Count - 1 &&
                segmentIndex < segmentLengths.Length + 1)
            {
                var intersectionPoint = GetIntersectionOfSegmentAndCircle(
                    pointsOnFlattenedPath[flattedPathIndex],
                    pointsOnFlattenedPath[flattedPathIndex + 1], currPoint, segmentLengths[segmentIndex - 1]);

                if (intersectionPoint == null)
                    flattedPathIndex++;
                else
                {
                    intersectionPoints.Add((Point)intersectionPoint);
                    currPoint = (Point)intersectionPoint;
                    pointsOnFlattenedPath[flattedPathIndex] = currPoint;
                    segmentIndex++;
                }
            }

            return intersectionPoints;
        }

        static List<Point> GetPointsOnFlattenedPath(PathGeometry flattenedPath)
        {
            var flattenedPathPoints = new List<Point>();

            // for flattened geometry there should be just one PathFigure in the Figures
            if (flattenedPath.Figures.Count != 1)
                return null;

            var pathFigure = flattenedPath.Figures[0];

            flattenedPathPoints.Add(pathFigure.StartPoint);

            // SegmentsCollection should contain PolyLineSegment and LineSegment
            foreach (var pathSegment in pathFigure.Segments)
            {
                if (pathSegment is PolyLineSegment)
                {
                    var seg = pathSegment as PolyLineSegment;

                    flattenedPathPoints.AddRange(seg.Points);
                }
                else if (pathSegment is LineSegment)
                {
                    var seg = pathSegment as LineSegment;

                    flattenedPathPoints.Add(seg.Point);
                }
                else
                    throw new Exception("GetIntersectionPoint - unexpected path segment type: " + pathSegment);

            }

            return (flattenedPathPoints);
        }

        static Point? GetIntersectionOfSegmentAndCircle(Point segmentPoint1, Point segmentPoint2,
            Point circleCenter, double circleRadius)
        {
            // linear equation for segment: y = mx + b
            var slope = (segmentPoint2.Y - segmentPoint1.Y) / (segmentPoint2.X - segmentPoint1.X);
            var intercept = segmentPoint1.Y - (slope * segmentPoint1.X);

            // special case when segment is vertically oriented
            if (double.IsInfinity(slope))
            {
                var root = Math.Pow(circleRadius, 2.0) - Math.Pow(segmentPoint1.X - circleCenter.X, 2.0);

                if (root < 0)
                    return null;

                // soln 1
                var solnX1 = segmentPoint1.X;
                var solnY1 = circleCenter.Y - FastSquareRoot.Sqrt(Convert.ToSingle(root));
                var soln1 = new Point(solnX1, solnY1);

                // have valid result if point is between two segment points
                if (IsBetween(solnX1, segmentPoint1.X, segmentPoint2.X) &&
                    IsBetween(solnY1, segmentPoint1.Y, segmentPoint2.Y))
                //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (soln1);
                }

                // soln 2
                var solnX2 = segmentPoint1.X;
                var solnY2 = circleCenter.Y + FastSquareRoot.Sqrt(Convert.ToSingle(root));
                var soln2 = new Point(solnX2, solnY2);

                // have valid result if point is between two segment points
                if (IsBetween(solnX2, segmentPoint1.X, segmentPoint2.X) &&
                    IsBetween(solnY2, segmentPoint1.Y, segmentPoint2.Y))
                //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (soln2);
                }
            }
            else
            {
                // use soln to quadradratic equation to solve intersection of segment and circle:
                // x = (-b +/ sqrt(b^2-4ac))/(2a)
                var a = 1 + Math.Pow(slope, 2.0);
                var b = (-2 * circleCenter.X) + (2 * (intercept - circleCenter.Y) * slope);
                var c = Math.Pow(circleCenter.X, 2.0) + Math.Pow(intercept - circleCenter.Y, 2.0) - Math.Pow(circleRadius, 2.0);

                // check for no solutions, is sqrt negative?
                var root = Math.Pow(b, 2.0) - (4 * a * c);

                if (root < 0)
                    return null;

                // we might have two solns...

                // soln 1
                var solnX1 = (-b + FastSquareRoot.Sqrt(Convert.ToSingle(root))) / (2 * a);
                var solnY1 = slope * solnX1 + intercept;
                var soln1 = new Point(solnX1, solnY1);

                // have valid result if point is between two segment points
                if (IsBetween(solnX1, segmentPoint1.X, segmentPoint2.X) &&
                    IsBetween(solnY1, segmentPoint1.Y, segmentPoint2.Y))
                //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (soln1);
                }

                // soln 2
                var solnX2 = (-b - FastSquareRoot.Sqrt(Convert.ToSingle(root))) / (2 * a);
                var solnY2 = slope * solnX2 + intercept;
                var soln2 = new Point(solnX2, solnY2);

                // have valid result if point is between two segment points
                if (IsBetween(solnX2, segmentPoint1.X, segmentPoint2.X) &&
                    IsBetween(solnY2, segmentPoint1.Y, segmentPoint2.Y))
                //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                {
                    // found solution
                    return (soln2);
                }
            }

            // shouldn't get here...but in case
            return null;
        }

        static bool IsBetween(double x, double x1, double x2)
        {
            if (x1 >= x2 && x <= x1 && x >= x2)
                return true;

            if (x1 <= x2 && x >= x1 && x <= x2)
                return true;

            return false;
        }
    }
}
