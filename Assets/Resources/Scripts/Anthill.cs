using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Anthill : NetworkBehaviour
{

	[SyncVar(hook="OnHealthChanged")]
	public float health;

	//[SyncVar]
	//public int id;

	public float maxHealth;

    public bool isAntHillSelected;

    public int damage;

    private GameObject spawnAttackAnt;
	private GameObject spawnBeatleUnit;
	private GameObject unitInfo;

	public GameObject healthBar;

    public Vector3 cameraAntHillPos = Vector3.zero;

	private float timer;

	private UnitIdentity unitIdentity;


	private void OnHealthChanged(float newHealth) {
		health = newHealth;
		float CalculatedHealth = newHealth / maxHealth;
		healthBar.transform.localScale = new Vector3 (CalculatedHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
		Debug.Log ("Anthill new health: " + newHealth);
	}

    void Start()
    {

		//healthBar = transform.FindChild ("Canvas").FindChild ("HealthBar").gameObject;

		unitIdentity = GetComponent<UnitIdentity> ();

        cameraAntHillPos = new Vector3(transform.position.x, 43.2f, transform.position.z);
		spawnAttackAnt = GameObject.Find("Canvas").transform.FindChild("SpawnAttackAnt").gameObject;
		spawnBeatleUnit = GameObject.Find ("Canvas").transform.FindChild ("SpawnBeatleUnit").gameObject;
		unitInfo = GameObject.Find ("Canvas").transform.FindChild ("UnitInfo").gameObject;

		spawnAttackAnt.GetComponent<Button>().onClick.AddListener(() => spawnBasicAntUnit());
		spawnBeatleUnit.GetComponent<Button> ().onClick.AddListener (() => spawnBeatle ());

		maxHealth = 200;
        health = 200;
        damage = 0;

        if (gameObject.name.Contains("anthill"))
        {
            gameObject.name = "Ant Hill";
        }

    }

    


    void Update()
    {

        if (!isLocalPlayer)
            return;

		timer += Time.deltaTime;


		// Set color of text based on team
		if (unitIdentity.id == 0) {
			unitInfo.GetComponent<Text> ().color = Color.red;
		} else if (unitIdentity.id == 1) {
			unitInfo.GetComponent<Text> ().color = Color.blue;
		}
		// End color setting.

        if (health <= 0)
        {
            GameObject.Find("Canvas").transform.FindChild("Defeat").gameObject.SetActive(true);
        }

        if (isAntHillSelected)
        {
			//spawnAttackAnt.transform.FindChild("UnitInfo").GetComponent<Text>().text = " Unit: " + gameObject.name + "\n Health: " + health;
			unitInfo.GetComponent<Text>().text = "Unit: " + gameObject.name + " \n Health: " + health;
			displayAntHillInfo();

            if (Input.GetKeyDown(KeyCode.Q)) // Hotkeys, q for basic ant, w for beatle unit.
            {
                spawnBasicAntUnit();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                spawnBeatle();
            }

        }
        else
        {
			/*spawnAttackAnt.SetActive(false);
			spawnBeatleUnit.SetActive (false);*/
            WorldHandler.hideAnthillInfo();
        }

        if (cameraAntHillPos != Vector3.zero)
        { // when space is pressed, move camera to the anthill
            if (Input.GetKey(KeyCode.Space) && isAntHillSelected)
            {
                Camera.main.transform.position = cameraAntHillPos;
            }
        }



    }


    void OnMouseDown()
    {

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (!GetComponent<NetworkIdentity>().hasAuthority)
        {
            return;
        }

        if (!WorldHandler.isShiftDown())
        {
            WorldHandler.deselectUnits();
        }


        GameObject indicator = gameObject.transform.FindChild("Indicator").gameObject;

        indicator.SetActive(!indicator.activeSelf);
        isAntHillSelected = indicator.activeSelf;




    }

    /// <summary>
    /// If your mouse is being right clicked over an ant you don't own and you have a beatle unit selected, check the range to see if it is less than BEATLE_RANGE, if it is less, stop the Navigation and fire projectile.
    /// </summary>
    void OnMouseOver()
    { // For Beatle Ranged Attacks
        // setup Timer for atk Spd


        if (Input.GetMouseButtonDown(1))
        {
            if (!hasAuthority)
            { // if it is not owned by you

                if (WorldHandler.isBeatleUnitSelected())
                { // for beatle ranged units
                    ArrayList beatles = WorldHandler.getBeatleUnitsAsArray();
                    foreach (GameObject beatle in beatles)
                    {
                        float range = beatle.GetComponent<Beatle>().Range;
                        Debug.Log("Beatle Range: " + range);
                        Debug.Log("Distance from ant and the beatle: " + Vector3.Distance(transform.position, beatle.transform.position));
                        if (Vector3.Distance(transform.position, beatle.transform.position) <= range)
                        {
                            WorldHandler localPlayer = WorldHandler.findLocalPlayer().GetComponent<WorldHandler>();
                            localPlayer.Cmd_FireProjectile(beatle, transform.position);


                        }
                    }
                } // end beatle ranged units
            }
        }
    }

    private void displayAntHillInfo()
    {
		spawnAttackAnt.SetActive(true);
		//spawnAttackAnt.transform.FindChild("UnitInfo").gameObject.SetActive(true);
		spawnBeatleUnit.SetActive (true);
		unitInfo.SetActive (true);
	}


	public void spawnBeatle() {
		if (hasAuthority)
		{
			WorldHandler localPlayer = WorldHandler.findLocalPlayer().GetComponent<WorldHandler>();

			if (WorldHandler.countUnits() < localPlayer.maxUnits)
			{ // if you have less units than the max number of units, it is okay to spawn more units.
				if (localPlayer.resourcesCount > localPlayer.BasicBeatleCost)
				{ // if you have enough unit space and you have enough resources
					Cmd_SpawnBeatleUnit(transform.position);

					localPlayer.resourcesCount -= localPlayer.BasicBeatleCost; // after spawning unit, spend 15 resources.
				}
				else if (localPlayer.resourcesCount < localPlayer.BasicBeatleCost)
				{
					WorldHandler.PlayResourcesSound(); // You require more resources (audio clip);
				}
			}
			else if (WorldHandler.countUnits() > localPlayer.maxUnits)
			{
				WorldHandler.PlayUnitCapSound(); // You require more unit Cap audio clip (this is if we decide to add a way for the player to increase the unit cap);
			}
		}
	}

    public void spawnBasicAntUnit()
    {
        if (hasAuthority)
        {
            WorldHandler localPlayer = WorldHandler.findLocalPlayer().GetComponent<WorldHandler>();

            if (WorldHandler.countUnits() < localPlayer.maxUnits)
            { // if you have less units than the max number of units, it is okay to spawn more units.
                if (localPlayer.resourcesCount > localPlayer.BasicAntCost)
                { // if you have enough unit space and you have enough resources
                    Cmd_SpawnAntUnit(transform.position);

                    localPlayer.resourcesCount -= localPlayer.BasicAntCost; // after spawning unit, spend 15 resources.
                }
                else if (localPlayer.resourcesCount < localPlayer.BasicAntCost)
                {
                    WorldHandler.PlayResourcesSound(); // You require more resources (audio clip);
                }
            }
            else if (WorldHandler.countUnits() > localPlayer.maxUnits)
            {
                WorldHandler.PlayUnitCapSound(); // You require more unit Cap audio clip (this is if we decide to add a way for the player to increase the unit cap);
            }
        }
    }

    [Command]
    public void Cmd_SpawnAntUnit(Vector3 position)
    {
        GameObject ant = Resources.Load("Prefabs/ant") as GameObject; // antprefab

        // Team Colors
		if (unitIdentity.id == 0)
        {
            ant = Resources.Load("Prefabs/RedAnt") as GameObject;
        }

		if (unitIdentity.id == 1)
        {
            ant = Resources.Load("Prefabs/BlueAnt") as GameObject;
        }
        // End Team Colors

        Vector3 antspawnLocation = new Vector3(position.x - Random.Range(-5, 10), 0, position.z); // ant spawnlocation
        GameObject antToSpawn = (GameObject)Instantiate(ant, antspawnLocation, Quaternion.identity); // antstospawn

		antToSpawn.GetComponent<UnitIdentity>().id = unitIdentity.id;

        //NetworkServer.Spawn(antToSpawn);
        NetworkServer.SpawnWithClientAuthority(antToSpawn, gameObject);
    }

	[Command]
	public void Cmd_SpawnBeatleUnit(Vector3 position) {
		GameObject beatle = Resources.Load("Prefabs/RedBeatle") as GameObject; // antprefab

		// Team Colors
		if (unitIdentity.id == 0)
		{
			beatle = Resources.Load("Prefabs/RedBeatle") as GameObject;
		}

		if (unitIdentity.id == 1)
		{
			beatle = Resources.Load("Prefabs/BlueBeatle") as GameObject;
		}
		// End Team Colors

		Vector3 beatleLocation = new Vector3(position.x - Random.Range(-5, 10), 0, position.z); // ant spawnlocation
		GameObject beatleToSpawn = (GameObject)Instantiate(beatle, beatleLocation, Quaternion.identity); // antstospawn

		beatleToSpawn.GetComponent<UnitIdentity>().id = unitIdentity.id;

		//NetworkServer.Spawn(beatleToSpawn);
		NetworkServer.SpawnWithClientAuthority(beatleToSpawn, gameObject);
	}

}

