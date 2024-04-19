using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float Radius = 200f; // Distance from the target (Vector3.zero)
    private Vector3 angles; // Current rotation angles
    public float sensitivityX = 4f; // Sensitivity of mouse movement in X direction
    public float sensitivityY = 4f; // Sensitivity of mouse movement in Y direction
    public Star start = null;
    public Star end = null;
    public StarManager manager = null;
    public Camera sceneCam;
    public Camera shipCam;
    public float zoomSpeed = 50.0f;

    void Start()
    {
        Reset();
    }

    void Update()
    {
        if (sceneCam.enabled && Input.GetMouseButton(0)) // Left mouse button held down
        {
            angles.x += Input.GetAxis("Mouse X") * sensitivityX;
            angles.y -= Input.GetAxis("Mouse Y") * sensitivityY;
            // Create a ray from the camera to the mouse cursor
            Ray ray = sceneCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // You can also access other components of the clicked object
                // For example, accessing a custom script attached to the object
                Star star = hit.collider.gameObject.GetComponent<Star>();
                if (star != null && star != start && star != end)
                {
                    if (start == null)
                    {
                        start = star;
                        star.ActivateOutline();
                    }
                    else if (end == null)
                    {
                        end = star;
                        star.ActivateOutline();
                    }
                }
                if (start != null  && end != null && manager != null)
                {
                    manager.ShowPath(start.transform.position, end.transform.position);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && sceneCam.enabled)
        {
            Reset();
        }
        if (Input.GetKeyDown(KeyCode.C) && sceneCam.enabled)
        {
            ClearPath();
        }
        if (Input.GetKeyDown(KeyCode.T) && sceneCam.enabled)
        {
            ClearPath();
            manager.ShowTraversalPath();
        }
        if (Input.GetKeyDown(KeyCode.M) && manager && manager.NavPath != null)
        {
            sceneCam.enabled = false;
            shipCam.enabled = true;
            shipCam.DOFieldOfView(60, 0.3f).From(30).SetEase(Ease.OutQuad);
            float moveDuration = manager.MoveSpaceShip();
            StartCoroutine(WaitForShipMovement(moveDuration + 0.5f));
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Radius -= scroll * zoomSpeed;

        Quaternion rotation = Quaternion.Euler(angles.y, angles.x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -Radius) + Vector3.zero;

        transform.rotation = rotation;
        transform.position = position;
    }

    private void Reset()
    {
        angles = Vector3.zero;
        Radius = 500.0f;
        ClearPath();
        sceneCam.enabled = true;
        shipCam.enabled = false;
        MoveToInitialPosition();
    }

    private void ClearPath()
    {
        if (manager.NavPath != null)
        {
            manager.ClearPath();
            if (start && end)
            {
                start.DeactivateOutline();
                end.DeactivateOutline();
                start = null;
                end = null;
            }
        }
    }
    private IEnumerator WaitForShipMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        Reset();
    }

    void MoveToInitialPosition()
    {
        // Positions the camera at the initial angle and distance from Vector3.zero
        Quaternion initialRotation = Quaternion.Euler(angles.y, angles.x, 0);
        Vector3 initialPosition = initialRotation * new Vector3(0.0f, 0.0f, -Radius) + Vector3.zero;

        transform.rotation = initialRotation;
        transform.position = initialPosition;
    }
}
