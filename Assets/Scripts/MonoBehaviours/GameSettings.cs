using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public MapSettings mapSettings;
    public static MapSettings MapSettingsInstance;

    public EndlessSettings endlessSettings;
    public static EndlessSettings EndlessSettingsInstance;

    public Material mapMaterial;
    public static Material MapMaterialInstance;

    private void Awake()
    {
        MapSettingsInstance     = mapSettings ?? new MapSettings();
        EndlessSettingsInstance = endlessSettings ?? new EndlessSettings();
        MapMaterialInstance     = mapMaterial ?? Resources.Load<Material>("Default Map Material");
    }
}
