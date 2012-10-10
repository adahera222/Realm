using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Realm;

public class CubeEnemy_Script : MonoBehaviour
{
	public float MaxSearchDistance = 15f;
	public Material WanderMaterial;
	public Material SeekMaterial;
	public Material AttackMaterial;

	public AudioClip AudioMeleeAttack;
	public AudioClip AudioAggro;
	public AudioClip AudioDeath;

	//private Vector3 WanderVector;
	private Vector3 AggroPosition;
	private State CurrentState;
	private float AttackCooldown = 3f;
	private float LastAttack;

	private float MoveSpeed = 2f;

	private enum State { Seek, Wander, Attack, Return };

	private void Awake()
	{
		CurrentState = State.Wander;
		LastAttack = -1f * AttackCooldown;
	}

	private void Start()
	{
		AggroPosition = GetComponent<Unit_Script>().Center;
	}

	private void Update()
	{
		GameObject targetPlayer;

		switch (CurrentState)
		{
			case (State.Wander):
				if (DetectPlayer(out targetPlayer))
				{
					audio.PlayOneShot(AudioAggro);
					AggroPosition = transform.position;
					Seek(targetPlayer);
				}
				else
				{
					Wander();
				}
				break;
			case (State.Seek):
				if (DetectPlayer(out targetPlayer))
				{
					Seek(targetPlayer);
				}
				else
				{
					ReturnToAggroPosition();
				}
				break;
			case (State.Attack):
				break;
			case (State.Return):
				renderer.material = WanderMaterial;
				ReturnToAggroPosition();
				break;
			default:
				Debug.Log("Error, unhandled case");
				break;
		}
	}

	private void ReturnToAggroPosition()
	{
		CurrentState = State.Return;
		transform.LookAt(AggroPosition);
		transform.Translate(new Vector3(0, 0, MoveSpeed) * Time.deltaTime);

		if (Vector3.Distance(AggroPosition, transform.position) < 0.1)
		{
			transform.position = AggroPosition;
			Wander();
		}
	}

	private void Wander()
	{
		CurrentState = State.Wander;
		renderer.material = WanderMaterial;
	}

	private void Seek(GameObject targetPlayer)
	{
		CurrentState = State.Seek;
		Vector3 targetCenter = targetPlayer.GetComponent<Unit_Script>().Center;
		transform.LookAt(targetCenter);
		transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);

		if (Vector3.Distance(transform.position, targetPlayer.transform.position) > 4)
		{
			renderer.material = SeekMaterial;
			transform.Translate(new Vector3(0, 0, MoveSpeed) * Time.deltaTime);
		}

		if (Vector3.Distance(transform.position, targetPlayer.transform.position) <= 4)
		{
			renderer.material = AttackMaterial;
			Attack(targetPlayer);
		}
	}

	private void Attack(GameObject targetPlayer)
	{
		if ((Time.time - LastAttack) > AttackCooldown)
		{
			audio.PlayOneShot(AudioMeleeAttack);
			targetPlayer.SendMessage("OnTakeDamage", 50f);
			LastAttack = Time.time;
		}
	}

	private bool DetectPlayer(out GameObject targetPlayer)
	{
		GameObject player = FindClosestPlayer();
		targetPlayer = player;

		return (player != null);
	}

	private GameObject FindClosestPlayer()
	{
		List<Player> players = GameDirector.GetPlayerList();
		GameObject closestPlayer = players[0].PlayerObject;
		if (closestPlayer == null) return null;
		float closestDistance = Vector3.Distance(transform.position, closestPlayer.transform.position);

		foreach (Player player in players)
		{
			float distance = Vector3.Distance(transform.position, player.PlayerObject.transform.position);
			if (distance < closestDistance)
			{
				closestPlayer = player.PlayerObject;
				closestDistance = distance;
			}
		}

		if (closestDistance > MaxSearchDistance)
		{
			return null;
		}

		return closestPlayer;
	}
}


