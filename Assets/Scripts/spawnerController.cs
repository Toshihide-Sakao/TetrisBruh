using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnerController : MonoBehaviour
{
    public List<GameObject> tetriminos;
    public List<Vector2> spawnPos;
    public bool isFalling = false;
    public GameObject currentTetrimino;
    int tetrimino = 0;

    // Start is called before the first frame update
    void Start()
    {

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

    }

    // Update is called once per frame
    void Update()
    {

        if (!isFalling)
        {
            assignNextObjs();
            currentTetrimino = Instantiate(tetriminos[tetrimino]);
            currentTetrimino.transform.position = spawnPos[tetrimino];

            // foreach (var item in tetriminos)
            // {
            //     Debug.Log(item);
            // }
        }

        isFalling = currentTetrimino.GetComponent<playerController2>().isActiveAndEnabled;
    }

    void assignNextObjs()
    {
        if (tetrimino == 0)
        {
            int random = Random.Range(0, 2);
            tetriminos.Sort((a, b)=> 1 - 2 * random);
            spawnPos.Sort((a, b)=> 1 - 2 * random);
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

}
