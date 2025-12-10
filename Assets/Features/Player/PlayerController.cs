using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputStats _playerInputStats;
    [SerializeField] private PlayerMoveStats _playerMoveStats;
    private MovementSystem _movementSystem;
    
    private void Awake()
    {
        _movementSystem = GetComponent<MovementSystem>();
    }

    private void FixedUpdate()
    {
        // 1. Compute world‑space move target
        Vector3 moveTarget = transform.position;

        // Forward/back (Pitch input → move along drone forward)
        moveTarget += transform.forward * 
                      (_playerInputStats.BF_PitchInput.Value * _playerMoveStats.MoveTargetDistanceMultiplier);

        // Right/left (Roll input → move along drone right)
        moveTarget += transform.right *
                      (_playerInputStats.BF_RollInput.Value * _playerMoveStats.MoveTargetDistanceMultiplier);

        // Height (Throttle input → move along world up)
        moveTarget.y += _playerInputStats.BF_HeightInput.Value * _playerMoveStats.MoveTargetDistanceMultiplier;

        // If no height input, keep current altitude
        if (_playerInputStats.BF_HeightInput.Value == 0)
            moveTarget.y = transform.position.y;


        // 2. Compute world‑space look target
        Vector3 lookTarget;

        if (_playerInputStats.BF_YawlInput.Value != 0)
        {
            // Look direction = forward + yaw input * right
            Vector3 lookDir =
                transform.forward / _playerMoveStats.LookTargetDistanceMultiplier +
                transform.right * (_playerInputStats.BF_YawlInput.Value * _playerMoveStats.LookTargetDistanceMultiplier);

            lookTarget = transform.position + lookDir.normalized * _playerMoveStats.LookTargetDistanceMultiplier;

            _movementSystem.SetTarget(moveTarget, lookTarget);
        }
        else
        {
            _movementSystem.SetTarget(moveTarget);
        }

        // 3. Move the drone
        _movementSystem.Move(Time.fixedDeltaTime);
    }


    private void OnDrawGizmos()
    {

    }
}
