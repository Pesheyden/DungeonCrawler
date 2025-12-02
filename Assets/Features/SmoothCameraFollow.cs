using System;
using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;


    [Header("Follow Settings")]
    [SerializeField] private float _positionSmoothStep = 0.3f;
    [SerializeField] private float _rotationSmoothStep = 0.1f;
    [SerializeField] private bool _followRotation = true;

    [Header("Jitter Control")]
    [SerializeField] private float _positionDeadband = 0.05f; // Minimum movement to trigger camera update
    [SerializeField] private float _rotationDeadband = 1f;    // Minimum angle (in degrees) to trigger rotation update
    
    
    [ShowNonSerializedField] private Vector3 _offset;

    private void Awake()
    {
        if (!Application.isPlaying) return;
        _offset = transform.position - _target.position;
    }
    
    void FixedUpdate()
    {
        if (_target == null) return;
        if (!Application.isPlaying) return;

        // Desired position
        Vector3 desiredPosition = _target.position + _target.TransformDirection(_offset);

        // Apply position deadband
        if ((transform.position - desiredPosition).sqrMagnitude > _positionDeadband * _positionDeadband)
        {
            transform.position = Vector3.Slerp(transform.position, desiredPosition, _positionSmoothStep * Time.deltaTime);
        }

        // Apply rotation deadband
        if (_followRotation)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(_target.position - transform.position);
            float angleDiff = Quaternion.Angle(transform.rotation, desiredRotation);

            if (angleDiff > _rotationDeadband)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationSmoothStep * Time.deltaTime);
            }
        }
    }


    private void Update()
    {
        if (_target == null) return;
        if (Application.isPlaying) return;
        
        Quaternion desiredRotation = Quaternion.LookRotation(_target.position - transform.position);
        float angleDiff = Quaternion.Angle(transform.rotation, desiredRotation);


        if (angleDiff > _rotationDeadband)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, _rotationSmoothStep * Time.deltaTime);
        }
    }
}
