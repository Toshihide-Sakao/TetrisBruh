using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class neuralController : MonoBehaviour
{
    float fallTimer;
    float rightTimer;
    float leftTimer;
    public float moveSpeed = 0.1f;
    float fallSpeed = 0.8f;
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

    // -- USE IF NEEDED
    //int[,] map;
    // -------------

    KeyCode right = KeyCode.RightArrow;
    KeyCode left = KeyCode.LeftArrow;
    KeyCode down = KeyCode.DownArrow;
    KeyCode rotateRight = KeyCode.UpArrow;
    KeyCode holdKey = KeyCode.C;

    // ------ NOT USED YET -------
    // KeyCode rotateLeft = KeyCode.Z;
    // ------------------------------

    // Start is called before the first frame update
    void Start()
    {
        //Find Script Reader for exporting positions
        scriptReader = GameObject.Find("scriptReader");

        //Find Spawner for holding
        spawner = GameObject.Find("Spawner");

        //Record fallspeed so it is reversable
        OriginalFallSpeed = fallSpeed;

        //Giving the right rotationpoint for this tetrimino
        rotationPoint = AllocateRotationPoint();
    }

    // Update is called once per frame
    void Update()
    {
        //Adds time to timer for falling
        fallTimer += Time.deltaTime;
        rightTimer += Time.deltaTime;
        leftTimer += Time.deltaTime;

        brickBelow = CheckCollisionY();
        brickLeft = CheckCollisionXLeft();
        brickRight = CheckCollisionXRight();

        //Methods for tetrimino
        Hold();
        Mover();
        Rotater();
        Faller();
    }

    //Method for moving on the x axis
    void Mover()
    {
        if (Input.GetKey(right) && rightTimer > moveSpeed)
        {
            if (!brickRight)
                transform.position += Vector3.right;
            rightTimer = 0;
        }
        if (Input.GetKey(left) && leftTimer > moveSpeed)
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
            if (Input.GetKeyDown(rotateRight))
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

                Debug.Log("after rotate pos: " + transform.position);
            }
        }
    }

    //Makes the tetrimino fall and possible to accelerate by pressing down
    void Faller()
    {
        //reverts back the speed, while not pressing down
        fallSpeed = OriginalFallSpeed;

        //accelerate the tetrimino by making the fall speed smaller
        if (Input.GetKey(down) && !brickBelow)
        {
            fallSpeed = 0.05f;
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

    void StopThemFall()
    {
        timerTrigger += Time.deltaTime;

        if (timerTrigger > 0.5f)
        {
            ExportPosition();
            while (true)
            {
                Debug.Log("I AM STUCK HELP");
                if (GameObject.Find("scriptReader").GetComponent<neuralPositionTracker>().GetPositions()[index].Any())
                {
                    spawner.GetComponent<spawnerControllerNeural>().SpawnNewTetrimino(index);
                    break;
                }
            }
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

            if (GameObject.Find("scriptReader").GetComponent<neuralPositionTracker>().GetPositions().Any())
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
        List<List<Transform>> positions = new neuralPositionTracker().GetPositions();
        foreach (Transform children in transform)
        {
            Vector3 newPos = children.transform.position + Vector3.left;
            bool inRange = newPos.x >= 0.9f;

            if (GameObject.Find("scriptReader").GetComponent<neuralPositionTracker>().GetPositions().Any())
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
        List<List<Transform>> positions = new neuralPositionTracker().GetPositions();
        foreach (Transform children in transform)
        {
            Vector3 newPos = children.transform.position + Vector3.right;
            bool inRange = newPos.x <= width;
            if (GameObject.Find("scriptReader").GetComponent<neuralPositionTracker>().GetPositions().Any())
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
