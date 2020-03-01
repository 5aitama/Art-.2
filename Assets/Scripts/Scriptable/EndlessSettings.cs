using UnityEngine;

[CreateAssetMenu(fileName = "New endless settings", menuName = "Game settings/Endless settings", order = 0)]
public class EndlessSettings : ScriptableObject
{
    public int chunkPerAxis = 8;
}
