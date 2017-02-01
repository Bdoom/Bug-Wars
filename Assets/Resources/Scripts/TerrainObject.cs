using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class TerrainObject : MonoBehaviour {

	void OnMouseDown() { // Change to OnMouseHover and check for clicks, then put a public static bool inside of dragselection script called "isDragging" if you are dragging none of this should apply
		if (!EventSystem.current.IsPointerOverGameObject ()) // If the mouse is not over an event system object (UI Object) and the user is pressing the left click
		{
				
				WorldHandler.deselectUnits ();	
				WorldHandler.deselectAntHill ();
				Cursor.lockState = CursorLockMode.Confined; // lock the cursor when the user clicks the terrain.
				WorldHandler.hideAnthillInfo ();
                WorldHandler.DestroyFlags();


		}
	}

}
