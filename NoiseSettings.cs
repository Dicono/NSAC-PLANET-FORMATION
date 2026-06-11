// File: Assets/PlanetFormation/Scripts/Noise/NoiseSettings.cs
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public int seed = 0;
    public float frequency = 1f;      // base frequency (scale)
    public int octaves = 4;
    public float lacunarity = 2f;     // frequency multiplier per octave
    public float persistence = 0.5f;  // amplitude multiplier per octave
    public float amplitude = 1f;      // final amplitude multiplier
    public Vector3 offset = Vector3.zero;
}
