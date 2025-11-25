using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private static PlayerInputActions _playerInputActions;
    public static PlayerInputHandler Instance { get; private set; }


    // Input values
    [SerializeField] private PlayerInputStats _playerInputStats;

    #region Unity Callbacks

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            _playerInputActions = new PlayerInputActions();

            SetUpInputActions();
        }
        else if (Instance != this)
        {
            Debug.LogError($"More than one {name} could not exist");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Registers the input action callbacks to trigger corresponding events.
    /// </summary>
    private void SetUpInputActions()
    {
        // Movement and camera input
        _playerInputActions.DroneMoevement.Pitch.performed += (callBack) =>
            _playerInputStats.BF_PitchInput.Value = callBack.ReadValue<float>();
        _playerInputActions.DroneMoevement.Pitch.canceled += (callBack) =>
            _playerInputStats.BF_PitchInput.Value = callBack.ReadValue<float>();

        _playerInputActions.DroneMoevement.Roll.performed += (callBack) =>
            _playerInputStats.BF_RollInput.Value = callBack.ReadValue<float>();
        _playerInputActions.DroneMoevement.Roll.canceled += (callBack) =>
            _playerInputStats.BF_RollInput.Value = callBack.ReadValue<float>();

        _playerInputActions.DroneMoevement.Height.performed += (callBack) =>
            _playerInputStats.BF_HeightInput.Value = callBack.ReadValue<float>();
        _playerInputActions.DroneMoevement.Height.canceled += (callBack) =>
            _playerInputStats.BF_HeightInput.Value = callBack.ReadValue<float>();

        _playerInputActions.DroneMoevement.Yawl.performed += (callBack) =>
            _playerInputStats.BF_YawlInput.Value = callBack.ReadValue<float>();
        _playerInputActions.DroneMoevement.Yawl.canceled += (callBack) =>
            _playerInputStats.BF_YawlInput.Value = callBack.ReadValue<float>();

        // Button inputs to trigger events
        //_playerInputActions.Player.Jump.started += _ => OnJump?.Invoke();
    }


    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    #endregion

    #region Input Action Management

    private void EnableInputActions()
    {
        _playerInputActions.DroneMoevement.Pitch.Enable();
        _playerInputActions.DroneMoevement.Roll.Enable();
        _playerInputActions.DroneMoevement.Height.Enable();
        _playerInputActions.DroneMoevement.Yawl.Enable();
    }

    private void DisableInputActions()
    {
        _playerInputActions.DroneMoevement.Pitch.Disable();
        _playerInputActions.DroneMoevement.Roll.Disable();
        _playerInputActions.DroneMoevement.Height.Disable();
        _playerInputActions.DroneMoevement.Yawl.Disable();
    }

    #endregion
}