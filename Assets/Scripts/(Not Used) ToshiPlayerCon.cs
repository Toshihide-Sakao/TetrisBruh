using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToshiPlayerCon : MonoBehaviour
{
    public GameObject explosion;
    public GameObject bolt;
    public float speed = 1;
    float timerX = 0;
    float timerY = 0;
    public float timeBetweenMoveX = 0.1f;
    public float timeBetweenMoveY = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timerX += Time.deltaTime;
        timerY += Time.deltaTime;
        float Inputx = Input.GetAxisRaw("Horizontal");
        //float Inputy = Input.GetAxisRaw("Vertical");

        Inputx *= speed;
        //Inputy *= speed;

        Debug.Log(timerX);
        //Debug.Log(Inputy);
        if (timerX > timeBetweenMoveX && Inputx != 0)
        {
            Vector3 movementX = Vector3.right * Inputx /* * Time.deltaTime * 0.5f */;
            transform.Translate(movementX);
            timerX = 0;
        }
        if (timerY > timeBetweenMoveY)
        {
            Vector3 movementY = Vector3.up * -1 /* * Time.deltaTime * 0.5f */;
            transform.Translate(movementY);
            timerY = 0;
        }





        float controlFire = Input.GetAxisRaw("Fire1");
/*
        if (timer > timeBetweenShots && controlFire > 0)
        {
            GameObject newBolt = Instantiate(bolt);
            newBolt.transform.position = transform.position;
            timer = 0;
        }*/
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            Instantiate(explosion, transform.position, transform.rotation);
            Destroy(this.gameObject);
        }
        
        
    }
}
