using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class neuralPositionTracker : MonoBehaviour
{
    int count = 0;
    List<List<Transform>> positions = new List<List<Transform>>();

    //For point system
    public bool completedRow;
    int numberOfCompletedRows;
    public int rowsForPoint;

    public void SetPositions(List<List<Transform>> newPositions)
    {
        for (int j = 0; j < newPositions.Count; j++)
        {
            for (int i = 0; i < newPositions[j].Count; i++)
            {
                positions[j].Add(newPositions[j][i]);
            }
        }

    }

    public List<List<Transform>> GetPositions()
    {
        return positions;
    }

    private void Update()
    {
        completedRow = false;
        numberOfCompletedRows = 0;

        float yRow = 0.5f;

        for (int j = 0; j < positions.Count; j++)
        {
            while (yRow != 21.5f)
            {
                int gg = 0;
                count = 0;
                for (int i = 0; i < positions[j].Count; i++)
                {
                    if (RoundPosition(positions[j][i].position).y >= 20)
                    {
                        Debug.Log("GAMEOVER!");
                        SceneManager.LoadScene("Menu");
                    }
                    if (RoundPosition(positions[j][i].position).y == yRow)
                    {
                        count++;
                        //Debug.Log("count: " + count + " row: " + yRow + " gg: " + gg);
                    }
                }
                if (count >= 10)
                {
                    completedRow = true;
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
                    numberOfCompletedRows++;
                }
                yRow++;

                rowsForPoint = numberOfCompletedRows;
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

    public int RecordForPoints()
    {
        return numberOfCompletedRows;
    }
}
