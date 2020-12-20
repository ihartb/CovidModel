using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PercentInfectedMenu : MonoBehaviour
{

    public GameObject currSimulation;
    private List<int> percents;
    
    void Start()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        percents = new List<int>();

        percents.Add(2);
        percents.Add(6);
        percents.Add(10);
        percents.Add(16);
        percents.Add(20);
        percents.Add(30);


        for (int i = 0; i < percents.Count; i++)
        {
            string txt = percents[i] + "% infected";
            dropdown.options.Add(new Dropdown.OptionData() { text = txt });
        }

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
    }

    void DropdownItemSelected(Dropdown dd)
    {
        currSimulation.GetComponent<Simulation>().percentInfected = percents[dd.value]/100f;
    }

    void Update()
    {

    }
}
