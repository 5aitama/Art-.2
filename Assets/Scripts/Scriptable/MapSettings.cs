using UnityEngine;

using Unity.Mathematics;

[CreateAssetMenu(fileName = "New map setting", menuName = "Game settings/Map settings")]
public class MapSettings : ScriptableObject
{
    /// <summary>
    /// Size of each map
    /// </summary>
    public int mapSize = 32;

    /// <summary>
    /// Frequency of noise.
    /// </summary>
    public float noiseFrequency = 0.1f;
    
    /// <summary>
    /// Amplitude of noise.
    /// </summary>
    public float noiseAmplitude = 8f;

    /// <summary>
    /// Position of noise.
    /// </summary>
    public float2 noisePosition = 0f;

    /// <summary>
    /// Map area :)
    /// </summary>
    public int MapArea => mapSize * mapSize;
}