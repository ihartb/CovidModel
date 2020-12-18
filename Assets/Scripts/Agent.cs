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
        //set how long this guy will stay infected for based on Simulation class
        //probabilities
    public bool willDie;
        //will this person die if infected
    public Material healthyMaterial;
    public Material infectedMaterial;


    //UPDATING VARIABLES
    public int currLocation;
        //location index in locationTimeList
    public float timeAtLoc;
        //float time remaining at location
    public float infectionTimer;
        //bool not needed bc timer > 0 ? infected : healthy
        //and set gameObject.tag to "Infected" on infection
        //gameObject.tag to "Healthy" on healthy

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
    }

    void Update()
    {
        if (infectionTimer > 0.0f) infectionTimer = Mathf.Max(0.0f, infectionTimer - Time.deltaTime);
        if (infectionTimer == 0.0f) {
            GetComponent<MeshRenderer>().material = healthyMaterial;
            gameObject.tag = "Healthy";
        }

        NavMeshAgent navMAgent = GetComponent<NavMeshAgent>();
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
        if (other.gameObject.CompareTag("Infected"))
        {
            gameObject.tag = "Infected";
            infectionTimer = infectionDuration;
            GetComponent<MeshRenderer>().material = infectedMaterial;
        }
    }

}
