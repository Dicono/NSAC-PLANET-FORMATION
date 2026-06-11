// File: Assets/PlanetFormation/Scripts/Data/PlanetData.cs
using UnityEngine;
using System.Collections.Generic;

public enum CubeFace { PosX = 0, NegX = 1, PosY = 2, NegY = 3, PosZ = 4, NegZ = 5 }

[CreateAssetMenu(fileName = "PlanetData", menuName = "PlanetFormation/PlanetData")]
public class PlanetData : ScriptableObject
{
    [System.Serializable]
    public class FaceHeightmap
    {
        public CubeFace face;
        public int resolution; // resolution used when generating -> grid is (resolution+1)^2
        public List<float> heights = new List<float>();
        
        // Spherical coordinates for each vertex (optional, used for texture generation in planet space)
        [System.NonSerialized] public List<SphericalCoordinate> sphericalCoords;

        public void SetSize(int resolution)
        {
            this.resolution = Mathf.Max(1, resolution);
            int size = (this.resolution + 1) * (this.resolution + 1);
            if (heights == null || heights.Count != size)
            {
                heights = new List<float>(new float[size]);
            }
            
            // also reset spherical coordinates
            sphericalCoords = new List<SphericalCoordinate>(new SphericalCoordinate[size]);
        }

        public int Size => resolution + 1;

        public int Index(int x, int y) => y * (resolution + 1) + x;

        public float Get(int x, int y)
        {
            if (heights == null) return 0f;
            return heights[Index(x, y)];
        }

        public void Set(int x, int y, float v)
        {
            if (heights == null) SetSize(resolution);
            heights[Index(x, y)] = v;
        }

        // Spherical coordinate access methods
        public SphericalCoordinate GetSphericalCoord(int x, int y)
        {
            if (sphericalCoords == null) return new SphericalCoordinate();
            return sphericalCoords[Index(x, y)];
        }

        public void SetSphericalCoord(int x, int y, SphericalCoordinate coord)
        {
            if (sphericalCoords == null) SetSize(resolution);
            sphericalCoords[Index(x, y)] = coord;
        }

        /// <summary>
        /// Produce a simple grayscale Texture2D for quick inspection/export.
        /// Assumes values are roughly in [-1,1] (maps to 0..1). This is a convenience for debugging.
        /// </summary>
        public Texture2D ToTexture(bool assumeMinusOneToOne = true)
        {
            int s = Size;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[s * s];
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float v = Get(x, y);
                    float c = assumeMinusOneToOne ? (v * 0.5f + 0.5f) : v;
                    pixels[y * s + x] = new Color(c, c, c, 1f);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }

    // six faces; index = (int)CubeFace
    public FaceHeightmap[] faces = new FaceHeightmap[6];

    private void OnEnable()
    {
        EnsureFacesInitialized();
    }

    public void EnsureFacesInitialized()
    {
        if (faces == null || faces.Length != 6)
        {
            faces = new FaceHeightmap[6];
        }
        for (int i = 0; i < 6; i++)
        {
            if (faces[i] == null)
            {
                faces[i] = new FaceHeightmap { face = (CubeFace)i, resolution = 16, heights = new List<float>() };
                faces[i].SetSize(16);
            }
            else
            {
                faces[i].face = (CubeFace)i;
                // Ensure spherical coordinates are initialized if not already
                if (faces[i].sphericalCoords == null)
                {
                    faces[i].SetSize(faces[i].resolution);
                }
            }
        }
    }

    public FaceHeightmap GetFace(CubeFace f)
    {
        EnsureFacesInitialized();
        return faces[(int)f];
    }
}
