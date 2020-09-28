using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelPosition : MonoBehaviour
{
    GameObject parent;
    void Start()
    {
        parent = gameObject.transform.root.gameObject;
        if(parent.transform.position.x > 0)
        {
            gameObject.transform.localPosition = new Vector3(292, -192, 0);
        }
        else
        {
            gameObject.transform.localPosition = new Vector3(-292, -192, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
