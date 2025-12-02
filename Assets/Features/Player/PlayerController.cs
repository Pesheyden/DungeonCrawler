using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputStats _playerInputStats;
    [SerializeField] private PlayerMoveStats _playerMoveStats;
    [SerializeField] private Transform _testTarget;
    [SerializeField] private Transform _testLookTarget;
    [SerializeField] private Transform _camera;
    private MovementSystem _movementSystem;

    private Vector3 _lastMoveTarget;
    private Vector3 _lastLookTarget;
    private void Awake()
    {
        _movementSystem = GetComponent<MovementSystem>();
    }

    private void FixedUpdate()
    {
        SetTargets();
        //_movementSystem.SetTarget(_testTarget.position,_testLookTarget.position);
        _movementSystem.Move(Time.fixedDeltaTime);
    }

    private void SetTargets()
    {
        Vector3 moveTarget = Vector3.zero;
        
        moveTarget += _playerInputStats.BF_RollInput.Value * transform.right;
        moveTarget += _playerInputStats.BF_PitchInput.Value * transform.forward;
        moveTarget += _playerInputStats.BF_HeightInput.Value * transform.up;

        if (moveTarget != Vector3.zero)
        {
            moveTarget = moveTarget.normalized * _playerMoveStats.MoveTargetDistanceMultiplier + transform.position;
            _lastMoveTarget = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        }
        else
            moveTarget = _lastMoveTarget;

        Vector3 lookTarget = Vector3.zero;

        lookTarget += _camera.forward;
        lookTarget.y = 0;
        lookTarget = lookTarget.normalized * _playerMoveStats.LookTargetDistanceMultiplier + transform.position;


        _movementSystem.SetTarget(moveTarget,lookTarget);   
        
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(_lastMoveTarget, Vector3.one);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(_lastLookTarget, Vector3.one);
    }
}
