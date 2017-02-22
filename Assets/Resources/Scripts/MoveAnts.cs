using UnityEngine;
using System.Collections;

public class MoveAnts : MonoBehaviour {

	void Awake() {
		GameObject[] units = GameObject.FindGameObjectsWithTag ("unit");
		GameObject[] positions = GameObject.FindGameObjectsWithTag ("position");
		foreach (GameObject unit in units) {			
			int positionPoint = Random.Range (0, positions.Length);
			Transform t = positions [positionPoint].transform;
			UnityEngine.AI.NavMeshAgent agent = unit.GetComponent<UnityEngine.AI.NavMeshAgent> ();
			agent.SetDestination (t.position);
			unit.GetComponent<Animation> ().CrossFade ("ant-walk");
		}
	}

}
