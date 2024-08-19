using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string Id;
    [HideInInspector] public List<string> ParentRoomNodeIdList = new List<string>();
    [HideInInspector] public List<string> ChildRoomNodeIdList = new List<string>();
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

        // if the room node has a parent or is of type entrance then display a label else display a popup
        if (ParentRoomNodeIdList.Count > 0 || RoomNodeType.IsEntrance)
        {
            // TODO: change validation
            // Display a label that can't be changed
            EditorGUILayout.LabelField(RoomNodeType.RoomNodeTypeName);
        }
        else
        {
            // Display a popup using the RoomNodeType name values that can be selected from (default to the currently set roomNodeType)
            int selected = RoomNodeTypeList.List.FindIndex(x => x == RoomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            RoomNodeType = RoomNodeTypeList.List[selection];

            // If the room type selection has changed making child connections potentially invalid
            if ((RoomNodeTypeList.List[selected].IsCorridor && !RoomNodeTypeList.List[selection].IsCorridor)
                || (!RoomNodeTypeList.List[selected].IsCorridor && RoomNodeTypeList.List[selection].IsCorridor)
                || (!RoomNodeTypeList.List[selected].IsBossRoom && RoomNodeTypeList.List[selection].IsBossRoom))
            {
                // If a room node type has been changed and it already has children then delete the parent child links since we need to revalidate any
                if (ChildRoomNodeIdList.Count > 0)
                {
                    for (int i = ChildRoomNodeIdList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = RoomNodeGraph.GetRoomNode(ChildRoomNodeIdList[i]);
                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.Id);
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(Id);
                        }
                    }
                }
            }
        }


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
        if (IsChildRoomValid(childId))
        {
            ChildRoomNodeIdList.Add(childId);
            return true;
        }
        return false;
    }

    public bool IsChildRoomValid(string childId)
    {
        if (RoomNodeType.IsNone)
            return false;

        // If the child node has a type of none then return false
        if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsNone)
            return false;

        // If the node already has a child with this child ID return false
        if (ChildRoomNodeIdList.Contains(childId))
            return false;

        // If this node ID and the child ID are the same return false
        if (Id == childId)
            return false;

        // If this childID is already in the parentID list return false
        if (ParentRoomNodeIdList.Contains(childId))
            return false;

        // If adding a corridor check that this node has < the maximum permitted child corridors
        if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsCorridor && ChildRoomNodeIdList.Count >= Settings.maxChildCorridors)
            return false;

        // if the child room is an entrance return false - the entrance must always be the top level parent node
        if (RoomNodeGraph.GetRoomNode(childId).RoomNodeType.IsEntrance)
            return false;

        // If adding a room to a corridor check that this corridor node doesn't already have a room added
        if (RoomNodeType.IsCorridor && ChildRoomNodeIdList.Count > 0)
            return false;

        return true;
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentId)
    {
        ParentRoomNodeIdList.Add(parentId);
        return true;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // if the node contains the child ID then remove it
        if (ChildRoomNodeIdList.Contains(childID))
        {
            ChildRoomNodeIdList.Remove(childID);
            return true;
        }
        return false;
    }

    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        if (ParentRoomNodeIdList.Contains(parentID))
        {
            ParentRoomNodeIdList.Remove(parentID);
            return true;
        }
        return false;
    }

#endif
    #endregion Editor Code
}