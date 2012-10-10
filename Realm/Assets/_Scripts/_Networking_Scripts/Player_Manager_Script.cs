using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Realm;

public class Player_Manager_Script: MonoBehaviour
{
	List<Player> PlayerList;

	void Awake()
	{
		DontDestroyOnLoad(this);
		PlayerList = new List<Player>();
	}

	public void AddPlayer(NetworkPlayer netPlayer, GameObject playerObject)
	{
		PlayerList.Add(new Player(netPlayer, playerObject));
	}

	public void RemovePlayer(NetworkPlayer netPlayer)
	{
		foreach (Player player in PlayerList)
		{
			if (player.NetPlayer == netPlayer)
			{
				PlayerList.Remove(player);
			}
		}
	}

	public List<Player> GetPlayers()
	{
		return PlayerList;
	}
}


/*void OnConnectedToServer()
	{
		AddPlayer(Network.player);
	}

	void OnServerInitialized()
	{
		PerformAddPlayer(Network.player);
	}

	public void AttachPlayerObjectToPlayer(NetworkPlayer netPlayer, NetworkViewID viewId)
	{
		if (Network.isClient)
		{
			networkView.RPC("RPCAttachPlayerObjectToPlayer", RPCMode.Server, netPlayer, viewId);
		}
		else if (Network.isServer)
		{
			PerformAttachPlayerObjectToPlayer(netPlayer, viewId);
		}
	}

	[RPC]
	void RPCAttachPlayerObjectToPlayer(NetworkPlayer netPlayer, NetworkViewID viewId)
	{
		PerformAttachPlayerObjectToPlayer(netPlayer, viewId);
	}

	public void PerformAttachPlayerObjectToPlayer(NetworkPlayer netPlayer, NetworkViewID viewId)
	{
		GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
		GameObject playerObject = null;

		foreach (GameObject po in playerObjects)
		{
			NetworkView poNetworkView = po.networkView;

			if (poNetworkView == null) Debug.LogError("Found a GameObject with tag \"Player\", but no NetworkView component");

			if ((poNetworkView != null) && po.networkView.viewID.Equals(viewId))
			{
				playerObject = po;
			}
		}

		if (playerObject == null)
		{
			Debug.Log("Error could not find playerObject");
			return;
		}

		foreach (Player p in PlayerList)
		{
			if (p.NetPlayer == netPlayer)
			{
				p.PlayerObject = playerObject;
			}
		}
	}

	public void AddPlayer(NetworkPlayer netPlayer)
	{
		networkView.RPC("RPCAddPlayer", RPCMode.Server, netPlayer);
	}

	[RPC]
	void RPCAddPlayer(NetworkPlayer netPlayer)
	{
		PerformAddPlayer(netPlayer);
	}

	public void PerformAddPlayer(NetworkPlayer netPlayer)
	{
		PlayerList.Add(new Player(netPlayer));
	}*/
