using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FireBeatleBullet : NetworkBehaviour
{

    [SyncVar(hook = "TargetUpdate")]
    public Vector3 positionToFireAt;

    private float speed = 1f;

    private int beatleDamage = 25;

    public UnityEngine.AI.NavMeshAgent agent;

    // Destroy after time vars
    // destroy the bullet if it does not collide after 5 seconds.
    float timer = 0f;
    float timerMax = 25f;
    // end time vars

    void FixedUpdate()
    {
        if (positionToFireAt != Vector3.zero)
        {
            transform.LookAt(positionToFireAt);
            //body.MovePosition(Vector3.MoveTowards(transform.position, positionToFireAt, Time.deltaTime * speed));
            transform.position = Vector3.Lerp(transform.position, positionToFireAt, speed * Time.deltaTime);
            Debug.Log(positionToFireAt);
        }
    }

    void TargetUpdate(Vector3 newTarget)
    {
        //Debug.Log(newTarget);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasAuthority)
            return;

		if (other.GetComponent<NetworkIdentity> ().hasAuthority)
			return;	


        if (other.name.Contains("Ant"))
        {
            if (other.GetComponent<BasicAnt>() != null)
            {
                WorldHandler.findLocalPlayer().GetComponent<WorldHandler>().Rpc_DamageBasicAnt(other.gameObject, Random.Range(0, beatleDamage), gameObject);

                Destroy(gameObject);
            }
        }

        if (other.tag == "anthill")
        {
            if (other.GetComponent<Anthill>().health <= 0)
            {
                GameObject.Find("Canvas").transform.FindChild("Victory").gameObject.SetActive(true); // WorldHandler is always checking for these to be true or false to switch scenes to defeat/victory
            }

            WorldHandler.findLocalPlayer().GetComponent<WorldHandler>().Rpc_DamageAntHill(other.gameObject, Random.Range(0, beatleDamage), gameObject);
        }
    }

    void Update()
    {

        timer += Time.deltaTime;

        if (timer >= timerMax)
        {
            Destroy(gameObject);
        }

    }
}
