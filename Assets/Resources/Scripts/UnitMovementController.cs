using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

/**
**  This script will be turned on when we are not building buildings and only using movement of units, MouseController.cs will be on when we are placing buildings, or units.
**  This script is for movement of units from their current point to a point determined by the users mouse position.
**/

public class UnitMovementController : NetworkBehaviour
{

    void Update()
    {

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mousePos = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit hit;


                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    if (WorldHandler.unitsSelected.Count > 0)
                    {
                        foreach (GameObject g in WorldHandler.unitsSelected)
                        {

                            // If network client does not own the object, do not allow it to be moved.
                            if (!g.GetComponent<NetworkIdentity>().hasAuthority)
                            {
                                return;
                            }

                            // Check to see if a flag is on the minimap
                            // TODO : Add shift clicking to set multiple waypoints.
                            if (GameObject.FindWithTag("flag") != null)
                            {
                                // if a flag exists, grab all of them 
                                GameObject[] flags = GameObject.FindGameObjectsWithTag("flag");

                                // loop through each of them and destroy them
                                foreach (GameObject mapflag in flags)
                                {
                                    Destroy(mapflag);
                                }
                            }



                            SetDestination(g, hit.point);
                            if (g.GetComponent<BasicAnt>() != null)
                            {
                                g.GetComponent<Animation>().CrossFade("ant-walk");
                            }
                            if (g.GetComponent<Beatle>() != null)
                            {
                                g.GetComponent<Animation>().CrossFade("walk");
                            }

                            //Debug.Log (hit.point);

                            GameObject flag = Resources.Load("Prefabs/flag2.0") as GameObject;
                            Instantiate(flag, new Vector3(hit.point.x, 0, hit.point.z), Quaternion.identity);



                        }
                    }

                    if (WorldHandler.firstControlGroup.Count > 0 && WorldHandler.isFirstControlGroupActive)
                    {
                        foreach (GameObject g in WorldHandler.firstControlGroup)
                        {

                            // If network client does not own the object, do not allow it to be moved.
                            if (!g.GetComponent<NetworkIdentity>().hasAuthority)
                            {
                                return;
                            }

                            // Check to see if a flag is on the minimap
                            // TODO : Add shift clicking to set multiple waypoints.
                            if (GameObject.FindWithTag("flag") != null)
                            {
                                // if a flag exists, grab all of them 
                                GameObject[] flags = GameObject.FindGameObjectsWithTag("flag");

                                // loop through each of them and destroy them
                                foreach (GameObject mapflag in flags)
                                {
                                    Destroy(mapflag);
                                }
                            }



                            SetDestination(g, hit.point);
                            if (g.GetComponent<BasicAnt>() != null)
                            {
                                g.GetComponent<Animation>().CrossFade("ant-walk");
                            }
                            if (g.GetComponent<Beatle>() != null)
                            {
                                g.GetComponent<Animation>().CrossFade("walk");
                            }

                            //Debug.Log (hit.point);

                            GameObject flag = Resources.Load("Prefabs/flag2.0") as GameObject;
                            Instantiate(flag, new Vector3(hit.point.x, 0, hit.point.z), Quaternion.identity);


                        }
                    }
                }
            }
        }
    }


    [Client]
    public void SetDestination(GameObject g, Vector3 destination)
    {
        NavMeshAgent agent = g.GetComponent<NavMeshAgent>();

        agent.SetDestination(destination);
    }


}
