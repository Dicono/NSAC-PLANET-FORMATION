// File: Assets/PlanetFormation/Scripts/Generation/PlanetDataComponent.cs
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(AdaptiveCubeSphere))]
public class PlanetDataComponent : MonoBehaviour
{
    public PlanetData planetData;             // assign an asset (or leave null to create a runtime instance)
    public NoiseSettings noiseSettings = new NoiseSettings();
    public bool autoUpdate = true;

    private AdaptiveCubeSphere sphere;

    private void Awake()
    {
        sphere = GetComponent<AdaptiveCubeSphere>();
        if (autoUpdate) GenerateHeightData();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        sphere = GetComponent<AdaptiveCubeSphere>();
        if (!Application.isPlaying && autoUpdate)
        {
            // delayCall avoids messy re-entrancy while the inspector is changing values.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this) GenerateHeightData();
            };
        }
    }
#endif

    [ContextMenu("Test Spherical Coordinates")]
    public void TestSphericalCoordinates()
    {
        if (planetData == null)
        {
            Debug.LogWarning("No planet data available. Generate height data first.");
            return;
        }

        planetData.EnsureFacesInitialized();
        
        // Test coordinates from different faces
        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            var face = planetData.faces[faceIndex];
            if (face.sphericalCoords != null && face.sphericalCoords.Count > 0)
            {
                var coord = face.GetSphericalCoord(0, 0);
                Debug.Log($"🧩 Face {face.face}: Corner (0,0) - Lat: {coord.latitude:F2}°, Lon: {coord.longitude:F2}°");
                
                if (face.resolution > 0)
                {
                    var centerCoord = face.GetSphericalCoord(face.resolution / 2, face.resolution / 2);
                    Debug.Log($"🧩 Face {face.face}: Center - Lat: {centerCoord.latitude:F2}°, Lon: {centerCoord.longitude:F2}°");
                }
            }
        }
    }

    [ContextMenu("Generate Height Data")]
    public void GenerateHeightData()
    {
        if (sphere == null) sphere = GetComponent<AdaptiveCubeSphere>();
        if (sphere == null)
        {
            Debug.LogWarning("PlanetDataComponent requires an AdaptiveCubeSphere on the same GameObject.");
            return;
        }

        if (planetData == null)
        {
            // create an in-memory PlanetData instance so generation still works if user hasn't created an asset.
            planetData = ScriptableObject.CreateInstance<PlanetData>();
            planetData.name = "PlanetData_RuntimeInstance";
        }

        planetData.EnsureFacesInitialized();

        // loop the six faces (same mapping as your AdaptiveCubeSphere)
        GenerateFace(CubeFace.PosX, sphere.faceResolutions.posX);
        GenerateFace(CubeFace.NegX, sphere.faceResolutions.negX);
        GenerateFace(CubeFace.PosY, sphere.faceResolutions.posY);
        GenerateFace(CubeFace.NegY, sphere.faceResolutions.negY);
        GenerateFace(CubeFace.PosZ, sphere.faceResolutions.posZ);
        GenerateFace(CubeFace.NegZ, sphere.faceResolutions.negZ);

        Debug.Log($"[PlanetDataComponent] Generated height data (radius={sphere.radius}).");
        
        // Quick test - debug spherical coordinates of a few points
        if (planetData.faces[0].sphericalCoords != null && planetData.faces[0].sphericalCoords.Count > 0)
        {
            var coord = planetData.faces[0].GetSphericalCoord(0, 0);
            Debug.Log($"🔍 Debug - Face[0] vertex (0,0): Lat: {coord.latitude:F2}, Lon: {coord.longitude:F2}");
            
            var coord2 = planetData.faces[0].GetSphericalCoord(planetData.faces[0].resolution, planetData.faces[0].resolution);
            Debug.Log($"🔍 Debug - Face[0] vertex (max,max): Lat: {coord2.latitude:F2}, Lon: {coord2.longitude:F2}");
        }
    }

    private void GenerateFace(CubeFace face, int resolution)
    {
        var faceMap = planetData.GetFace(face);
        faceMap.SetSize(resolution);

        Vector3 localUp = FaceToVector(face);
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        for (int y = 0; y <= resolution; y++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / resolution;
                Vector3 pointOnUnitCube = localUp +
                    (percent.x - 0.5f) * 2f * axisA +
                    (percent.y - 0.5f) * 2f * axisB;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized; // unit sphere direction

                // --- NEW PART ---
                var spherical = SphericalCoordinate.FromDirection(pointOnUnitSphere);
                faceMap.SetSphericalCoord(x, y, spherical);
                // --- END NEW PART ---

                // sample the noise in 3D space around the unit sphere. Multiply by radius if you want scale-based sampling:
                float h = NoiseGenerator.SampleNoise(pointOnUnitSphere * noiseSettings.frequency, noiseSettings);
                faceMap.Set(x, y, h);
            }
        }
    }

    private Vector3 FaceToVector(CubeFace f)
    {
        switch (f)
        {
            case CubeFace.PosX: return Vector3.right;
            case CubeFace.NegX: return Vector3.left;
            case CubeFace.PosY: return Vector3.up;
            case CubeFace.NegY: return Vector3.down;
            case CubeFace.PosZ: return Vector3.forward;
            case CubeFace.NegZ: return Vector3.back;
        }
        return Vector3.zero;
    }
}
