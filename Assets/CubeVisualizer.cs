using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor.UIElements;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class CubeVisualizer : MonoBehaviour
{
    private List<Vector3[]> cubeList;

    private void Start()
    {
        cubeList = new List<Vector3[]>();
    }

    public void VisualizeDepthMap(List<Vector3> depthMap)
    {
        var minX = depthMap.Min(pt => pt.x);
        var maxX = depthMap.Max(pt => pt.x);
        var minY = depthMap.Min(pt => pt.y);
        var maxY = depthMap.Max(pt => pt.y);
        var minZ = depthMap.Min(pt => pt.z);
        var maxZ = depthMap.Max(pt => pt.z);

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
        
        cubeList.Add(new Vector3[] {center, size});
    }

    private void OnDrawGizmos()
    {
        if (cubeList == null) return;
        
        Gizmos.color = Color.cyan;
        foreach (var cube in cubeList)
        {
            var center = cube[0];
            var size = cube[1];
            Gizmos.DrawWireCube(center, size);    
        }
    }

    public void Reset()
    {
        cubeList.Clear();
    }
}
