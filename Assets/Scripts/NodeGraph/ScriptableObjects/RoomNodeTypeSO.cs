using UnityEngine;


[CreateAssetMenu(fileName = "RoomNodeType", menuName = "Scriptable Objects/Dungeon/Room Node Type")]

public class RoomNodeTypeSO : ScriptableObject
{
    public string RoomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool DisplayInNodeGraphEditor = true;

    #region Header
    [Header("Should be a corridor")]
    #endregion Header
    public bool IsCorridor;

    #region Header
    [Header("Should be a corridor north-south")]
    #endregion Header
    public bool IsCorridorNS;

    #region Header
    [Header("Should be a corridor east-west")]
    #endregion Header
    public bool IsCorridorEW;

    #region Header
    [Header("Should be an entrance")]
    #endregion Header
    public bool IsEntrance;

    #region Header
    [Header("Should be a boss room")]
    #endregion Header
    public bool IsBossRoom;

    #region Header
    [Header("Should be none (unassigned)")]
    #endregion Header
    public bool IsNone;

    #region Validation

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(RoomNodeTypeName), RoomNodeTypeName);
    }
#endif

    #endregion
}
