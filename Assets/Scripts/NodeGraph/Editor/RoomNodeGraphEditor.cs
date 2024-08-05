using UnityEngine;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow {
    GUIStyle roomNodeStyle;
    const float k_NodeWidth = 160f;
    const float k_NodeHeight = 75f;
    const int k_NodePadding = 25;
    const int k_NodeBorder = 12;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    static void OpenWindow() {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    void OnEnable() {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(k_NodePadding, k_NodePadding, k_NodePadding, k_NodePadding);
        roomNodeStyle.border = new RectOffset(k_NodeBorder, k_NodeBorder, k_NodeBorder, k_NodeBorder);
    }

    void OnGUI() {
        AddNode(100, 100, "Node 1");
        AddNode(300, 300, "Node 2");
        AddNode(500, 500, "Node 3");
    }

    void AddNode(int x, int y, string label) {
        GUILayout.BeginArea(
            new Rect(x, y, k_NodeWidth, k_NodeHeight),
            roomNodeStyle);

        EditorGUILayout.LabelField(label);

        GUILayout.EndArea();
    }
}
