using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Timers;

public class HUDController : NetworkBehaviour {

	private Text unitCapText;
	private Text gameTimerText;
	private Text resourcesCountText;

    // Game Time
    float gameTime;
    float minutes;
    float seconds;
    string timeFixed;
    // Don't touch!!!

	SyncListString chatLogs = new SyncListString ();

	// FPS Data
	public float FPS;
	public float deltaTime;
	public bool displayFrameRate;
	public Text fpsText;
	// end FPS Data

	GameObject inputField;

	GameObject scrollText; // for in game chat

    GameObject anthillInfo;

	GameObject EscapeMenu;

	WorldHandler localPlayer;

    bool isExitMenuOpen = false;

    // resources per five timers
    float timer = 0f;
    float timerMax = 5f;
    // end timers

    void Start () {
		localPlayer = WorldHandler.findLocalPlayer ().GetComponent<WorldHandler> ();
        anthillInfo = GameObject.Find("Canvas").transform.FindChild("SpawnAttackAnt").gameObject;
		fpsText = GameObject.Find ("FPSCounter").GetComponent<Text>();
		unitCapText = GameObject.Find ("UnitCap").transform.FindChild ("Text").GetComponent<Text> ();
		gameTimerText = GameObject.Find ("GameTimer").transform.FindChild ("Text").GetComponent<Text> ();
		resourcesCountText = GameObject.Find ("ResourcesCount").transform.FindChild ("Text").GetComponent<Text> ();
		scrollText = GameObject.Find ("ScrollBackground").transform.FindChild ("Text").gameObject;
		EscapeMenu = GameObject.Find ("Canvas").transform.FindChild ("EscapeMenu").gameObject;

		Cursor.lockState = CursorLockMode.Confined;

		chatLogs.Callback = OnChatUpdate; // chatlog 
		inputField = GameObject.Find("EnterMessage"); // of type InputField
	}

	void UpdateResources() {

		localPlayer.resourcesCount += localPlayer.resourcesPerFive;

	}



	void Update()
    {
        // Time does not start until Time.timeScale has been set to 1.
        gameTime += Time.deltaTime;
        minutes = Mathf.FloorToInt(gameTime / 60F);
        seconds = Mathf.FloorToInt(gameTime - minutes * 60);
        timeFixed = string.Format("{0:0}:{1:00}", minutes, seconds);
        // End time       
    
       timer += Time.deltaTime;
       if (timer >= timerMax)
       {
           if (isLocalPlayer)
           {
               UpdateResources();

               timer = 0;
           }
       }

       if (!WorldHandler.isBeatleUnitSelected() && !WorldHandler.isAntUnitSelected() && !WorldHandler.isAntHillSelected())
       {
           hideUnitInfo();
           anthillInfo.SetActive(false);
       }

       
		

		if (Input.GetKeyDown (KeyCode.Escape)) { // if the Escape key is pressed, unconfine the cursor from its state set in the Start() function
			Cursor.lockState = CursorLockMode.None;
			// Open Exit Menu
            isExitMenuOpen = !isExitMenuOpen;
            EscapeMenu.SetActive(isExitMenuOpen);
            
		}

		updateHUD ();


		if (!isLocalPlayer)
			return;
		

		if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter) && EventSystem.current.currentSelectedGameObject.name == "EnterMessage") {
			
			InputField theInputField = inputField.GetComponent<InputField> ();


			if (!string.IsNullOrEmpty (theInputField.text)) {
				string txtToSend = theInputField.text;



				if (txtToSend == "cls") {
					scrollText.GetComponent<Text> ().text = "";
				}

				if (GetComponent<UnitIdentity> ().id == 0) {
					Cmd_SendMessageToAllClients ("<color=#ff0000ff>Player " + GetComponent<UnitIdentity>().id + "</color>: " + txtToSend);
				}
				if (GetComponent<UnitIdentity> ().id == 1) {
					Cmd_SendMessageToAllClients ("<color=#0000ffff>Player " + GetComponent<UnitIdentity>().id + "</color>: " + txtToSend);
				}

				theInputField.text = "";
			}
		}


		if (inputField.GetComponent<InputField> ().isFocused) {
			Camera.main.GetComponent<CameraController> ().enabled = false;
		} else {
			Camera.main.GetComponent<CameraController> ().enabled = true;
		}

		// Update FPS Counter

		if (Input.GetKey (KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) {
			
			if(Input.GetKeyDown(KeyCode.F))
				displayFrameRate = !displayFrameRate;
		}

		if (displayFrameRate) {

            if (float.IsInfinity(FPS))
            {
                fpsText.text = "Game is paused.";
            }
            else
            {
                // Round FPS
                FPS = Mathf.RoundToInt(FPS);
                fpsText.text = "FPS: " + FPS;
            }

		} else {
			fpsText.text = "";
		}

		deltaTime += Time.deltaTime;
		deltaTime /= 2.0f;
		FPS = 1.0f / deltaTime;
		// End Update FPS Counter
        // Sync timer and resource gathering

    }


    /// <summary>
    /// Hides unit info on the HUD
    /// </summary>
    public void hideUnitInfo()
    {
        GameObject unitInfo = GameObject.Find("Canvas").gameObject.transform.FindChild("UnitInfo").gameObject;
        unitInfo.SetActive(false);
    }


	// Start Game chat
	[Client]
	private void OnChatUpdate(SyncListString.Operation op, int index) {
		if (chatLogs.Count == 0) {
			scrollText.GetComponent<Text> ().text = "";
		}

		scrollText.GetComponent<Text> ().text += chatLogs [chatLogs.Count - 1] + "\n";
	}

	[Command]
	void Cmd_SendMessageToAllClients(string txtToSend) {
		if (txtToSend.Equals ("cls")) {
			chatLogs.Clear ();
			return;
		}

		chatLogs.Add(txtToSend);
	}
	// End Game chat

		

	/// <summary>
	/// Replaces OnGUI
	/// </summary>
	private void updateHUD() {
		unitCapText.text = "Unit Cap : " + WorldHandler.countUnits () + "   / " + localPlayer.maxUnits;
		resourcesCountText.text = "Resources: " + localPlayer.resourcesCount;
        gameTimerText.text = timeFixed;

	}

}
