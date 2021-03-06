﻿using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnerControllerNeural : MonoBehaviour
{
    public List<GameObject> tetriminos;
    List<Vector2> spawnPos;
    //public bool isFalling = false;
    public List<GameObject> currentTetrimino;
    public List<int> tetrimino;
    public bool HaveHold = false;
    public bool BackFromHold = false;
    public bool JustMadeHold = false;
    public GameObject tetriminoInHold;
    public int tetriminoInHoldInt;
    public int populationSize;
    public bool[] isFalling;
    public int showingIndex = 0;
    //private List<Bot> bots = new List<Bot>();
    public List<NeuralNetwork> networks;
    int[] layers = new int[3] { 213, 160, 4 };//initializing network to the right size

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;
    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    public Quaternion originalRotationValue;

    bool CreateBots2HasBeenCalled = false;


    static int janneI = 0;

    // Start is called before the first frame update
    void Start()
    {
        originalRotationValue = transform.rotation;

        // tetriminos = new List<GameObject>()
        // {
        //     GameObject.Find("I TetriminoN"),
        //     GameObject.Find("J TetriminoN"),
        //     GameObject.Find("L TetriminoN"),
        //     GameObject.Find("O TetriminoN"),
        //     GameObject.Find("S TetriminoN"),
        //     GameObject.Find("T TetriminoN"),
        //     GameObject.Find("Z TetriminoN"),
        // };

        spawnPos = new List<Vector2>()
        {
            new Vector2(5f, 20f), //I
            new Vector2(5f, 20f), //J
            new Vector2(5f, 20f), //L
            new Vector2(6f, 20f), //O
            new Vector2(5f, 20f), //S
            new Vector2(5f, 19.5f), //T
            new Vector2(5f, 20f)  //Z
        };

        for (int i = 0; i < populationSize; i++)
        {
            tetrimino.Add(0);
        }

        isFalling = new bool[populationSize];

        InitNetworks();
        CreateBots();
    }

    public void CreateBots()
    {
        

        GameObject scriptReader = GameObject.Find("scriptReader");
        scriptReader.GetComponent<neuralPositionTracker>().InitiatePosition(populationSize);
        List<List<Transform>> populationList = new List<List<Transform>>();
        for (int j = 0; j < populationSize; j++)
        {
            populationList.Add(new List<Transform>());
        }

        for (int i = 0; i < populationSize; i++)
        {
            // assignNextObjs();
            // UnityEngine.Debug.Log("okkkkkkk l" + i + " "+ tetrimino[i]);
            currentTetrimino.Add(Instantiate(tetriminos[tetrimino[i]], spawnPos[tetrimino[i]], new Quaternion(0, 0, 0, 0)));//create botes

            //Debug.Log($"spawn position: {spawnPos[tetrimino]}, tetrimino: {currentTetrimino}");

            //Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), ).GetComponent<Bot>();
            // Debug.Log(currentTetrimino.network);
            currentTetrimino[i].GetComponent<neuralController>().network = networks[i];//deploys network to each learner
            //bots.Add(currentTetrimino);

            isFalling[i] = currentTetrimino[i].GetComponent<neuralController>().isActiveAndEnabled;
            currentTetrimino[i].GetComponent<neuralController>().index = i;

            //Debug.Log(populationList.Count);
        }
        scriptReader.GetComponent<neuralPositionTracker>().SetPositions(populationList);
    }

    public void CreateBots2()
    {
        GameObject scriptReader = GameObject.Find("scriptReader");
        scriptReader.GetComponent<neuralPositionTracker>().InitiatePosition(populationSize);
        List<List<Transform>> populationList = new List<List<Transform>>();
        for (int j = 0; j < populationSize; j++)
        {
            populationList.Add(new List<Transform>());
        }

        // for (int i = 0; i < populationSize; i++)
        // {
        //     assignNextObjs();

        //     isFalling[i] = currentTetrimino.GetComponent<neuralController>().isActiveAndEnabled;
        //     currentTetrimino.GetComponent<neuralController>().index = i;

        //     //Debug.Log(populationList.Count);
        // }
        CreateBots2HasBeenCalled = true;
        scriptReader.GetComponent<neuralPositionTracker>().SetPositions(populationList);
    }
    // Update is called once per frame
    void Update()
    {
        // for (int i = 0; i < populationSize; i++)
        // {
        //     assignNextObjs();
        //     currentTetrimino = Instantiate(tetriminos[tetrimino], spawnPos[tetrimino], new Quaternion(0, 0, 0, 0));
        //     //JustMadeHold = false;
        // }
    }

    public void SpawnNewTetrimino(int index)
    {
        assignNextObjs(index);
        currentTetrimino[index] = Instantiate(tetriminos[tetrimino[index]], spawnPos[tetrimino[index]], new Quaternion(0, 0, 0, 0));
        currentTetrimino[index].GetComponent<neuralController>().network = networks[index];
        currentTetrimino[index].GetComponent<neuralController>().index = index;

        // UnityEngine.Debug.Log("spawned new tetrimino for: " + index + "  the index: " + tetrimino[index]);
    }

    public void CheckStoppedTetrimino(int index) 
    {
        if (currentTetrimino[index].GetComponent<neuralController>().allahHelpMePls == true)
        {
            SpawnNewTetrimino(index);
        }
    }

    public void SpawnNewTetriminoWhenGameOver(int index)
    {
        tetrimino[index] = 0;
        currentTetrimino[index] = Instantiate(tetriminos[tetrimino[index]], spawnPos[tetrimino[index]], new Quaternion(0, 0, 0, 0));
        currentTetrimino[index].GetComponent<neuralController>().network = networks[index];
        currentTetrimino[index].GetComponent<neuralController>().index = index;
        // UnityEngine.Debug.Log("deployed network ");

        // UnityEngine.Debug.Log("spawned new tetrimino for: " + index + "  the index: " + currentTetrimino[index].GetComponent<neuralController>().index);
    }

    void assignNextObjs(int i)
    {
            if (tetrimino[i] == 6)
            {
                tetrimino[i] = 0;
            }
            else
            {
                tetrimino[i]++;
                // UnityEngine.Debug.Log("tetrimino is added " + i + " " + tetrimino[i]);
            }
    }

    void updateNextObjs(int[] nextObjs)
    {
        nextObjs[0] = nextObjs[1];
        nextObjs[1] = nextObjs[2];
        nextObjs[2] = Random.Range(0, 7);
    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Save.txt");//on start load the network save
            networks.Add(net);
        }
    }

    public void SortNetworks()
    {
        // for (int i = 0; i < populationSize; i++)
        // {
        //     networks[i].UpdateFitness(); //gets bots to set their corrosponding networks fitness
        // }
        networks.Sort();
        // UnityEngine.Debug.Log("populationsioze " + populationSize);
        networks[populationSize - 1].Save("Assets/Save.txt"); // saves networks weights and biases to file, to preserve network performance
        // UnityEngine.Debug.Log("best bias 00" + networks[populationSize - 1].biases[0][0]);
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].Copy(new NeuralNetwork(layers));
            
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }

        janneI++;
    }

    // public void HoldTetrimino()
    // {
    //     if (!JustMadeHold)
    //     {
    //         if (!HaveHold)
    //         {
    //             currentTetrimino.transform.position = new Vector3(-2, 15, 0);
    //             currentTetrimino.transform.rotation = originalRotationValue;

    //             tetriminoInHold = currentTetrimino;
    //             tetriminoInHoldInt = tetrimino;

    //             HaveHold = true;
    //             tetriminoInHold.GetComponent<playerController2>().enabled = false;

    //             Debug.Log("First Hold");
    //         }
    //         else if (HaveHold)
    //         {
    //             SwitchGameObjects();
    //             SwitchInts();

    //             tetriminoInHold.transform.position = new Vector3(-2, 15, 0);
    //             tetriminoInHold.transform.rotation = originalRotationValue;

    //             currentTetrimino.transform.position = spawnPos[tetrimino];
    //             currentTetrimino.GetComponent<playerController2>().enabled = true;
    //             tetriminoInHold.GetComponent<playerController2>().enabled = false;
    //         }
    //         JustMadeHold = true;
    //     }

    // }

    // void SwitchGameObjects()
    // {
    //     GameObject a2 = currentTetrimino;
    //     GameObject b2 = tetriminoInHold;

    //     currentTetrimino = b2;
    //     tetriminoInHold = a2;
    // }

    // void SwitchInts()
    // {
    //     int a2 = tetrimino;
    //     int b2 = tetriminoInHoldInt;

    //     tetrimino = b2;
    //     tetriminoInHoldInt = a2;
    // }

}
