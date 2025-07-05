using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotController : MonoBehaviour
{

    public void activate() {
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void deactivate()
    {
        this.GetComponent<Renderer>().material.color = Color.gray;
    }

    public void block() {
        this.GetComponent<Renderer>().material.color = Color.red;
    }

    public void guess(bool correct) {
        if (correct)
        {
            this.GetComponent<Renderer>().material.color = Color.green;
        }
        else {
            this.GetComponent<Renderer>().material.color = Color.red;
        }
    }

}
