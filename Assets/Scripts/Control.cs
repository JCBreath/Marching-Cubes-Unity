using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Control : MonoBehaviour
{
    // Start is called before the first frame update
    float mouse_x, mouse_y;
    Vector3 velocity;
    float movement_speed;
    float rotation_x, rotation_y;
    float mouseSensitivity;
    GraphicRaycaster raycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;

    void Start()
    {
        velocity = Vector3.zero;
        movement_speed = 3f;
        rotation_x = 0f;
        mouseSensitivity = 100f;
        raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("Canvas").GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);
            if (results.Count == 0 && GameObject.Find("ResolutionDropdown").transform.childCount == 3 && GameObject.Find("DatasetDropdown").transform.childCount == 3 && GameObject.Find("CompMethodDropdown").transform.childCount == 3)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if(Cursor.lockState == CursorLockMode.Locked)
        {
            velocity = Vector3.zero;
            mouse_x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouse_y = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            rotation_x -= mouse_y;
            rotation_y += mouse_x;

            if (rotation_y > 360)
            {
                rotation_y -= 360;
            }
            if (rotation_y < 0)
            {
                rotation_y += 360;
            }




            rotation_x = Mathf.Clamp(rotation_x, -90f, 90f);

            // transform.Rotate(Vector3.up * mouse_x);
            transform.localRotation = Quaternion.Euler(rotation_x, rotation_y, 0);


            velocity += Input.GetAxis("Horizontal") * transform.right * movement_speed;
            velocity += Input.GetAxis("Vertical") * transform.forward * movement_speed;

            transform.position += velocity * Time.deltaTime;
        }

        
    }
}
