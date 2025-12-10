using UnityEngine;

/// <summary>
/// Physics-based velocity controller for a quadcopter.
/// Uses cascaded control loops:
/// Position → Velocity → Acceleration → Tilt → Angular Velocity → Angular Acceleration → Torque.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DroneMovementSystem : MovementSystem
{
    [Header("Propeller Visuals")]
    [SerializeField] private GameObject _propFl;
    [SerializeField] private GameObject _propFr;
    [SerializeField] private GameObject _propRr;
    [SerializeField] private GameObject _propRl;

    [Header("Control Targets")]
    [SerializeField] private float _desiredHeight = 4f;      // Target altitude
    [SerializeField] private float _desiredForwardVel = 0f;  // Target forward velocity (Unity Z)
    [SerializeField] private Vector2 _desiredForwardVelClamp;
    [SerializeField] private float _desiredRightVel = 0f;    // Target right velocity (Unity X)
    [SerializeField] private Vector2 _desiredRightVelClamp;
    [SerializeField] private float _desiredYawRate = 0f;     // Target yaw rate
    [SerializeField] private Vector2 _desireYawRateClamp;
    [SerializeField] private float _initialHeight = 4f;

    [Header("Drone Data (Control Limits + Time Constants + Visuals)")]
    [SerializeField] private DroneMovementSystemData _data;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // Initial upward force to counter gravity (same as original VelocityControl)
        Vector3 desiredForce = new Vector3(0.0f, _data.Gravity * _rb.mass, 0.0f);
        //_rb.AddForce(desiredForce, ForceMode.Acceleration);
    }

    public override void Move(float deltaTime)
    {
        ComputeDesireVelocities();

        // -----------------------------
        // 1. READ STATE (LOCAL SPACE)
        // -----------------------------
        Vector3 velocity = transform.InverseTransformDirection(_rb.linearVelocity);      // local velocity
        Vector3 angularVelocity = transform.InverseTransformDirection(_rb.angularVelocity); // local angular velocity
        Vector3 inertia = _rb.inertiaTensor;
        float altitude = transform.position.y;

        // -----------------------------
        // 2. TILT (PITCH, YAW, ROLL)
        //    Small-angle approximation from world down, just like StateFinder
        // -----------------------------
        // Forward vector
        Vector3 fwd = transform.forward;

        // Project forward onto XZ plane (removes pitch)
        Vector3 fwdProjected = Vector3.ProjectOnPlane(fwd, Vector3.up).normalized;

        // Pitch = angle between forward and its projection, around local X axis
        float pitch = Vector3.SignedAngle(fwdProjected, fwd, transform.right) * Mathf.Deg2Rad;

        // Right vector
        Vector3 right = transform.right;

        // Project right onto XZ plane (removes roll)
        Vector3 rightProjected = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

        // Roll = angle between right and its projection, around local Z axis
        float roll = Vector3.SignedAngle(rightProjected, right, transform.forward) * Mathf.Deg2Rad;

        // Yaw stays world yaw (in radians)
        float yaw = transform.eulerAngles.y * Mathf.Deg2Rad;

        // Unified angle vector
        Vector3 angles = new Vector3(pitch, yaw, roll);


        Vector3 desiredTheta;
        Vector3 desiredOmega;

        float heightError = altitude - _desiredHeight;

        Vector3 desiredVelocity = new Vector3 (_desiredRightVel, -1.0f * heightError / _data.TcVerticalVel, _desiredForwardVel);
        Vector3 velocityError = velocity - desiredVelocity;

        Vector3 desiredAcceleration = velocityError * -1.0f / _data.TcAcceleration;

        desiredTheta = new Vector3 (desiredAcceleration.z / _data.Gravity, 0.0f, -desiredAcceleration.x / _data.Gravity);
        if (desiredTheta.x > _data.MaxPitch) {
            desiredTheta.x = _data.MaxPitch;
        } else if (desiredTheta.x < -1.0f * _data.MaxPitch) {
            desiredTheta.x = -1.0f * _data.MaxPitch;
        }
        if (desiredTheta.z > _data.MaxRoll) {
            desiredTheta.z = _data.MaxRoll;
        } else if (desiredTheta.z < -1.0f * _data.MaxRoll) {
            desiredTheta.z = -1.0f * _data.MaxRoll;
        }

        Vector3 thetaError = angles - desiredTheta;

        desiredOmega = thetaError * -1.0f / _data.TcOmegaXY;
        desiredOmega.y = _desiredYawRate;

        Vector3 omegaError = angularVelocity - desiredOmega;

        Vector3 desiredAlpha = Vector3.Scale(omegaError, new Vector3(-1.0f/_data.TcAlphaXY, -1.0f/_data.TcAlphaZ, -1.0f/_data.TcAlphaXY));
        desiredAlpha = Vector3.Min (desiredAlpha, Vector3.one * _data.MaxAngularAccel);
        desiredAlpha = Vector3.Max (desiredAlpha, Vector3.one * _data.MaxAngularAccel * -1.0f);

        float desiredThrust = (_data.Gravity + desiredAcceleration.y) / (Mathf.Cos (angles.z) * Mathf.Cos (angles.x));
        desiredThrust = Mathf.Min (desiredThrust, 2.0f * _data.Gravity);
        desiredThrust = Mathf.Max (desiredThrust, 0.0f);

        Vector3 desiredTorque = Vector3.Scale (desiredAlpha, inertia);
        Vector3 desiredForce = new Vector3 (0.0f, desiredThrust, 0.0f);

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.AddRelativeTorque (desiredTorque, ForceMode.Acceleration);
        rb.AddRelativeForce (desiredForce , ForceMode.Acceleration);


        // -----------------------------
        // 10. PROPELLER VISUAL ROTATION
        // -----------------------------
        float spin = desiredThrust * _data.PropSpeedScale * deltaTime;
        _propFl.transform.Rotate(Vector3.forward * spin);
        _propFr.transform.Rotate(Vector3.forward * spin);
        _propRr.transform.Rotate(Vector3.forward * spin);
        _propRl.transform.Rotate(Vector3.forward * spin);

        // Optional debug
        // Debug.Log("Velocity " + velocity);
        // Debug.Log("Desired Velocity " + desiredVelocity);
        // Debug.Log("Desired Acceleration " + desiredAcceleration);
        // Debug.Log("Angles " + angles);
        // Debug.Log("Desired Angles " + desiredTheta);
        // Debug.Log("Angular Velocity " + angularVelocity);
        // Debug.Log("Desired Angular Velocity " + desiredOmega);
        // Debug.Log("Desired Angular Acceleration " + desiredAlpha);
        // Debug.Log("Desired Torque " + desiredTorque);
    }

    private void ComputeDesireVelocities()
    {
        // Target-based movement from MovementSystem
        Vector3 toTarget = TargetPosition - transform.position;

        // Desired local velocities (X = right, Z = forward)
        _desiredRightVel = Mathf.Clamp(toTarget.x, _desiredRightVelClamp.x, _desiredRightVelClamp.y);
        _desiredForwardVel = Mathf.Clamp(toTarget.z, _desiredForwardVelClamp.x, _desiredForwardVelClamp.y);
        _desiredHeight = TargetPosition.y;

        // Yaw target
        Vector3 toLook = LookTargetPosition - transform.position;
        toLook.y = 0f;

        if (toLook.sqrMagnitude < 0.01f)
        {
            _desiredYawRate = 0f;
            return;
        }

        float angle = Vector3.SignedAngle(transform.forward, toLook, Vector3.up) * toLook.sqrMagnitude;

        // Simple proportional yaw controller
        _desiredYawRate = Mathf.Clamp(angle * 0.05f, _desireYawRateClamp.x, _desireYawRateClamp.y);
    }

    /// <summary>
    /// Reset drone state and controller targets.
    /// </summary>
    public void Reset()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _desiredForwardVel = 0f;
        _desiredRightVel = 0f;
        _desiredYawRate = 0f;
        _desiredHeight = _initialHeight;

        transform.position = new Vector3(transform.position.x, _initialHeight, transform.position.z);
        transform.rotation = Quaternion.identity;

        enabled = true;
    }
}