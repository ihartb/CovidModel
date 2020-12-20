using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PercentDistancingMenu : MonoBehaviour
{

    public GameObject currSimulation;
    private List<int> percents;
    
    void Start()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        percents = new List<int>();

        percents.Add(100);
        percents.Add(90);
        percents.Add(80);
        percents.Add(74);
        percents.Add(50);
        percents.Add(24);
        percents.Add(0);

        for (int i = 0; i < percents.Count; i++)
        {
            string txt = percents[i] + "%";
            dropdown.options.Add(new Dropdown.OptionData() { text = txt });
        }

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
    }

    void DropdownItemSelected(Dropdown dd)
    {
        currSimulation.GetComponent<Simulation>().percentDistancing = percents[dd.value]/100f;
    }

    void Update()
    {

    }
}
