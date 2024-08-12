using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO RoomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> RoomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> RoomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        RoomNodeDictionary.Clear();

        foreach (RoomNodeSO node in RoomNodeList)
        {
            RoomNodeDictionary[node.Id] = node;
        }
    }

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public RoomNodeSO RoomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 LinePosition;

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        RoomNodeToDrawLineFrom = node;
        LinePosition = position;
    }

#endif

    #endregion
}

