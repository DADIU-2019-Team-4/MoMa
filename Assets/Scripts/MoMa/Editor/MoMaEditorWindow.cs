using UnityEngine;
using UnityEditor;

public class MoMaEditorWindow : EditorWindow
{
    string myString = "Hello World!";

    [MenuItem("MoMa Settings/Show Window")]
    public static void ShowWindow ()
    {
        // Get existing open window or spawn a new one
        EditorWindow.GetWindow<MoMaEditorWindow>("MoMa Settings");
    }

    private void OnGUI ()
    {
        GUILayout.Label("This is a label.", EditorStyles.boldLabel);

        myString = EditorGUILayout.TextField("Name", myString);

        if (GUILayout.Button("Press me"))
        {
            Debug.Log(myString);
        }
    }
}
