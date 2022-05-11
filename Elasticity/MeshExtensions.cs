using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FE_Analysis.Elasticity
{
    public static class MeshExtensions
    {
        // return MeshGeometry3D for the wire frame of the mesh
        public static MeshGeometry3D ToWireframe(this MeshGeometry3D mesh, double thickness)
        {
            // generate Dictionary for identifying triangles with identical edges,
            // so that these are only drawn once
            var alreadyDrawn = new Dictionary<int, int>();

            // generate a mesh for the wire frame model
            var wireframe = new MeshGeometry3D();

            // loop over triangles of mesh
            for (var triangle = 0; triangle < mesh.TriangleIndices.Count; triangle += 3)
            {
                // get nodal indices of triangles
                var index1 = mesh.TriangleIndices[triangle];
                var index2 = mesh.TriangleIndices[triangle + 1];
                var index3 = mesh.TriangleIndices[triangle + 2];

                // generate the 3 edges of triangle
                AddTriangleSegment(mesh, wireframe, alreadyDrawn, index1, index2, thickness);
                AddTriangleSegment(mesh, wireframe, alreadyDrawn, index2, index3, thickness);
                AddTriangleSegment(mesh, wireframe, alreadyDrawn, index3, index1, thickness);
            }

            return wireframe;
        }

        // add triangle edge to wire frame model
        private static void AddTriangleSegment(MeshGeometry3D mesh,
            MeshGeometry3D wireframe, IDictionary<int, int> alreadyDrawn,
            int index1, int index2, double thickness)
        {
            // unique ID for an edge with 2 points
            if (index1 > index2)
            {
                (index1, index2) = (index2, index1);
            }

            var segmentId = index1 * mesh.Positions.Count + index2;

            // ignore edge if already added to another triangle
            if (alreadyDrawn.ContainsKey(segmentId)) return;
            alreadyDrawn.Add(segmentId, segmentId);

            // otherwise generate edge
            AddSegment(wireframe, mesh.Positions[index1], mesh.Positions[index2], thickness);
        }

        // add triangle to mesh without reusing points, 
        // so that triangles do not have identical normal vector
        private static void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
        {
            // generate points
            var index1 = mesh.Positions.Count;
            mesh.Positions.Add(point1);
            mesh.Positions.Add(point2);
            mesh.Positions.Add(point3);

            // generate triangle
            mesh.TriangleIndices.Add(index1++);
            mesh.TriangleIndices.Add(index1++);
            mesh.TriangleIndices.Add(index1);
        }

        // generate thin, rectangular prism between 2 points
        // if (extend is true), extend edge by half line weight,
        // so that edges with 2 identical endpoints fit together
        // if an up-Vector is missing, generate an orthogonal vector for it
        private static void AddSegment(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, double thickness, bool extend)
        {
            // find an Up-Vector that is not colinear with edge
            // start with a vector parallel to Y-Axis
            var up = new Vector3D(0, 1, 0);

            // if an edge and an Up-Vector point in the same direction
            // us an Up-Vector parallel to X-Axis
            var segment = point2 - point1;
            segment.Normalize();
            if (Math.Abs(Vector3D.DotProduct(up, segment)) > 0.9)
                up = new Vector3D(1, 0, 0);

            // add edge to mesh
            AddSegment(mesh, point1, point2, up, thickness, extend);
        }

        private static void AddSegment(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, double thickness)
        {
            AddSegment(mesh, point1, point2, thickness, false);
        }

        public static void AddSegment(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, Vector3D up, double thickness)
        {
            AddSegment(mesh, point1, point2, up, thickness, false);
        }

        private static void AddSegment(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, Vector3D up, double thickness,
            bool extend)
        {
            // edge vector
            var v = point2 - point1;

            if (extend)
            {
                // extend edge length at both ends by edge weight/2
                var n = ScaleVector(v, thickness / 2.0);
                point1 -= n;
                point2 += n;
            }

            // scaled edge vector
            var n1 = ScaleVector(up, thickness / 2.0);

            // an additional orthogonal vector
            var n2 = Vector3D.CrossProduct(v, n1);
            n2 = ScaleVector(n2, thickness / 2.0);

            // generate a thin box
            // p1pm means point1 PLUS n1 MINUS n2
            var p1pp = point1 + n1 + n2;
            var p1mp = point1 - n1 + n2;
            var p1pm = point1 + n1 - n2;
            var p1mm = point1 - n1 - n2;
            var p2pp = point2 + n1 + n2;
            var p2mp = point2 - n1 + n2;
            var p2pm = point2 + n1 - n2;
            var p2mm = point2 - n1 - n2;

            // faces
            AddTriangle(mesh, p1pp, p1mp, p2mp);
            AddTriangle(mesh, p1pp, p2mp, p2pp);

            AddTriangle(mesh, p1pp, p2pp, p2pm);
            AddTriangle(mesh, p1pp, p2pm, p1pm);

            AddTriangle(mesh, p1pm, p2pm, p2mm);
            AddTriangle(mesh, p1pm, p2mm, p1mm);

            AddTriangle(mesh, p1mm, p2mm, p2mp);
            AddTriangle(mesh, p1mm, p2mp, p1mp);

            // ends
            AddTriangle(mesh, p1pp, p1pm, p1mm);
            AddTriangle(mesh, p1pp, p1mm, p1mp);

            AddTriangle(mesh, p2pp, p2mp, p2mm);
            AddTriangle(mesh, p2pp, p2mm, p2pm);
        }

        // vector length
        private static Vector3D ScaleVector(Vector3D vector, double length)
        {
            var scale = length / vector.Length;
            return new Vector3D(
                vector.X * scale,
                vector.Y * scale,
                vector.Z * scale);
        }
    }
}