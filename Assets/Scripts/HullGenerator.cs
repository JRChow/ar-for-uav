using System.Collections.Generic;
using System.Linq;
using ConcaveHull;
using UnityEngine;

namespace DefaultNamespace
{
    public static class HullGenerator
    {
        /// <summary>
        /// Generate concave hull edges (not in a particular order) based on input points
        /// </summary>
        /// <param name="pointsOnLine">The points drawn by the user</param>
        /// <param name="concavity">Concavity is a value used to restrict the concave angles.
        /// It can go from -1 (no concavity) to 1 (extreme concavity).
        /// Avoid concavity == 1 if you don't want 0º angles</param>
        /// <param name="scaleFactor">his sets how big is the area where concavities are going to be searched.
        /// The bigger, the more sharp the angles can be. Setting it to a very high value might affect the performance of the program.
        /// This value should be relative to how close to each other the points to be connected are.</param>
        /// <returns></returns>
        public static IEnumerable<Line> Compute(List<Vector3> pointsOnLine, double concavity, int scaleFactor)
        {
            Reset();
            
            var nodeList = Vector3ToNodes(pointsOnLine);
            Hull.setConvexHull(nodeList);  // Pre-req for concave hull
            Hull.setConcaveHull(concavity, scaleFactor);
            // Must create deep copy because it's a static class
            return new List<Line>(Hull.hull_concave_edges.ToArray());
        }
        
        /// <summary>
        /// Convert a list of Vector3 to a list of Node
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static List<Node> Vector3ToNodes(IReadOnlyList<Vector3> points)
        {
            var nodeList = new List<Node>();
            for (var i = 0; i < points.Count; i++)
            {
                nodeList.Add(new Node(points[i].x, points[i].y, i));
            }

            return nodeList;
        }
        
        /// <summary>
        /// Convert a list of Line into a list of Vector3 (order not guaranteed)
        /// </summary>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public static List<Vector3> CastLineListToPoints(IEnumerable<Line> lineList)
        {
            return lineList.Select(line => line.nodes[0])
                .Select(node => new Vector3((float) node.x, (float) node.y)).ToList();
        }
        
        /// <summary>
        /// Convert a list of Line into a list of Vector3[2] array representation
        /// </summary>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public static List<Vector3[]> CastLineListToVecArrays(IEnumerable<Line> lineList)
        {
            var vecArrList = new List<Vector3[]>();
            foreach (var line in lineList)
            {
                var from = new Vector3((float) line.nodes[0].x, (float) line.nodes[0].y);
                var to = new Vector3((float) line.nodes[1].x, (float) line.nodes[1].y);
                vecArrList.Add(new[] {from, to});
            }

            return vecArrList;
        }

        private static void Reset()
        {
            Hull.Reset();
        }
    }
}