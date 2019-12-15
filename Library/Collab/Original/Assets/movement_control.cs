using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement_control : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void KeyboardMovement()
    {
        var sensitivity = 0.01f;
        var movementAmount = 0.5f;
        var movementVector = new Vector3(0f, 0f, 0f);
        var rot_movementVector = new Vector3(0f, 0f, 0f);
        var yMove = 0.0f;
        var yawMove = 0.0f;
        var pitchMove = 0.0f;
        if (Input.GetKey(KeyCode.Space))
        {
            yMove += 1.0f;
        }else if (Input.GetKey(KeyCode.LeftShift))
        {
            yMove -= 1.0f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            yawMove -= 1.0f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            yawMove += 1.0f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            pitchMove += 1.0f;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            pitchMove -= 1.0f;
        }

        var hMove = Input.GetAxis("Horizontal");
        var vMove = Input.GetAxis("Vertical");
        // left arrow
        if (hMove < -sensitivity) movementVector.x = -movementAmount;
        // right arrow
        if (hMove > sensitivity) movementVector.x = movementAmount;
        // up arrow
        if (vMove < -sensitivity) movementVector.z = -movementAmount;
        // down arrow
        if (vMove > sensitivity) movementVector.z = movementAmount;

        movementVector.y = movementAmount * yMove;
        rot_movementVector.y = movementAmount * yawMove;
        rot_movementVector.x = movementAmount * pitchMove;

        // Using Translate allows you to move while taking the current rotation into consideration
        transform.Translate(movementVector);
        transform.Rotate(rot_movementVector);
    }


    // Update is called once per frame
    void Update()
    {
        KeyboardMovement();
    }
}
