// File: Assets/PlanetFormation/Scripts/Generation/PlanetTextureGenerator.cs
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(AdaptiveCubeSphere))]
[RequireComponent(typeof(PlanetDataComponent))]
[RequireComponent(typeof(MeshRenderer))]
public class PlanetTextureGenerator : MonoBehaviour
{
    [Header("Color Mapping Settings")]
    [Tooltip("Define color based on height (0 = low/water, 1 = high/land).")]
    public Gradient heightGradient;

    [Range(-1f, 1f)]
    public float minHeight = -0.2f;

    [Range(-1f, 1f)]
    public float maxHeight = 0.8f;

    [Tooltip("Texture resolution per face for cubemap generation.")]
    [Range(16, 1024)]
    public int textureResolution = 256;

    [Header("Material & Update")]
    public bool autoUpdate = true;
    public string textureProperty = "_MainTex"; // property name used by material (e.g. "_BaseMap" for URP)

    // ✅ Correct field names
    private PlanetDataComponent dataComponent;
    private MeshRenderer meshRenderer;
    private Material material;

    private void Awake()
    {
        // Automatically get required components
        dataComponent = GetComponent<PlanetDataComponent>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (dataComponent == null || dataComponent.planetData == null)
        {
            Debug.LogError("[PlanetTextureGenerator] PlanetDataComponent or its planetData is missing!");
            return;
        }

        // Create or use existing material
        if (meshRenderer.sharedMaterial == null)
        {
            material = new Material(Shader.Find("Standard"));
            meshRenderer.sharedMaterial = material;
        }
        else
        {
            material = meshRenderer.sharedMaterial;
        }

        GenerateAndApplyTexture();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && autoUpdate)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this && gameObject)
                    GenerateAndApplyTexture();
            };
        }
    }
#endif

    [ContextMenu("Generate Planet Texture")]
    public void GenerateAndApplyTexture()
    {
        if (dataComponent == null)
            dataComponent = GetComponent<PlanetDataComponent>();

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (dataComponent == null || dataComponent.planetData == null)
        {
            Debug.LogWarning("[PlanetTextureGenerator] PlanetData is missing, cannot generate texture.");
            return;
        }

        Texture2D texture = GenerateEquirectangularTexture(dataComponent.planetData);

        if (meshRenderer.sharedMaterial == null)
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshRenderer.sharedMaterial.SetTexture(textureProperty, texture);

        Debug.Log("[PlanetTextureGenerator] Planet texture generated and applied.");
    }

    private Texture2D GenerateEquirectangularTexture(PlanetData data)
    {
        int width = textureResolution * 2;
        int height = textureResolution;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float latitude = Mathf.Lerp(90f, -90f, y / (float)(height - 1));

            for (int x = 0; x < width; x++)
            {
                float longitude = Mathf.Lerp(-180f, 180f, x / (float)(width - 1));

                float heightValue = SampleHeightFromPlanet(data, latitude, longitude);
                float normalized = Mathf.InverseLerp(minHeight, maxHeight, heightValue);

                Color c = heightGradient.Evaluate(normalized);
                pixels[y * width + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

    private float SampleHeightFromPlanet(PlanetData data, float latitude, float longitude)
    {
        float bestDist = float.MaxValue;
        float bestHeight = 0f;

        foreach (var face in data.faces)
        {
            if (face.sphericalCoords == null) continue;
            int count = face.sphericalCoords.Count;
            for (int i = 0; i < count; i++)
            {
                var sc = face.sphericalCoords[i];
                float dLat = Mathf.Abs(sc.latitude - latitude);
                float dLon = Mathf.Abs(sc.longitude - longitude);

                float dist = dLat * dLat + dLon * dLon;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestHeight = face.heights[i];
                }
            }
        }
        return bestHeight;
    }
}
