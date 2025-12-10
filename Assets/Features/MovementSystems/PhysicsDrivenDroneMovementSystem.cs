using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PhysicsDrivenDroneMovementSystem : MovementSystem
{
    [Header("SetUp")]
    [Tooltip("FL,FR,BR,BL")] 
    [SerializeField] private Transform[] _droneMotors;
    [SerializeField] private PhysicsDrivenDroneMovementSystemData _physicsDrivenDroneMovementSystemData;
    
    [HorizontalLine] 
    [Header("Debug")]
    [SerializeField] private bool _debugRollPid;
    [SerializeField] private bool _debugPitchPid;
    [SerializeField] private bool _debugThrottlePid;
    [SerializeField] private bool _debugRotationPid;
    [SerializeField] private bool _debugDistances;
    [SerializeField] private bool _debugForces;
    [HorizontalLine]

    private PIDFloatController _rollController;
    private PIDFloatController _pitchController;
    private PIDFloatController _throttleController;
    private PIDFloatController _heightController;
    private PIDFloatController _yawlController;

    private Vector3 _flMotorLastForce;
    private Vector3 _frMotorLastForce;
    private Vector3 _brMotorLastForce;
    private Vector3 _blMotorLastForce;
    
    private float _areaX;
    private float _areaY;
    private float _areaZ;
    
    private Rigidbody _rigidbody;

    private float _forwardDistDebug;
    private float _rightDistDebug;
    private float _upDistDebug;
    private float _flMotorForceDebug;
    private float _frMotorForceDebug;
    private float _brMotorForceDebug;
    private float _blMotorForceDebug;
    private Vector3 _yawForwarDebug;
    private Vector3 _yawRightDebug;
    private Vector3 _yawUpDebug;


    private void Awake()
    {
        _rollController = new PIDFloatController(_physicsDrivenDroneMovementSystemData.RollValues);
        _pitchController = new PIDFloatController(_physicsDrivenDroneMovementSystemData.PitchValues);
        _throttleController = new PIDFloatController(_physicsDrivenDroneMovementSystemData.ThrottleValues);
        _heightController = new PIDFloatController(_physicsDrivenDroneMovementSystemData.HeightValues);
        _yawlController = new PIDFloatController(_physicsDrivenDroneMovementSystemData.YawlValues);

        _rigidbody = GetComponent<Rigidbody>();

        var collider = GetComponent<BoxCollider>();
        _areaX = collider.size.y * collider.size.z;
        _areaY = collider.size.z * collider.size.z;
        _areaZ = collider.size.z * collider.size.y;
    }
       public override void Move(float deltaTime)
    {
        // Vector from drone to target
        Vector3 toTarget = TargetPosition - transform.position;
        Vector3 toLookTarget = LookTargetPosition - transform.position;
        
        // Extract yaw-only rotation (around world Y axis)
        float yaw = transform.eulerAngles.y;
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        // Build yaw-only forward/right vectors
        Vector3 yawForward = yawRotation * Vector3.forward;
        Vector3 yawRight   = yawRotation * Vector3.right;
        Vector3 yawUp      = Vector3.up;
    
        // Distances projected onto yaw-only axes
        float forwardDist = Vector3.Dot(toTarget, yawForward);
        float rightDist   = Vector3.Dot(toTarget, yawRight);
        float upDist      = Vector3.Dot(toTarget, yawUp);

        // Current orientation
        Vector3 euler      = transform.rotation.eulerAngles;
        float currentRoll  = NormalizeAngle(euler.z);
        float currentPitch = NormalizeAngle(euler.x);
        float currentYaw   = euler.y;

        // Desired orientation
        float desiredRoll  = Mathf.Clamp(-rightDist   * _physicsDrivenDroneMovementSystemData.RollSensitivity,
                                         -_physicsDrivenDroneMovementSystemData.RollClamp,
                                          _physicsDrivenDroneMovementSystemData.RollClamp);

        float desiredPitch = Mathf.Clamp(forwardDist * _physicsDrivenDroneMovementSystemData.PitchSensitivity,
                                         -_physicsDrivenDroneMovementSystemData.PitchClamp,
                                          _physicsDrivenDroneMovementSystemData.PitchClamp);
        Vector3 flatDirection = new Vector3(toLookTarget.x, 0f, toLookTarget.z).normalized;
        float desiredYawl = Vector3.SignedAngle(Vector3.forward, flatDirection, Vector3.up);
        
        
        // PID outputs
        float rollValue     = _rollController.Update(deltaTime, currentRoll, desiredRoll);
        float pitchValue    = _pitchController.Update(deltaTime, currentPitch, desiredPitch);
        float throttle      = _throttleController.Update(deltaTime, -SignedForce(toTarget, -transform.up, 0.75f) , 0);
        float height        = _heightController.Update(deltaTime, transform.position.y , TargetPosition.y);
        float flHeightValue = _heightController.Update(deltaTime, _droneMotors[0].transform.position.y, TargetPosition.y);
        float frHeightValue = _heightController.Update(deltaTime, _droneMotors[1].transform.position.y, TargetPosition.y);
        float brHeightValue = _heightController.Update(deltaTime, _droneMotors[2].transform.position.y, TargetPosition.y);
        float blHeightValue = _heightController.Update(deltaTime, _droneMotors[3].transform.position.y, TargetPosition.y);
        float yawValue      = -_yawlController.UpdateAngle(deltaTime, transform.eulerAngles.y, desiredYawl, _rigidbody.angularVelocity.y);


        // Motor forces
        Vector3 flMotorForce = _droneMotors[0].transform.up *
                               (- pitchValue * _physicsDrivenDroneMovementSystemData.PitchForceMultiplier
                                - rollValue * _physicsDrivenDroneMovementSystemData.RollForceMultiplier
                                + yawValue   * _physicsDrivenDroneMovementSystemData.YawlForceMultiplier
                                + flHeightValue * _physicsDrivenDroneMovementSystemData.HeightForceMultiplier
                                + throttle * _physicsDrivenDroneMovementSystemData.ThrottleForceMultiplier);

        Vector3 frMotorForce = _droneMotors[1].transform.up *
                               ( -pitchValue * _physicsDrivenDroneMovementSystemData.PitchForceMultiplier
                                + rollValue * _physicsDrivenDroneMovementSystemData.RollForceMultiplier
                                - yawValue   * _physicsDrivenDroneMovementSystemData.YawlForceMultiplier
                                + frHeightValue * _physicsDrivenDroneMovementSystemData.HeightForceMultiplier
                                + throttle * _physicsDrivenDroneMovementSystemData.ThrottleForceMultiplier);

        Vector3 brMotorForce = _droneMotors[2].transform.up *
                               (pitchValue * _physicsDrivenDroneMovementSystemData.PitchForceMultiplier
                                + rollValue * _physicsDrivenDroneMovementSystemData.RollForceMultiplier
                                + yawValue   * _physicsDrivenDroneMovementSystemData.YawlForceMultiplier
                                + brHeightValue * _physicsDrivenDroneMovementSystemData.HeightForceMultiplier
                                + throttle * _physicsDrivenDroneMovementSystemData.ThrottleForceMultiplier);

        Vector3 blMotorForce = _droneMotors[3].transform.up *
                               (pitchValue * _physicsDrivenDroneMovementSystemData.PitchForceMultiplier
                                - rollValue * _physicsDrivenDroneMovementSystemData.RollForceMultiplier
                                - yawValue   * _physicsDrivenDroneMovementSystemData.YawlForceMultiplier
                                + blHeightValue * _physicsDrivenDroneMovementSystemData.HeightForceMultiplier
                                + throttle * _physicsDrivenDroneMovementSystemData.ThrottleForceMultiplier);

        /*Vector3 flMotorForce = _droneMotors[0].transform.up *
                               (throttle * _droneMovementSystemData.ThrottleForceMultiplier *
                                (1 + ((Average(-pitchValue, -rollValue) + 1) / 2) * (_droneMovementSystemData.RollPitchYawMaxEffect)));

        Vector3 frMotorForce = _droneMotors[1].transform.up * 
                               (throttle * _droneMovementSystemData.ThrottleForceMultiplier * 
                                (1 + ((Average(-pitchValue, rollValue) + 1) / 2) * (_droneMovementSystemData.RollPitchYawMaxEffect)));
        Vector3 brMotorForce = _droneMotors[2].transform.up * 
                               (throttle * _droneMovementSystemData.ThrottleForceMultiplier * 
                                (1 + ((Average(pitchValue, rollValue) + 1) / 2) * (_droneMovementSystemData.RollPitchYawMaxEffect)));
        Vector3 blMotorForce = _droneMotors[3].transform.up * 
                               (throttle * _droneMovementSystemData.ThrottleForceMultiplier * 
                                (1 + ((Average(pitchValue, -rollValue) + 1) / 2) * _droneMovementSystemData.RollPitchYawMaxEffect));*/

        // Apply forces
        _rigidbody.AddForceAtPosition(ForceMagnitudeClamped(_flMotorLastForce,flMotorForce, _physicsDrivenDroneMovementSystemData.ForceStep),_droneMotors[0].position, ForceMode.Force);
        _rigidbody.AddForceAtPosition(ForceMagnitudeClamped(_frMotorLastForce,frMotorForce, _physicsDrivenDroneMovementSystemData.ForceStep),_droneMotors[1].position, ForceMode.Force);
        _rigidbody.AddForceAtPosition(ForceMagnitudeClamped(_brMotorLastForce,brMotorForce, _physicsDrivenDroneMovementSystemData.ForceStep),_droneMotors[2].position, ForceMode.Force);
        _rigidbody.AddForceAtPosition(ForceMagnitudeClamped(_blMotorLastForce,blMotorForce, _physicsDrivenDroneMovementSystemData.ForceStep),_droneMotors[3].position, ForceMode.Force);
        
        //Air resistance based on the collider
        Vector3 localVel = transform.InverseTransformDirection(_rigidbody.linearVelocity);
        Vector3 localAngVel = transform.InverseTransformDirection(_rigidbody.angularVelocity);

        Vector3 dragLocal = new Vector3(
            localVel.x * Mathf.Abs(localVel.x) * _areaX,
            localVel.y * Mathf.Abs(localVel.y) * _areaY,
            localVel.z * Mathf.Abs(localVel.z) * _areaZ
        );
        
        Vector3 torqueLocal = new Vector3(
            localAngVel.x * Mathf.Abs(localAngVel.x) * _areaX,
            localAngVel.y * Mathf.Abs(localAngVel.y) * _areaY,
            localAngVel.z * Mathf.Abs(localAngVel.z) * _areaZ
        );

        Vector3 dampedVel = transform.TransformDirection(dragLocal);
        Vector3 dampedAngVel = transform.TransformDirection(torqueLocal);

        _rigidbody.AddForce(-dampedVel * _rigidbody.linearDamping);
        _rigidbody.AddTorque(-dampedAngVel * _rigidbody.angularDamping);

        _rigidbody.AddForce(Physics.gravity * _physicsDrivenDroneMovementSystemData.GravityScale,ForceMode.Acceleration);
        
        _flMotorLastForce = flMotorForce;
        _frMotorLastForce = frMotorForce;
        _brMotorLastForce = brMotorForce;
        _blMotorLastForce = blMotorForce;

        // Debug values
        _forwardDistDebug = forwardDist;
        _rightDistDebug   = rightDist;
        _upDistDebug      = upDist;

        _yawForwarDebug = yawForward;
        _yawRightDebug = yawRight;
        _yawUpDebug = yawUp;

        _flMotorForceDebug = SignedForce(_flMotorLastForce);
        _frMotorForceDebug = SignedForce(_frMotorLastForce);
        _brMotorForceDebug = SignedForce(_brMotorLastForce);
        _blMotorForceDebug = SignedForce(_blMotorLastForce);
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private static float SignedForce(Vector3 force, float down = 0)
    {
        return force.magnitude * (Vector3.Dot(force.normalized, Vector3.down) > down ? -1 : 1);
    }
    private static float SignedForce(Vector3 force, Vector3 downVector, float down = 0)
    {
        return force.magnitude * (Vector3.Dot(force.normalized, downVector) > down ? -1 : 1);
    }
    
    public static Vector3 ForceMagnitudeClamped(Vector3 current, Vector3 target, float maxStep)
    {
        float currentMag = current.magnitude;
        float targetMag  = target.magnitude;

        // Clamp magnitude toward target
        float newMag = Mathf.MoveTowards(currentMag, targetMag, maxStep);

        // Preserve direction of current force
        Vector3 direction = target.normalized;

        return direction * newMag;
    }


    public static float Average(params float[] values)
    {
        return values.ToList().Average();
    }


    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
            return;
        #region PIDs

        //PIDs
        //
        //HorizontalTilt
        if (_debugRollPid)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + transform.up * 1,
                transform.position + transform.up * 1 + transform.forward * _rollController.ProportionalDebug);

            Gizmos.color = Color.salmon;
            Gizmos.DrawLine(transform.position + transform.up * 1,
                transform.position + transform.up * 1 + transform.forward * _rollController.IntegralDebug);

            Gizmos.color = Color.seaGreen;
            Gizmos.DrawLine(transform.position + transform.up * 1,
                transform.position + transform.up * 1 + transform.forward * _rollController.DerivativeDebug);

#if UNITY_EDITOR
            Handles.Label(
                transform.position + transform.up * 1 +
                transform.forward * _rollController.ProportionalDebug,
                $"HT P: {_rollController.ProportionalDebug:F2}");
            Handles.Label(
                transform.position + transform.up * 1 +
                transform.forward * _rollController.IntegralDebug,
                $"HT I: {_rollController.IntegralDebug:F2}");
            Handles.Label(
                transform.position + transform.up * 1 +
                transform.forward * _rollController.DerivativeDebug,
                $"HT D: {_rollController.DerivativeDebug:F2}");
#endif
        }

        //VerticalTilt
        if (_debugPitchPid)
        {
            Gizmos.color = Color.lightCyan;
            Gizmos.DrawLine(transform.position + transform.up * 2,
                transform.position + transform.up * 2 + transform.forward * _pitchController.ProportionalDebug);

            Gizmos.color = Color.lightSalmon;
            Gizmos.DrawLine(transform.position + transform.up * 2,
                transform.position + transform.up * 2 + transform.forward * _pitchController.IntegralDebug);

            Gizmos.color = Color.lightSeaGreen;
            Gizmos.DrawLine(transform.position + transform.up * 2,
                transform.position + transform.up * 2 + transform.forward * _pitchController.DerivativeDebug);

#if UNITY_EDITOR
            Handles.Label(
                transform.position + transform.up * 2 +
                transform.forward * _pitchController.ProportionalDebug,
                $"VT P: {_pitchController.ProportionalDebug:F2}");
            Handles.Label(
                transform.position + transform.up * 2 +
                transform.forward * _pitchController.IntegralDebug,
                $"VT I: {_pitchController.IntegralDebug:F2}");
            Handles.Label(
                transform.position + transform.up * 2 +
                transform.forward * _pitchController.DerivativeDebug,
                $"VT D: {_pitchController.DerivativeDebug:F2}");
#endif
        }

        //Throttle
        if (_debugThrottlePid)
        {
            Gizmos.color = Color.darkCyan;
            Gizmos.DrawLine(transform.position + transform.up * 3,
                transform.position + transform.up * 2.9f + transform.forward * _throttleController.ProportionalDebug);

            Gizmos.color = Color.darkSalmon;
            Gizmos.DrawLine(transform.position + transform.up * 3,
                transform.position + transform.up * 3 + transform.forward * _throttleController.IntegralDebug);

            Gizmos.color = Color.darkSeaGreen;
            Gizmos.DrawLine(transform.position + transform.up * 3,
                transform.position + transform.up * 3.1f + transform.forward * _throttleController.DerivativeDebug);

#if UNITY_EDITOR
            Handles.Label(
                transform.position + transform.up * 2.9f +
                transform.forward * _throttleController.ProportionalDebug,
                $"T P: {_throttleController.ProportionalDebug:F2}");
            Handles.Label(
                transform.position + transform.up * 3 +
                transform.forward * _throttleController.IntegralDebug, $"T I: {_throttleController.IntegralDebug:F2}");
            Handles.Label(
                transform.position + transform.up * 3.1f +
                transform.forward * _throttleController.DerivativeDebug,
                $"T D: {_throttleController.DerivativeDebug:F2}");
#endif
        }

        //Rotation
        if (_debugRotationPid)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + transform.up,
                transform.position + transform.up * _yawlController.ProportionalDebug);

            Gizmos.color = Color.salmon;
            Gizmos.DrawLine(transform.position + transform.up,
                transform.position + transform.up * _yawlController.IntegralDebug);

            Gizmos.color = Color.seaGreen;
            Gizmos.DrawLine(transform.position + transform.up,
                transform.position + transform.up * _yawlController.DerivativeDebug);

