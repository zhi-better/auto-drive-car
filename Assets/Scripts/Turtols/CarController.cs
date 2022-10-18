using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    public enum carState
    {
        Resting = 0,
        Training = 1,
        Testing = 2
    }

    //public string networkFileName;
    public Vector3 startPosition, startRotation;
    public NNet network;
    public carState runningState = carState.Resting;

    [Range(-1.0f, 1.0f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultipler = 0.2f;
    public float sensorMultipler = 0.1f;

    [Header("network options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    [Space(50)]
    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    public float avgSpeed;

    public float lSensor, fSensor, rSensor;

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticController>().Death(overallFitness, network);
        Reset();
    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
        runningState = carState.Training;
    }

    // Start is called before the first frame update
    void Start()
    {
//         startPosition = transform.position;
//         startRotation = transform.eulerAngles;
        startPosition = new Vector3(-1.89f,0.74f,0);
        startRotation = Vector3.zero;

        //the GeneticController will set the NNet, so it here will reset the network to a uninitialized matrix
        //network = GetComponent<NNet>();

        //read the network which is already trained
        //network.SaveNetworkToFile(networkFileName);
        //network.SaveNetworkToFile(networkFileName);

        //TEST CODE
        //network.Initialise(LAYERS, NEURONS);
    }

    public void Reset()
    {
        //TEST CODE
        //network.Initialise(LAYERS, NEURONS);

        timeSinceStart = 0;
        overallFitness = 0;
        totalDistanceTravelled = 0;
        avgSpeed = 0;

        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private Vector3 inp;
    public void MoveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, a * transform.forward * 11.4f, 0.02f);

        transform.position += inp;
        transform.eulerAngles += new Vector3(0, h * 90 * 0.02f, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Reset();
        Death();
    }

    private void InputSensors()
    {
        Vector3 a, b, c;
        a = transform.forward - transform.right;
        b = transform.forward;
        c = transform.forward + transform.right;

        Ray ray = new Ray(transform.position, a);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            lSensor = hit.distance / 25;
            //Debug.Log("left sensor distance:" + lSensor);
            //Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        ray.direction = b;
        if (Physics.Raycast(ray, out hit))
        {
            fSensor = hit.distance / 25;
            //Debug.Log("forward sensor distance:" + lSensor);
            //Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        ray.direction = c;
        if (Physics.Raycast(ray, out hit))
        {
            rSensor = hit.distance / 25;
            //Debug.Log("right sensor distance:" + lSensor);
            //Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness =
            totalDistanceTravelled * distanceMultipler +
            avgSpeed * avgSpeedMultipler +
            (lSensor + fSensor + rSensor) / 3;

        if (timeSinceStart > 20 && overallFitness <40)
        {
            //Reset();
            Death();
        }
        
        if (overallFitness > 1000)
        {
            //save the network to a json file
            if (runningState != carState.Testing)
            {
                network.SaveNetworkToFile(DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".txt");
                Death();
            }
        }
    }

    private void FixedUpdate()
    {
        if (runningState != carState.Resting)
        {
            InputSensors();
            lastPosition = transform.position;

            //neutral network here
            (a, t) = network.RunNetwork(lSensor, fSensor, rSensor);

            MoveCar(a, t);

            timeSinceStart += Time.fixedDeltaTime;

            CalculateFitness();
            //         a = 0;
            //         t = 0;
        }
    }

}
