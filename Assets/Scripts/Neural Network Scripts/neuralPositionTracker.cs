using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class neuralPositionTracker : MonoBehaviour
{
    int count = 0;
    static List<List<Transform>> positions = new List<List<Transform>>();
    static bool[] gameOvers;
    static spawnerControllerNeural spawnerController = new spawnerControllerNeural();
    //For point system
    public static bool[] completedRow;
    static int[] numberOfCompletedRows;
    public static int[] rowsForPoint;

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

    public void SetPositions(List<List<Transform>> newPositions)
    {
        for (int j = 0; j < newPositions.Count; j++)
        {
            for (int i = 0; i < newPositions[j].Count; i++)
            {
                positions[j].Add(newPositions[j][i]);
            }
        }
        for (int i = 0; i < positions.Count; i++)
        {

        }
        //Debug.Log("okkk" + positions.Count);
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

            positions[index].Add(newPositions[i]);
        }
    }


    public List<List<Transform>> GetPositions()
    {
        return positions;
    }

    private void Update()
    {


        for (int j = 0; j < positions.Count; j++)
        {
            completedRow[j] = false;
            numberOfCompletedRows[j] = 0;

            float yRow = 0.5f;

            while (yRow != 21.5f)
            {
                int gg = 0;
                count = 0;
                for (int i = 0; i < positions[j].Count; i++)
                {
                    if (gameOvers.All(x => x))
                    {
                        for (int a = 0; a < positions.Count; a++)
                        {
                            for (int b = 0; b < positions[a].Count; b++)
                            {
                                GameObject.Destroy(positions[a][b].gameObject);//if there are Prefabs in the scene this will get rid of them
                            }
                        }
                        spawnerController.SortNetworks();
                        spawnerController.CreateBots();
                    }
                    if (RoundPosition(positions[j][i].position).y >= 20)
                    {
                        Debug.Log("GAMEOVER! for " + j);
                        gameOvers[j] = true;
                    }
                    if (RoundPosition(positions[j][i].position).y == yRow)
                    {
                        count++;
                        //Debug.Log("count: " + count + " row: " + yRow + " gg: " + gg);
                    }
                }
                if (count >= 10)
                {
                    completedRow[j] = true;
                    Debug.Log("Row complete, row: " + yRow);
                    for (int i = 0; i < positions[j].Count; i++)
                    {
                        if (RoundPosition(positions[j][i].position).y == yRow)
                        {
                            //Debug.Log("removing row: " + yRow);
                            Destroy(positions[j][i].gameObject);
                        }
                    }
                    while (gg != positions.Count)
                    {
                        Debug.Log("gg: " + gg);
                        Debug.Log("j: " + j);
                        Debug.Log("position count: " + positions.Count);
                        Debug.Log("positon j.count: " + positions[j].Count);
                        
                        //Debug.Log("gg is going hard");
                        for (int i = 0; i < positions[j].Count; i++)
                        {
                            if (RoundPosition(positions[j][i].position).y == yRow)
                            {
                                //Debug.Log("removing list" + " y positions:" + yRow + "  y: " + RoundPosition(positions[i].position).y);
                                positions.RemoveAt(i);
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
    }

    Vector3 RoundPosition(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x * 10) / 10f;
        pos.y = Mathf.Round(pos.y * 10) / 10f;
        pos.z = Mathf.Round(pos.z * 10) / 10f;

        return pos;
    }

}
