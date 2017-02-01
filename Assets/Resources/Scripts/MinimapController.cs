using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class MinimapController : NetworkBehaviour {

    private Camera minimapCamera;

    void Start()
    {
        minimapCamera = this.GetComponent<Camera>();
    }

    void Update () {


		if (minimapCamera.pixelRect.Contains (Input.mousePosition) && Input.GetMouseButton (0)) {
			Ray MouseRay = minimapCamera.ScreenPointToRay (Input.mousePosition);
			Camera.main.transform.position = MouseRay.origin;
			Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, 43.2f, Camera.main.transform.position.z);
		}

		if (EventSystem.current.IsPointerOverGameObject ()) {
			if (minimapCamera.pixelRect.Contains (Input.mousePosition) && Input.GetMouseButtonDown (1)) {
				Vector3 destination = minimapCamera.ScreenPointToRay (Input.mousePosition).origin;
				Vector3 newDestination = new Vector3 (destination.x, 0, destination.z);

				foreach (GameObject g in WorldHandler.unitsSelected) {

					// If network client does not own the object, do not allow it to be moved.
					if (!g.GetComponent<NetworkIdentity> ().hasAuthority) {
						return;
					}

					// Check to see if a flag is on the minimap
					if (GameObject.FindWithTag ("flag") != null) {
						// if a flag exists, grab all of them 
						GameObject[] flags = GameObject.FindGameObjectsWithTag ("flag");

						// loop through each of them and destroy them
						foreach (GameObject mapflag in flags) {
							Destroy (mapflag);
						}
					}

					NavMeshAgent agent = g.GetComponent<NavMeshAgent> ();
					agent.SetDestination (newDestination);

					if (g.GetComponent<BasicAnt> () != null) {
						g.GetComponent<Animation> ().CrossFade ("ant-walk");
					}
					if (g.GetComponent<Beatle> () != null) {
						g.GetComponent<Animation> ().CrossFade ("walk");
					}

					GameObject flag = Resources.Load ("Prefabs/flag2.0") as GameObject;
					Instantiate (flag, new Vector3 (newDestination.x, 0, newDestination.z), Quaternion.identity);



				}
			}
		}
    }


}
