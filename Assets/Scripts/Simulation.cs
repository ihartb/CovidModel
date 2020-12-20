using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
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
    public ProtectiveMeasure distancingMeasure;
    public float percentDistancing;
    List<int> percentDistancingList;
    public float percentInfected;
    List<int> percentInfectedList;

    public int distMeasure;
    public int percDist;
    public int percInf;
    public int simNum;
    public Results simResults;
    private Stopwatch stopWatch;

    public Dropdown percentInfectedMenu;
    public Dropdown percentDistancingMenu;
    public Dropdown levelProtectionMenu;
    public Button startButton;
    public Text resultsText;

    public enum ProtectiveMeasure
    {
        NONE,
        LIGHT, //some distancing
        STRICT //heavy distancing, quarantine areas, wearing masks
    }

    public class Results
    {
        public long totalTime; //time it takes for all infections to be gone
        public float numDeaths; //number of deaths during that time
        public float totalRuns;

        public Results()
        {
            this.totalTime = 0;
            this.numDeaths = 0;
            this.totalRuns = 0;
        }

        public void reset()
        {
            this.totalTime = 0;
            this.numDeaths = 0;
            this.totalRuns = 0;
        }

        public void setVals(long timeElapsed, float numDeaths)
        {
            ++totalRuns;
            this.totalTime += timeElapsed;
            this.numDeaths += numDeaths;

            //if (totalRuns == 20)
            //{
            //    this.totalTime /= 20;
            //    this.numDeaths /= 20;

            //    WriteToFile(this.toString());
            //    reset();

            //}
        }

        public string toString()
        {
            string p4 = "" + this.numDeaths + ", ";
            string p5 = "" + this.totalTime + "\n";

            return p4 + p5 + "\n";
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        percentDistancing = 1f;
        percentInfected = .02f;
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

        percentDistancingList = new List<int>();
        percentDistancingList.Add(100);
        percentDistancingList.Add(90);
        percentDistancingList.Add(80);
        percentDistancingList.Add(74);
        percentDistancingList.Add(50);
        percentDistancingList.Add(24);
        percentDistancingList.Add(0);

        percentInfectedList = new List<int>();
        percentInfectedList.Add(2);
        percentInfectedList.Add(6);
        percentInfectedList.Add(10);
        percentInfectedList.Add(16);
        percentInfectedList.Add(20);
        percentInfectedList.Add(30);

        stopWatch = new Stopwatch();
        simResults = new Results();
        agents = new List<GameObject>(new GameObject[numAgents]);

        distMeasure = -1;
        percDist = -1;
        percInf = -1;
        simNum = -1;
        string p1 = "Distancing measure, ";
        string p2 = "Percent Distancing, ";
        string p3 = "Percent Infected, ";
        string p4 = "Num Dead, ";
        string p5 = "Time to eradicate \n\n";
        WriteToFile(p1 + p2 + p3 + p4 + p5);
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

            if (GameObject.FindGameObjectsWithTag("Infected").Length == 0)
            {
                stopWatch.Stop();
                simResults.setVals(stopWatch.ElapsedMilliseconds, 50 - numAgents);

                WriteToFile(simResults.toString());
                simResults.reset();

                for (int i = 0; i < numAgents; i++)
                {
                    Destroy(agents[i]);
                }

                foreach (var dead in GameObject.FindGameObjectsWithTag("Dead"))
                {
                    Destroy(dead);
                }

                start = false;
                startButton.interactable = true;
                percentInfectedMenu.interactable = true;
                percentDistancingMenu.interactable = true;
                levelProtectionMenu.interactable = true;
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
        start = false;
        rooms = new float[12];
        for (int j = 0; j < 12; j++) rooms[j] = 0;
        numAgents = 50;
        agents = new List<GameObject>(new GameObject[numAgents]);
        incrSetParams();
        stopWatch.Start();

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
            agentScript.maskMult = 1f;
            agentScript.SDMult = 1f;

            if (i <= percentInfected * numAgents)
            {
                agent.tag = "Infected";
                agent.GetComponent<MeshRenderer>().material = InfectedMaterial;
                agentScript.infectionTimer = agentScript.infectionDuration;
            }
            // which agents are following distancing if any
            print(percentDistancing);
            print(percentInfected);
            if (i <= percentDistancing * numAgents)
            {

                if (distancingMeasure == ProtectiveMeasure.LIGHT) agentScript.SDMult = 1.5f;
                else if (distancingMeasure == ProtectiveMeasure.STRICT)
                {
                    agentScript.maskMult = 3f;
                    agentScript.SDMult = 4.0f;
                }
            }

            agents[i] = agent;
        }


        start = true;
    }

    public void incrSetParams()
    {
        //simNum = (simNum + 1) % 20;
        //if (simNum == 0)
        //{
        //    percInf = (percInf + 1) % percentInfectedList.Count;
        //    if (percInf == 0)
        //    {
        //        percDist = (percDist + 1) % percentDistancingList.Count;

        //        if (percDist == 0)
        //        {
        //            distMeasure = (distMeasure + 1);
        //            if (distMeasure == 3)
        //            {
        //                //stop the gameplay
        //            }
        //        }

        //    }
        //}

        //distancingMeasure = (ProtectiveMeasure)distMeasure;
        //percentDistancing = percentDistancingList[percDist] / 100f;
        //percentInfected = percentInfectedList[percInf] / 100f;

        string p1 = distancingMeasure.ToString() + ", ";
        string p2 = "" + percentDistancing + ", ";
        string p3 = "" + percentInfected + ", ";
        WriteToFile(p1 + p2 + p3);
    }

    public void WriteToFile(string res)
    {
        //using (System.IO.StreamWriter file =
        //new System.IO.StreamWriter(@"/Users/bhartimehta/Desktop/CovidModelData.txt", true))
        //{

        //    file.WriteLine(res);
        //}

        string curr = resultsText.text + res;
        resultsText.text = curr;

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
