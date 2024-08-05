using System.Collections.Generic;
using UnityEngine;

public class RoomNodeSO : ScriptableObject {
    [HideInInspector] public string Id;
    [HideInInspector] public List<string> ParentRoomNodeList = new List<string>();
    [HideInInspector] public List<string> ChildRoomNodeList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO RoomNodeGraph;
    public RoomNodeTypeSO RoomNodeType;
    [HideInInspector] public RoomNodeTypeListSO RoomNodeTypeList;
}