using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class PIDFloatControllerValues
{
    public float ProportionalGain;
    public float IntegralGain;
    public float IntegralSaturation;
    public float DerivativeGain;

    public Vector2 ClampFinalValue;
}

public class PIDFloatController
{
    private PIDFloatControllerValues _pidValues;

    private float _lastValue;
    private Quaternion _lastQuaternionValue;
    private float _integrationStored;

    public float ProportionalDebug;
    public float IntegralDebug;
    public float DerivativeDebug;
    
    public PIDFloatController(PIDFloatControllerValues pidValues)
    {
        _pidValues = pidValues;
    }
    
    public float Update(float deltaTime, float currentValue, float targetValue)
    {
        //Calculate error
        float error = targetValue - currentValue;

        //Calculate PID
        //P
        float proportional = _pidValues.ProportionalGain * error;

        //I
        _integrationStored += error * deltaTime;
        _integrationStored =
            Mathf.Clamp(_integrationStored, -_pidValues.IntegralSaturation, _pidValues.IntegralSaturation);
        float integral = _pidValues.IntegralGain * _integrationStored;

        //D
        float valueChangeRate = 0; 
        if(_lastValue != 0) 
            valueChangeRate = -(currentValue - _lastValue) / deltaTime;
        float derivative = _pidValues.DerivativeGain * valueChangeRate;
        _lastValue = currentValue;
        
        //DebugStorage
        ProportionalDebug = proportional;
        IntegralDebug = integral;
        DerivativeDebug = derivative;
        
        //FinalValue
        return Mathf.Clamp(proportional + integral + derivative, _pidValues.ClampFinalValue.x, _pidValues.ClampFinalValue.y);
    }
    public float UpdateAngle(float deltaTime, float currentValue, float targetValue, float angularVelocity = Single.NaN)
    {
        //Calculate error
        float error = AngularDifference(currentValue, targetValue);

        //Calculate PID
        //P
        float proportional = _pidValues.ProportionalGain * error;

        //I
        _integrationStored += error * deltaTime;
        _integrationStored =
            Mathf.Clamp(_integrationStored, -_pidValues.IntegralSaturation, _pidValues.IntegralSaturation);
        float integral = _pidValues.IntegralGain * _integrationStored;

        //D
        float valueChangeRate = 0;
        if (_lastValue != 0)
        {
            if (angularVelocity == Single.NaN)
            {
                valueChangeRate = -AngularDifference(currentValue,_lastValue) / deltaTime;
            }
            else
            {
                valueChangeRate = (angularVelocity - _lastValue) / deltaTime;
            }
        }
            
        float derivative = _pidValues.DerivativeGain * valueChangeRate;
        _lastValue = angularVelocity == Single.NaN ? currentValue : angularVelocity;

        
        //DebugStorage
        ProportionalDebug = proportional;
        IntegralDebug = integral;
        DerivativeDebug = derivative;
        
        //FinalValue
        return Mathf.Clamp(proportional + integral + derivative, _pidValues.ClampFinalValue.x, _pidValues.ClampFinalValue.y);
    }

    private static float AngularDifference(float a, float b) => (a - b + 540) % 360 - 180;
}
