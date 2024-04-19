using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;

public class SpaceShip : MonoBehaviour
{
    public float speed = 5f; // 移动速度

    void Start()
    {
    }

    public float Move(List<Vector3> path)
    {
        if (path.Count <= 0) {
            return 0.0f;
        }

        Sequence sequence = DOTween.Sequence(); // Create a new DOTween Sequence
        transform.position = path[0];
        // For each point in the path, create a movement and rotation tween and add them to the sequence
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 destination = path[i];
            // Calculate duration based on distance and speed to ensure consistent movement speed
            float duration = Vector3.Distance(path[i - 1], destination) / speed;

            // Append rotation tween to face the next point in the path
            Vector3 directionToTarget = destination - path[i - 1];
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
            sequence.Append(transform.DORotateQuaternion(targetRotation, 0.5f));

            // Append movement tween to move to the next point
            sequence.Append(transform.DOMove(destination, duration));
        }
        return sequence.Duration();
    }

    void SingleMove(Vector3 Start, Vector3 destination)
    {
        transform.position = Start;
        // Calculate the direction to the target
        Vector3 directionToTarget = destination - transform.position;
        // Calculate the rotation required to point at the target
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Rotate the object to face the target over the specified duration
        transform.DORotateQuaternion(targetRotation, 0.5f).SetEase(Ease.InOutQuad);
        float duration = (destination - transform.position).magnitude / speed;
        transform.DOMove(destination, duration).SetEase(Ease.InOutQuad);
    }
}

