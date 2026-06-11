// File: Assets/PlanetFormation/Scripts/Noise/NoiseGenerator.cs
using UnityEngine;

public static class NoiseGenerator
{
    // Simple fBM-like 3D sampler built from Unity's 2D PerlinNoise.
    // It blends Perlin on different axis pairs to emulate a 3D field.
    public static float SampleNoise(Vector3 p, NoiseSettings settings)
    {
        // apply offset + seed (converted to float)
        float seedOffset = settings.seed * 0.12345f;
        p += settings.offset;

        float amplitude = 1f;
        float frequency = settings.frequency;
        float noise = 0f;

        for (int i = 0; i < Mathf.Max(1, settings.octaves); i++)
        {
            // evaluate three 2D PerlinNoise variations and average them
            float n1 = Mathf.PerlinNoise((p.x + seedOffset) * frequency, (p.y + seedOffset) * frequency);
            float n2 = Mathf.PerlinNoise((p.y + 13.7f + seedOffset) * frequency, (p.z + 93.1f + seedOffset) * frequency);
            float n3 = Mathf.PerlinNoise((p.z + 57.3f + seedOffset) * frequency, (p.x + 29.2f + seedOffset) * frequency);

            // map [0,1] -> [-1,1] and average the three
            float n = ((n1 + n2 + n3) / 3f) * 2f - 1f;

            noise += n * amplitude;

            amplitude *= settings.persistence;
            frequency *= settings.lacunarity;
        }

        return noise * settings.amplitude;
    }
}
