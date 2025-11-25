using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "DroneMovementSystemData", menuName = "Scriptable Objects/DroneMovementSystemData")]
public class DroneMovementSystemData : ScriptableObject
{
    public float ForceStep;
    public float RollPitchYawMaxEffect = 5;
    public float GravityScale = 5;
    [Header("PID Controllers")] 
    [Label("RollTilt")] 
    [SerializeField]
    public PIDFloatControllerValues RollValues;
    public float RollForceMultiplier;
    public float RollSensitivity;
    public float RollClamp;

    [Label("PitchTilt")]
    [SerializeField]
    public PIDFloatControllerValues PitchValues;
    public float PitchForceMultiplier;
    public float PitchSensitivity;
    public float PitchClamp;
    
    [Label("Throttle")]
    [SerializeField]
    public PIDFloatControllerValues ThrottleValues;
    public float ThrottleForceMultiplier;
    
    [Label("Height")]
    [SerializeField]
    public PIDFloatControllerValues HeightValues;
    public float HeightForceMultiplier;
    
    [Label("Yawl")]
    [SerializeField]
    public PIDFloatControllerValues YawlValues;
    public float YawlForceMultiplier;
}
