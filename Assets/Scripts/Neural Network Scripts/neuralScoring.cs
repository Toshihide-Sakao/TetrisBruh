using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class neuralScoring : MonoBehaviour
{
    public GameObject scriptReader;
    public int[] totalScore;
    public Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        scriptReader = GameObject.Find("scriptReader");

        int populationSize = GameObject.Find("Spawner").GetComponent<spawnerControllerNeural>().populationSize;
        totalScore = new int[populationSize];
    }

    // Update is called once per frame
    void Update()
    {
        bool[] completeRow = scriptReader.GetComponent<neuralPositionTracker>().completedRow;
        int[] rows = scriptReader.GetComponent<neuralPositionTracker>().rowsForPoint;

        for (int i = 0; i < completeRow.Length; i++)
        {
            if (completeRow[i])
            {
                totalScore[i] += lineScore(1, rows[i]);
            }
        }
        scoreText.text = "Score1: " + totalScore[0].ToString();

    }

    int hardDropScore()
    {
        return 0;
    }

    int lineScore(int level, int lines)
    {
        int score = level + 1;
        switch (lines)
        {
            case 1:
                score *= 40;
                break;
            case 2:
                score *= 100;
                break;
            case 3:
                score *= 300;
                break;
            case 4:
                score *= 1200;
                break;
        }

        return score;
    }
}
