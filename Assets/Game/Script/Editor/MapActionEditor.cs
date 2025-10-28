#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapAction))]
public class MapActionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var mapAction = (MapAction)target;

        if (GUILayout.Button("Сканировать POI"))
        {
            mapAction.ScanPOIs();
        }
    }

    private void OnSceneGUI()
    {
        var mapAction = (MapAction)target;

        Handles.color = Color.yellow;
        Handles.DrawWireDisc(mapAction.transform.position, Vector3.up, mapAction.ScanRadius);
        
        Handles.color = Color.red;
        Handles.DrawWireDisc(mapAction.transform.position, Vector3.up, mapAction.ActivateRadius);
    }
}
#endif