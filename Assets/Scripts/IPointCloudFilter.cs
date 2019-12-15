using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public interface IPointCloudFilter
    {
        List<Vector3> Filter(List<List<Vector3>> pointMatrix);
    }
}