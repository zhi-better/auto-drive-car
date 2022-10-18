using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform transTarget;

    private CarController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = transTarget.GetComponent<CarController>();
        transform.position = transTarget.position - transTarget.forward * 6 + transTarget.up * 2.7f;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (controller.runningState == CarController.carState.Testing)
            transform.position = Vector3.Lerp(transform.position, transTarget.position - transTarget.forward * 6 + transTarget.up * 2.7f, 0.02f);
        else
            transform.position = transTarget.position - transTarget.forward * 6 + transTarget.up * 2.7f;

        transform.LookAt(transTarget);
        //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, transTarget.eulerAngles + rotationDiff, 0.02f);
    }
    
}
