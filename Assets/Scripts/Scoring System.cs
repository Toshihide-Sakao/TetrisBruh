using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject currentTetrimino;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float Inputy = Input.GetAxisRaw("Vertical");
        int score = 0;

        float currentTetriminoY = currentTetrimino.transform.position.y;
    }
    
    int hardDropScore() 
    {
        float currentTetriminoY = currentTetrimino.transform.position.y;
        currentTetriminoY += 10.5f;

        Debug.Log(currentTetriminoY);


        return (int)currentTetriminoY;
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
