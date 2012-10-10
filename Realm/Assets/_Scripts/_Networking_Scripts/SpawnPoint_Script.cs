using UnityEngine;
using System.Collections;

public class SpawnPoint_Script : MonoBehaviour
{
	public Transform player;

	void Awake ()
	{
		SpawnPlayer();
	}
	
	void SpawnPlayer()
	{
		Network.Instantiate(player, transform.position, transform.rotation, 0);
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
}
