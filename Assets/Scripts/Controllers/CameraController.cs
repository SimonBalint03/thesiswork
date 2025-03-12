using System;
using System.Collections;
using UnityEngine;

namespace Controllers
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera")]
        public float dragSpeed = 2.0f;
        public float scrollSpeed = 5.0f;
        public float zoomStepSpeed = 5.0f; // Speed of zoom lerp

        // Zoom limits (min and max camera height)
        public float minY = 10.0f;
        public float maxY = 50.0f;
        
        public float maxX = 50.0f;
        public float maxZ = 50.0f;

        // Rotation angles at min and max zoom
        public float angleAtMin = 30.0f;
        public float angleAtMax = 60.0f;

        public float offsetZ;

        // Lerp type (0 = Linear, 1 = SmoothStep, 2 = Exponential)
        public int lerpType = 0;
        
        private Vector3 dragOrigin;
        private float targetY; // The Y position we are zooming to
        
        [Header("Player")]
        public float playerFollowSpeed = 1.0f;
        public bool followingPlayer = false;

        private GameObject centerTarget;
       

        void Start()
        {
            targetY = transform.position.y; // Initialize the target Y to the current Y position
        }

        void Update()
        {
            UpdateDragSpeed();
            HandleCameraDrag();
            HandleScrollZoom();
            if (followingPlayer)
            {
                Vector3 newPos = new Vector3(centerTarget.transform.position.x, transform.position.y, centerTarget.transform.position.z - offsetZ);
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * playerFollowSpeed);
                if (Mathf.Abs(transform.position.z - newPos.z) <= 0.25f && Mathf.Abs(transform.position.x - newPos.x) <= 0.25f)
                {
                    followingPlayer = false;
                }
                
            }

            if (Mathf.Abs(transform.position.x) >= maxX)
            {
                transform.position = transform.position.x > 0 ? new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z) : new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z);
            }
            if (Mathf.Abs(transform.position.z) >= maxZ)
            {
                transform.position = transform.position.z > 0 ? new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f) : new Vector3(transform.position.x, transform.position.y, transform.position.z + 1f);
            }
        }

        public void CenterOnObject(GameObject target)
        {
            followingPlayer = true;
            centerTarget = target;
        }

        private void UpdateDragSpeed()
        {
            dragSpeed = transform.position.y * 1.5f;
        }

        // Handles the drag functionality of the camera
        void HandleCameraDrag()
        {
            if (Input.GetMouseButtonDown(1))
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            if (!Input.GetMouseButton(1)) return;

            Vector3 difference = Input.mousePosition - dragOrigin;
            difference.x /= Screen.width;
            difference.y /= Screen.height;

            // Move the camera on the x-z plane based on the mouse drag
            Vector3 move = new Vector3(-difference.x * dragSpeed, 0, -difference.y * dragSpeed);
            transform.Translate(move, Space.World);

            dragOrigin = Input.mousePosition;
        }

        // Handles the zoom and rotation lerping
        void HandleScrollZoom()
        {
            // Get scroll wheel input (positive or negative)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Adjust the target Y position based on scroll input
            targetY -= scrollInput * scrollSpeed;
            targetY = Mathf.Clamp(targetY, minY, maxY);

            // Smoothly lerp towards the target Y position
            float currentY = transform.position.y;
            float newY = Mathf.Lerp(currentY, targetY, zoomStepSpeed * Time.deltaTime);

            // Calculate the normalized position between minY and maxY
            float t = Mathf.InverseLerp(minY, maxY, newY);

            // Apply different types of interpolation based on user selection
            switch (lerpType)
            {
                case 0: // Linear
                    break;
                case 1: // SmoothStep
                    t = Mathf.SmoothStep(0, 1, t);
                    break;
                case 2: // Exponential
                    t = t * t * (3 - 2 * t);
                    break;
            }

            // Lerp the camera's rotation between angleAtMin and angleAtMax
            float targetRotationX = Mathf.Lerp(angleAtMin, angleAtMax, t);
            transform.rotation = Quaternion.Euler(targetRotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            // Apply the new camera position
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
