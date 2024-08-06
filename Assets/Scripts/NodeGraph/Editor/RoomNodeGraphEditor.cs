using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class RoomNodeGraphEditor : EditorWindow
{
    GUIStyle m_RoomNodeStyle;
    RoomNodeTypeListSO m_RoomNodeTypeList;

    static RoomNodeGraphSO m_CurrentRoomNodeGraph;

    const float k_NodeWidth = 160f;
    const float k_NodeHeight = 75f;
    const int k_NodePadding = 25;
    const int k_NodeBorder = 12;


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    void OnEnable()
    {
        // Define node layout style
        m_RoomNodeStyle = new GUIStyle();
        m_RoomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        m_RoomNodeStyle.normal.textColor = Color.white;
        m_RoomNodeStyle.padding = new RectOffset(k_NodePadding, k_NodePadding, k_NodePadding, k_NodePadding);
        m_RoomNodeStyle.border = new RectOffset(k_NodeBorder, k_NodeBorder, k_NodeBorder, k_NodeBorder);

        // Load room node types
        m_RoomNodeTypeList = GameResources.Instance.RoomNodeTypeList;
    }

    void OnGUI()
    {
        // A RoomNodeGraphSO is selected
        if (m_CurrentRoomNodeGraph != null)
        {
            ProcessEvents(Event.current);

            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clicked in the inspector
    /// </summary>
    [OnOpenAssetAttribute(0)] // UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility
            .InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();

            m_CurrentRoomNodeGraph = roomNodeGraph;

            return true;
        }

        return false;
    }


    private void ProcessEvents(Event currentEvent)
    {
        ProcessRoomNodeGraphEvents(currentEvent);
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Process mouse down events on the room node graph (not over a node)
    /// </summary>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Process right click mouse down on graph event (show context menu)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object userData)
    {

        CreateRoomNode((Vector2)userData, m_RoomNodeTypeList.List.Find(x => x.IsNone));
    }

    // <summary>
    // Create a room node at mouse position
    // </summary>
    private void CreateRoomNode(Vector2 mousePosition, RoomNodeTypeSO roomNodeType)
    {
        // Create room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // Add room node to current room node graph room node list
        m_CurrentRoomNodeGraph.RoomNodeList.Add(roomNode);

        // Set room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(k_NodeWidth, k_NodeHeight)), m_CurrentRoomNodeGraph, roomNodeType);

        // Add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, m_CurrentRoomNodeGraph);

        AssetDatabase.SaveAssets();
    }

    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            roomNode.Draw(m_RoomNodeStyle);
        }

        GUI.changed = true;
    }
}


