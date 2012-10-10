using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Realm
{
	public static class GameDirector
	{
		private static GameObject GetDirector()
		{
			return GameObject.FindGameObjectWithTag("GameDirector");
		}

		public static void AddPlayer(NetworkPlayer netPlayer, GameObject playerObject)
		{
			GameObject gameDirector = GetDirector();
			gameDirector.GetComponent<Player_Manager_Script>().AddPlayer(netPlayer, playerObject);
		}

		public static void RemovePlayer(NetworkPlayer netPlayer)
		{
			GameObject gameDirector = GetDirector();
			gameDirector.GetComponent<Player_Manager_Script>().RemovePlayer(netPlayer);
		}

		public static List<Player> GetPlayerList()
		{
			GameObject gameDirector = GetDirector();
			return gameDirector.GetComponent<Player_Manager_Script>().GetPlayers();
		}
	}
}
