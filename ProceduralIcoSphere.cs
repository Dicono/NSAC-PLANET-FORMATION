// File: Assets/PlanetFormation/Scripts/ProceduralCubeSphere.cs
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCubeSphere : MonoBehaviour
{
    [Header("Sphere Resolution")]
    [Range(1, 256)] public int subdivisions = 16;

    [Header("Sphere Shape Settings")]
    public float radius = 1f;
    public bool autoUpdate = true;

    private MeshFilter meshFilter;
    private Mesh mesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        BuildMesh();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && autoUpdate)
            UnityEditor.EditorApplication.delayCall += BuildMesh;
    }

    private void BuildMesh()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        mesh = GenerateCubeSphere(subdivisions, radius);
        mesh.name = "Procedural Cube Sphere";

        meshFilter.sharedMesh = mesh;
    }

    private Mesh GenerateCubeSphere(int resolution, float radius)
    {
        var mesh = new Mesh();
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();

        // Generate 6 cube faces
        CreateFace(Vector3.forward, resolution, radius, vertices, triangles);
        CreateFace(Vector3.back, resolution, radius, vertices, triangles);
        CreateFace(Vector3.left, resolution, radius, vertices, triangles);
        CreateFace(Vector3.right, resolution, radius, vertices, triangles);
        CreateFace(Vector3.up, resolution, radius, vertices, triangles);
        CreateFace(Vector3.down, resolution, radius, vertices, triangles);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void CreateFace(Vector3 localUp, int resolution, float radius,
        System.Collections.Generic.List<Vector3> vertices,
        System.Collections.Generic.List<int> triangles)
    {
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        int vertexIndexStart = vertices.Count;

        for (int y = 0; y <= resolution; y++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / resolution;
                Vector3 pointOnUnitCube = localUp +
                    (percent.x - 0.5f) * 2f * axisA +
                    (percent.y - 0.5f) * 2f * axisB;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * radius;
                vertices.Add(pointOnUnitSphere);

                if (x != resolution && y != resolution)
                {
                    int i = vertexIndexStart + x + y * (resolution + 1);
                    int nextRow = vertexIndexStart + x + (y + 1) * (resolution + 1);

                    triangles.Add(i);
                    triangles.Add(nextRow + 1);
                    triangles.Add(nextRow);

                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(nextRow + 1);
                }
            }
        }
    }
}
