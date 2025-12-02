using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoveStats", menuName = "Scriptable Objects/Player/PlayerMoveStats")]
public class PlayerMoveStats : ScriptableObject
{
    public float MoveTargetDistanceMultiplier;
    public float LookTargetDistanceMultiplier;
}
