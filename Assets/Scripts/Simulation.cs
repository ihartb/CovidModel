using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Agent;

public class Simulation : MonoBehaviour
{

    public Material HealthyMaterial;
    public Material InfectedMaterial;
    public Material curedMaterial;
    public GameObject agentPrefab;
    public GameObject agentParent;
    public float maxX;
    public float maxZ;
    public int numAgents;

    private List<GameObject> agents;
    public List<Tuple<Vector2, Vector2>> roomDimensions;
    public float[] rooms;
    public bool start = false;
    //experiment variables
    public float percentInfected;
        //percent of population infected at which
        //protective measures implemented(5\%,15\%,25\%,50\%)
    public float percentDistancing;
        //percent of population that
        //follows safety measures (0\%, 10\%, 25\%, 70\%, 100\%)
    public ProtectiveMeasure distancingMeasure;
        //level of protective measures which affect infection rate
    public enum ProtectiveMeasure
    {
        NONE,
        LIGHT, //some distancing
        STRICT //heavy distancing, quarantine areas, wearing masks
    }

    public class Results
    {
        public float time; //time it takes for all infections to be gone
        public int numDeaths; //number of deaths during that time

        public Results(float time, int numDeaths)
        {
            this.time = time;
            this.numDeaths = numDeaths;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        roomDimensions = new List<Tuple<Vector2, Vector2>>();
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(0f, 0f), new Vector2(0f, 0f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(-17f, -7f), new Vector2(-11.9f, -3f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(-17f, -1.4f), new Vector2(-11.9f, 1f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(-17f, 3.5f), new Vector2(-11.9f, 7f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(-7.2f, 5f), new Vector2(-2.7f, 7f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(-7.2f, .6f), new Vector2(-2.7f, 3.2f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(-4.5f, -7f), new Vector2(-2f, -4.5f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(0f, -7f), new Vector2(3.8f, -4.5f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(5.4f, -7f), new Vector2(10f, -1.6f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(5.8f, 4.7f), new Vector2(10.4f, 7f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(12.9f, 2.8f), new Vector2(17f, 7f)));
        roomDimensions.Add(new Tuple<Vector2, Vector2>(new Vector2(12.9f, -7f), new Vector2(17f, .8f)));
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            for (int i = 0; i < numAgents; i++)
            {
                if (agents[i].tag == "Dead")
                {
                    print("Removed " + agents[i].name);
                    agents[i].SetActive(false);
                    agents.RemoveAt(i);
                    i--;
                    numAgents--;
                }
            }
            UpdateIndoorParticles();

            if (GameObject.FindGameObjectsWithTag("Infected").Length >= percentInfected * numAgents)
            {
                print("start distancing");
                int i = 0;
                foreach (var agent in agents)
                {
                    createLocationList(i, agent, true);
                    i++;
                }
            }
        }
    }

    // loosely based on numbers suggested by https://www.sciencedirect.com/science/article/pii/S0925753520302630#e0030
    void UpdateIndoorParticles()
    {
        for (int j = 1; j < 12; j++) // room number
        {
            for (int i = 0; i < numAgents; i++) // agent
            {
                // 5 particles per second
                if (agents[i].tag == "Infected" && GetRoom(agents[i].transform)==j)
                {
                    //update this based on agent.GetComponenet<Agent>().distancing == ProtectiveMeasure.Strict

                    rooms[j] += 500f*Time.deltaTime; // virus particles increase by this value
                }
            }
            rooms[j] -= 400f*Time.deltaTime; // virus particles die over time according to this value (about 3 hours halflife translated to deltatime)
            rooms[j] = Mathf.Max(rooms[j],0f);
        }
    }

    public void startSimulation()
    {
        rooms = new float[12];
        for (int j = 0; j < 12; j++) rooms[j] = 0;
        agents = new List<GameObject>(new GameObject[numAgents]);

        for (int i = 0; i < numAgents; i++)
        {
            Tuple<Vector2, Vector2> ranges = roomDimensions[Random.Range(1, 11)];
            var randPos = new Vector3(Random.Range(ranges.Item1.x, ranges.Item2.x), 1, Random.Range(ranges.Item1.y, ranges.Item2.y));

            //instantiate game object
            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "Agent " + i;
            agent.transform.parent = agentParent.transform;
            agent.SetActive(true);

            //set script variables
            Agent agentScript = agent.GetComponent<Agent>();


            createLocationList(i, agent, percentInfected == 1f/50f);
            agentScript.infectionDuration = 14f;
            agentScript.healthyMaterial = HealthyMaterial;
            agentScript.infectedMaterial = InfectedMaterial;
            agentScript.curedMaterial = curedMaterial;
            agentScript.infectionTimer = -1f;
            agentScript.rooms = rooms;

            if (i == 0)
            {
                agent.tag = "Infected";
                agent.GetComponent<MeshRenderer>().material = InfectedMaterial;
                agentScript.infectionTimer = agentScript.infectionDuration;
            }

            agents[i] = agent;
        }


        start = true;
    }

    void createLocationList(int agentIndex, GameObject agent, bool isDistancing)
    {
        int maxLoc = 6;
        float minDur = .5f;
        float maxDur = 3f;

        Agent agentScript = agent.GetComponent<Agent>();
        agentScript.distancing = ProtectiveMeasure.NONE;
        if (distancingMeasure != ProtectiveMeasure.NONE && isDistancing && (agentIndex <= percentDistancing * numAgents))
        {
            if (distancingMeasure == ProtectiveMeasure.LIGHT)
            {
                agentScript.distancing = ProtectiveMeasure.LIGHT;
                maxLoc = 4;
                minDur = 3f;
                maxDur = 6f;
            }
            else if (distancingMeasure == ProtectiveMeasure.STRICT)
            {
                agentScript.distancing = ProtectiveMeasure.STRICT;
                maxLoc = 2;
                minDur = 5f;
                maxDur = 10f;
            }
        }

        int len = Random.Range(1, maxLoc);
        List<Tuple<Vector3, float>> res = new List<Tuple<Vector3, float>>();
        if (agentScript.locationList != null)
        {
            agentScript.currLocation = 0;
            Tuple<Vector3, float> curr = agentScript.locationList[agentScript.currLocation];
            res.Add(new Tuple<Vector3, float>(curr.Item1, Random.Range(minDur, maxDur)));

        }
        else {
            res.Add(new Tuple<Vector3, float>(agent.transform.position, Random.Range(minDur, maxDur)));
        }

        for (int i = 1; i < len; i++)
        {
            Tuple<Vector2, Vector2> ranges = roomDimensions[Random.Range(1, 11)];
            var pos = new Vector3(Random.Range(ranges.Item1.x, ranges.Item2.x), 1, Random.Range(ranges.Item1.y, ranges.Item2.y));
            var dur = Random.Range(minDur, maxDur);
            res.Add(new Tuple<Vector3, float>(pos, dur));
        }

        agentScript.locationList = res;
    }

    public static int GetRoom(Transform t)
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
}
