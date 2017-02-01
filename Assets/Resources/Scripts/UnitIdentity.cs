using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class UnitIdentity : NetworkBehaviour {

	[SyncVar]
	public int id;

}
