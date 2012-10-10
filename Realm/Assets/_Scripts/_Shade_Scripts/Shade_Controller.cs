using UnityEngine;
using System.Collections;
using Realm;

public class Shade_Controller : Player_Controller
{
	public override void OnNetworkInstantiate(NetworkMessageInfo info)
	{
		GameDirector.AddPlayer(info.sender, gameObject);

		if (networkView.isMine)
		{
			GetComponent<AudioListener>().enabled = true;
			Camera_Script.Player = gameObject;
			Camera.mainCamera.GetComponent<AudioListener>().enabled = false;
			GetComponent<NetworkInterpolatedTransform>().enabled = false;
		}
		else
		{
			name += " - Remote";
			GetComponent<AudioListener>().enabled = false;
			GetComponent<Shade_Controller>().enabled = false;
			GetComponent<NetworkInterpolatedTransform>().enabled = true;
		}
	}

	public override void HandleActionInput()
	{
		if (Input.GetButton("Jump"))
		{
			Jump();
		}
		else if (Input.GetMouseButtonDown(0))
		{
			Ray ray;
			RaycastHit hitInfo;

			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
			{
				transform.LookAt(new Vector3(hitInfo.point.x, gameObject.transform.position.y, hitInfo.point.z));

				Vector3 position = GetComponent<Shade_Unit>().Center;
				Quaternion rotation = transform.rotation;

				Shade_Spells.Instance.CastFrostBolt(position, rotation);
			}
		}
		else if (Input.GetKeyDown("q"))
		{
		}
		else if (Input.GetKeyDown("3"))
		{
			Debug.Log("3 was pressed!");
		}
	}
}