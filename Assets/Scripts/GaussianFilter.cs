using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class GaussianFilter : PointCloudFilterBase, IPointCloudFilter
    {
        private const double FilterTolerance = 3;

        /// <summary>
        /// Filter out all points that are more than some standard deviations from the centroid.
        /// </summary>
        /// <param name="pointMatrix"></param>
        /// <returns></returns>
        public new List<Vector3> Filter(List<List<Vector3>> pointMatrix)
        {
            var pointList = UnrollPointMatrix(pointMatrix);
            if (pointList.Count == 0) return pointList;
        
            var centroid = GetCentroid(pointList);

            var xThreshold = CalculateAxisThreshold(pointList.Select(p => p.x), centroid.x);
            var yThreshold = CalculateAxisThreshold(pointList.Select(p => p.y), centroid.y);
            var zThreshold = CalculateAxisThreshold(pointList.Select(p => p.z), centroid.z);

            return pointList.Where(pt => IsPointInBound(pt, centroid, xThreshold, yThreshold, zThreshold)).ToList();
        }
        
        // Given points on an axis and a pivot, calculate the filtering threshold.
        private static double CalculateAxisThreshold(IEnumerable<float> points, float pivot)
        { 
            var axisDistanceList = CalculateDistanceList(points, pivot);
            return FilterTolerance * StandardDeviation(axisDistanceList);
        }
        
        // Calculate the distances between each point and the pivot (discarding infinite distances).
        private static IEnumerable<double> CalculateDistanceList(IEnumerable<float> points, float pivot)
        {
            return points.Select(pt => (double) Math.Abs(pt - pivot)).Where(d => !double.IsInfinity(d)).ToList();
        }
        
        // Return true if the point is close enough to the centroid to not be filtered.
        private static bool IsPointInBound(Vector3 point, Vector3 centroid,
            double xThreshold, double yThreshold, double zThreshold)
        {
            return Math.Abs(point.x - centroid.x) <= xThreshold &&
                   Math.Abs(point.y - centroid.y) <= yThreshold &&
                   Math.Abs(point.z - centroid.z) <= zThreshold;
        }
        
        // Helper function for calculating standard deviation.
        private static double StandardDeviation(IEnumerable<double> values)
        {
            var enumerable = values.ToList();
            var avg = enumerable.Average();
            return Math.Sqrt(
                enumerable.Average( v => Math.Pow(v - avg, 2) )
            );
        }
    }
}