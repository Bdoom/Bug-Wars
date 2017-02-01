using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BasicAnt : NetworkBehaviour
{ // changed class name from Unit.cs to BasicAnt.cs

	[SyncVar(hook="OnHealthChanged")]
	public float health;

	//[SyncVar]
	//public int id;

	public float maxHealth;

    public bool isUnitSelected;

    public int damage;
    public int AttackSpeed;

    private GameObject unitInfo;

    public Vector3 antPosition;

    public float timer;

	public GameObject healthBar;

	private WorldHandler localPlayer;

	private void OnHealthChanged(float newHealth) {
		health = newHealth;
		float CalculatedHealth = newHealth / maxHealth;
		Debug.Log ("basic ant health:"  + newHealth);
		healthBar.transform.localScale = new Vector3 (CalculatedHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
	}


    void Start()
    {
		//healthBar = transform.FindChild ("Canvas").FindChild ("HealthBar").gameObject;

		localPlayer = WorldHandler.findLocalPlayer ().GetComponent<WorldHandler> ();

        unitInfo = GameObject.Find("Canvas").gameObject.transform.FindChild("UnitInfo").gameObject;
		maxHealth = 100;
		health = 100;
        damage = 15;
        AttackSpeed = 2;

        if (gameObject.name.Contains("RedAnt(Clone)") || gameObject.name.Contains("BlueAnt(Clone)"))
        {
            gameObject.name = "Ant";
        }

    }


    // Update is called once per frame
    void Update()
    {
        antPosition = new Vector3(transform.position.x, 43.2f, transform.position.z); // not sure if i should add a transform.hasChanged test so it doesnt call this every frame?
        timer += Time.deltaTime;

        if (health <= 0)
        {
            //gameObject.SetActive(false);
            WorldHandler.unitsSelected.Remove(gameObject);
            isUnitSelected = false;
            WorldHandler.DestroyFlags();
            GetComponent<Collider>().isTrigger = false;
			localPlayer.Cmd_disableUnit (gameObject);
        }

        if (isUnitSelected)
        {
            displayAntInfo();
        }

        if (antPosition != Vector3.zero)
        { // when space is pressed, move camera to the anthill
            if (Input.GetKey(KeyCode.Space) && isUnitSelected)
            {
                Camera.main.transform.position = antPosition;
            }
        }

    }

    /// <summary>
    /// If your mouse is being right clicked over an ant you don't own and you have a beatle unit selected, check the range to see if it is less than BEATLE_RANGE, if it is less, stop the Navigation and fire projectile.
    /// </summary>
    void OnMouseOver()
    { // For Beatle Ranged Attacks
        // setup TImer for atk Spd

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
                            localPlayer.Cmd_FireProjectile(beatle, transform.position);


                        }
                    }
                } // end beatle ranged units
            }
        }
    }




    void OnTriggerStay(Collider other)
    {

	
		if (other.GetComponent<UnitIdentity> () == null) {
			return;
		}

		if (other.GetComponent<UnitIdentity> ().id == GetComponent<UnitIdentity> ().id) {
			return;
		}



		if(timer >= AttackSpeed) {
			timer = 0;
		} else {
			return;
		}



		if (other.tag == "anthill")
		{

			if (other.GetComponent<Anthill> ().health <= 0) {
				GameObject.Find ("Canvas").transform.FindChild ("Victory").gameObject.SetActive (true);
			} else {
				localPlayer.Cmd_dealDamage (gameObject, other.gameObject);
			}
				
			Debug.Log ("anthill detected");
		}
			

        if (other.tag == "unit")
        {
			

            
            if (other.GetComponent <Beatle>())
            {
                other.GetComponent<Animation>().CrossFade("fire");
            }

			if (other.GetComponent<BasicAnt>())
            {
                other.GetComponent<Animation>().CrossFade("ant-bite");
            }
			Debug.Log ("works");

			if (localPlayer == null) {
				localPlayer = WorldHandler.findLocalPlayer ().GetComponent<WorldHandler> ();
			}

			localPlayer.Cmd_dealDamage (gameObject, other.gameObject);

           
        }

		if (other.GetComponent<UnitIdentity> ().id != GetComponent<UnitIdentity> ().id && other.GetComponent<Anthill>() == null) {

			other.transform.LookAt (transform);
			transform.LookAt (other.transform);

		}
    }



   

    void OnMouseDown()
    {

        if (!hasAuthority)
        {
            return;
        }


        if (!WorldHandler.isShiftDown())
        {
            WorldHandler.deselectAntHill();
            WorldHandler.deselectUnits();
        }

        GameObject indicator = gameObject.transform.FindChild("Indicator").gameObject;

        indicator.SetActive(!indicator.activeSelf);
        isUnitSelected = indicator.activeSelf;

        if (isUnitSelected)
        {
            WorldHandler.PlayUnitBattleSound();
            WorldHandler.unitsSelected.Add(gameObject);

        }
        else
        {
            WorldHandler.unitsSelected.Remove(gameObject);
        }

    }




    void displayAntInfo()
    {
        Text unitInfoText = unitInfo.GetComponent<Text>();
        unitInfo.SetActive(true);
        unitInfoText.text = "Unit: " + gameObject.name + "\n Health: " + health;
    }



}
