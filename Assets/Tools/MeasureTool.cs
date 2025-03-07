using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(Transform))]
public class MeasureToolEditor : Editor
{
    private static bool IsOn => Menu.GetChecked(MenuPath);

    private const string MenuPath = "Tools/Measure Tool";

    [MenuItem(MenuPath)]
    static void LaunchMeasureTool()
    {
        Menu.SetChecked(MenuPath, !Menu.GetChecked(MenuPath));
    }

    private void OnSceneGUI()
    {
        if (!IsOn)
        {
            return;
        }

        Handles.BeginGUI();
        GUI.color = new Color(1, 1, 1, 0.5f);
        GUI.Box(new Rect(30, 10, 200, 300), "Measure Tool");
        Handles.EndGUI();
    }
}
#endif
