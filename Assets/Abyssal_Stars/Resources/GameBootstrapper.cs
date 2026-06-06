using UnityEngine;

public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeManagers()
    {
        if (Object.FindAnyObjectByType<DataManager>() == null)
        {
            GameObject managerPrefab = Resources.Load<GameObject>("GameManagers");

            if (managerPrefab != null)
            {
                GameObject managerInstance = Object.Instantiate(managerPrefab);
                managerInstance.name = "[GameManagers_Runtime]";
                Object.DontDestroyOnLoad(managerInstance);

                Debug.Log("🟢 Bootstrapper: Managers globales inyectados con éxito.");
            }
            else
            {
                Debug.LogError("🔴 Bootstrapper: No se encontró el prefab 'GameManagers' en la carpeta Resources.");
            }
        }
    }
}