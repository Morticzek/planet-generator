using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static GenerateTerrain;

public class MouseLook : MonoBehaviour
{

    public float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f;

    public Vector3 collision = Vector3.zero;

    public Camera camera;

    GameObject mySphere;

    GameObject generatorObject;

    GenerateTerrain generatorScript; 

    // Start is called before the first frame update
    void Start()
    {
        mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mySphere.layer = LayerMask.NameToLayer ("Ignore Raycast");
        Cursor.lockState = CursorLockMode.Locked;

        generatorObject = GameObject.Find("Generator");
        generatorScript = (GenerateTerrain) generatorObject.GetComponent(typeof(GenerateTerrain));
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
        
        if(Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 100.0f))
        {
            mySphere.SetActive(true);
            collision = hit.point;

            //terraforming on mosue presses
            if (Input.GetMouseButton(0))
                generatorScript.createNewTerrain(collision);

            mySphere.transform.position = hit.point;

        }
        else
        {
            mySphere.SetActive(false);
        }
    }

}
