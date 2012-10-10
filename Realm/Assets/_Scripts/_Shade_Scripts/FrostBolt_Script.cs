using UnityEngine;
using System.Collections;


public class FrostBolt_Script : MonoBehaviour
{
	private Vector3 Start_Position;
	private float Max_Distance = 20f;
	public float test;

	void Awake()
	{
		if (networkView.isMine)
		{
			Start_Position = transform.position;
			GetComponent<NetworkInterpolatedTransform>().enabled = false;
		}
		else
		{
			name += " - Remote";
			GetComponent<FrostBolt_Script>().enabled = false;
			GetComponent<NetworkInterpolatedTransform>().enabled = false;
		}
	}

	void Update()
	{
		transform.Translate(new Vector3(0, 0, 15) * Time.deltaTime);

		RaycastHit rh;

		if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out rh, 0.5f))
		{
			GameObject receiverObject = rh.collider.gameObject;
			//if ((rh.distance < 0.5f) && (receiverObject.tag != "Player"))
			if (rh.distance < 0.5f)
			{
				receiverObject.SendMessage("OnTakeDamage", 50f);
				Network.Destroy(gameObject);
				return;
			}
		}

		if ((Vector3.Distance(transform.position, Start_Position)) > Max_Distance)
		{
			Network.Destroy(gameObject);
		}
	}
}
