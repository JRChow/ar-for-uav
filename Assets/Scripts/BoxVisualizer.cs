using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BoxVisualizer : MonoBehaviour
{
    // A list of cubes parametrized by center and size, used for visualization.
    private List<Vector3[]> _cubeList;
    private bool _show2DBox;
    
    /// <summary>
    /// Colors corresponding to each object (limited to at most 8 objects)
    /// </summary>
    public static readonly List<Color> VisColors = new List<Color>
    {
        Color.blue, Color.green, Color.yellow, Color.grey, 
        Color.magenta, Color.cyan, Color.black, Color.red
    };

    private void Start()
    {
        _cubeList = new List<Vector3[]>();
    }
    
    /// <summary>
    /// Add a new point cloud as a cube to visualize.
    /// </summary>
    /// <param name="newPointCloud">The new point cloud to be visualized as a cube.</param>
    public void AddNewCube(List<Vector3> newPointCloud)
    {
        var centerAndSize = GetCubeCenterAndSize(newPointCloud);
        _cubeList.Add(centerAndSize);
    }
    
    /// <summary>
    /// Update the cube associated with an object with a new point cloud.
    /// </summary>
    /// <param name="updatedPointCloud">The updated point cloud.</param>
    /// <param name="objIdx">The object's index.</param>
    public void UpdateCube(List<Vector3> updatedPointCloud, int objIdx)
    {
        var centerAndSize = GetCubeCenterAndSize(updatedPointCloud);
        _cubeList[objIdx] = centerAndSize;
    }

    // Helper function to convert point cloud to a cube parametrized by center and size.
    private static Vector3[] GetCubeCenterAndSize(IReadOnlyCollection<Vector3> pointCloud)
    {
        var minX = pointCloud.Min(pt => pt.x);
        var maxX = pointCloud.Max(pt => pt.x);
        var minY = pointCloud.Min(pt => pt.y);
        var maxY = pointCloud.Max(pt => pt.y);
        var minZ = pointCloud.Min(pt => pt.z);
        var maxZ = pointCloud.Max(pt => pt.z);

        var center = new Vector3(
            (minX + maxX) / 2,
            (minY + maxY) / 2,
            (minZ + maxZ) / 2
        );
        var size = new Vector3(
            maxX - minX,
            maxY - minY,
            maxZ - minZ
        );

        return new[] {center, size};
    }

    private void OnDrawGizmos()
    {
        if (_show2DBox) return;
        VisualizeBox();
    }

    private void OnGUI()
    {
        if (!_show2DBox) return;
        VisualizeBox();
    }

    // Render box in 2D or 3D based on toggle value
    private void VisualizeBox()
    {
        if (_cubeList == null) return;
        
        for (var i = 0; i < _cubeList.Count; i++)   
        {
            var color = VisColors[i];
            var cube = _cubeList[i];
            var center = cube[0];
            var size = cube[1];

            if (_show2DBox)
            {
                RenderCubeIn2D(center, size, color);
            }
            else
            {
                RenderCubeIn3D(center, size, color);                
            }
        }
    }

    /// <summary>
    /// Toggle if we should show 2D vs 3D.
    /// </summary>
    public void Toggle2D()
    {
        _show2DBox = !_show2DBox;
    }

    private static void RenderCubeIn3D(Vector3 center, Vector3 size, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
    }

    private static void RenderCubeIn2D(Vector3 center, Vector3 size, Color color)
    {
        Handles.color = color;
        var rectVertices = CubeTo2D(center, size);
        Handles.DrawPolyLine(rectVertices);
        // Close the line into a rectangle
        Handles.DrawLine(rectVertices[rectVertices.Length - 1], rectVertices[0]);
    }
    
    /// <summary>
    /// Convert 3D cube to 2D rectangle.
    /// </summary>
    /// <param name="center">Center of 3D cube.</param>
    /// <param name="size">Size of 3D cube.</param>
    /// <returns>Array of vertices of 2D rectangle.</returns>
    private static Vector3[] CubeTo2D(Vector3 center, Vector3 size)
    {
        var min3DCorner = center - size / 2;
        var max3DCorner = center + size / 2;

        var cornersIn2D = new List<Vector2>
        {
            WorldToScreenPointInGUICoordinate(min3DCorner.x, min3DCorner.y, min3DCorner.z),
            WorldToScreenPointInGUICoordinate(min3DCorner.x, min3DCorner.y, max3DCorner.z),
            WorldToScreenPointInGUICoordinate(min3DCorner.x, max3DCorner.y, min3DCorner.z),
            WorldToScreenPointInGUICoordinate(max3DCorner.x, min3DCorner.y, min3DCorner.z),
            WorldToScreenPointInGUICoordinate(max3DCorner.x, max3DCorner.y, min3DCorner.z),
            WorldToScreenPointInGUICoordinate(max3DCorner.x, min3DCorner.y, max3DCorner.z),
            WorldToScreenPointInGUICoordinate(min3DCorner.x, max3DCorner.y, max3DCorner.z),
            WorldToScreenPointInGUICoordinate(max3DCorner.x, max3DCorner.y, max3DCorner.z),
        };

        var minX = cornersIn2D.Select(p => p.x).Min();
        var minY = cornersIn2D.Select(p => p.y).Min();
        var maxX = cornersIn2D.Select(p => p.x).Max();
        var maxY = cornersIn2D.Select(p => p.y).Max();

        return new []
        {
            new Vector3(minX, minY),
            new Vector3(maxX, minY),
            new Vector3(maxX, maxY),
            new Vector3(minX, maxY),
        };
    }

    private static Vector2 WorldToScreenPointInGUICoordinate(float worldX, float worldY, float worldZ)
    {
        Assert.IsNotNull(Camera.main);
        var screenPoint = Camera.main.WorldToScreenPoint(new Vector3(worldX, worldY, worldZ));
        return new Vector2(screenPoint.x, Screen.height - screenPoint.y);
    }

    /// <summary>
    ///  Getter for cube list 
    /// </summary>
    public List<Vector3[]> GetCubesList()
    {
        return _cubeList;
    }

    /// <summary>
    /// Delete all bounding boxes in the scene.
    /// </summary>
    public void Reset()
    {
        _cubeList = new List<Vector3[]>();
    }

}
