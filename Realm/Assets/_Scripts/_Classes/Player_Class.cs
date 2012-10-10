using UnityEngine;
using System.Collections;

namespace Realm
{
	public class Player
	{
		public int Id;
		public string Name;
		public NetworkPlayer NetPlayer;
		public GameObject PlayerObject;

		public Player(int id, string name, NetworkPlayer netPlayer, GameObject playerObject)
		{
			Id = id;
			Name = name;
			NetPlayer = netPlayer;
			PlayerObject = playerObject;
		}

		public Player(NetworkPlayer netPlayer)
		{
			NetPlayer = netPlayer;
		}

		public Player(NetworkPlayer netPlayer, GameObject playerObject)
		{
			NetPlayer = netPlayer;
			PlayerObject = playerObject;
		}
	}
}
