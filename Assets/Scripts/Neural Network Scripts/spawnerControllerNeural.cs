using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnerControllerNeural : MonoBehaviour
{
    public List<GameObject> tetriminos;
    public List<Vector2> spawnPos;
    //public bool isFalling = false;
    public GameObject currentTetrimino;
    int tetrimino = 0;
    public bool HaveHold = false;
    public bool BackFromHold = false;
    public bool JustMadeHold = false;
    public GameObject tetriminoInHold;
    public int tetriminoInHoldInt;
    public int populationSize;
    public bool[] isFalling;
    private List<Bot> bots = new List<Bot>();
    public List<NeuralNetwork> networks;
    public int[] layers = new int[3] { 5, 3, 2 };//initializing network to the right size
    GameObject scriptReader;

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;
    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    public Quaternion originalRotationValue;

    // Start is called before the first frame update
    void Start()
    {
        scriptReader = GameObject.Find("scriptReader");
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
            new Vector2(5f, 19), //I
            new Vector2(5f, 19), //J
            new Vector2(5f, 19), //L
            new Vector2(6f, 19f), //O
            new Vector2(5f, 19), //S
            new Vector2(5f, 18.5f), //T
            new Vector2(5f, 19f)  //Z
        };

        isFalling = new bool[populationSize];

        InitNetworks();
        CreateBots();
    }

    public void CreateBots()
    {
        scriptReader.GetComponent<neuralPositionTracker>().InitiatePosition(populationSize);
        List<List<Transform>> populationList = new List<List<Transform>>();
        for (int j = 0; j < populationSize; j++)
        {
            populationList.Add(new List<Transform>());
        }

        for (int i = 0; i < populationSize; i++)
        {
            assignNextObjs();
            currentTetrimino = Instantiate(tetriminos[tetrimino], spawnPos[tetrimino], new Quaternion(0, 0, 0, 0));//create botes

            //Debug.Log($"spawn position: {spawnPos[tetrimino]}, tetrimino: {currentTetrimino}");

            //Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), ).GetComponent<Bot>();
            // Debug.Log(currentTetrimino.network);
            //currentTetrimino.network = networks[i];//deploys network to each learner
            //bots.Add(currentTetrimino);

            isFalling[i] = currentTetrimino.GetComponent<neuralController>().isActiveAndEnabled;
            currentTetrimino.GetComponent<neuralController>().index = i;

            //Debug.Log(populationList.Count);
        }
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
        assignNextObjs();
        //Debug.Log(spawnPos);
        //Debug.Log(tetriminos);
        currentTetrimino = Instantiate(tetriminos[tetrimino], spawnPos[tetrimino], new Quaternion(0, 0, 0, 0));
        currentTetrimino.GetComponent<neuralController>().index = index;
    }

    void assignNextObjs()
    {
        if (tetrimino == 0)
        {
            // int random = Random.Range(0, 2);
            // tetriminos.Sort((a, b) => 1 - 2 * random);
            // spawnPos.Sort((a, b) => 1 - 2 * random);
        }
        if (tetrimino == 6)
        {
            tetrimino = 0;
        }
        else
        {
            tetrimino++;
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
        for (int i = 0; i < populationSize; i++)
        {
            bots[i].UpdateFitness();//gets bots to set their corrosponding networks fitness
        }
        networks.Sort();
        networks[populationSize - 1].Save("Assets/Save.txt");//saves networks weights and biases to file, to preserve network performance
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
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
