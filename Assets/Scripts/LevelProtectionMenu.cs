using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Simulation;

public class LevelProtectionMenu : MonoBehaviour
{

    public GameObject currSimulation;
    private List<string> options;

    void Start()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        options = new List<string>();

        options.Add("None");
        options.Add("Some distancing");
        options.Add("Mask + distancing");


        foreach(var option in options)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = option });
        }

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
    }

    void DropdownItemSelected(Dropdown dd)
    {
        currSimulation.GetComponent<Simulation>().distancingMeasure = (ProtectiveMeasure) dd.value;
    }

    void Update()
    {

    }
}
