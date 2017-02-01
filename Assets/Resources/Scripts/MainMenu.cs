using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {

	public GameObject joinGameBtn;

	public RectTransform joinMenuPanel;

	public GameObject JoinMenuLoading;

	public GameObject networkIP;

	public GameObject mainMenu;
	public GameObject createMenu;
	public GameObject joinMenu;

	private Camera cam;
	private Animator anim;

	public GameObject video;
	public GameObject leftAnt;
	public GameObject rightAnt;
	public GameObject logo;
	public GameObject returnToMainMenuFromTutorial;
	public GameObject tutorialVideo;
	public GameObject pressSpaceToPlayTutorial;

	private Tutorial tutorial;


    void Awake()
    {
        Time.timeScale = 1f;
		cam = Camera.main;
		//mainMenu = GameObject.Find ("Main Camera").transform.FindChild("Canvas").transform.FindChild ("MainMenu").gameObject;
		//createMenu = GameObject.Find ("Canvas").transform.FindChild ("CreateMenu").gameObject;
		//joinMenu = GameObject.Find ("Canvas").transform.FindChild ("JoinMenu").gameObject;
		anim = cam.GetComponent<Animator> ();
		tutorial = tutorialVideo.GetComponent<Tutorial> ();
    }


	public void HostGame() {
		mainMenu.SetActive (false);
		createMenu.SetActive (true);
	}

	public void CreateRoom() {
		NetworkManager.singleton.maxConnections = 2;

        NetworkManager.singleton.StartHost();

	}

	public void PlayTutorial() {




		anim.SetBool ("playTutorial", true);
		//cam.transform.Translate (video.transform.position * Time.deltaTime);

		tutorialVideo.SetActive (true);
		pressSpaceToPlayTutorial.SetActive (true);

		returnToMainMenuFromTutorial.SetActive (true);
		leftAnt.SetActive (false);
		rightAnt.SetActive (false);
		logo.SetActive (false);
		mainMenu.SetActive (false);
	}



	// Match Attributes
	public void Map1Selected() {
		Debug.Log ("map 1 selected");
        NetworkManager.singleton.onlineScene = "Map 1";
	}

	public void Map2Selected() {
		Debug.Log ("map 2 selected");
		NetworkManager.singleton.onlineScene = "Map 2";
	}

	public void Map3Selected() {
		Debug.Log ("map 3 selected");
		NetworkManager.singleton.onlineScene = "Map 3";
	}

	public void Map4Selected() {
		Debug.Log ("map 4 selected");
		NetworkManager.singleton.onlineScene = "Map 4";
	}

	public void Map5Selected() {
		Debug.Log ("map 5 selected");
		NetworkManager.singleton.onlineScene = "Map 5";
	}
	// End Match Attributes


	public void ActuallyJoinGame() {

        NetworkManager.singleton.networkAddress = networkIP.GetComponent<Text>().text.Trim();


        if (string.IsNullOrEmpty(networkIP.GetComponent<Text>().text))
        {
            NetworkManager.singleton.networkAddress = "localhost";
        }


		NetworkManager.singleton.StartClient ();
	}

	public void JoinGame() {
		mainMenu.SetActive (false);
		joinMenu.SetActive (true);
	}


	public void BackFromHostMenu() {
		mainMenu.SetActive (true);
		createMenu.SetActive (false);

	}

	public void BackFromJoinMenu() {
		mainMenu.SetActive (true);
		joinMenu.SetActive (false);
	}

	public void BackFromTutorial() {
		mainMenu.SetActive (true);
		leftAnt.SetActive (true);
		rightAnt.SetActive (true);
		logo.SetActive (true);
		anim.SetBool ("playTutorial", false);
		anim.Play ("CameraAnimation");
		tutorialVideo.SetActive (false);
		tutorial.movie.Stop ();
		tutorial.sound.Stop ();
		returnToMainMenuFromTutorial.SetActive (false);
		pressSpaceToPlayTutorial.SetActive (false);
	}


	public void ExitGame() {
		Application.Quit ();
	}


}
