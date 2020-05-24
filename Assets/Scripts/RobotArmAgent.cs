using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.UIElements;

public class RobotArmAgent : Agent
{
    public GameObject arm;
    public GameObject axle;
    public GameObject endEffector;
	Rigidbody rb_arm;
    Rigidbody rb_axle;
    Rigidbody rb_endEffector;
	public float torqueMultiplier = 15000;

    public GameObject text;
    TextMesh mesh;


    float lastAngle = 2000;
    float lastAxleVel = 0;
    int rotationCounter;

    long stepCounter = 0;

    public override void Initialize() {
    	rb_arm = arm.GetComponent<Rigidbody>();
        rb_arm.maxAngularVelocity = 5;
        rb_axle = axle.GetComponent<Rigidbody>();
        rb_endEffector = endEffector.GetComponent<Rigidbody>();
        mesh = text.GetComponent<TextMesh>();
    }
	public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 localArmAngVel = 
            arm.transform.InverseTransformDirection(rb_arm.angularVelocity);
        Vector3 localAxleAngVel = 
            axle.transform.InverseTransformDirection(rb_axle.angularVelocity);
        float continuousAxleAngle = getContinuousAxleAngle(axle);

        sensor.AddObservation(localAxleAngVel.z);
        sensor.AddObservation(continuousAxleAngle);
        sensor.AddObservation(localArmAngVel.y);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Analog for motor current
        float controlTorque = vectorAction[0];
        float torqueToAdd = controlTorque * torqueMultiplier;
        rb_arm.AddTorque(rb_arm.transform.up * torqueToAdd);

        updateRotationCounter(axle, rb_axle);

        // if (stepCounter%2==0){
            collectObservation();
        // }
        stepCounter++;
    }

    private void collectObservation(){
        float continuousAxleAngle = getContinuousAxleAngle(axle);
        float posReward = 90-Mathf.Abs(continuousAxleAngle);  // Should range 0 - 180
        float reward = (posReward*posReward) / (90f*90f);

        mesh.text = (continuousAxleAngle.ToString("0.0##"));
        SetReward(Mathf.Abs(continuousAxleAngle) < 75 ? reward : -0.025f);

        float spawnDistance = 
            Academy.Instance.EnvironmentParameters.GetWithDefault("spawn_angle_max", 180f);
        float cancelAngle = spawnDistance*1.5f;
    }

    public override void OnEpisodeBegin() {
        float spawnDistance = 
            Academy.Instance.EnvironmentParameters.GetWithDefault("spawn_angle_max", 180f);
        resetToStart();
        float startAngleOffset = Random.Range(-1*spawnDistance, spawnDistance);
        if (startAngleOffset > 0) {
            rotationCounter = 0;
        } else {
            rotationCounter = -1;
        }
        lastAngle = startAngleOffset;
        axle.transform.localRotation = Quaternion.Euler(0f, 0f, startAngleOffset);

        stepCounter = 0;
    }
    
    private void resetToStart() {
        lastAngle = 0;
        lastAxleVel = 0;
        // Reset arm 
        rb_arm.angularVelocity = new Vector3(0f, 0f, 0f);
        arm.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Reset axle
        rb_axle.velocity = new Vector3(0f, 0f, 0f);
        rb_axle.angularVelocity = new Vector3(0f, 0f, 0f);
        axle.transform.localPosition = new Vector3(0f, 1.5f, 1.5f);

        // Reset end effector positions should be reset by the axle and its joint
        rb_endEffector.angularVelocity = new Vector3(0f, 0f, 0f);
        rb_endEffector.velocity = new Vector3(0f, 0f, 0f);
    }

    private void updateRotationCounter(GameObject axle, Rigidbody rb_axle) {
        float rawAngle = axle.transform.localRotation.eulerAngles.z;
        // CW is positive
        float axleVel = 
            axle.transform.InverseTransformDirection(rb_axle.angularVelocity).z;
        if ((lastAngle >= 0 && lastAngle < 180) && (rawAngle > 180) && lastAxleVel < 0){
            // Cross over zero CCW
            rotationCounter--;
        } else if ((lastAngle > 180) && (rawAngle >= 0 && rawAngle < 180) && lastAxleVel > 0){
            // Cross over zero CW
            rotationCounter++;
        }
        lastAngle = rawAngle;
        lastAxleVel = axleVel;
    }

    public float getContinuousAxleAngle(GameObject axle){
        float rawAngle = axle.transform.localRotation.eulerAngles.z;
        return rotationCounter*360 + rawAngle;
    }
}
