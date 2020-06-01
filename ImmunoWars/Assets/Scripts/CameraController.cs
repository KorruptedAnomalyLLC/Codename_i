using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed;
    public float movementTime;

    public Vector3 newPosition;



    void Start()
    {

        newPosition = transform.position;

    }

    void Update()
    {
        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        // Need to add Mobile controlls
        // PC controlls, giving the option for WASD and Arrow Keys
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
        }


        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);


    }

}
