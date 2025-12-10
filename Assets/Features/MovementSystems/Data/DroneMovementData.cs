using UnityEngine;

[CreateAssetMenu(fileName = "DroneMovementSystemData", menuName = "Scriptable Objects/DroneMovementSystemData")]
public class DroneMovementSystemData : ScriptableObject
{
    [Header("Control Limits")]
    [Tooltip("Maximum pitch angle in radians (~10 degrees).")]
    public float MaxPitch = 0.175f;

    [Tooltip("Maximum roll angle in radians (~10 degrees).")]
    public float MaxRoll = 0.175f;

    [Tooltip("Maximum angular acceleration (rad/s²).")]
    public float MaxAngularAccel = 10f;


    [Header("Time Constants (Controller Gains)")]
    [Tooltip("Gravity constant used for thrust calculations.")]
    public float Gravity = 9.81f;

    [Tooltip("Time constant for vertical velocity correction.")]
    public float TcVerticalVel = 1.0f;

    [Tooltip("Time constant for horizontal acceleration correction.")]
    public float TcAcceleration = 0.5f;

    [Tooltip("Time constant for pitch/roll angular velocity correction.")]
    public float TcOmegaXY = 0.1f;

    [Tooltip("Time constant for pitch/roll angular acceleration correction.")]
    public float TcAlphaXY = 0.05f;

    [Tooltip("Time constant for yaw angular acceleration correction.")]
    public float TcAlphaZ = 0.05f;


    [Header("Propeller Visuals")]
    [Tooltip("Multiplier for propeller spin speed.")]
    public float PropSpeedScale = 500f;

}