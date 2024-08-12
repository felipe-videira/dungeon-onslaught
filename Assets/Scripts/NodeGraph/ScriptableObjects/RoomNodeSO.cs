using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    //TODO: add back the hide in inspector
    public string Id;
    public List<string> ParentRoomNodeIdList = new List<string>();
    public List<string> ChildRoomNodeIdList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO RoomNodeGraph;
    public RoomNodeTypeSO RoomNodeType;
    [HideInInspector] public RoomNodeTypeListSO RoomNodeTypeList;

    #region Editor Code

#if UNITY_EDITOR

    [HideInInspector] public Rect Rect;
    [HideInInspector] public bool IsLeftClickDragging = false;
    [HideInInspector] public bool IsSelected = false;

    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        Rect = rect;
        Id = Guid.NewGuid().ToString();
        name = "RoomNode";
        RoomNodeGraph = nodeGraph;
        RoomNodeType = roomNodeType;

        // Load room node type list
        RoomNodeTypeList = GameResources.Instance.RoomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        //  Draw Node Box Using Begin Area
        GUILayout.BeginArea(Rect, nodeStyle);

        // Start Region To Detect Popup Selection Changes
        EditorGUI.BeginChangeCheck();

        // Display a popup using the RoomNodeType name values that can be selected from (default to the currently set roomNodeType)
        int selected = RoomNodeTypeList.List.FindIndex(x => x == RoomNodeType);

        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

        RoomNodeType = RoomNodeTypeList.List[selection];

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    /// <summary>
    /// Populate a string array with the room node types to display that can be selected
    /// </summary>
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[RoomNodeTypeList.List.Count];

        for (int i = 0; i < RoomNodeTypeList.List.Count; i++)
        {
            if (RoomNodeTypeList.List[i].DisplayInNodeGraphEditor)
            {
                roomArray[i] = RoomNodeTypeList.List[i].RoomNodeTypeName;
            }
        }

        return roomArray;
    }

    /// <summary>
    /// Process mouse events in the editor for the node
    /// </summary>
    public void ProcessEvents(Event currentEvent)
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

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Left click down
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        // Right click down
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        RoomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        // Toggle node selection
        IsSelected = !IsSelected;
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // Left click up
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if (IsLeftClickDragging)
        {
            IsLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // Left click drag event
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        IsLeftClickDragging = true;

        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        Rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childId)
    {
        ChildRoomNodeIdList.Add(childId);
        return true;
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentId)
    {
        ParentRoomNodeIdList.Add(parentId);
        return true;
    }

#endif
    #endregion Editor Code
}