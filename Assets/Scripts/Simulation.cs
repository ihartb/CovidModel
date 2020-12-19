using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : MonoBehaviour
{

    public Material HealthyMaterial;
    public Material InfectedMaterial;
    public Material curedMaterial;
    public GameObject agentPrefab;
    public GameObject locations;
    public GameObject agentParent;
    public float maxX;
    public float maxZ;
    public int numAgents;
    private List<GameObject> agents;
    public float[] rooms;

    //experiment variables
    public float percentInfected;
        //percent of population infected at which
        //protective measures implemented(5\%,15\%,25\%,50\%)
    public float percentDistancing;
        //percent of population that
        //follows safety measures (0\%, 10\%, 25\%, 70\%, 100\%)
    public float distancingMeasure;
        //level of protective measures which affect infection rate
    public enum ProtectiveMeasure
    {
        NONE,
        LIGHT, //wearing masks (particle number reduced), moderate distancing (distance)
        STRICT //heavy distancing, quarantine areas, wearing masks
    }
    // Start is called before the first frame update
    void Start()
    {
        rooms = new float[12];
        for (int j = 0; j < 12; j++) rooms[j] = 0;
        agents = new List<GameObject>(new GameObject[numAgents]);
        for (int i = 0; i < numAgents; i++)
        {
            var randPos = new Vector3(Random.Range(-1f, 1f)*maxX, 1, Random.Range(-1f, 1f)*maxZ);

            //instantiate game object
            GameObject agent = null;
            agent = Instantiate(agentPrefab, randPos, Quaternion.identity);
            agent.name = "Agent " + i;
            agent.transform.parent = agentParent.transform;
            agent.SetActive(true);

            //set script variables
            Agent agentScript = agent.GetComponent<Agent>();
            agentScript.locationList = createLocationList(randPos);
            agentScript.distancing = ProtectiveMeasure.NONE;
            agentScript.infectionDuration = 14f;
            agentScript.willDie = Random.Range(1, 100) <= 30; //30% will die LOLLLL
            agentScript.healthyMaterial = HealthyMaterial;
            agentScript.infectedMaterial = InfectedMaterial;
            agentScript.curedMaterial = curedMaterial;
            agentScript.infectionTimer = -1f;
            agentScript.rooms = rooms;
            // agentScript.meshR = agent.GetComponent<MeshRenderer>();
            if (i == 0)
            {
                agent.tag = "Infected";
                agent.GetComponent<MeshRenderer>().material = InfectedMaterial;
                // agentScript.meshR.material = agentScript.infectedMaterial;
                agentScript.infectionTimer = agentScript.infectionDuration;
            }
            agents[i] = agent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < numAgents; i++)
        {
            if (agents[i].tag == "Dead")
            {
                print("Removed " +agents[i].name);
                agents[i].SetActive(false);
                agents.RemoveAt(i);
                i--;
                numAgents--;
            }
        }
        UpdateIndoorParticles();
    }

    List<Agent.Tuple<Vector3, float>> createLocationList(Vector3 randPos) {
        int len = Random.Range(1, 5);
        List<Agent.Tuple<Vector3, float>> res = new List<Agent.Tuple<Vector3, float>>();
        res.Add(new Agent.Tuple<Vector3, float>(randPos, Random.Range(.5f, 3f)));
        for (int i = 1; i < len; i++) {
            var pos = new Vector3(Random.Range(-1f, 1f) * maxX, 1, Random.Range(-1f, 1f) * maxZ);
            var dur = Random.Range(.5f, 3f);
            res.Add(new Agent.Tuple<Vector3, float>(pos, dur));
        }

        return res;
    }

    // numbers suggested by https://www.sciencedirect.com/science/article/pii/S0925753520302630#e0030
    void UpdateIndoorParticles()
    {
        for (int j = 1; j < 12; j++) // room number
        {
            for (int i = 0; i < numAgents; i++) // agent
            {
                // 5 particles per second
                if (agents[i].tag == "Infected" && GetRoom(agents[i].transform)==j)
                {
                    rooms[j] += 500f*Time.deltaTime; // virus particles increase by this value
                }
            }
            rooms[j] -= 1f/360f*Time.deltaTime; // virus particles die over time according to this value (about 3 hours halflife translated to deltatime)
            rooms[j] = Mathf.Max(rooms[j],0f);
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

}
