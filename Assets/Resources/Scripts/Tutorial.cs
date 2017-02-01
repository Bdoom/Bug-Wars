using UnityEngine;
using System.Collections;

public class Tutorial : MonoBehaviour {

	public AudioSource sound;
	public MovieTexture movie;
	private Renderer r;

	void Start () {
		sound = GetComponent<AudioSource> ();
		r = GetComponent<Renderer> ();
		movie = (MovieTexture)r.material.mainTexture;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Jump")) {



			if (movie.isPlaying) {
				movie.Pause();
				sound.Pause ();
			}
			else {
				movie.Play();
				sound.Play ();
			}
		}
	}
}
