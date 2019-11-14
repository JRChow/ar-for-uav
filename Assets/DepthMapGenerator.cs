using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class DepthMapGenerator : MonoBehaviour
{
    // Distance between sample points on screen for ray-casting
    public float samplePointDistance;
    public double filterTolerance;

    private ObjectSelector _objectSelector;
    private CubeVisualizer _cubeVisualizer;
    
    private List<List<Vector3>> _depthMapList;

    // Start is called before the first frame update
    void Start()
    {
        _objectSelector = GetComponent<ObjectSelector>();
        _cubeVisualizer = GetComponent<CubeVisualizer>();
        _depthMapList = new List<List<Vector3>>();
    }

    private void Reset()
    {
        _depthMapList.Clear();
    }

    // For all user-selected boxes, get their depth maps.
    public void GetDepthMapList()
    {
        Reset();
        
        var selectedList = _objectSelector.SelectedList;
        
        _objectSelector.Reset();
        _cubeVisualizer.Reset();
        
        // Perform ray-cast to get depth maps
        foreach (var depthMap in selectedList.Select(
            diagonalPoints => GetDepthMap(diagonalPoints[0], diagonalPoints[1])))
        {
            // Filter outliers in each depth map
            var filteredDepthMap = FilterDepthMap(depthMap);
            _depthMapList.Add(filteredDepthMap);
        }

        // Visualize each bounding box
        foreach (var depthMap in _depthMapList)
        {
            _cubeVisualizer.VisualizeDepthMap(depthMap);
        }
    }

    // Given two diagonal points on screen, perform a "mesh-grid" ray-cast to get depth map. 
    private List<Vector3> GetDepthMap(Vector2 startPos, Vector2 endPos)
    {
        var depthMap = new List<Vector3>();
        
        var minX = Math.Min(startPos.x, endPos.x);
        var maxX = Math.Max(startPos.x, endPos.x);
        var minY = Math.Min(startPos.y, endPos.y);
        var maxY = Math.Max(startPos.y, endPos.y);

        for (var x = minX; x <= maxX; x += samplePointDistance)
        {
            for (var y = minY; y <= maxY; y += samplePointDistance)
            {
                var hitPoint = GetRayCastHitPointFromScreen(new Vector2(x, y));
                if (hitPoint != Vector3.positiveInfinity) depthMap.Add(hitPoint);
            }
        }

        return depthMap;
    }

    // Filter out all points that are more than some standard deviations from the centroid
    private List<Vector3> FilterDepthMap(IReadOnlyCollection<Vector3> depthMap)
    {
        var centroid = new Vector3(
            depthMap.Where(pt => !float.IsInfinity(pt.x)).Average(pt => pt.x),
            depthMap.Where(pt => !float.IsInfinity(pt.y)).Average(pt => pt.y),
            depthMap.Where(pt => !float.IsInfinity(pt.z)).Average(pt => pt.z)
        );

        var distanceList = depthMap.Select(pt => (double) Vector3.Distance(pt, centroid))
            .Where(d => !double.IsInfinity(d)).ToList();
        var threshold = filterTolerance * StandardDeviation(distanceList);
        return depthMap.Where(pt => Vector3.Distance(pt, centroid) <= threshold).ToList();
    }

    // Given a position on screen, do ray-cast and return the hit point.
    // If failed, return the positive infinity vector.
    private static Vector3 GetRayCastHitPointFromScreen(Vector2 screenPos)
    {
        if (Camera.main == null) return Vector3.positiveInfinity;
        var ray = Camera.main.ScreenPointToRay(screenPos);

        return Physics.Raycast(ray, out var hit) ? hit.point : Vector3.positiveInfinity;
    }
    
    // Helper function for calculating standard deviation
    private static double StandardDeviation(IEnumerable<double> values)
    {
        var enumerable = values.ToList();
        var avg = enumerable.Average();
        return Math.Sqrt(enumerable.Average(v=>Math.Pow(v-avg,2)));
    }
}
