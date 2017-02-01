using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class OnVictoryDefeat : MonoBehaviour {
	
	public void ExitGame() {
		Application.Quit ();
	}

	public void ReturnToMainMenu() {
        NetworkManager.Shutdown();
    }

}
