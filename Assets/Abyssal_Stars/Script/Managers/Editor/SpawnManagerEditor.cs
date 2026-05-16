using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnManager))]
public class SpawnManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Dibujamos el cartelito de ayuda arriba del todo
        EditorGUILayout.HelpBox(
            "GUÍA DE CANALES DE AUDIO (4 Elementos Fijos)\n\n" +
            "• Índice 0 [Low]: Frecuencias Bajas (Ej: Bombos).\n" +
            "• Índice 1 [Mid]: Frecuencias Medias (Ej: Voces/Sintetizador).\n" +
            "• Índice 2 [High]: Frecuencias Altas (Ej: Platillos/Hi-Hats).\n" +
            "• Índice 3 [SubLow]: Bajos muy profundos.\n\n" +
            "Si dejas el campo 'Prefab' o 'Zone' vacío, simplemente no aparecerán enemigos para ese ritmo.",
            MessageType.Info);

        EditorGUILayout.Space(5);

        DrawDefaultInspector();

        SpawnManager spawner = (SpawnManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("── Herramientas de Zona ──", EditorStyles.boldLabel);

        if (GUILayout.Button("Crear Zonas de Spawn faltantes", GUILayout.Height(32)))
        {
            CreateMissingZones(spawner);
        }
    }

    private void CreateMissingZones(SpawnManager spawner)
    {
        SerializedObject so = new SerializedObject(spawner);

        // Buscamos los 4 canales fijos por su nombre de variable
        string[] channelProperties = { "_channelLow", "_channelMid", "_channelHigh", "_channelSubLow" };

        Color[] defaultColors = {
            new Color(0.2f, 0.6f, 1f,  0.4f),   // Low    -> azul
            new Color(0.2f, 1f,  0.4f, 0.4f),   // Mid    -> verde
            new Color(1f,  0.9f, 0.1f, 0.4f),   // High   -> amarillo
            new Color(1f,  0.5f, 0.1f, 0.4f),   // SubLow -> naranja
        };

        Transform zonesParent = spawner.transform.Find("SpawnZones");
        if (zonesParent == null)
        {
            GameObject zonesGO = new GameObject("SpawnZones");
            zonesGO.transform.SetParent(spawner.transform, worldPositionStays: false);
            zonesParent = zonesGO.transform;
            Undo.RegisterCreatedObjectUndo(zonesGO, "Crear SpawnZones");
        }

        int created = 0;

        for (int i = 0; i < channelProperties.Length; i++)
        {
            SerializedProperty channel = so.FindProperty(channelProperties[i]);
            if (channel == null) continue;

            SerializedProperty zoneProp = channel.FindPropertyRelative("zone");
            SerializedProperty nameProp = channel.FindPropertyRelative("name");

            if (zoneProp.objectReferenceValue != null) continue;

            string channelName = nameProp.stringValue;
            if (string.IsNullOrEmpty(channelName)) channelName = $"Channel_{i}";

            GameObject zoneGO = new GameObject($"Zone_{channelName}");
            zoneGO.transform.SetParent(zonesParent, worldPositionStays: false);
            zoneGO.transform.localPosition = new Vector3(0f, i * 2f, 0f);
            Undo.RegisterCreatedObjectUndo(zoneGO, $"Crear Zone_{channelName}");

            SpawnZone spawnZone = zoneGO.AddComponent<SpawnZone>();

            SerializedObject zoneSO = new SerializedObject(spawnZone);
            SerializedProperty colorProp = zoneSO.FindProperty("_gizmoColor");
            if (colorProp != null && i < defaultColors.Length)
            {
                colorProp.colorValue = defaultColors[i];
                zoneSO.ApplyModifiedProperties();
            }

            zoneProp.objectReferenceValue = spawnZone;
            created++;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);

        if (created > 0)
            Debug.Log($"[SpawnManagerEditor] {created} zona(s) creada(s) bajo 'SpawnZones'.");
        else
            Debug.Log("[SpawnManagerEditor] Todos los canales ya tienen zona asignada.");
    }
}