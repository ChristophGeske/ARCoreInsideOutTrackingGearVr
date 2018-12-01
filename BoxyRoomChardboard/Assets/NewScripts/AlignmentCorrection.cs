using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentCorrection : MonoBehaviour
{
    public static float ANGLETHRESHOLD = 2f;
    public static int ROTATIONQUEUESIZE = 10;

    //camera for rotation values
    public Transform gearCamera;
    //AR Core Camera for real world position values
    public Transform arcoreCamera;
    //class which "followy" the arcore cam position with the daydream camera
    FollowARCoreCamera Arcorecam;

    GameObject treasure;
    //stores rotation each frame from daydream camera
    Queue<Vector3> rotations;
    //flag which indicates that recentered got triggered
    bool watchRotation;

    // Use this for initialization
    void Start()
    {
        Arcorecam = GameObject.FindObjectOfType<FollowARCoreCamera>();
        watchRotation = false;
        rotations = new Queue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {

        //only watch last ROTATIONQUEUESIZE rotations. The rotation recenter can last some frames, so we have to track more rotation values to spot the action and the angle
        if (rotations.Count > ROTATIONQUEUESIZE)
        {
            //if queue is full kick out the oldest element
            rotations.Dequeue();
        }
        //enque the newest rotation value of daydream cam
        rotations.Enqueue(gearCamera.rotation.eulerAngles);

        /*
        //recentered got pressed might be executing soon / executed
        if (GvrControllerInput.Recentered)
        {
            //Debug.Log("[IO] Recentering Done");
            //watch the Queue with stored rotations to spot the angle difference
            watchRotation = true;
        }
        */

        var fingerCount = 0;
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
            {
                fingerCount++;
            }
        }

        if (fingerCount > 0)
        {
            watchRotation = true;
            fingerCount = 0;
            if (watchRotation)
            {
                treasure = GameObject.Find("Treasure");
                treasure.SetActive(false);
            }

        }


        if (watchRotation)
        {
            //get current y angle of daydream camera
            float yAngle = gearCamera.rotation.eulerAngles.y;
            //check if its inside our "recetered" anglethreshhold
            if ((yAngle > 360f - ANGLETHRESHOLD && yAngle <= 360) ||
                    (yAngle >= 0f && yAngle < ANGLETHRESHOLD))
            {
                //get the odlest y angle of queue
                yAngle = rotations.Dequeue().y;
                //remeber the angle before yAngle
                float angleBeforeRecenter = 0f;
                //as long ad the yAngle is not in range, dequeue the rotations
                while (!((yAngle > 360f - ANGLETHRESHOLD && yAngle <= 360) || (yAngle >= 0f && yAngle < ANGLETHRESHOLD)))
                {
                    angleBeforeRecenter = yAngle;
                    yAngle = rotations.Dequeue().y;
                }
                //now angleBeforeRecenter has the angle before the recenterd rotation of (almost) 0
                //Debug.Log("[IO] Recenter finished, y Angle almost 0f");
                //tell arcore cam the angle, so the delta Vectors can be rotated accordingly
                //we have to substract the old vector, cause this vector is always the new origin rotation
                Arcorecam.mirrorAngle = (Arcorecam.mirrorAngle - angleBeforeRecenter) % 360;
                //Debug.Log("[IO] ARCore mirrorAngle = " + Arcorecam.mirrorAngle);
                //stop watching the rotations
                watchRotation = false;
            }
            //if not, keep on recording the rotations of daydream cam
            rotations.Enqueue(gearCamera.rotation.eulerAngles);
        }
    }
}