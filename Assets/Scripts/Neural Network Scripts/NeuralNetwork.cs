﻿using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.IO;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;//layers
    private float[][] neurons;//neurons
    public float[][] biases;//biasses
    private float[][][] weights;//weights
    private int[] activations;//layers

    public float fitness = 0;//fitness

    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        // layers is [200, 300, 4] here
        InitNeurons();
        InitBiases();
        InitWeights();
    }

    private void InitNeurons()//create empty storage array for the neurons in the network.
    {
        // Creates list of float arrays.
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            // adds a float array with the size of layer
            neuronsList.Add(new float[layers[i]]);
        }
        // neuronlist is a list of three float arrays with the size of 200, 300 and 4
        neurons = neuronsList.ToArray();
    }

    private void InitBiases()//initializes and populates array for the biases being held within the network.
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();

        // UnityEngine.Debug.Log("initbiases" + biases[0][0]);
    }

    private void InitWeights()//initializes random array for the weights being held in the network.
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //float sd = 1f / ((neurons[i].Length + neuronsInPreviousLayer) / 2f);
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();

        // UnityEngine.Debug.Log("initwrights" + weights[0][0][0]);
    }

    public float[] FeedForward(float[] inputs)//feed forward, inputs >==> outputs.
    {
        //var bla = neurons[0][100];
        for (int i = 0; i < inputs.Length; i++)
        {
            //Puts in the inputs in the first layer of neurons
            neurons[0][i] = inputs[i];
        }
        //for loop from hidden layer to output layer
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            //loops the amount of neurons in layer
            for (int j = 0; j < neurons[i].Length; j++)
            {
                // UnityEngine.Debug.Log("neurons layer" + i + " " + neurons[i].Length);
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++) // this loops so many times as la
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                neurons[i][j] = Activate(value + biases[i][j]);
            }
        }

        return RoundResults(neurons[neurons.Length - 1]);
        //return neurons[neurons.Length - 1];
    }

    public float[] RoundResults(float[] rawOutputs)
    {
        float[] rounded = new float[rawOutputs.Length];

        for (int i = 0; i < rawOutputs.Length; i++)
        {
            if (rawOutputs[i] > 0)
            {
                rounded[i] = 1;
            }
            else
            {
                rounded[i] = 0;
            }
        }
        return rounded;
    }

    public float Activate(float value)
    {
        return (float)Math.Tanh(value);
    }

    public void Mutate(int chance, float val)//used as a simple mutation function for any genetic implementations.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, chance) <= 5) ? biases[i][j] += UnityEngine.Random.Range(-val, val) : biases[i][j];

            }
        }
        // UnityEngine.Debug.Log("mutating " + biases[0][0]);

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, chance) <= 5) ? weights[i][j][k] += UnityEngine.Random.Range(-val, val) : weights[i][j][k];
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other) //Comparing For NeuralNetworks performance.
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }

    public NeuralNetwork Copy(NeuralNetwork nn) //For creatinga deep copy, to ensure arrays are serialzed.
    {
        // UnityEngine.Debug.Log("biasesFeedForward before" + biases[0][0]);
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                nn.biases[i][j] = biases[i][j];
            }
        }
        // UnityEngine.Debug.Log("biasesFeedForward after" + biases[0][0]);
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    nn.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return nn;
    }

    public void Load(string path)//this loads the biases and weights from within a file into the neural network.
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]);
                        index++;
                    }
                }
            }
        }
    }

    float[][] GetBiases()
    {
        return biases;
    }

    public void Save(string path)//this is used for saving the biases and weights within the network to a file.
    {
        float[][] realBiases = GetBiases();
        // UnityEngine.Debug.Log("burhhhhhhh");
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        // UnityEngine.Debug.Log("savebias" + biases[0][0] + " realbias" + realBiases[0][0]);
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}
