using UnityEngine;
using System.Collections;
using Realm;

public class Shade_Unit : Unit_Script
{
	public virtual void Awake()
	{
		unit = new Unit();
		unit.health = 100;
		unit.mana = 100;
	}

	public virtual void Update()
	{
		CharacterController c = GetComponent<CharacterController>();
		Center = new Vector3(gameObject.transform.position.x,
			gameObject.transform.position.y + (c.height / 2),
			gameObject.transform.position.z);
	}

	public virtual void OnTakeDamage(float damage)
	{
		networkView.RPC("RPCOnTakeDamage", RPCMode.All, damage);
	}

	[RPC]
	public void RPCOnTakeDamage(float damage)
	{
		unit.health -= damage;
		if (unit.health <= 0)
		{
			if (networkView.isMine)
			{
				Camera.mainCamera.GetComponent<AudioListener>().enabled = true;
				GameDirector.RemovePlayer(Network.player);
				Network.Destroy(gameObject);
			}
		}
	}
}
