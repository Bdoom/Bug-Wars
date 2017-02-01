using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
 
public class CustomNetworkManager : NetworkManager
{
   
    public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
    {
         Vector3 startPos = GetStartPosition ().position;
 
        GameObject antHill = Resources.Load ("Prefabs/anthill") as GameObject;
        GameObject antHillToSpawn = (GameObject)Instantiate (antHill, startPos, Quaternion.identity);
 
		antHillToSpawn.GetComponent<UnitIdentity> ().id = conn.connectionId;
 
        GameObject ant = null;
        GameObject beatle = null;
 
 
 
        // Team Colors
        if (conn.connectionId == 0)
        {
            ant = Resources.Load("Prefabs/RedAnt") as GameObject;
            beatle = Resources.Load("Prefabs/RedBeatle") as GameObject;
 
        }
 
        if (conn.connectionId == 1)
        {
            ant = Resources.Load("Prefabs/BlueAnt") as GameObject;
            beatle = Resources.Load("Prefabs/BlueBeatle") as GameObject;
 
        }
        // End Team Colors
 
 
 
 
        if (ant == null || beatle == null)
            return;
 
        //GameObject myAntHill = WorldHandler.findLocalPlayer ();
 
 
        Vector3 antspawnLocation = new Vector3(antHillToSpawn.transform.position.x + 5, 0, antHillToSpawn.transform.position.z); // ant spawnlocation
        Vector3 beatleSpawnLocation = new Vector3(antHillToSpawn.transform.position.x - 5, 0, antHillToSpawn.transform.position.z); // beatle spawnlocation
 
        GameObject antToSpawn = (GameObject)Instantiate(ant, antspawnLocation, Quaternion.identity); // ant to spawn
 
        GameObject beatleToSpawn = (GameObject)Instantiate(beatle, beatleSpawnLocation, Quaternion.identity); // beatle to spawn
 
		antToSpawn.GetComponent<UnitIdentity> ().id = conn.connectionId;
		beatleToSpawn.GetComponent<UnitIdentity> ().id = conn.connectionId;
 
 		

		NetworkServer.AddPlayerForConnection (conn, antHillToSpawn, playerControllerId);

        NetworkServer.SpawnWithClientAuthority(antToSpawn, antHillToSpawn);
        NetworkServer.SpawnWithClientAuthority(beatleToSpawn, antHillToSpawn);

		Camera.main.transform.position = new Vector3 (antHillToSpawn.transform.position.x, WorldHandler.CAMERA_Y, antHillToSpawn.transform.position.z);

    }
 
}