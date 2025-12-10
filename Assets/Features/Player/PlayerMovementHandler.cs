using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
    [SerializeField] private PlayerMoveStats _playerMoveStats;

    private PlayerInputStats _playerInputStats;

    private bool HasInput =>
        _playerInputStats.BF_HeightInput.Value != 0 &&
        _playerInputStats.BF_PitchInput.Value != 0 &&
        _playerInputStats.BF_RollInput.Value != 0 &&
        _playerInputStats.BF_YawlInput.Value != 0;

    public void Initialize(PlayerInputStats playerInputStats)
    {
        _playerInputStats = playerInputStats;
    }
    
    public void HandleMovement(float deltaTime)
    {
        if (!HasInput)
        {
            AutoLevel();
            return;
        }

        Move();
        Rotate();

    }

    private void Move()
    {
        throw new NotImplementedException();
    }

    private void Rotate()
    {
        throw new NotImplementedException();
    }

    private void AutoLevel()
    {
        throw new NotImplementedException();
    }
}
