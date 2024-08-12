using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle m_RoomNodeStyle;
    private RoomNodeTypeListSO m_RoomNodeTypeList;
    private RoomNodeSO m_CurrentRoomNode = null;
    private float m_ConnectingLineWidth = k_StandardConnectingLineWidth;

    private static RoomNodeGraphSO m_CurrentRoomNodeGraph;

    private const float k_NodeWidth = 160f;
    private const float k_NodeHeight = 75f;
    private const float k_StandardConnectingLineWidth = 3f;
    private const float k_SelectingConnectingLineWidth = 5f;
    private const float k_ConnectingLineArrowSize = 5f;
    private const int k_NodePadding = 25;
    private const int k_NodeBorder = 12;



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
            DrawDraggedLine();

            ProcessEvents(Event.current);

            DrawRoomConnections();

            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawRoomConnections()
    {
        // Loop through all room nodes
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            if (roomNode.ChildRoomNodeIdList.Count > 0)
            {
                // Loop through child room nodes
                foreach (string childRoomNodeID in roomNode.ChildRoomNodeIdList)
                {
                    // get child room node from dictionary
                    if (m_CurrentRoomNodeGraph.RoomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, m_CurrentRoomNodeGraph.RoomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draw connection line between the parent room node and child room node
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.Rect.center;
        Vector2 endPosition = childRoomNode.Rect.center;

        // calculate midway point
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        Vector2 direction = endPosition - startPosition;

        // Calulate normalised perpendicular positions from the mid point
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * k_ConnectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * k_ConnectingLineArrowSize;

        // Calculate mid point offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * k_ConnectingLineArrowSize;

        // Draw Arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, k_StandardConnectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, k_StandardConnectingLineWidth);

        // Draw line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, k_StandardConnectingLineWidth);

        GUI.changed = true;
    }


    private void DrawDraggedLine()
    {
        if (m_CurrentRoomNodeGraph.LinePosition != Vector2.zero)
        {
            //Draw line from node to line position
            Handles.DrawBezier(m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom.Rect.center,
                m_CurrentRoomNodeGraph.LinePosition,
                m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom.Rect.center,
                m_CurrentRoomNodeGraph.LinePosition,
                Color.white,
                null,
                m_ConnectingLineWidth);
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

    /// <summary>
    /// Process events on the room node graphs and nodes, like mouse down events and other
    /// </summary>
    private void ProcessEvents(Event currentEvent)
    {
        if (m_CurrentRoomNode == null || !m_CurrentRoomNode.IsLeftClickDragging)
        {
            m_CurrentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        Debug.Log(m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom);

        // if mouse isn't over a room node or we are currently dragging a line from the room node
        if (m_CurrentRoomNode == null || m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            m_CurrentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    ///  Check to see to mouse is over a room node - if so then return the room node else return null
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = m_CurrentRoomNodeGraph.RoomNodeList.Count - 1; i >= 0; i--)
        {
            if (m_CurrentRoomNodeGraph.RoomNodeList[i].Rect.Contains(currentEvent.mousePosition))
            {
                return m_CurrentRoomNodeGraph.RoomNodeList[i];
            }
        }

        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // if releasing the right mouse button and currently dragging a line
        if (currentEvent.button == 1 && m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                if (m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.Id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom.Id);
                }
            }

            ClearLineDrag();
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);

            if (IsMouseOverRoomNode(currentEvent) != null)
            {
                m_ConnectingLineWidth = k_SelectingConnectingLineWidth;
            }
            else
            {
                m_ConnectingLineWidth = k_StandardConnectingLineWidth;
            }

            GUI.changed = true;
        }
    }

    public void DragConnectingLine(Vector2 delta)
    {
        m_CurrentRoomNodeGraph.LinePosition += delta;
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

        m_CurrentRoomNodeGraph.OnValidate();
    }

    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            roomNode.Draw(m_RoomNodeStyle);
        }

        GUI.changed = true;
    }

    private void ClearLineDrag()
    {
        m_CurrentRoomNodeGraph.RoomNodeToDrawLineFrom = null;
        m_CurrentRoomNodeGraph.LinePosition = Vector2.zero;

        GUI.changed = true;
    }
}


