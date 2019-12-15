using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class PointCloudFilterBase : IPointCloudFilter
    {
        public List<Vector3> Filter(List<List<Vector3>> pointMatrix)
        {
            return (from row in pointMatrix 
                from point in row
                where !float.IsInfinity(point.x) && 
                      !float.IsInfinity(point.y) && 
                      !float.IsInfinity(point.z) 
                select point).ToList();
        }
        
        /// <summary>
        /// Calculate the centroid of the point cloud barring infinity points.
        /// </summary>
        /// <param name="pointCloud">A point cloud that may contain infinity points.</param>
        /// <returns>The centroid of the point cloud, not considering infinity values.</returns>
        protected static Vector3 GetCentroid(List<Vector3> pointCloud)
        {
            return new Vector3(
                pointCloud.Where(pt => !float.IsInfinity(pt.x)).Average(pt => pt.x),
                pointCloud.Where(pt => !float.IsInfinity(pt.y)).Average(pt => pt.y),
                pointCloud.Where(pt => !float.IsInfinity(pt.z)).Average(pt => pt.z)
            );
        }

        /// <summary>
        /// Unroll the 2D point matrix to an 1D list.
        /// </summary>
        /// <param name="pointMatrix">The 2D point matrix.</param>
        protected static List<Vector3> UnrollPointMatrix(List<List<Vector3>> pointMatrix)
        {
            return pointMatrix.SelectMany(row => row).ToList();
        }
    }
}