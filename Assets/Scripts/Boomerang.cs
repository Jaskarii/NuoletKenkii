using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour {
    private bool initilized = false;
    private Transform hex;

    private NeuralNetwork net;
    private Rigidbody2D rBody;
    private Material[] mats;

    private Quaternion RightAngle = Quaternion.AngleAxis(-20, new Vector3(0, 0, 1));
    private Quaternion LeftAngle = Quaternion.AngleAxis(20, new Vector3(0, 0, 1));


    void Start()
    {
        rBody = GetComponent<Rigidbody2D>();
        mats = new Material[transform.childCount];
        for(int i = 0; i < mats.Length; i++)
            mats[i] = transform.GetChild(i).GetComponent<Renderer>().material;
    }

    void FixedUpdate ()
    {
        if (!initilized)
        {
            return;
        }

        //Kästää rayt, saadaan etäisyydet seinistä kolmeen suuntaan
        RaycastHit2D rayCasthit = Physics2D.Raycast(this.transform.position, rBody.velocity);
        RaycastHit2D rayCasthitLeft = Physics2D.Raycast(this.transform.position, LeftAngle * rBody.velocity);
        RaycastHit2D rayCasthitRight = Physics2D.Raycast(this.transform.position, RightAngle * rBody.velocity);

        Debug.DrawRay(this.transform.position, rBody.velocity);
        Debug.DrawRay(this.transform.position, LeftAngle * rBody.velocity);
        Debug.DrawRay(this.transform.position, RightAngle * rBody.velocity);

        //eäisyys ja kulma kohteesta
        float distance = Math.Min(Vector2.Distance(transform.position, hex.position),100);

        for (int i = 0; i < mats.Length; i++)
            mats[i].color = new Color(distance / 100f, (1f-(distance / 100f)), (1f - (distance / 100f))); 

        float[] inputs = new float[5];


        float angle = transform.eulerAngles.z % 360f;
        if (angle < 0f)
            angle += 360f;

        Vector2 deltaVector = (hex.position - transform.position).normalized;
   

        float rad = Mathf.Atan2(deltaVector.y, deltaVector.x);
        rad *= Mathf.Rad2Deg;

        rad = rad % 360;
        if (rad < 0)
        {
            rad = 360 + rad;
        }

        rad = 90f - rad;
        if (rad < 0f)
        {
            rad += 360f;
        }
        rad = 360 - rad;
        rad -= angle;
        if (rad < 0)
            rad = 360 + rad;
        if (rad >= 180f)
        {
            rad = 360 - rad;
            rad *= -1f;
        }

        //Syötetään kulma ja etäisyys kohteesta, sekä säiteiden osumaetäisyydet seinistä verkolla sisään.
        //Fitnessiin summataan etäisyys kohteesta, kulma sekä etäisyyksien käänteisluvut seinistä --> pienempi fitness on parempi.
        float raycastDist = 30;
        float raycastDistLeft = 30;
        float raycastDistRight = 30;

        FitnessBasedOnCollision(rayCasthit, ref raycastDist);
        FitnessBasedOnCollision(rayCasthitLeft, ref raycastDistLeft);
        FitnessBasedOnCollision(rayCasthitRight, ref raycastDistRight);
			
        inputs[0] = rad/180f;
        inputs[1] = distance / 20;
        inputs[2] = raycastDist / 10;
        inputs[3] = raycastDistLeft / 10;
        inputs[4] = raycastDistRight / 10;

        float[] output = net.FeedForward(inputs);

        rBody.velocity = Math.Abs(30f * output[1])* transform.up;
        rBody.angularVelocity = 200f*output[0];
        net.AddFitness(Math.Abs(inputs[0]) + inputs[1]);
	}

    private void FitnessBasedOnCollision(RaycastHit2D rayCastHit, ref float rayDistance)
    {
        if (rayCastHit.collider != null)
        {
            rayDistance = Vector2.Distance(rayCastHit.point, transform.position);
            rayDistance = Math.Min(rayDistance, 30);

            net.AddFitness(5f/ rayDistance);
        }
    }

    public void setFitness()
    {
        net.SetFitness((50f - (Vector2.Distance(transform.position, hex.position))));
    }

    public void Init(NeuralNetwork net, Transform hex)
    {
        this.hex = hex;
        this.net = net;
        initilized = true;
    }

    
}
