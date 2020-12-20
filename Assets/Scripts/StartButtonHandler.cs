using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButtonHandler : MonoBehaviour
{

    public GameObject currSimulation;
    public Dropdown percentInfected;
    public Dropdown percentDistancing;
    public Dropdown levelProtection;


    public void clickedButton()
    {
        currSimulation.GetComponent<Simulation>().startSimulation();
        gameObject.GetComponent<Button>().interactable = false;
        percentInfected.interactable = false;
        percentDistancing.interactable = false;
        levelProtection.interactable = false;
    }
}
