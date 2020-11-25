using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class neuralPositionTracker : MonoBehaviour
{
    int[] count;
    static List<List<Transform>> positions = new List<List<Transform>>();
    static bool[] gameOvers;
    public bool[] completedRow;
    static int[] numberOfCompletedRows;
    public int[] rowsForPoint;
    List<int[]> positions1D = new List<int[]>();

    public bool allGameover = false;

    private void Start()
    {
    }

    public void InitiatePosition(int population)
    {
        positions = new List<List<Transform>>();
        for (int i = 0; i < population; i++)
        {
            positions.Add(new List<Transform>());
        }


        gameOvers = new bool[positions.Count];

        //for scoring ---------------------------
        completedRow = new bool[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            completedRow[i] = false;
        }

        numberOfCompletedRows = new int[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            numberOfCompletedRows[i] = 0;
        }

        rowsForPoint = new int[positions.Count];
        //------------------------------------------

        // Debug.Log("initiate running");
        // Debug.Log(positions.Count);
    }

    public void SetPositions1D(List<List<Transform>> positions)
    {
        // for (int i = 0; i < positions.Count; i++)
        // {
        //     Debug.Log("positions1d.count: " + positions1D.Count);
        //     List<int> okkys = new List<int>();
        //     positions1D.Add(okkys);
        // }

        for (int index = 0; index < positions.Count; index++)
        {
            positions1D.Add(new int[200]);
            int counter = 0;
            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int i = 0; i < positions[index].Count; i++)
                    {
                        if (positions[index][i].position == new Vector3((float)x, (float)y, 0))
                        {
                            positions1D[index][counter] = 1;
                        }
                        else
                        {
                            positions1D[index][counter] = 0;
                        }
                    }
                }
            }
        }

        //Debug.Log("in setpositions count" + positions1D.Count);

    }

    public List<int[]> GetPositions1D()
    {
        return positions1D;
    }

    public void SetPositions(List<List<Transform>> newPositions)
    {
        for (int j = 0; j < newPositions.Count; j++)
        {
            for (int i = 0; i < newPositions[j].Count; i++)
            {
                positions[j].Add(newPositions[j][i]);
            }
        }
        // Debug.Log("okkk" + positions.Count + " okkkkkkk " + newPositions.Count);
    }

    public void SetPositionsForIndex(List<Transform> newPositions, int index)
    {
        for (int i = 0; i < newPositions.Count; i++)
        {
            // Debug.Log("index " + index);
            // Debug.Log(positions.Count);
            // foreach (var item in positions)
            // {
            //     Debug.Log(item);
            // }
            // Debug.Log(positions[index]);
            if (positions.Any())
            {
                positions[index].Add(newPositions[i]);
            }
        }
    }


    public List<List<Transform>> GetPositions()
    {
        return positions;
    }

    public bool GetGameOver(int index)
    {
        return gameOvers[index];
    }

    public bool[] GetGameOvers()
    {
        return gameOvers;
    }

    private void Update()
    {
        Tracker();
    }

    Vector3 RoundPosition(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x * 10) / 10f;
        pos.y = Mathf.Round(pos.y * 10) / 10f;
        pos.z = Mathf.Round(pos.z * 10) / 10f;

        return pos;
    }

    void Tracker()
    {
        for (int j = 0; j < positions.Count; j++)
        {
            completedRow[j] = false;
            numberOfCompletedRows[j] = 0;
            count = new int[positions.Count];

            // For bug
            List<Vector3> countedPos = new List<Vector3>();

            float yRow = 0.5f;

            while (yRow != 22.5f)
            {
                int gg = 0;
                count[j] = 0;
                
                for (int i = 0; i < positions[j].Count; i++)
                {
                    if (RoundPosition(positions[j][i].position).y >= 19f)
                    {
                        Debug.Log("GAMEOVER! for " + j);
                        gameOvers[j] = true;

                        // if (gameOvers.All(x => x))
                        // {
                        //     allGameover = true;
                        // }
                    }
                    else if (RoundPosition(positions[j][i].position).y == yRow)
                    {
                        if (!countedPos.Contains(RoundPosition(positions[j][i].position)))
                        {
                            count[j]++;
                            countedPos.Add(RoundPosition(positions[j][i].position));
                        }
                        
                        //Debug.Log("count: " + count + " row: " + yRow + " gg: " + gg);
                    }
                }
                if (gameOvers.All(x => x))
                // if (gameOvers[0] && gameOvers[1] && gameOvers[2] && gameOvers[3] && gameOvers[4] && gameOvers[5])
                // if (gameOvers[0] && gameOvers[1])
                {
                    Debug.Log("gameover for all");
                    for (int a = 0; a < positions.Count; a++)
                    {
                        for (int b = 0; b < positions[a].Count; b++)
                        {
                            GameObject.Destroy(positions[a][b].gameObject);//if there are Prefabs in the scene this will get rid of them
                        }
                        positions[a].Clear();
                        Debug.Log("cleared positions");
                    }

                    GameObject.Find("Spawner").GetComponent<spawnerControllerNeural>().SortNetworks();
                    GameObject.Find("Spawner").GetComponent<spawnerControllerNeural>().CreateBots2();

                    Debug.Log("creat bots was called");

                    gameOvers = new bool[positions.Count];
                    allGameover = false;

                    return;
                }
                if (count[j] >= 10)
                {
                    completedRow[j] = true;
                    Debug.Log("Row complete, row: " + yRow);
                    for (int i = 0; i < positions[j].Count; i++)
                    {
                        if (RoundPosition(positions[j][i].position).y == yRow)
                        {
                            Debug.Log("removing row: " + yRow);
                            Destroy(positions[j][i].gameObject);
                        }
                    }
                    while (gg != positions[j].Count)
                    {
                        // Debug.Log("gg: " + gg);
                        // Debug.Log("j: " + j);
                        // Debug.Log("positon j.count: " + positions[j].Count);

                        //Debug.Log("gg is going hard");
                        for (int i = 0; i < positions[j].Count; i++)
                        {
                            if (RoundPosition(positions[j][i].position).y == yRow)
                            {
                                //Debug.Log("removing list" + " y positions:" + yRow + "  y: " + RoundPosition(positions[i].position).y);
                                positions[j].RemoveAt(i);
                                gg = 0;
                            }
                        }
                        gg++;
                    }
                    for (int i = 0; i < positions[j].Count; i++)
                    {
                        if (RoundPosition(positions[j][i].position).y > yRow)
                        {
                            //Debug.Log("moving");
                            positions[j][i].transform.position += Vector3.down;
                        }
                    }
                    numberOfCompletedRows[j]++;
                }
                yRow++;

                rowsForPoint[j] = numberOfCompletedRows[j];


            }
        }
        SetPositions1D(positions);
    }
}
