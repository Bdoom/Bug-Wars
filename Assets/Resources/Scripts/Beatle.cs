using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Beatle : NetworkBehaviour
{

	[SyncVar(hook="OnHealthChanged")]
	public float health;

	//[SyncVar]
	//public int id;

	public float maxHealth;

    public bool isUnitSelected;

    public int damage;

    private GameObject unitInfo;

    public float Range = 20f; // Vector3.Distance(this, that)  (was 4.3f)
    public float AttackSpeed = 0.5f; // attack speed of the unit

    public Vector3 beatlePosition;

	public GameObject healthBar;

	private float timer;


	private void OnHealthChanged(float newHealth) {
		health = newHealth;
		float CalculatedHealth = newHealth / maxHealth;
		healthBar.transform.localScale = new Vector3 (CalculatedHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
	}


    void Start()
    {

		//healthBar = transform.FindChild ("Canvas").FindChild ("HealthBar").gameObject;

        unitInfo = GameObject.Find("Canvas").gameObject.transform.FindChild("UnitInfo").gameObject;
		maxHealth = 50;
		health = 50;
        damage = 20;
        Range = 20f;
        AttackSpeed = 0.5f;



        if (gameObject.name.Contains("RedBeatle(Clone)") || gameObject.name.Contains("BlueBeatle(Clone)"))
        {
            gameObject.name = "Beatle";
        }
    }

	void OnTriggerStay(Collider other) {

		if (other.GetComponent<NetworkIdentity>() != null)
		{
			NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
			if (identity.isLocalPlayer)
			{
				return;
			}
			if (identity.hasAuthority)
			{
				return;
			}
		}

		if (!hasAuthority)
		{
			return;
		}

		if(other.GetComponent<BasicAnt>() == null) {
			return;
		}
			

		if(timer >= other.GetComponent<BasicAnt>().AttackSpeed) {
			timer = 0;
		} else {
			return;
		}



		if (other.GetComponent<BasicAnt> () != null) {
			int damageToDealToBeatle = Random.Range(0, damage);
			Cmd_TakeDamage (damageToDealToBeatle);
		}

	}

	[Command]
	void Cmd_TakeDamage(int amount) {
		Rpc_TakeDamage (amount);
	}

	[ClientRpc]
	void Rpc_TakeDamage(int amount) {
		health -= amount;
	}

    // Update is called once per frame
    void Update()
    {
        beatlePosition = new Vector3(transform.position.x, WorldHandler.CAMERA_Y, transform.position.z); // not sure if i should add a transform.hasChanged test so it doesnt call this every frame?
		timer += Time.deltaTime;



        if (health <= 0)
        {
            gameObject.SetActive(false);
            WorldHandler.unitsSelected.Remove(gameObject);
            isUnitSelected = false;
            WorldHandler.DestroyFlags();
            GetComponent<Collider>().isTrigger = false;
			if (isServer)
				WorldHandler.findLocalPlayer().GetComponent<WorldHandler>().Rpc_DespawnObject(gameObject);
        }

        if (isUnitSelected)
        {
            displayBeatleInfo();
        }

        if (beatlePosition != Vector3.zero)
        { // when space is pressed, move camera to the anthill
            if (Input.GetKey(KeyCode.Space) && isUnitSelected)
            {
                Camera.main.transform.position = beatlePosition;
                Debug.Log(beatlePosition);
            }
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



    void displayBeatleInfo()
    {
        Text unitInfoText = unitInfo.GetComponent<Text>();
        unitInfo.SetActive(true);
        unitInfoText.text = "Unit: " + gameObject.name + "\n Health: " + health;
    }


}
