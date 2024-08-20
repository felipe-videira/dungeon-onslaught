using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle m_RoomNodeStyle;
    private GUIStyle m_RoomNodeSelectedStyle;
    private RoomNodeTypeListSO m_RoomNodeTypeList;
    private RoomNodeSO m_CurrentRoomNode = null;
    private float m_ConnectingLineWidth = k_StandardConnectingLineWidth;
    private Vector2 m_GraphOffset;
    private Vector2 m_GraphDrag;

    private static RoomNodeGraphSO m_CurrentRoomNodeGraph;

    private const float k_GridLarge = 100f;
    private const float k_GridSmall = 25f;
    private const float k_NodeWidth = 160f;
    private const float k_NodeHeight = 75f;
    private const float k_StandardConnectingLineWidth = 3f;
    private const float k_SelectingConnectingLineWidth = 5f;
    private const float k_ConnectingLineArrowSize = 5f;
    private const int k_NodePadding = 25;
    private const int k_NodeBorder = 12;



    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;

        // Define node layout style
        m_RoomNodeStyle = new GUIStyle();
        m_RoomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        m_RoomNodeStyle.normal.textColor = Color.white;
        m_RoomNodeStyle.padding = new RectOffset(k_NodePadding, k_NodePadding, k_NodePadding, k_NodePadding);
        m_RoomNodeStyle.border = new RectOffset(k_NodeBorder, k_NodeBorder, k_NodeBorder, k_NodeBorder);

        // Define selected node style
        m_RoomNodeSelectedStyle = new GUIStyle();
        m_RoomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        m_RoomNodeSelectedStyle.normal.textColor = Color.white;
        m_RoomNodeSelectedStyle.padding = new RectOffset(k_NodePadding, k_NodePadding, k_NodePadding, k_NodePadding);
        m_RoomNodeSelectedStyle.border = new RectOffset(k_NodeBorder, k_NodeBorder, k_NodeBorder, k_NodeBorder);

        // Load room node types
        m_RoomNodeTypeList = GameResources.Instance.RoomNodeTypeList;
    }

    private void OnDisable()
    {
        // Unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void OnGUI()
    {
        // A RoomNodeGraphSO is selected
        if (m_CurrentRoomNodeGraph != null)
        {
            DrawBackgroundGrid(k_GridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(k_GridLarge, 0.3f, Color.gray);

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

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        m_GraphOffset += m_GraphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(m_GraphOffset.x % gridSize, m_GraphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;
    }

    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            m_CurrentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
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
        m_GraphDrag = Vector2.zero;

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
        // process left click
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
        // process right click
        else if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        m_GraphDrag = dragDelta;

        for (int i = 0; i < m_CurrentRoomNodeGraph.RoomNodeList.Count; i++)
        {
            m_CurrentRoomNodeGraph.RoomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
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
        // Process left click mouse down 
        if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
        // Process right click mouse down on graph event (show context menu)
        else if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            if (roomNode.IsSelected)
            {
                roomNode.IsSelected = false;
                GUI.changed = true;
            }
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object userData)
    {
        // If graph is empty then add entrace room node first
        if (m_CurrentRoomNodeGraph.RoomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), m_RoomNodeTypeList.List.Find(x => x.IsEntrance));
        }


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

    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            if (roomNode.IsSelected && !roomNode.RoomNodeType.IsEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                foreach (string childRoomNodeID in roomNode.ChildRoomNodeIdList)
                {
                    RoomNodeSO childRoomNode = m_CurrentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.Id);
                    }
                }

                foreach (string parentRoomNodeID in roomNode.ParentRoomNodeIdList)
                {
                    RoomNodeSO parentRoomNode = m_CurrentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.Id);
                    }
                }
            }
        }

        // Delete queued room nodes
        while (roomNodeDeletionQueue.Count > 0)
        {
            // Get room node from queue
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            m_CurrentRoomNodeGraph.RoomNodeDictionary.Remove(roomNodeToDelete.Id);
            m_CurrentRoomNodeGraph.RoomNodeList.Remove(roomNodeToDelete);

            // Remove node from Asset database
            DestroyImmediate(roomNodeToDelete, true);
            // Save asset database
            AssetDatabase.SaveAssets();

        }
    }

    private void DeleteSelectedRoomNodeLinks()
    {
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            if (roomNode.IsSelected && roomNode.ChildRoomNodeIdList.Count > 0)
            {
                for (int i = roomNode.ChildRoomNodeIdList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = m_CurrentRoomNodeGraph.GetRoomNode(roomNode.ChildRoomNodeIdList[i]);

                    if (childRoomNode != null && childRoomNode.IsSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.Id);
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.Id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }


    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            roomNode.IsSelected = true;
        }

        GUI.changed = true;
    }

    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in m_CurrentRoomNodeGraph.RoomNodeList)
        {
            roomNode.Draw(roomNode.IsSelected ? m_RoomNodeSelectedStyle : m_RoomNodeStyle);
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


