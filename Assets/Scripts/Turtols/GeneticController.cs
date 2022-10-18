using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticController : MonoBehaviour
{
    [Header("Reference")]
    public CarController controller;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0f, 1f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new List<int>();
    private int naturallySelected;

    private NNet[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;


    private void OnGUI()
    {
        string strToShow = "—µ¡∑–≈œ¢£∫\n";
        strToShow = strToShow +
            "current generation: " +
            currentGeneration +"\n"+
            "current genome: "+
            currentGenome + "\n"+
            "fitness: " + controller.overallFitness;

        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.black;
        GUI.Label(new Rect(Screen.width - 185, 10, 180, 120), strToShow, style);
    }

    // Start is called before the first frame update
    void Start()
    {
        //CreatePopulation();
    }

    public void CreatePopulation()
    {
        currentGeneration = 0;
        currentGenome = 0;

        population = new NNet[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    private void FillPopulationWithRandomValues(NNet[] newPopulation, int startIndex)
    {
        //because some of the networks has already been crossed over
        while (startIndex < initialPopulation)
        {
            newPopulation[startIndex] = new NNet();
            newPopulation[startIndex].Initialise(controller.LAYERS, controller.NEURONS);
            startIndex++;
        }
    }

    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }

    public void Death(float fitness, NNet network)
    {
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            Repopulation();
        }
    }

    private void Repopulation()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();
        NNet[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);
        population = newPopulation;
        currentGenome = 0;
        ResetToCurrentGenome();
    }

    public void Crossover(NNet[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i+=2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count > 1)
            {
                // why here use 100
                for (int j = 0; j < 100; j++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];
                    if (AIndex != BIndex)
                    {
                        break;
                    }
                }
            }

            NNet child1 = new NNet();
            NNet child2 = new NNet();
            child1.Initialise(controller.LAYERS, controller.NEURONS);
            child2.Initialise(controller.LAYERS, controller.NEURONS);

            child1.fitness = 0;
            child2.fitness = 0;

            for (int w = 0; w < child1.weights.Count; w++)
            {
                if (Random.Range(0f,1f)>0.5f)
                {
                    child1.weights[w] = population[AIndex].weights[w];
                    child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    child1.weights[w] = population[BIndex].weights[w];
                    child2.weights[w] = population[AIndex].weights[w];
                }
            }

            for (int w = 0; w < child1.biases.Count; w++)
            {
                if (Random.Range(0, 1) > 0.5f)
                {
                    child1.biases[w] = population[AIndex].biases[w];
                    child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    child1.biases[w] = population[BIndex].biases[w];
                    child2.biases[w] = population[AIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = child1;
            naturallySelected++;
            newPopulation[naturallySelected] = child2;
            naturallySelected++;
        }

    }

    private Matrix<float> MutateMatrix(Matrix<float> A)
    {
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;
        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1, 1), -1, 1);
        }

        return C;
    }

    public void Mutate(NNet[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if (Random.Range(0,1) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
        }

    }

    private NNet[] PickBestPopulation()
    {
        NNet[] newPopulation = new NNet[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);
            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }
        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);
            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }
        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }

}
