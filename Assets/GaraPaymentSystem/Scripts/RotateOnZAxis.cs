using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnZAxis : MonoBehaviour
{
    // Rotation speed in degrees per second
    public float rotationSpeed = 250;

    void Update()
    {
        // Rotate object around Z axis
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}
