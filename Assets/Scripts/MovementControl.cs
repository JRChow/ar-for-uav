using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementControl : MonoBehaviour
{
    /// <summary>
    /// movement amount to control speed speed
    /// </summary>
    ///

    public float trans_movement_amount = 0.1f;

    public float rot_movement_amount = 0.1f;

    public float purturbation_amount = 0.01f;

    private ObjectSelector _objectSelector;

    private bool RotationInitiated;
    private bool RotationInProgress;
    private Vector3 startingPosition;
    private Vector3 targetCenter;

    public GameObject left_joystick;
    public GameObject right_joystick;
    public GameObject vertical_joystick;
    private FixedJoystick left_joystick_handle;
    private FixedJoystick right_joystick_handle;
    private FixedJoystick vertical_joystick_handle;

    private void Start()
    {
        _objectSelector = GetComponent<ObjectSelector>();
        left_joystick_handle = left_joystick.GetComponent<FixedJoystick>();
        right_joystick_handle = right_joystick.GetComponent<FixedJoystick>();
        vertical_joystick_handle = vertical_joystick.GetComponent<FixedJoystick>();
    }

    public bool IsMoving()
    {
        return left_joystick_handle.is_dragging || right_joystick_handle.is_dragging || vertical_joystick_handle.is_dragging;
    }

    void KeyboardMovement()
    {
        // activation sensitivity
        var sensitivity = 0.01f;
        var movementVector = new Vector3(0f, 0f, 0f);
        var rot_movementVector = new Vector3(0f, 0f, 0f);
        var yMove = 0.0f;
        var yawMove = 0.0f;
        var pitchMove = 0.0f;

        var horizontal_left = left_joystick_handle.Horizontal;
        var vertical_left = left_joystick_handle.Vertical;
        var horizontal_right = right_joystick_handle.Horizontal;
        var vertical_right = right_joystick_handle.Vertical;
        var vertical_vertical = vertical_joystick_handle.Vertical;

        // Vertical translation
        if (Input.GetKey(KeyCode.Space) || vertical_vertical > sensitivity)
        {
            yMove += 1.0f;
        }else if (Input.GetKey(KeyCode.LeftShift) || vertical_vertical < -sensitivity)
        {
            yMove -= 1.0f;
        }
        // Yaw & pitch
        if (Input.GetKey(KeyCode.LeftArrow) || horizontal_right < -sensitivity)
        {
            yawMove -= 1.0f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || horizontal_right > sensitivity)
        {
            yawMove += 1.0f;
        }
        if (Input.GetKey(KeyCode.DownArrow) || vertical_right < -sensitivity)
        {
            pitchMove += 1.0f;
        }
        else if (Input.GetKey(KeyCode.UpArrow) || vertical_right > sensitivity)
        {
            pitchMove -= 1.0f;
        }

        var hMove = Input.GetAxis("Horizontal");
        var vMove = Input.GetAxis("Vertical");
        // a left
        if (hMove < -sensitivity || horizontal_left < -sensitivity) movementVector.x = -trans_movement_amount;
        // d right
        if (hMove > sensitivity || horizontal_left > sensitivity) movementVector.x = trans_movement_amount;
        // s backward
        if (vMove < -sensitivity || vertical_left < -sensitivity) movementVector.z = -trans_movement_amount;
        // w forward
        if (vMove > sensitivity || vertical_left > sensitivity  ) movementVector.z = trans_movement_amount;

        
        rot_movementVector.y = rot_movement_amount * yawMove;
        rot_movementVector.x = rot_movement_amount * pitchMove;

        // Using Translate allows you to move while taking the current rotation into consideration
        transform.Rotate(rot_movementVector);

        LockZRotation();

        // Lock translation to be along y and z axies in world space
        movementVector.y = trans_movement_amount * yMove;
        var theta = Mathf.Deg2Rad * (-transform.eulerAngles.x);
        var y_rela = movementVector.y;
        var z_rela = movementVector.z;
        movementVector.z = Mathf.Sin(theta) * y_rela + Mathf.Cos(theta) * z_rela;
        movementVector.y =  - Mathf.Sin(theta) * z_rela + Mathf.Cos(theta) * y_rela;
        transform.Translate(movementVector);
    }

    private void AddRandomPurturbation()
    {
        var random = new System.Random();
        var purturbation_trans = new Vector3(0f, 0f, 0f);
        var purturbation_rot = new Vector3(0f, 0f, 0f);
        purturbation_trans.x = (((float)random.NextDouble()) * 2 - 1) * purturbation_amount;
        purturbation_trans.y = (((float)random.NextDouble()) * 2 - 1) * purturbation_amount;
        purturbation_trans.z = (((float)random.NextDouble()) * 2 - 1) * purturbation_amount;
        purturbation_rot.x = (((float)random.NextDouble()) * 2 - 1) * purturbation_amount;
        purturbation_rot.y = (((float)random.NextDouble()) * 2 - 1) * purturbation_amount;
        purturbation_rot.z = (((float)random.NextDouble()) * 2 - 1) * purturbation_amount;
        transform.Translate(purturbation_trans);
        transform.Rotate(purturbation_rot);
    }

    // Update is called once per frame
    void Update()
    {
        // Forbid user movement during annotation.
        if (!_objectSelector.WaitingButtonClick && !RotationInitiated)
        {
            KeyboardMovement();
        }
        if (!_objectSelector.WaitingButtonClick)
        {
            AddRandomPurturbation();
        }
        if (RotationInitiated)
        {
            if (RotationInProgress)
            {
                PerformRotation();
            }
            else
            {
                NavigateToStartPosition();
            }
            
        }
    }

    // Performe Rotation after drone is at start position
    private void PerformRotation()
    {
        transform.LookAt(targetCenter);
        transform.Translate(trans_movement_amount * Vector3.right);
    }

    private void NavigateToStartPosition()
    {
        var transDirection = (startingPosition - transform.position).normalized;

        bool trans_aligned = (startingPosition - transform.position).magnitude < 1;
        if (!trans_aligned)
        {
            transform.Translate(transDirection * trans_movement_amount * 0.8f, Space.World);
        }

        Vector3 targetDirection = targetCenter - transform.position;
        bool rot_aligned = (transform.forward.normalized - targetDirection.normalized).magnitude < 0.01;
        if (!rot_aligned)
        {
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Mathf.Deg2Rad * rot_movement_amount * 0.8f, 0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
            //LockZRotation();
        }
        if (trans_aligned && rot_aligned)
        {
            RotationInProgress = true;
        }
//        Debug.Log(trans_aligned.ToString() + rot_aligned.ToString());
    }

    // Lock camera transform rotation z value at 0
    private void LockZRotation()
    {
        var z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
    }

    private float Pythagoras(float a, float b)
    {
        return Mathf.Sqrt(a * a + b * b);
    }

    public void StartOrStopAutoPilot()
    {
        if (RotationInitiated)
        {
            RotationInitiated = false;
            RotationInProgress = false;
        }
        else
        {
            InitiateRotatationAroundObjectsCenter();
        }
    }

    // Rotate Around the Center of Selected Objects
    private void InitiateRotatationAroundObjectsCenter()
    {
        var boxVisualizer = GetComponent<BoxVisualizer>();
        var cubeList = boxVisualizer.GetCubesList();
        if (cubeList.Count == 0) return;
        float x_min = float.PositiveInfinity;
        float x_max = float.NegativeInfinity;
        float z_min = float.PositiveInfinity;
        float z_max = float.NegativeInfinity;
        float y_min = float.PositiveInfinity;
        float y_max = float.NegativeInfinity;
        for (var i = 0; i < cubeList.Count; i++)
        {
            var cube = cubeList[i];
            var center = cube[0];
            var size = cube[1];

            var min3DCorner = center - size / 2;
            var max3DCorner = center + size / 2;

            x_min = Mathf.Min(x_min, min3DCorner.x);
            x_max = Mathf.Max(x_max, max3DCorner.x);
            z_min = Mathf.Min(z_min, min3DCorner.z);
            z_max = Mathf.Max(z_max, max3DCorner.z);
            y_min = Mathf.Min(y_min, min3DCorner.y);
            y_max = Mathf.Max(y_max, max3DCorner.y);
        }

        var centerOfObjects = new Vector3((x_min + x_max) / 2, (y_max + y_min) / 2, (z_min + z_max) / 2);
        var startingPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        startingPoint.y = Mathf.Max(startingPoint.y, y_max + 10f, 95f);
        var radius = Pythagoras(z_max - z_min, x_max - x_min) / 2f * 2f;
        var distanceToCenter = Pythagoras(centerOfObjects.z - startingPoint.z, centerOfObjects.x - startingPoint.x);
        if (distanceToCenter < radius)
        {
            startingPoint.x = centerOfObjects.x + (startingPoint.x - centerOfObjects.x) / distanceToCenter * radius;
            startingPoint.z = centerOfObjects.z + (startingPoint.z - centerOfObjects.z) / distanceToCenter * radius;
        }

        startingPosition = startingPoint;
        targetCenter = centerOfObjects;
        RotationInitiated = true;
//        Debug.Log(centerOfObjects.ToString());
//        Debug.Log(startingPoint.ToString());
//        Debug.Log(transform.position.ToString());
    }
}
