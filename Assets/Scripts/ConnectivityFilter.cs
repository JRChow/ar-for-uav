using System;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;

namespace DefaultNamespace
{
    public class ConnectivityFilter : PointCloudFilterBase, IPointCloudFilter
    {
        // Min distance between two 3D points required to be considered "connected" 
        private const float ConnectivityThreshold = 2.5f;
        private const float GroundYValue = 70;
        private const float Tolerance = 0.5f;  // Error tolerance for equality check
        
        /// <summary>
        /// Remove ground points and perform a DFS to filter out outliers.
        /// </summary>
        /// <param name="pointCloud">The raw point cloud.</param>
        /// <returns>A filtered point cloud based on connectivity.</returns>
        public new List<Vector3> Filter(List<List<Vector3>> pointCloud)
        {
            var pointsNoGround = SubtractGround(pointCloud);
            var startPos = GetPointClosestToCentroid(pointsNoGround);
            var visited = new HashSet<Vector3>();
            DFS(startPos, pointsNoGround, visited);
            return new List<Vector3>(visited);
        }

        private static void DFS(Tuple<int, int> startPos, List<List<Vector3>> pointMat, HashSet<Vector3> visited)
        {
            if (pointMat.Count < 1) return;

            var (row, col) = startPos;
            var point = pointMat[row][col];
            visited.Add(point);

            // Up
            if (row - 1 >= 0)
            {
                var up = pointMat[row - 1][col];
                if (IsConnected(point, up) && !visited.Contains(up))
                {
                    DFS(new Tuple<int, int>(row - 1, col), pointMat, visited);                    
                }
            }
            // Down
            if (row + 1 < pointMat.Count)
            {
                var down = pointMat[row + 1][col];
                if (IsConnected(point, down) && !visited.Contains(down))
                {
                    DFS(new Tuple<int, int>(row + 1, col), pointMat, visited);
                }
            }
            // Left
            if (col - 1 >= 0)
            {
                var left = pointMat[row][col - 1];
                if (IsConnected(point, left) && !visited.Contains(left))
                {
                    DFS(new Tuple<int, int>(row, col - 1), pointMat, visited);                    
                }    
            }
            // Right
            if (col + 1 < pointMat[0].Count)
            {
                var right = pointMat[row][col + 1];
                if (IsConnected(point, right) && !visited.Contains(right))
                {
                    DFS(new Tuple<int, int>(row, col + 1), pointMat, visited);                    
                }                
            }
        }
        
        private static bool IsConnected(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2) <= ConnectivityThreshold;
        }

        /// <summary>
        /// Return the row & column of the point in the point matrix closest to the centroid.
        /// </summary>
        /// <param name="pointMat">The input point matrix.</param>
        /// <returns>The row & column of the point closest to the centroid.</returns>
        private static Tuple<int, int> GetPointClosestToCentroid(List<List<Vector3>> pointMat)
        {
            var centroid = GetCentroid(UnrollPointMatrix(pointMat));
            var minDist = Vector3.Distance(pointMat[0][0], centroid);
            var rowCol = new Tuple<int, int>(0, 0);
            for (var r = 0; r < pointMat.Count; r++)
            {
                for (var c = 0; c < pointMat[0].Count; c++)
                {
                    var dist = Vector3.Distance(pointMat[r][c], centroid);
                    if (!(dist < minDist)) continue;
                    minDist = dist;
                    rowCol = new Tuple<int, int>(r, c);
                }
            }

            return rowCol;
        }

        /// <summary>
        /// Set ground-level points to positive infinity.
        /// </summary>
        /// <param name="pointCloud">The point cloud to perform ground subtraction on.</param>
        /// <returns>A point cloud with ground-level points set to infinity.</returns>
        private static List<List<Vector3>> SubtractGround(List<List<Vector3>> pointCloud)
        {
            var pointsNoGround = new List<List<Vector3>>();
            foreach (var row in pointCloud)
            {
                pointsNoGround.Add(new List<Vector3>());
                foreach (var point in row)
                {
                    var newPoint = point;
                    if (IsPointOnGround(point))
                    {
                        newPoint = Vector3.positiveInfinity;
                    }
                    pointsNoGround[pointsNoGround.Count - 1].Add(newPoint);
                }
            }

            return pointsNoGround;
        }

        private static bool IsPointOnGround(Vector3 point)
        {
            return Math.Abs(point.y - GroundYValue) <= Tolerance;
        }
    }
}