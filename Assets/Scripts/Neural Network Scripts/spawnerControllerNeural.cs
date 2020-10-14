using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnerControllerNeural : MonoBehaviour
{
    public List<GameObject> tetriminos;
    public List<Vector2> spawnPos;
    public bool isFalling = false;
    public Bot currentTetrimino;
    int tetrimino = 0;
    public bool HaveHold = false;
    public bool BackFromHold = false;
    public bool JustMadeHold = false;
    public GameObject tetriminoInHold;
    public int tetriminoInHoldInt;
    public int populationSize;
    private List<Bot> bots = new List<Bot>();
    public List<NeuralNetwork> networks;
    public int[] layers = new int[3] { 5, 3, 2 };//initializing network to the right size

    public Quaternion originalRotationValue;

    // Start is called before the first frame update
    void Start()
    {
        originalRotationValue = transform.rotation;

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

        InitNetworks();

        for (int i = 0; i < populationSize; i++)
        {
            assignNextObjs();
            currentTetrimino = Instantiate(tetriminos[tetrimino], spawnPos[tetrimino], new Quaternion(0, 0, 0, 0)).GetComponent<Bot>();//create botes

            //Debug.Log($"spawn position: {spawnPos[tetrimino]}, tetrimino: {currentTetrimino}");

            //Bot car = (Instantiate(prefab, new Vector3(0, 1.6f, -16), ).GetComponent<Bot>();
            // Debug.Log(currentTetrimino.network);
            currentTetrimino.network = networks[i];//deploys network to each learner
            bots.Add(currentTetrimino);
        }

    }

    // Update is called once per frame
    void Update()
    {
        // if (!isFalling)
        // {
        //     assignNextObjs();
        //     currentTetrimino = Instantiate(tetriminos[tetrimino], spawnPos[tetrimino], new Quaternion(0, 0, 0, 0)).GetComponent<Bot>();
        //     //Debug.Log($"spawn position: {spawnPos[tetrimino]}, tetrimino: {currentTetrimino}");
        //     JustMadeHold = false;
        // }
        //isFalling = currentTetrimino.GetComponent<playerController2>().isActiveAndEnabled;
    }

    void assignNextObjs()
    {
        if (tetrimino == 0)
        {
            int random = Random.Range(0, 2);
            tetriminos.Sort((a, b) => 1 - 2 * random);
            spawnPos.Sort((a, b) => 1 - 2 * random);
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
