using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f;

    public Vector3 collision = Vector3.zero;

    public Camera camera;

    GameObject mySphere;

    // Start is called before the first frame update
    void Start()
    {
        mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mySphere.layer = LayerMask.NameToLayer ("Ignore Raycast");
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);

        

        castRay();
    }

    void castRay()
    {
        
        RaycastHit hit;
        Ray ray;
        // Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        LayerMask mask = LayerMask.GetMask("Sphere");
        
        if(Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 100.0f))
        {
            mySphere.SetActive(true);
            collision = hit.point;
            mySphere.transform.position = hit.point;
        }
        else
        {
            mySphere.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(collision, 0.2f);
    }
}
