using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed      = 10f;
    public float sprintFactor   = 1.5f;
    public float rotateSpeed    = 5f;

    public KeyCode KeyMoveFront;
    public KeyCode KeyMoveBack;
    public KeyCode KeyMoveRight;
    public KeyCode KeyMoveLeft;

    public KeyCode KeySprint;

    private Vector3 previousMousePosition;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        previousMousePosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;

        if(Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
        
        if(Cursor.lockState != CursorLockMode.Locked) 
            return;
        
        var speed = moveSpeed;

        if(Input.GetKey(KeySprint))
            speed *= sprintFactor;

        if(Input.GetKey(KeyMoveFront))
            transform.Translate(Vector3.forward * speed * Time.deltaTime); 
        if(Input.GetKey(KeyMoveBack))
            transform.Translate(Vector3.back * speed * Time.deltaTime); 
        if(Input.GetKey(KeyMoveLeft))
            transform.Translate(Vector3.left * speed * Time.deltaTime); 
        if(Input.GetKey(KeyMoveRight))
            transform.Translate(Vector3.right * speed * Time.deltaTime); 

        Vector2 mouseDir = Input.mousePosition - previousMousePosition;
        previousMousePosition = Input.mousePosition;

        var d = mouseDir.x < 0 ? -1 : 1;

        float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotateSpeed;
        float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * rotateSpeed;
        transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
    }
}
