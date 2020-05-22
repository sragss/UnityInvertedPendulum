using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public float speed = 2.0f;
    public float rotationRadius = 20.0f;
    public float lookAngle = 30;
    public Transform target; // Entity to look at
    float currentRotation = 0;
    // Start is called before the first frame update
    void Start(){}

    // Update is called once per frame
    void Update()
    {
        currentRotation = updateRotation(currentRotation);
        
        transform.position = target.position + Quaternion.Euler(lookAngle, currentRotation, 0) * (rotationRadius * Vector3.back);
        transform.LookAt(target.position, Vector3.up);
    }

    private float updateRotation(float currentRotation) {
        currentRotation += speed;
        if (currentRotation > 360) {
            return currentRotation - 360;
        }
        return currentRotation;
    }
}
