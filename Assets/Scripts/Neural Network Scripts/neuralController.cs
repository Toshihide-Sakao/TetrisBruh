﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class neuralController : MonoBehaviour
{
    float fallTimer;
    float rightTimer;
    float leftTimer;
    public float fitnessTimer;
    float rotateTimer;
    float neuralUpdateTimer;
    public float moveSpeed = 0.3f;
    float fallSpeed = 0.2f;
    float rotateSpeed = 0.3f;
    float OriginalFallSpeed;
    GameObject scriptReader;
    GameObject spawner;
    Vector3 rotationPoint = new Vector3(0, 0, 0);
    int width = 10;
    int rotateNumber = 0;
    public int index = 0;
    float timerTrigger = 0;
    bool brickBelow;
    bool brickLeft;
    bool brickRight;

    KeyCode right = KeyCode.RightArrow;
    KeyCode left = KeyCode.LeftArrow;
    KeyCode down = KeyCode.DownArrow;
    KeyCode rotateRight = KeyCode.UpArrow;
    KeyCode holdKey = KeyCode.C;

    public NeuralNetwork network;
    float[] outputs = new float[4];
    float[] input = new float[213];
    int[] layers = new int[3] { 213, 160, 4 };//initializing network to the right size


    public bool allahHelpMePls = false;


    bool hasEvaluatedFitness = false;


    // Start is called before the first frame update
    void Start()
    {
        //Find Script Reader for exporting positions
        scriptReader = GameObject.Find("scriptReader");

        //Find Spawner for holding
        spawner = GameObject.Find("Spawner");

        foreach (Transform child in transform)
        {
            if (index != spawner.GetComponent<spawnerControllerNeural>().showingIndex)
            {
                child.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        //Record fallspeed so it is reversable
        OriginalFallSpeed = fallSpeed;

        //Giving the right rotationpoint for this tetrimino
        rotationPoint = AllocateRotationPoint();
    }

    // Update is called once per frame
    void Update()
    {
        //Adds time to timer for falling
        fallTimer += Time.deltaTime * 2;
        rightTimer += Time.deltaTime * 2;
        leftTimer += Time.deltaTime * 2;
        neuralUpdateTimer += Time.deltaTime * 2;
        fitnessTimer += Time.deltaTime * 2;
        rotateTimer += Time.deltaTime;

        brickBelow = CheckCollisionY();
        brickLeft = CheckCollisionXLeft();
        brickRight = CheckCollisionXRight();

        FeedForward();

        //Methods for tetrimino
        Hold();
        Mover();
        Rotater();
        Faller();

        Time.timeScale = 4f;

    }

    void FeedForward()
    {
        if (neuralUpdateTimer > moveSpeed)
        {
            input = new float[213];
            List<Transform> currentPositionList = new List<Transform>();
            foreach (Transform children in transform)
            {
                children.transform.position = RoundPosition(children.transform.position);
                //Debug.Log(children.transform.position);
                currentPositionList.Add(children.transform);
            }
            float[] currentPositionArray = new float[currentPositionList.Count * 2];
            for (int i = 0; i < currentPositionList.Count; i++)
            {
                currentPositionArray[i] = currentPositionList[i].position.x;
            }
            for (int i = currentPositionList.Count; i < currentPositionArray.Length; i++)
            {
                currentPositionArray[i] = currentPositionList[i - currentPositionList.Count].position.y;
            }
            int currentTetrimino = spawner.GetComponent<spawnerControllerNeural>().tetrimino[index];

            int[,] positions1D = scriptReader.GetComponent<neuralPositionTracker>().GetPositions1D();
            float[] inputRotations = GetRotation();
            for (int i = 0; i < 200; i++)
            {
                input[i] = positions1D[index, i];
            }
            // Debug.Log("!" + input.Length);
            inputRotations.CopyTo(input, 200);
            currentPositionArray.CopyTo(input, 200 + inputRotations.Length);
            input[212] = currentTetrimino;

            // Debug.Log("rounded res: " + input[18]);

            outputs = network.FeedForward(input);//Call to network to feedforward

            //debug --------------
            for (int i = 0; i < outputs.Length; i++)
            {
                // Debug.Log("output" + i + ": " + outputs[i]);
            }
            //------------------------

            neuralUpdateTimer = 0;
        }
    }

    public void EvaluateFitness()
    {
        float score = GameObject.Find("scoreText").GetComponent<neuralScoring>().totalScore[index];
        int[,] positions1D = scriptReader.GetComponent<neuralPositionTracker>().GetPositions1D();
        float fitnessBox = 0;
        float fitnessHeight = 0;
        float fitnessWidth = 0;

        for (int i = 0; i < positions1D.GetLength(1); i++)
        {
            if (positions1D[index, i] == 0 && i + 10 < positions1D.GetLength(1))
            {
                if (positions1D[index, i + 10] == 1) // there is one above
                {
                    fitnessBox -= 1;

                    int counterhere = 0;
                    for (int y = i - 10; y >= 0; y -= 10)
                    {
                        if (positions1D[index, y] == 1)
                        {
                            fitnessBox = fitnessBox + (counterhere * -1);
                            counterhere = 0;
                        }
                        else if (y - 10 < 0)
                        {
                            fitnessBox = fitnessBox + ((counterhere + 1) * -1);
                        }
                        else
                        {
                            counterhere++;
                        }
                    }
                }
            }
        }

        bool[] checkedd = new bool[20];
        List<float> fitnessWidths = new List<float>();
        for (int i = 0; i < positions1D.GetLength(1); i++)
        {
            int yValue = i == 0 ? 0 : i / 10;
            if (positions1D[index, i] == 1 && checkedd[yValue] == false)
            {
                fitnessWidths.Add(10);
                // Debug.Log("width added on " + yValue);
                checkedd[yValue] = true;
            }
            else if (positions1D[index, i] == 1)
            {
                fitnessWidths[yValue] += 10;
            }
        }
        // Debug.Log(fitnessWidths.Count + " width count");
        // foreach (var item in fitnessWidths)
        // {
        //     Debug.Log("width" + item);
        // }
        fitnessWidth = fitnessWidths.Count > 0 ? fitnessWidths.Average() : 0f;

        float[] fitnessHeights = new float[10];
        bool hejehj = false;
        for (int x = 0; x < 10; x++)
        {
            for (int i = 0; i + x < positions1D.GetLength(1); i += 10)
            {
                int pos = i + x;
                int y = i == 0 ? 0 : i / 10;
                // Debug.Log("position " + pos);

                if (positions1D[index, pos] == 1)
                {
                    // Debug.Log("hejhejhej");
                    hejehj = true;
                    // fitnessHeights[x] = y - 1;
                }
                if (hejehj == true && positions1D[index, pos] == 0)
                {
                    // Debug.Log("recorded height: (" + x + ", " + y + ")");
                    fitnessHeights[x] = y - 1;
                    hejehj = false;
                }
            }
        }
        // Debug.Log("fitness " + fitnessHeights[0]);
        fitnessHeight = Mathf.Abs(fitnessHeights[0] - fitnessHeights[1]) + Mathf.Abs(fitnessHeights[1] - fitnessHeights[2]) + Mathf.Abs(fitnessHeights[2] - fitnessHeights[3]) + Mathf.Abs(fitnessHeights[3] - fitnessHeights[4]) + Mathf.Abs(fitnessHeights[4] - fitnessHeights[5]) + Mathf.Abs(fitnessHeights[5] - fitnessHeights[6]) + Mathf.Abs(fitnessHeights[6] - fitnessHeights[7]) + Mathf.Abs(fitnessHeights[7] - fitnessHeights[8]) + Mathf.Abs(fitnessHeights[8] - fitnessHeights[9]);
        // fitnessHeight = -(20 - fitnessHeights.Min());

        network.fitness = score * 3 + fitnessBox + fitnessWidth * 4; //updates fitness of network for sorting
        // network.fitness = score * 3 + fitnessBox + fitnessHeight * -2 + fitnessWidth * 5;
        // network.fitness = fitnessBox + fitnessHeight;

        Debug.Log("fitness res: " + network.fitness + " width: " + (fitnessWidth * 2) + " score: " + (score * 3) + " box: " + fitnessBox);
        // Debug.Log("fitness res: " + network.fitness + " score: " + (score * 3) + " box: " + fitnessBox + " height: " + fitnessHeight * -2 + " width: " + fitnessWidth * 5);
        // Debug.Log("fitness res: " + network.fitness + " box: " + fitnessBox + " height: " + fitnessHeight);

        fitnessTimer = 0;
    }

    // public void EvaluateFitness()
    // {
    //     float score = GameObject.Find("scoreText").GetComponent<neuralScoring>().totalScore[index];
    //     network.fitness = score * 10;
    //     int wellHeight = 9;
    //     int wellWidth = 10;

    //     int[,] positions1D = scriptReader.GetComponent<neuralPositionTracker>().GetPositions1D();
    //     // for (int i = 0; i < 200; i++)
    //     // {
    //     //     Debug.Log("value"+ positions1D[0,i]);
    //     // }
    //     for (int i = 1; i < wellHeight; i++)
    //     {
    //         int rowBalance = 0;

    //         for (int j = 0; j < wellWidth; j++)
    //         {

    //             if (positions1D[index,j+(10 * (i - 1))] == 1)
    //             {
    //                 rowBalance++;
    //                 // Debug.Log("some row balance");
    //             }
    //             else
    //             {
    //                 rowBalance--;
    //             }
    //         }

    //         network.fitness += SquarePreservingSign(rowBalance);
    //     }

    //     Debug.Log("fitness for index " + index + ": " + network.fitness);
    //     // fitnessTimer = 0;
    // }

    // private long SquarePreservingSign(int rowBalance)
    // {
    //     if (rowBalance == 0)
    //     {
    //         return rowBalance;
    //     }
    //     return (long)(Mathf.Pow(rowBalance, 2) * (Mathf.Abs(rowBalance) / rowBalance));
    // }

    //Method for moving on the x axis
    void Mover()
    {
        if (/*Input.GetKey(right)*/ outputs[1] == 1 && rightTimer > moveSpeed) // goes right
        {
            if (!brickRight)
                transform.position += Vector3.right;
            rightTimer = 0;
        }
        if (/*Input.GetKey(left)*/ outputs[3] == 1 && leftTimer > moveSpeed) //goes left
        {
            if (!brickLeft)
                transform.position += Vector3.left;
            leftTimer = 0;
        }
    }

    //Method for rotating objects (currently only right)
    void Rotater()
    {
        // O tetrimino will not be rotated
        if (!(this.name == "O TetriminoN(Clone)"))
        {
            if (/*Input.GetKeyDown(rotateRight)*/ outputs[0] == 1 && rotateTimer > rotateSpeed)
            {
                if (rotateNumber == 3)
                {
                    rotateNumber = 0;
                }
                else
                {
                    rotateNumber++;
                }

                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);

                if (RotateBack())
                {
                    //Debug.Log("Returned Rotation");
                    transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -270);
                }
                else
                {
                    transform.position += FixRotate();
                    //Debug.Log("fixed rotation: " + FixRotate());
                }

                //Debug.Log("after rotate pos: " + transform.position);

                //reset rotation button
                outputs[0] = 0;
                rotateTimer = 0;
            }
        }
    }

    float[] GetRotation()
    {
        switch (rotateNumber)
        {
            case 0:
                return new float[] { 1, 0, 0, 0 };
            case 1:
                return new float[] { 0, 1, 0, 0 };
            case 2:
                return new float[] { 0, 0, 1, 0 };
            case 3:
                return new float[] { 0, 0, 0, 1 };
            default:
                break;
        }
        return new float[] { 0, 0, 0, 0 };
    }

    //Makes the tetrimino fall and possible to accelerate by pressing down
    void Faller()
    {
        //reverts back the speed, while not pressing down
        // fallSpeed = OriginalFallSpeed;

        fallSpeed = 0.05f;

        //accelerate the tetrimino by making the fall speed smaller
        if (/*Input.GetKey(down)*/ outputs[2] == 1 && !brickBelow)
        {
            fallSpeed = 0.02f;
        }

        brickBelow = CheckCollisionY();

        //Makes the tetrimino fall
        if (fallTimer > fallSpeed && !brickBelow)
        {
            transform.position += Vector3.down;
            fallTimer = 0;
        }

        if (brickBelow)
        {
            StopThemFall();
        }
    }

    void Hold()
    {
        if (Input.GetKeyDown(holdKey))
        {
            spawner.GetComponent<spawnerController>().HoldTetrimino();
        }
    }

    //Stop them fall is to stop current tetrimino to fall, this should not be used to check if all tetriminos are dead
    void StopThemFall()
    {
        timerTrigger += Time.deltaTime;

        if (timerTrigger > 0.5f)
        {
            ExportPosition();

            foreach (Transform children in transform)
            {
                children.transform.position = RoundPosition(children.transform.position);
                // Debug.Log("ok we are on the waya");
                if (children.transform.position.y >= 19.5f && hasEvaluatedFitness == false) // gameover
                {
                    // UpdateFitness();//gets bots to set their corrosponding networks fitness
                    EvaluateFitness();

                    GameObject.Find("scoreText").GetComponent<neuralScoring>().totalScore[index] = 0;
                    // Debug.Log("row reset points");
                    // Debug.Log("evaluate fitness is done for " + index);
                    hasEvaluatedFitness = true;
                    break;
                }

            }
            foreach (Transform children in transform)
            {
                children.transform.position = RoundPosition(children.transform.position);
                if (children.transform.position.y < 19.5f)
                {
                    // spawner.GetComponent<spawnerControllerNeural>().SpawnNewTetrimino(index);
                    allahHelpMePls = true;
                    break;
                }
            }

            // bool[] gameOvers = scriptReader.GetComponent<neuralPositionTracker>().GetGameOvers();
            // spawner.GetComponent<spawnerControllerNeural>().SpawnNewTetrimino(index);
            enabled = false;
            timerTrigger = 0;
        }
    }

    //Method for tetriminos not going out from the map
    bool StopThemX(int moveX)
    {
        foreach (Transform children in transform)
        {
            //Bool for the x range tetriminos can be (1 - 20)
            bool inXRange = children.transform.position.x + moveX >= 1 && children.transform.position.x + moveX <= width;
            if (!inXRange)
            {
                return true;
            }
            if (brickRight != false && moveX == 1)
            {
                return true;
            }
            if (brickLeft != false && moveX == -1)
            {
                return true;
            }
        }
        return false;
    }

    bool RotateBack()
    {
        //Debug.Log("Started Rotation check");
        foreach (Transform children in transform)
        {
            List<List<Transform>> positions = scriptReader.GetComponent<neuralPositionTracker>().GetPositions();
            for (int i = 0; i < positions[index].Count; i++)
            {
                if (children.transform.position == positions[index][i].position)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Method for fixing the rotations by walls
    Vector3 FixRotate()
    {
        // Position to add
        Vector3 newPos = new Vector3(0, 0, 0);
        // Position for storing old positions
        Vector3 oldPos = new Vector3(0, 0, 0);
        string thingToRotate = this.name;

        foreach (Transform children in transform)
        {
            List<List<Transform>> positions = scriptReader.GetComponent<neuralPositionTracker>().GetPositions();
            for (int i = 0; i < positions[index].Count; i++)
            {
                //Checking left brick
                if (children.transform.position == positions[index][i].position && brickLeft == true)
                {
                    //Debug.Log("Same pos " + rotateNumber + " name: " + thingToRotate);

                    newPos.x = 1;

                    //If there are more than one square outside. Then check which is the furthest in
                    if (thingToRotate == "I TetriminoN(Clone)" && rotateNumber == 1)
                    {
                        //Debug.Log("bruh ok okok");
                        newPos.x = 2;
                    }

                    //If there are more than one square outside. Then check which is the furthest in
                    if (oldPos.x > newPos.x)
                        newPos.x = oldPos.x;

                    //Store current value so it is comparable. Used for code above
                    oldPos.x = newPos.x;


                    //Debug
                    //Debug.Log("Collided with brick " + newPos.x);
                }
                if (children.transform.position == positions[index][i].position && brickRight == true)
                {
                    //Debug.Log("Same pos right " + rotateNumber + " name: " + thingToRotate);

                    newPos.x = -1;

                    //If there are more than one square outside. Then check which is the furthest in
                    if (thingToRotate == "I TetriminoN(Clone)" && rotateNumber == 3)
                    {
                        //Debug.Log("bruh ok okok");
                        newPos.x = -2;
                    }

                    //If there are more than one square outside. Then check which is the furthest in
                    if (oldPos.x < newPos.x)
                        newPos.x = oldPos.x;

                    //Store current value so it is comparable. Used for code above
                    oldPos.x = newPos.x;
                    //Debug.Log("Collided with brick right " + newPos.x);
                }
            }
            //Checking left side
            if (children.transform.position.x < 1)
            {
                //Fixing position
                newPos.x = -(children.transform.position.x - 1);

                //If there are more than one square outside. Then check which is the furthest in
                if (oldPos.x > newPos.x)
                    newPos.x = oldPos.x;

                //Store current value so it is comparable. Used for code above
                oldPos.x = newPos.x;

                //Debug
                //Debug.Log("Fixed Left Side " + newPos.x);
            }
            //Checking right side
            else if (children.transform.position.x > width)
            {
                //Fixing position
                newPos.x = -((children.transform.position.x - width));

                //If there are more than one square outside. Then check which is the furthest in
                if (oldPos.x < newPos.x)
                    newPos.x = oldPos.x;

                //Store current value so it is comparable. Used for code above
                oldPos.x = newPos.x;

                //Debug
                //Debug.Log("Fixed Right Side " + newPos.x);
            }
            if (children.transform.position.y < 0)
            {

                //Fixing position
                newPos.y = -(Mathf.Round(children.transform.position.y - 0.1f));


                //Debug
                //Debug.Log("Fixed Down children y: " + children.transform.position.y);
                //Debug.Log("Fixed Down Side " + children.name + " " + newPos.y + " " + children.transform.position);
            }
        }
        //Returning vector3 which should be added to transform.position
        return newPos;
    }

    //Export positions to scriptreader
    void ExportPosition()
    {
        List<Transform> positions = new List<Transform>();

        foreach (Transform children in transform)
        {
            children.transform.position = RoundPosition(children.transform.position);
            //Debug.Log(children.transform.position);
            positions.Add(children.transform);
        }
        scriptReader.GetComponent<neuralPositionTracker>().SetPositionsForIndex(positions, index);
    }

    Vector3 RoundPosition(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x * 10) / 10f;
        pos.y = Mathf.Round(pos.y * 10) / 10f;
        pos.z = Mathf.Round(pos.z * 10) / 10f;

        return pos;
    }

    bool CheckCollisionY()
    {
        GameObject botCollision = GameObject.Find("BottomCollision");
        List<List<Transform>> positions = scriptReader.GetComponent<neuralPositionTracker>().GetPositions();

        foreach (Transform children in transform)
        {
            //Debug.Log("checking for something below me");

            Vector3 newPos = children.transform.position + Vector3.down;

            // Debug.Log("what tetrimino: " + this);
            // Debug.Log("index: " + index);
            // Debug.Log("positon j.count: " + positions[index].Count);

            if (scriptReader.GetComponent<neuralPositionTracker>().GetPositions().Any())
            {
                for (int i = 0; i < positions[index].Count; i++)
                {
                    if (newPos == positions[index][i].position)
                    {
                        //Debug.Log("something below me");
                        return true;
                    }
                }
            }
            if (botCollision.transform.position.y >= newPos.y)
            {
                //Debug.Log("botCollision below me");
                return true;
            }
        }
        return false;
    }
    bool CheckCollisionXLeft()
    {
        List<List<Transform>> positions = scriptReader.GetComponent<neuralPositionTracker>().GetPositions();
        foreach (Transform children in transform)
        {
            Vector3 newPos = children.transform.position + Vector3.left;
            bool inRange = newPos.x >= 0.9f;

            if (scriptReader.GetComponent<neuralPositionTracker>().GetPositions().Any())
            {
                for (int i = 0; i < positions[index].Count; i++)
                {
                    if (newPos == positions[index][i].position)
                    {
                        //Debug.Log("collision left");
                        return true;
                    }
                }
            }
            if (!inRange)
            {
                //Debug.Log(children.transform.position);
                //Debug.Log("collision left wall");
                return true;
            }
        }
        return false;
    }

    bool CheckCollisionXRight()
    {
        List<List<Transform>> positions = scriptReader.GetComponent<neuralPositionTracker>().GetPositions();
        foreach (Transform children in transform)
        {
            Vector3 newPos = children.transform.position + Vector3.right;
            bool inRange = newPos.x <= width;
            if (scriptReader.GetComponent<neuralPositionTracker>().GetPositions().Any())
            {
                for (int i = 0; i < positions[index].Count; i++)
                {
                    if (newPos == positions[index][i].position)
                    {
                        //Debug.Log("collision right");
                        return true;
                    }
                }
            }
            if (!inRange)
            {
                //Debug.Log("collision right wall");
                return true;
            }
        }
        return false;
    }

    bool CheckCollisionX(int moveX)
    {
        List<List<Transform>> positions = scriptReader.GetComponent<neuralPositionTracker>().GetPositions();
        foreach (Transform children in transform)
        {
            Vector3 newPos = children.transform.position + new Vector3(moveX, 0, 0);
            bool inRange = newPos.x >= 0.9f;
            for (int i = 0; i < positions[index].Count; i++)
            {
                if (newPos == positions[index][i].position)
                {
                    //Debug.Log("collision left");
                    return true;
                }
            }
            if (!inRange)
            {
                //Debug.Log(children.transform.position);
                //Debug.Log("collision left wall");
                return true;
            }
        }
        return false;
    }

    //Allocate rotation point for right tetrimino
    Vector3 AllocateRotationPoint()
    {
        if (this.name == "I TetriminoN(Clone)")
            return new Vector3(-0.5f, 1, 0);
        else if (this.name == "S TetriminoN(Clone)")
            return new Vector3(0, -0.5f, 0);
        else if (this.name == "O TetriminoN(Clone)" || this.name == "T TetriminoN(Clone)")
            return new Vector3(0, 0, 0);
        else
            return new Vector3(0, 0.5f, 0);
    }
}
