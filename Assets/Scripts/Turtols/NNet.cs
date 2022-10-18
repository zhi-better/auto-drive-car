using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using MathNet.Numerics.LinearAlgebra;
using System;

using Random = UnityEngine.Random;

public class NNet : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness = 0.0f;

    private int numberHiddenLayers = 0;
    private int numberHiddenNeurons = 0;

    public bool LoadNetworkFile(string fileName)
    {
        if (!File.Exists(Application.streamingAssetsPath + "/" + fileName))
            return false;
        else
        {
            Debug.Log("model load successfully.");
        }

        string[] textData = File.ReadAllLines(Application.streamingAssetsPath + "/" + fileName);

        if (textData.Length == 0)
        {
            Debug.Log("read network parameters error, could not find the data file.");
            return false;
        }

        //add the first input to hidden layer weight
        int index = 1;
        string[] strhiddenToHidden;
        for (int i = 0; i < numberHiddenLayers + 2; i++)
        {
            for (int x = 0; x < (i == 0 ? 3 : numberHiddenNeurons); x++)
            {
                strhiddenToHidden = textData[index].Split(' ');
                for (int y = 0; y < numberHiddenNeurons; y++)
                {
                    weights[i][x, y] = float.Parse(strhiddenToHidden[y]);
                }
                index++;
            }
            index++;
        }

        //read the output weights
        string[] strOutputs;
        for (int x = 0; x < numberHiddenNeurons; x++)
        {
            strOutputs = textData[index].Split(' ');
            for (int y = 0; y < 2; y++)
            {
                weights[numberHiddenLayers + 2][x, y] = float.Parse(strOutputs[y]);
            }
            index++;
        }

        //read the biases data
        index += 1;
        string[] strBiases = textData[index].Split(' ');
        for (int i = 0; i < numberHiddenLayers + 2; i++)
        {
            biases[i] = float.Parse(strBiases[i]);
        }

        return true;
    }

    public void SaveNetworkToFile(string fileName)
    {
        string strData = "";
        for (int i = 0; i < weights.Count; i++)
        {
            strData += "matrix\n";
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    strData = strData + weights[i][x, y] + " ";
                }
                strData += "\n";
            }
        }
        strData += "biases data:\n";
        for (int i = 0; i < biases.Count; i++)
        {
            strData = strData + biases[i].ToString() + " ";
        }

        if (!File.Exists(Application.streamingAssetsPath + "/" + fileName))
            File.CreateText(Application.streamingAssetsPath + "/" + fileName);

        File.WriteAllText(Application.streamingAssetsPath + "/" + fileName, strData);
    }

    public NNet InitialiseCopy(int hiddenLayersCount, int hiddenNeuronCount)
    {
        NNet net = new NNet();
        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }
            newWeights.Add(currentWeight);
        }

        List<float> newBiases = new List<float>();
        newBiases.AddRange(biases);

        net.weights = newWeights;
        net.biases = newBiases;

        net.InitialiseHidden(hiddenLayersCount, hiddenNeuronCount);

        return net;
    }

    private void InitialiseHidden(int hiddenLayersCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        //here we meet the first bug which makes there is no more memory could be get
        for (int i = 0; i < hiddenLayersCount + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }

    }

    public void Initialise(int hiddenLayersCount, int hiddenNeuronCount)
    {
        numberHiddenLayers = hiddenLayersCount;
        numberHiddenNeurons = hiddenNeuronCount;

        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        //add the first input to hidden layer weight
        Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);
        weights.Add(inputToH1);
        //add another weights
        for (int i = 0; i < hiddenLayersCount + 1; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(f);

            biases.Add(Random.Range(-1f, 1f));

            //weights
            Matrix<float> hiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(hiddenToHidden);
        }
        //add the output weight
        Matrix<float> outputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(outputWeight);

        biases.Add(Random.Range(-1f, 1f));

        RandomiseWeights();
    }

    public void RandomiseWeights()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public (float, float) RunNetwork(float a, float b, float c)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        inputLayer = inputLayer.PointwiseTanh();    //before input to the network, first perform tanh operation on the input data
        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        for (int i = 0; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i] * weights[i + 1]) + biases[i]).PointwiseTanh();
        }

        outputLayer = 
            (hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count-1] + biases[biases.Count-1]).PointwiseTanh();

        return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0, 1]));
    }

    private float Sigmoid(float s)
    {
        return 1 / (1 + Mathf.Exp(-s));
    }
}
