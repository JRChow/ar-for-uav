using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Experimental.XR;

namespace DefaultNamespace
{
    /// <summary>
    /// Base class for generating a filtered point cloud.
    /// </summary>
    public class PointCloudGenerator : MonoBehaviour
    {
        /// <summary>
        /// Distance between 2D sample points on screen during ray-casting 
        /// </summary>
        public float samplePointDistance = 3;
        
        /// <summary>
        /// If false, generate complete point cloud by scanning rectangle defined by extrema of user drawing.
        /// If true, filter out points outside user-drawn polygon by setting them to infinity.
        /// </summary>
        public bool dropPointsOutsidePolygon;
        
        private List<List<Vector3>> _pointCloudList;
        private BoxVisualizer _boxVisualizer;
        private IPointCloudFilter _pointCloudFilter;

        private void Start()
        {
            _boxVisualizer = GetComponent<BoxVisualizer>();
            _pointCloudFilter = new ConnectivityFilter();  // <- Change filter here
            _pointCloudList = new List<List<Vector3>>();
        }

        public void Reset()
        {
            _pointCloudList = new List<List<Vector3>>();
            _boxVisualizer.Reset();
        }

        public void AddNewPointCloud(List<Vector3> userDrawnShapeHull)
        {
            var newPointCloudFiltered = GenerateFilteredPointCloud(userDrawnShapeHull);
            _pointCloudList.Add(newPointCloudFiltered);
            _boxVisualizer.AddNewCube(newPointCloudFiltered);
        }

        public void UpdateExistingPointCloud(List<Vector3> userDrawnShapeHull, int objIdx)
        {
            var pointCloudFiltered = GenerateFilteredPointCloud(userDrawnShapeHull);
            _pointCloudList[objIdx].AddRange(pointCloudFiltered);
            var updatedPointCloud = _pointCloudList[objIdx];
            _boxVisualizer.UpdateCube(updatedPointCloud, objIdx);
        }

        private List<Vector3> GenerateFilteredPointCloud(List<Vector3> userDrawnShapeHull)
        {
            var newPointCloud = GeneratePointCloudByRayCast(userDrawnShapeHull);
            return _pointCloudFilter.Filter(newPointCloud);
        }

        /// <summary>
        /// Given lower & upper bounds of X & Y, perform a "mesh-grid" ray-cast to get point cloud.
        /// </summary>
        /// <param name="userDrawnShapeHull"></param>
        /// <returns></returns>
        private List<List<Vector3>> GeneratePointCloudByRayCast(List<Vector3> userDrawnShapeHull)
        {
            var minX = userDrawnShapeHull.Min(pt => pt.x);
            var maxX = userDrawnShapeHull.Max(pt => pt.x);
            var minY = userDrawnShapeHull.Min(pt => pt.y);
            var maxY = userDrawnShapeHull.Max(pt => pt.y);
        
            var pointMatrix = new List<List<Vector3>>();

            for (var x = minX; x <= maxX; x += samplePointDistance)
            {
                pointMatrix.Add(new List<Vector3>());
                for (var y = minY; y <= maxY; y += samplePointDistance)
                {
                    var pos = new Vector2(x, y);
                    var hitPoint = GetRayCastHitPointFromScreen(pos);
                    if (dropPointsOutsidePolygon &&
                        !PolygonContainsPoint(userDrawnShapeHull.ToArray(), pos))
                    {
                        hitPoint = Vector3.positiveInfinity;
                    }
                    pointMatrix[pointMatrix.Count - 1].Add(hitPoint);
                }
            }

            return pointMatrix;
        }

        /// <summary>
        /// Check if a point is inside a polygon.
        /// </summary>
        /// <param name="polyPoints">The polygon.</param>
        /// <param name="p">The point.</param>
        /// <returns>True if the point is inside the polygon.</returns>
        private static bool PolygonContainsPoint(IReadOnlyList<Vector3> polyPoints, Vector2 p)
        {
            var j = polyPoints.Count - 1;
            var inside = false;
            for (var i = 0; i < polyPoints.Count; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }

        // Given a position on screen, do ray-cast and return the hit point.
        // If failed, return the positive infinity vector.
        private static Vector3 GetRayCastHitPointFromScreen(Vector2 screenPos)
        {
            if (Camera.main == null) return Vector3.positiveInfinity;
            var ray = Camera.main.ScreenPointToRay(screenPos);

            return Physics.Raycast(ray, out var hit) ? hit.point : Vector3.positiveInfinity;
        }
    }
}