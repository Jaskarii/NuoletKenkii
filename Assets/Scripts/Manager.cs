using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager : MonoBehaviour {

    public GameObject boomerPrefab;
    public GameObject hex;

    private bool isTraning = false;
    private int populationSize = 100;
    private int generationNumber = 0;
    private int[] layers = new int[] { 5, 30, 30,20, 2 }; //1 input and 1 output
    private List<NeuralNetwork> nets;
    private bool leftMouseDown = false;
    private List<Boomerang> boomerangList = null;


    void Timer()
    {
        isTraning = false;
    }


	void Update ()
    {
        if (isTraning == false)
        {
            if (generationNumber == 0)
            {
                InitBoomerangNeuralNetworks();
            }
            else
            {
                nets.Sort();
                for (int i = 20; i < populationSize; i++)
                {
                    nets[i].CopyWeights(nets[i % 20]);
                    nets[i].Mutate();
                }

                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }

           
            generationNumber++;
            
            isTraning = true;
            Invoke("Timer",10f);
            CreateBoomerangBodies();
        }


        if (Input.GetMouseButtonDown(0))
        {
            leftMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            leftMouseDown = false;
        }

        if(leftMouseDown == true)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            hex.transform.position = mousePosition;
        }
    }


    private void CreateBoomerangBodies()
    {
        if (boomerangList != null)
        {
            for (int i = 0; i < boomerangList.Count; i++)
            {
                GameObject.Destroy(boomerangList[i].gameObject);
            }

        }

        boomerangList = new List<Boomerang>();

        for (int i = 0; i < populationSize; i++)
        {
            Boomerang boomer = ((GameObject)Instantiate(boomerPrefab, new Vector3(UnityEngine.Random.Range(30f,50f), UnityEngine.Random.Range(-10, 10f), 0),Quaternion.Euler(new Vector3(0, 0, UnityEngine.Random.Range(0f, 360f))))).GetComponent<Boomerang>();
            boomer.Init(nets[i],hex.transform);
            boomerangList.Add(boomer);
        }

    }

    void InitBoomerangNeuralNetworks()
    {
        //population must be even, just setting it to 20 incase it's not
        if (populationSize % 2 != 0)
        {
            populationSize = 20; 
        }

        nets = new List<NeuralNetwork>();
        
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            nets.Add(net);
        }
    }
}
