using UnityEngine;
using System.Collections;
using Realm;

public class Enemy_Unit_Script : Unit_Script
{
	public AudioClip AudioDeath;

	public virtual void Update()
	{
		CharacterController c = GetComponent<CharacterController>();
		Center = new Vector3(gameObject.transform.position.x,
			gameObject.transform.position.y + (c.height / 2),
			gameObject.transform.position.z);
	}

	public virtual void Awake()
	{
		unit = new Unit();
		unit.health = 200;
		unit.mana = 100;
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
				audio.PlayOneShot(AudioDeath);
				Network.Destroy(gameObject);
			}
		}
	}
}


