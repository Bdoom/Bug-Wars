using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldHandler : NetworkBehaviour
{

    /// <summary>
    /// ArrayList which holds GameObjects for the Units
    /// </summary>
    public static ArrayList unitsSelected = new ArrayList();
	public static ArrayList firstControlGroup = new ArrayList ();

    public static bool isFirstControlGroupActive = false;

    // Resources and Units
	public float resourcesPerFive = 3f; // may change dependent on buildings as well as game balance. For some reason UpdateResources() is called twice so when you put 3 it will give 6. 
	public float resourcesCount = 0;

	public int maxUnits =  50; // may change dependent on buildings built later in the game. (originally was 25, changed to 50 on 4/14/2016 for fun)

    // Mouse Buttons
    public static float LEFT_CLICK = 0;
    public static float RIGHT_CLICK = 1;
    public static float MIDDLE_CLICK = 2;
    // End Mouse Buttons 

    public static float CAMERA_Y = 43.2f; //replace inside of all camera changing scripts (Beatle.cs, BasicAnt.cs, Anthill.cs update functions); replace the y value with this.

    private Text resourceCountText;

    public float BasicAntCost = 15;
    public float BasicBeatleCost = 25;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // If less than one anthill, pause game, if more than 1, unpause the game.
        if (GameObject.FindGameObjectsWithTag("anthill").Length < 2)
        {
            PauseGame();
        }
        else if (GameObject.FindGameObjectsWithTag("anthill").Length > 1)
        {
            UnPauseGame();
        }

        if (GameObject.Find("Canvas").transform.FindChild("Defeat").gameObject.activeSelf)
        {
            SceneManager.LoadScene("Defeat");
        }

        if (GameObject.Find("Canvas").transform.FindChild("Victory").gameObject.activeSelf)
        {
            SceneManager.LoadScene("Victory");
        }

		UpdateControlGroups ();
        
    }

	[Command]
	public void Cmd_disableUnit(GameObject unit) {
		Rpc_disableUnit (unit);
	}

	[ClientRpc]
	public void Rpc_disableUnit(GameObject unit) {
		unit.SetActive (false);
	}





	/// <summary>
	/// Used for binding a set of units to a key for easy, and fast unit selection.
    /// Must also update deselectUnits() to not include any other control groups you wish to add, it is only setup for "firstControlGroup" for now.
	/// </summary>
	private void UpdateControlGroups() {

        if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.B))
        { // For control groups
            Debug.Log(firstControlGroup.Count);

            
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {

                firstControlGroup = new ArrayList(unitsSelected);
                
            }

        }

		if (Input.GetKeyDown (KeyCode.Alpha1) && firstControlGroup.Count > 0) {  // Moves camera to first unit in control group one, add double tapping and auto selection of the unit.
            Debug.Log(firstControlGroup.Count);
            isFirstControlGroupActive = true;
			GameObject firstUnit = firstControlGroup [0] as GameObject;
			Camera.main.transform.position = new Vector3 (firstUnit.transform.position.x, CAMERA_Y, firstUnit.transform.position.z);


			foreach (GameObject unit in unitsSelected) {
				GameObject indicator = unit.transform.FindChild ("Indicator").gameObject;
				indicator.SetActive (false);
				if (unit.GetComponent<BasicAnt> () != null) {
					unit.GetComponent<BasicAnt> ().isUnitSelected = true;
				}
				if (unit.GetComponent<Beatle> () != null) {
					unit.GetComponent<Beatle> ().isUnitSelected = true;
				}
			}


			foreach (GameObject unit in firstControlGroup) {
				GameObject indicator = unit.transform.FindChild("Indicator").gameObject;
                indicator.SetActive(true);
				if (unit.GetComponent<BasicAnt> () != null) {
					unit.GetComponent<BasicAnt> ().isUnitSelected = true;
				}
				if (unit.GetComponent<Beatle> () != null) {
					unit.GetComponent<Beatle> ().isUnitSelected = true;
				}
			}
		}


	}


    // Beatle Battle Code

    [Command]
    public void Cmd_FireProjectile(GameObject beatle, Vector3 target)
    {
		beatle.GetComponent<NavMeshAgent> ().ResetPath (); // Resets the path


        if (timer >= beatle.GetComponent<Beatle>().AttackSpeed)
        {
            timer = 0;
        }
        else
        {
            return;
        }

        fireAfterWait(beatle, target);
    }

    private void fireAfterWait(GameObject beatle, Vector3 target)
    {


        GameObject bullet = Resources.Load("Prefabs/bomb") as GameObject; // bullet prefab

        GameObject bulletToSpawn = (GameObject)Instantiate(bullet, beatle.transform.position, beatle.transform.rotation); // antstospawn

        // Set after it is spawned
        bulletToSpawn.GetComponent<FireBeatleBullet>().positionToFireAt = target; // spawns the bullet and then feeds it a vector3 of the position of the ant its being fired at (this ant).

        NetworkServer.SpawnWithClientAuthority(bulletToSpawn, WorldHandler.findLocalPlayer());

    }

    // End Beatle Battle Code


    public static bool isAntHillSelected()
    {
        GameObject[] anthills = GameObject.FindGameObjectsWithTag("anthill");
        foreach (GameObject anthill in anthills)
        {
            GameObject Indicator = anthill.transform.FindChild("Indicator").gameObject;
            if (anthill.GetComponent<NetworkIdentity>().isLocalPlayer && Indicator.activeSelf)
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Deselects the ant hill.
    /// </summary>
    public static void deselectAntHill()
    {
        GameObject[] anthills = GameObject.FindGameObjectsWithTag("anthill");
        foreach (var anthill in anthills)
        {
            if (anthill.GetComponent<NetworkIdentity>().hasAuthority)
            {
                anthill.transform.FindChild("Indicator").gameObject.SetActive(false);
                anthill.GetComponent<Anthill>().isAntHillSelected = false;
            }
        }
    }

    /// <summary>
    /// Hides the anthill info.
    /// </summary>
    public static void hideAnthillInfo()
    {
        GameObject anthillinfo = GameObject.Find("Canvas").transform.FindChild("SpawnAttackAnt").gameObject;
        GameObject beatleUnitInfo = GameObject.Find("Canvas").transform.FindChild("SpawnBeatleUnit").gameObject;

        anthillinfo.SetActive(false);
        beatleUnitInfo.SetActive(false);

    }


    /// <summary>
    /// Checks the game state
    /// </summary>
    /// <returns><c>true</c>, if the game is paused, <c>false</c> if the game is not paused..</returns>
    public static bool isGamePaused()
    {

        if (Time.timeScale == 0)
        {
            return true;
        }
        if (Time.timeScale == 1)
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Finds the local player.
    /// </summary>
    /// <returns>The local player.</returns>
    public static GameObject findLocalPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("anthill");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                return player;
            }
        }

        Debug.LogWarning("No Local Player Found!");
        return null;
    }

    /// <summary>
    /// Counts the units, owned by the player.
    /// </summary>
    public static int countUnits()
    {
        int unitsCount = 0;
        foreach (var g in GameObject.FindGameObjectsWithTag("unit"))
        {
            if (g.GetComponent<NetworkIdentity>() != null)
            { // Added with addition of beatle.
                if (g.GetComponent<NetworkIdentity>().hasAuthority)
                { //if unit is owned by the player increase the max units counter.
                    unitsCount++;
                }
            }
        }
        return unitsCount;
    }

    /// <summary>
    /// Counts client owned units as array.
    /// </summary>
    /// <returns>The units as array.</returns>
    public static ArrayList countUnitsAsArray()
    {
        ArrayList units = new ArrayList();

		foreach (GameObject g in GameObject.FindGameObjectsWithTag("unit"))
        {
            if (g.GetComponent<NetworkIdentity>() != null)
            { // added with addition of Beatle.
                if (g.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    units.Add(g);
                }
            }
        }

        return units;
    }

    /// <summary>
    /// Rpc to despawn object
    /// </summary>
    /// <param name="obj"></param>
    [ClientRpc]
    public void Rpc_DespawnObject(GameObject obj)
    {
        NetworkServer.UnSpawn(obj);
    }


    /// <summary>
    /// Destroys all flag objects on screen. (For the client)
    /// </summary>
    public static void DestroyFlags()
    {
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
    }

    /// <summary>
    /// Gets the beatle units.
    /// </summary>
    /// <returns>The beatle units.</returns>
    public static ArrayList getBeatleUnitsAsArray()
    {
        ArrayList units = new ArrayList();

        foreach (var g in GameObject.FindGameObjectsWithTag("unit"))
        {
            if (g.GetComponent<NetworkIdentity>() != null)
            {
                if (g.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    if (g.name.Contains("Beatle"))
                    {
                        units.Add(g);
                    }
                }
            }
        }

        return units;
    }

    /// <summary>
    /// Checks to see if a beatle unit is selected
    /// </summary>
    /// <returns><c>true</c>, if a beatle unit selected is selected, <c>false</c> otherwise.</returns>
    public static bool isBeatleUnitSelected()
    {

		if (unitsSelected.Count == 0) {
			return false;
		}


        foreach (GameObject g in unitsSelected)
        {

            if (g.GetComponent<NetworkIdentity>() != null)
            {
                if (g.GetComponent<NetworkIdentity>().hasAuthority)
                {
					if (g.GetComponent<Beatle> ()) {
						return true;
					}
                }
            }
        }

        return false;

    }

    /// <summary>
    /// Checks if a ant unit is selected and is inside of the UnitsSelected array. This function and isBeatleUnitSelected() need to be cleaned up a bit though.
    /// </summary>
    /// <returns>true, if there is a ant unit inside of the untisSelected array</returns> false, otherwise.
    public static bool isAntUnitSelected()
    {
        foreach (GameObject g in unitsSelected)
        {
            if (g.GetComponent<NetworkIdentity>() != null)
            {
                if (g.GetComponent<NetworkIdentity>().hasAuthority)
                {
                    if (g.name.Contains("Ant"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    /// <summary>
    /// Returns true if the shift keys are down, returns false if neither are held down.
    /// </summary>
    /// <returns><c>true</c>, if shift down was held down, <c>false</c> otherwise.</returns>
    public static bool isShiftDown()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Deselect all units in the unitsSelected ArrayList 
    /// </summary>
    public static void deselectUnits()
    {
        foreach (GameObject g in unitsSelected)
        {
            Debug.Log(g.name);
            g.transform.FindChild("Indicator").gameObject.SetActive(false);
            if (g.GetComponent<BasicAnt>() != null)
            {
                g.GetComponent<BasicAnt>().isUnitSelected = false;
            }
            if (g.GetComponent<Beatle>() != null)
            {
                g.GetComponent<Beatle>().isUnitSelected = false;
            }
        }

        foreach (GameObject g in firstControlGroup)
        {
            Debug.Log(g.name);
            g.transform.FindChild("Indicator").gameObject.SetActive(false);
            if (g.GetComponent<BasicAnt>() != null)
            {
                g.GetComponent<BasicAnt>().isUnitSelected = false;
            }
            if (g.GetComponent<Beatle>() != null)
            {
                g.GetComponent<Beatle>().isUnitSelected = false;
            }

        }

        isFirstControlGroupActive = false;
        unitsSelected.Clear();
    }

    /// <summary>
    /// Sets Time.timeScale to 0
    /// </summary>
    public static void PauseGame()
    {
        GameObject.Find("Canvas").transform.FindChild("Pause").gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    /// <summary>
    /// Returns the enemy anthill as a GameObject
    /// </summary>
    /// <returns>The enemy ant hill.</returns>
    public static GameObject findEnemyAntHill()
    {

        GameObject[] anthills = GameObject.FindGameObjectsWithTag("anthill");

        foreach (GameObject anthill in anthills)
        {
            if (!anthill.GetComponent<NetworkIdentity>().hasAuthority)
            {
                return anthill;
            }
        }

        return null;
    }



    /// <summary>
    /// Sets Time.timeScale to 1
    /// </summary>
    public static void UnPauseGame()
    {
        GameObject.Find("Canvas").transform.FindChild("Pause").gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    [Client]
    public static void PlayUnitBattleSound()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/BattleSounds");
        // note sure if Resources.loadAll is faster than using an array.
        //string[] clipNames = { "Attack", "DontGiveThemAmilimeter", "ForQueenAndColony", "ForTheColony", "ForwardCrawl", "HeadingTowardTarget", "LeaveNoAntAlive", "LetsGetThem", "LetsMove", "MovingOut", "PointTheWay", "ReadyForAction", "Roger", "TodayIsAGoodDayToDie", "Understood", "WhatAreYourOrders", "WhatDoYouNeed", "WhatIsIt", "WhatIsIt_2", "YesSir" };
        int randomClip = Random.Range(0, clips.Length);
        //AudioClip clip = Resources.Load<AudioClip>("Audio/BattleSounds/" + clips[randomClip]);
        Debug.Log("Playing sound: " + clips[randomClip].name);
        AudioSource.PlayClipAtPoint(clips[randomClip], Vector3.zero);

    }


	[Command]
	public void Cmd_dealDamage(GameObject me, GameObject myEnemy) {
		Rpc_dealDamage (me, myEnemy);
	}

	[ClientRpc]
	public void Rpc_dealDamage(GameObject attackingUnit, GameObject enemy) {
		Debug.Log (enemy.name);

		if (enemy.GetComponent<BasicAnt> () && attackingUnit.GetComponent<BasicAnt> ()) {
			BasicAnt enemyAnt = enemy.GetComponent<BasicAnt> ();
			UnitIdentity enemyAntIdentity = enemy.GetComponent<UnitIdentity> ();
			BasicAnt myAnt = attackingUnit.GetComponent<BasicAnt> ();
			UnitIdentity myAntIdentity = attackingUnit.GetComponent<UnitIdentity> ();

			if (myAntIdentity.id == enemyAntIdentity.id) {
				return;
			}

			enemyAnt.health -= Random.Range (0, myAnt.damage);
			myAnt.health -= Random.Range (0, enemyAnt.damage);
		}


		if (enemy.GetComponent<Anthill> () && attackingUnit.GetComponent<BasicAnt> ()) {
			Anthill enemyAntHill = enemy.GetComponent<Anthill> ();
			BasicAnt myAnt = attackingUnit.GetComponent<BasicAnt> ();
			UnitIdentity antHillIdentity = enemy.GetComponent<UnitIdentity> ();
			UnitIdentity myAntIdentity = attackingUnit.GetComponent<UnitIdentity> ();

			Debug.Log ("shit works up to here1");

			if (myAntIdentity.id == antHillIdentity.id) {
				return;
			}
			Debug.Log ("shit works up to here2");

			enemyAntHill.health -= Random.Range (0, myAnt.damage);

			Debug.Log ("shit works up to here3");


		} else {
			Debug.Log (enemy.name);
			Debug.Log (attackingUnit.name);
		}
	}
		


    [Client]
    public static void PlayResourcesSound()
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/RequireMoreResources");
        Debug.Log("Playing: " + clip.name);
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }

    [Client]
    public static void PlayUnitCapSound()
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/MaximumUnits");
        Debug.Log("Playing: " + clip.name);
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }

    // For FireBeatleBullet.cs
    [ClientRpc]
    public void Rpc_DamageBasicAnt(GameObject ant, int damageToDeal, GameObject theBullet)
    {
        ant.GetComponent<BasicAnt>().health -= damageToDeal;
        Destroy(theBullet);
        // Play Particle effect for explosion and slow the movespeed of the ant

    }
    [ClientRpc]
    public void Rpc_DamageAntHill(GameObject anthill, int damageToDeal, GameObject theBullet)
    {
        anthill.GetComponent<Anthill>().health -= damageToDeal;
        Destroy(theBullet);
    }
    // End beatleBullet code



    #region helper functions for beatle and basic ant unit.
    // The helper functions may need to have their static functionality removed.

    /// <summary>
    /// Sets ResourcesPerFive (for this player).
    /// </summary>
    /// <param name="changeToWhat">Change to what.</param>
    public void setResourcesPerFive(int changeToWhat)
    {
        resourcesPerFive = changeToWhat;
    }

    // Basic Ant Setter Functions
    /// <summary>
    /// Sets the basic ant damage.
    /// </summary>
    /// <param name="changeToWhat">Change to what.</param>
    public static void setBasicAntDamage(int changeToWhat)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<BasicAnt>() != null)
            {
                unit.GetComponent<BasicAnt>().damage = changeToWhat;
                unitsChanged++;
            }
        }

        Debug.Log("Units Damage changed to: " + changeToWhat);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    /// <summary>
    /// Sets the basic ant attack speed.
    /// </summary>
    /// <param name="atkSpd">Atk spd.</param>
    public static void setBasicAntAttackSpeed(int atkSpd)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<BasicAnt>() != null)
            {
                unit.GetComponent<BasicAnt>().AttackSpeed = atkSpd;
                unitsChanged++;
            }
        }
        Debug.Log("Units AttackSpeed changed to: " + atkSpd);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    /// <summary>
    /// Sets the basic ant move speed.
    /// </summary>
    /// <param name="speed">Speed.</param>
    public static void setBasicAntMoveSpeed(int speed)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<BasicAnt>() != null)
            {
                unit.GetComponent<NavMeshAgent>().speed = speed;
                unitsChanged++;
            }
        }
        Debug.Log("Units Speed changed to: " + speed);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    /// <summary>
    /// Sets the basicAnt max health.
    /// </summary>
    /// <param name="maxHealth">Max health.</param>
    public static void setBasicAntMaxHealth(int maxHealth)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<BasicAnt>() != null)
            {
                unit.GetComponent<BasicAnt>().maxHealth = maxHealth;
                unitsChanged++;
            }
        }
        Debug.Log("Units MaxHealth changed to: " + maxHealth);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    // End BasicAnt Setter functions (not sure if I need to use SyncVar for some of these like the speed and damage? idk yet.

    // Beatle Setter Functions
    /// <summary>
    /// Sets all beatle damage.
    /// </summary>
    /// <param name="damage">Damage.</param>
    public static void setBeatleDamage(int damage)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<Beatle>() != null)
            {
                unit.GetComponent<Beatle>().damage = damage;
                unitsChanged++;
            }
        }

        Debug.Log("Units Damage changed to: " + damage);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    /// <summary>
    /// Sets the beatle attack speed.
    /// </summary>
    /// <param name="atkSpd">Atk spd.</param>
    public static void setBeatleAttackSpeed(int atkSpd)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<Beatle>() != null)
            {
                unit.GetComponent<Beatle>().AttackSpeed = atkSpd;
                unitsChanged++;
            }
        }
        Debug.Log("Units AttackSpeed changed to: " + atkSpd);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    /// <summary>
    /// Sets the beatle move speed.
    /// </summary>
    /// <param name="moveSpd">Move spd.</param>
    public static void setBeatleMoveSpeed(int moveSpd)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<Beatle>() != null)
            {
                unit.GetComponent<NavMeshAgent>().speed = moveSpd;
                unitsChanged++;
            }
        }
        Debug.Log("Units Speed changed to: " + moveSpd);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }

    public static void setBeatleMaxHealth(int maxHealth)
    {
        ArrayList allUnits = countUnitsAsArray();
        int unitsChanged = 0;
        foreach (GameObject unit in allUnits)
        {
            if (unit.GetComponent<Beatle>() != null)
            {
                unit.GetComponent<Beatle>().maxHealth = maxHealth;
                unitsChanged++;
            }
        }
        Debug.Log("Units MaxHealth changed to: " + maxHealth);
        Debug.Log("Number of Units changed: " + unitsChanged);
    }
    // End Beatle Setter functions

    #endregion

	/// <summary>
	/// Stops all units.
	/// I realized this function is fucking stupid so yeah. Would be better to make a function like "moveAllUnitsToAntHill" or something.
	/// </summary>
	public static void StopAllUnits() {
		ArrayList units = countUnitsAsArray ();
		foreach (GameObject unit in units) {
			if (unit.GetComponent<NavMeshAgent> () != null) {
				unit.GetComponent<NavMeshAgent> ().ResetPath ();
			}
		}
	}

}