using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

public class UIEvents : MonoBehaviour
{
    public Dropdown dropdownItem;
    public Slider slider;
    public Text textTimeScale;
    public GameObject car;

    public void Quit()
    {
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void LoadModel()
    {
        if (dropdownItem.value != 0)
        {
            GetComponent<GeneticController>().CreatePopulation();
            CarController controller = car.GetComponent<CarController>();
            controller.Reset();
            controller.network.LoadNetworkFile(dropdownItem.captionText.text);
            controller.runningState = CarController.carState.Testing;
            Debug.Log("model load successfully");
        }
        else
            Debug.Log("please select your model first");
    }

    public void StartTraining()
    {
        Debug.Log("start training...");
        GetComponent<GeneticController>().CreatePopulation();
//         CarController controller = car.GetComponent<CarController>();
//         controller.Reset();
//         controller.runningState = CarController.carState.Training;
    }   
    public void StopTraining()
    {
        Debug.Log("stop training...");
        CarController controller = car.GetComponent<CarController>();
        controller.Reset();
        controller.runningState = CarController.carState.Resting;
    }
    public void sldTimeScale()
    {
        //Debug.Log("changing the time scale");
        Time.timeScale = slider.value;
        textTimeScale.text = String.Format("{0:N2}", slider.value) + "x";
    }

}
