using UnityEngine;

public abstract class MovementSystem : MonoBehaviour
{
    protected Vector3 TargetPosition;
    protected Vector3 LookTargetPosition;



    public virtual void SetTarget(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        LookTargetPosition =  targetPosition;
    }
    public virtual void SetTarget(Vector3 targetPosition, Vector3 lookTarget )
    {
        TargetPosition = targetPosition;
        LookTargetPosition = lookTarget;
    }

    public abstract void Move(float deltaTime);
}
