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
    private float deathCountdown = 14f + 2f;
    private float immunityTimer = -1f;
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
    public MeshRenderer meshR;

    public float[] rooms;

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

    private int currRoom;
    private float timeInRoom;

    void Start()
    {
        currLocation = 0;
        timeAtLoc = locationList[0].Item2;
        GetComponent<NavMeshAgent>().SetDestination(locationList[currLocation].Item1);
        navMAgent = GetComponent<NavMeshAgent>();
        meshR = GetComponent<MeshRenderer>();
        currRoom = GetRoom(transform);
        timeInRoom = 0f;
    }

    void Update()
    {
        if (infectionTimer > 0.0f) infectionTimer = Mathf.Max(0.0f, infectionTimer - Time.deltaTime);
        if (immunityTimer > 0.0f) immunityTimer = Mathf.Max(0.0f, immunityTimer - Time.deltaTime);
        if (infectionTimer == 0f) {
            meshR.material = curedMaterial;
            gameObject.tag = "Cured";
            immunityTimer = 7f;
            infectionTimer = -1f;
            print("cured");
        }
        if (immunityTimer == 0f) {
            print("Immunity gone");
            meshR.material = healthyMaterial;
            gameObject.tag = "Healthy";
            infectionTimer = -1f;
        }
        if (infectionDuration - infectionTimer > deathCountdown) {
            gameObject.tag = "Dead";
            print("died");
            print(infectionDuration);
            print(infectionTimer);
            print(deathCountdown);
        }
        // see if infected from indoor room
        if (gameObject.tag == "Healthy")
        {
            int room = GetRoom(transform);
            if (room == currRoom)
            {
                timeInRoom += Time.deltaTime;
            }
            else
            {
                timeInRoom = 0;
                currRoom = room;
            }
            if (room != 0)
            {
                // need to divide by a constant to normalize this to correct units
                float tcritical = 5000f* Mathf.Exp(-.05f*rooms[room] / GetRoomArea(transform));
                if (timeInRoom > tcritical)
                {
                    InfectAgent();
                }
            }
            //
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
                InfectAgent();
            }
        }
    }

    private void InfectAgent()
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
        else if (x > 11.14 && x < 19 && y < 8.5 && y > 1.65) return 11;
        else return 0; // outside corridors
    }
    private float GetRoomArea(Transform t)
    {
        float x = t.position[0];
        float y = t.position[2];
        int room = GetRoom(t);
        double area;
        switch (room)
        {
            case 1:
                area = (-10.94-(-19))*(-2.47-(-8.5));
                break;
            case 2:
                area = (-10.94-(-19))*(2.25-(-2.47));
                break;
            case 3:
                area = (-10.94-(-19))*(8.5-(2.25));
                break;
            case 4:
                area = (-1.45-(-8.27))*(8.5-(4));
                break;
            case 5:
                area = (-1.45-(-8.27))*(4-(.4));
                break;
            case 6:
                area = (-.98-(-5.56))*(-3.72-(-8.5));
                break;
            case 7:
                area = (4.5-(-.98))*(-3.72-(-8.5));
                break;
            case 8:
                area = (11.14-4.5)*(-.79-(-8.5));
                break;
            case 9:
                area = (11.14-5.21)*(8.5-3.66);
                break;
            case 10:
                area = (19-11.14)*(1.65-(-8.5));
                break;
            case 11:
                area = (19-11.14)*(8.5-1.65);
                break;
            default:
                area = 0;
                break;
        }
        return (float) area;
    }

}