#if UNITY_EDITOR
            Handles.Label(transform.position + transform.up + transform.up * _yawlController.ProportionalDebug,
                $"R P: {_yawlController.ProportionalDebug:F2}");
            Handles.Label(transform.position + transform.up + transform.up * _yawlController.IntegralDebug,
                $"R I: {_yawlController.IntegralDebug:F2}");
            Handles.Label(transform.position + transform.up + transform.up * _yawlController.DerivativeDebug,
                $"R D: {_yawlController.DerivativeDebug:F2}");
#endif
        }

        #endregion

        // Forces
        //
        if (_debugForces)
        {
            // Draw base line to target
            Gizmos.color = Color.white;
            Gizmos.DrawLine(_droneMotors[0].transform.position,
                _droneMotors[0].transform.position + _droneMotors[0].transform.up *  _flMotorForceDebug / 100);
            Gizmos.DrawLine(_droneMotors[1].transform.position,
                _droneMotors[1].transform.position + _droneMotors[1].transform.up *  _frMotorForceDebug / 100);
            Gizmos.DrawLine(_droneMotors[2].transform.position,
                _droneMotors[2].transform.position + _droneMotors[2].transform.up * _brMotorForceDebug / 100);
            Gizmos.DrawLine(_droneMotors[3].transform.position,
                _droneMotors[3].transform.position + _droneMotors[3].transform.up * _blMotorForceDebug / 100);


            // Optional: label distances
#if UNITY_EDITOR
            Handles.Label(_droneMotors[0].transform.position + _droneMotors[0].transform.up * _flMotorForceDebug / 100,
                $"Force: {_flMotorForceDebug:F2}");
            Handles.Label(_droneMotors[1].transform.position + _droneMotors[1].transform.up * _frMotorForceDebug / 100,
                $"Force: {_frMotorForceDebug:F2}");
            Handles.Label(_droneMotors[2].transform.position + _droneMotors[2].transform.up * _brMotorForceDebug / 100,
                $"Force: {_brMotorForceDebug:F2}");
            Handles.Label(_droneMotors[3].transform.position + _droneMotors[3].transform.up * _blMotorForceDebug / 100,
                $"Force: {_blMotorForceDebug:F2}");
#endif
        }

        // Distances
        //
        if (_debugDistances)
        {
            // Draw base line to target
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, TargetPosition);

            // Draw axis-aligned projections
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _yawForwarDebug* _forwardDistDebug);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _yawRightDebug * _rightDistDebug);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _yawUpDebug * _upDistDebug);

            // Optional: label distances
#if UNITY_EDITOR
            Handles.Label(transform.position + _yawForwarDebug * _forwardDistDebug,
                $"Forward: {_forwardDistDebug:F2}");
            Handles.Label(transform.position + _yawRightDebug * _rightDistDebug, $"Right: {_rightDistDebug:F2}");
            Handles.Label(transform.position + _yawUpDebug * _upDistDebug, $"Up: {_upDistDebug:F2}");
#endif
        }

    }
}