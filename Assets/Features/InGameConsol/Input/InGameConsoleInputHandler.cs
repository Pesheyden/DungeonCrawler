using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameConsoleInputHandler : Singleton<InGameConsoleInputHandler>
{
    private InGameConsoleInputActions _inputActions;

    public event Action<bool> OnEnter;
    public event Action<int> OnDirectionInput;

    private void Awake()
    {
        _inputActions = new InGameConsoleInputActions();
        SetUpInputActions();
    }

    private void SetUpInputActions()
    {
        _inputActions.Console.Enter.started += _ => OnEnter?.Invoke(true);
        //_inputActions.Console.Enter.canceled += _ => OnEnter?.Invoke(false);
        
        _inputActions.Console.DirectionInput.performed += (context) => OnDirectionInput?.Invoke(Mathf.RoundToInt(context.ReadValue<float>()));
    }
    
    private void OnEnable()
    {
        _inputActions.Console.Enter.Enable();
        _inputActions.Console.DirectionInput.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Console.Enter.Disable();
        _inputActions.Console.DirectionInput.Disable();
    }
}
