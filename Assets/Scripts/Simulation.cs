using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Simulation : MonoBehaviour
{

    public Material HealthyMaterial;
    public Material InfectedMaterial;
    public GameObject agentPrefab;
    public GameObject locations;
    public GameObject agentParent;
    public float maxX;
    public float maxZ;
    public int numAgents;
    private GameObject[] agents;

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
        agents = new GameObject[numAgents];
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
            agentScript.infectionDuration = 7f;
            agentScript.willDie = Random.Range(1, 100) <= 30; //30% will die LOLLLL
            agentScript.healthyMaterial = HealthyMaterial;
            agentScript.infectedMaterial = InfectedMaterial;

            agents[i] = agent;
        }

        agents[0].tag = "Infected";
        agents[0].GetComponent<MeshRenderer>().material = InfectedMaterial;
        agents[0].GetComponent<Agent>().infectionTimer = agents[0].GetComponent<Agent>().infectionDuration;
    }

    // Update is called once per frame
    void Update()
    {
        
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

}
