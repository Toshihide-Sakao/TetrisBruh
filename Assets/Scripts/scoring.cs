using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scoring : MonoBehaviour
{
    public GameObject scriptReader;
    int totalScore = 0;
    public Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        scriptReader = GameObject.Find("scriptReader");
    }

    // Update is called once per frame
    void Update()
    {
        bool completeRow = scriptReader.GetComponent<positionTracker>().completedRow;
        int rows = scriptReader.GetComponent<positionTracker>().rowsForPoint;

        if (completeRow)
        {
            totalScore += lineScore(1, rows);
        }
        scoreText.text = "Score: " + totalScore.ToString();
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
