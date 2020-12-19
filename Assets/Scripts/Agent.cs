using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Simulation;

public class Agent : MonoBehaviour
{
    //VARIABLES SET AT INSTANTIATING
    public List<Tuple<Vector3, float>> locationList;
        //stay at vector3 location for float time
        //we can remove the time to stay at location
    public ProtectiveMeasure distancing;
    public float infectionDuration;
    public float deathChance = .04f;
    public float infectionRate = .3f;
    private float deathCountdown = 7f + 1f;
        //set how long this guy will stay infected for based on Simulation class
        //probabilities
    public bool willDie;
        //will this person die if infected
    public Material healthyMaterial;
    public Material infectedMaterial;
    public Material curedMaterial;


    //UPDATING VARIABLES
    public int currLocation;
        //location index in locationTimeList
    public float timeAtLoc;
        //float time remaining at location
    public float infectionTimer = -1f;
        //bool not needed bc timer > 0 ? infected : healthy
        //and set gameObject.tag to "Infected" on infection
        //gameObject.tag to "Healthy" on healthy
    private NavMeshAgent navMAgent;
    private MeshRenderer meshR;

    public class Tuple<K, V>
    {
        public K Item1;
        public V Item2;

        public Tuple(K k, V v)
        {
            Item1 = k;
            Item2 = v;
        }
    }

    void Start()
    {
        currLocation = 0;
        timeAtLoc = locationList[0].Item2;
        GetComponent<NavMeshAgent>().SetDestination(locationList[currLocation].Item1);
        navMAgent = GetComponent<NavMeshAgent>();
        meshR = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (infectionTimer > 0.0f) infectionTimer = Mathf.Max(0.0f, infectionTimer - Time.deltaTime);
        if (infectionTimer == -1f) {
            GetComponent<MeshRenderer>().material = healthyMaterial;
            gameObject.tag = "Healthy";
        } else if (infectionTimer == 0f) {
            GetComponent<MeshRenderer>().material = curedMaterial;
            gameObject.tag = "Cured";
            print("cured");
        }
        if (infectionDuration - infectionTimer > deathCountdown) {
            gameObject.tag = "Dead";
            print("died");
            print(infectionDuration);
            print(infectionTimer);
            print(deathCountdown);
        }


        if (navMAgent.remainingDistance <= 2f) timeAtLoc = Mathf.Max(0.0f, timeAtLoc - Time.deltaTime);
        if (timeAtLoc == 0)
        {
            currLocation = (currLocation+1)%locationList.Count;
            timeAtLoc = locationList[currLocation].Item2;
            navMAgent.SetDestination(locationList[currLocation].Item1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Infected") &&
            gameObject.tag == "Healthy" &&
            Random.Range(1f,1000f) <= infectionRate * 1000)
        {
            if (GetRoom(transform) == GetRoom(other.transform))
            {
                gameObject.tag = "Infected";
                infectionTimer = infectionDuration;
                meshR.material = infectedMaterial;
                if (Random.Range(1f,1000f) < deathChance * 1000)
                {
                    deathCountdown = Random.Range(1f, infectionDuration);
                }
                else
                {
                    deathCountdown = infectionDuration + 1; // will not die
                }
            }
        }
    }

    private int GetRoom(Transform t)
    {
        float x = t.position[0];
        float y = t.position[2];
        if (x < -10.95 && y < -2.47) return 1;
        else if (x < -10.95 && y > -2.47 && y < 2.25) return 2;
        else if (x < -10.95 && y > 2.25) return 3;
        else if (x > -8.27 && x < -1.45 && y > 4) return 4;
        else if (x > -8.27 && x < -1.45 && y < 4 && y > 0.4) return 5;
        else if (x > -5.56 && x < -.98 && y < -3.72 && y > -8.5) return 6;
        else if (x > -.98 && x < 4.5 && y < -3.59 && y > -8.5) return 7;
        else if (x > 4.5 && x < 11.14 && y < -.79 && y > -8.5) return 8;
        else if (x > 5.21 && x < 11.14 && y < 8.5 && y > 3.66) return 9;
        else if (x > 11.14 && x < 19 && y < 1.65 && y > -8.5) return 10;
        else if (x > 11.14 && x < 19 && y < 1.65 && y > -8.5) return 11;
        else return 0; // outside corridors
    }

}
