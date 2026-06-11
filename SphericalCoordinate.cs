using UnityEngine;

[System.Serializable]
public struct SphericalCoordinate
{
    public float radius;
    public float theta; // azimuthal angle (0 to 2π)
    public float phi;   // polar angle (0 to π)

    public SphericalCoordinate(float radius, float theta, float phi)
    {
        this.radius = radius;
        this.theta = theta;
        this.phi = phi;
    }

    // Convenience properties for latitude and longitude
    public float latitude => Mathf.Rad2Deg * (Mathf.PI / 2f - phi); // -90 to +90 degrees
    public float longitude => Mathf.Rad2Deg * theta; // -180 to +180 degrees

    /// <summary>
    /// Convert spherical coordinates to cartesian coordinates
    /// </summary>
    public Vector3 ToCartesian()
    {
        float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = radius * Mathf.Cos(phi);
        float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Create spherical coordinate from cartesian position
    /// </summary>
    public static SphericalCoordinate FromCartesian(Vector3 position)
    {
        float radius = position.magnitude;
        float theta = Mathf.Atan2(position.z, position.x);
        float phi = Mathf.Acos(position.y / radius);
        return new SphericalCoordinate(radius, theta, phi);
    }

    /// <summary>
    /// Create spherical coordinate from a unit direction vector (assumes radius = 1)
    /// </summary>
    public static SphericalCoordinate FromDirection(Vector3 direction)
    {
        Vector3 normalized = direction.normalized;
        float theta = Mathf.Atan2(normalized.z, normalized.x);
        float phi = Mathf.Acos(normalized.y);
        return new SphericalCoordinate(1f, theta, phi);
    }

    public override string ToString()
    {
        return $"Spherical(r={radius:F3}, θ={theta:F3}, φ={phi:F3})";
    }
}
