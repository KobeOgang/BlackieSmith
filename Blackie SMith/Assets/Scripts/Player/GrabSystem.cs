using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSystem : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabDistance = 3f;           // Max distance to grab objects
    public float minHoldDistance = 1f;        // Minimum hold distance
    public float maxHoldDistance = 4f;        // Maximum hold distance
    public float moveSpeed = 10f;             // How fast the object moves with mouse
    public float rotateSpeed = 100f;          // How fast the object rotates (optional)
    public float scrollSpeed = 2f;            // Speed of adjusting hold distance
    public LayerMask grabbableLayer;          // Layer for grabbable objects

    [Header("References")]
    public Camera playerCamera;               // Reference to the player's camera
    public Transform holdPosition;            // Reference to the hold position (child of camera)

    // Private variables
    private Rigidbody grabbedObject;
    private bool isGrabbing = false;
    private float currentHoldDistance;        // The distance of held object from player

    void Start()
    {
        currentHoldDistance = (minHoldDistance + maxHoldDistance) / 2f; // Set to mid-range by default
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))      // Left mouse button pressed
        {
            TryGrabObject();
        }
        else if (Input.GetMouseButtonUp(0))   // Left mouse button released
        {
            if (isGrabbing)
            {
                ReleaseItem();
            }
        }

        if (isGrabbing && grabbedObject != null)
        {
            AdjustHoldDistance();
            MoveObject();
        }
    }

    void TryGrabObject()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Ray from center of screen

        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                grabbedObject = rb;
                isGrabbing = true;

                // Disable gravity while being held
                grabbedObject.useGravity = false;
                // Make it more stable while held
                grabbedObject.drag = 10f;
                grabbedObject.angularDrag = 10f;

                // Set hold distance based on grab distance
                currentHoldDistance = Mathf.Clamp(Vector3.Distance(playerCamera.transform.position, hit.point), minHoldDistance, maxHoldDistance);
            }
        }
    }

    void AdjustHoldDistance()
    {
        // Adjust hold distance with scroll wheel input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentHoldDistance = Mathf.Clamp(currentHoldDistance + scrollInput * scrollSpeed, minHoldDistance, maxHoldDistance);
    }

    void MoveObject()
    {
        if (grabbedObject == null || holdPosition == null)
            return;

        // Update hold position's local position based on hold distance
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * currentHoldDistance;
        grabbedObject.velocity = (targetPosition - grabbedObject.position) * moveSpeed;

        // Optional: Rotate object with keys (Q/E)
        if (Input.GetKey(KeyCode.Q))
            grabbedObject.AddTorque(Vector3.up * -rotateSpeed * Time.deltaTime, ForceMode.VelocityChange);
        if (Input.GetKey(KeyCode.E))
            grabbedObject.AddTorque(Vector3.up * rotateSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }

    public void ReleaseItem()
    {
        if (grabbedObject != null)
        {
            // Re-enable gravity
            grabbedObject.useGravity = true;
            // Reset drag values
            grabbedObject.drag = 0f;
            grabbedObject.angularDrag = 0.05f;
            // Add velocity from movement
            grabbedObject.velocity = grabbedObject.velocity * 0.5f; // Reduce throw velocity
        }

        grabbedObject = null;
        isGrabbing = false;
    }

    public bool IsGrabbing()
    {
        return grabbedObject != null;
    }

    public ItemIdentifier GetHeldItem()
    {
        return grabbedObject != null ? grabbedObject.GetComponent<ItemIdentifier>() : null;
    }
}
