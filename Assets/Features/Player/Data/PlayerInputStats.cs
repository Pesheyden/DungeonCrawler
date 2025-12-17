using BSOAP.Variables;
using UnityEngine;

[CreateAssetMenu(fileName = "BS_PlayerInputStats", menuName = "Scriptable Objects/Player/PlayerInputStats")]
public class PlayerInputStats : ScriptableObject
{
    [SubAsset] public FloatVariable BF_PitchInput;
    [SubAsset] public FloatVariable BF_RollInput;
    [SubAsset] public FloatVariable BF_HeightInput;
    [SubAsset] public FloatVariable BF_YawlInput;
    [SubAsset] public EventSo B_OnInventoryOpenInput;
}
