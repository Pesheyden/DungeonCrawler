using System;
using UnityEngine;

public abstract class MovementSystem : MonoBehaviour
{
    protected Vector3 TargetPosition;
    protected Vector3 LookTargetPosition;



    public virtual void SetTarget(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        LookTargetPosition = transform.position;
    }
    public virtual void SetTarget(Vector3 targetPosition, Vector3 lookTarget )
    {
        TargetPosition = targetPosition;
        LookTargetPosition = lookTarget;
    }

    public abstract void Move(float deltaTime);

    private void OnDrawGizmosSelected()
    {
        if(TargetPosition == default)
            return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(TargetPosition, Vector3.one);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(LookTargetPosition, Vector3.one);
    }
}
