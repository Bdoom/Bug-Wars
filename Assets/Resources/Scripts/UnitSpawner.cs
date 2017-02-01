using UnityEngine;
using System.Collections;

/** 
 ** This class loads up the game units and then places them on the map depdendent on where the mouse is pointing.
**/

public class UnitSpawner : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {

            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 1000f))
            {
                

            }           
        }
	}
}
