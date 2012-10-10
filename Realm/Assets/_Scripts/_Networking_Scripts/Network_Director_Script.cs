using UnityEngine;
using System.Collections;
using System;
using Realm;

[RequireComponent(typeof(NetworkView))]
public class Network_Director_Script : MonoBehaviour
{
	public int Connection_Port = 25005;
	public bool Use_Nat = false;
	public Transform GameDirector;
	public AudioClip AudioJoinGame;
	public AudioClip AudioLeaveGame;

	private float Last_Host_List_Request = -1000.0f;
	private float Host_List_Refresh_Timeout = 10.0f;
	private bool Filter_NAT_Hosts = false;
	private string testMessage = "Undetermined NAT capabilities";
	private ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;

	private bool probingPublicIP = false;
	private float IP_Probe_Timer = 0.0f;
	private bool doneTesting = false;

	private string Game_Name = "Realm";
	private bool Is_Ingame = false; // for server use only, set to false if gameover?
	private Rect Window_Rect;
	private Rect ServerList_Rect;

	private string Server_Name = "Join!";
	private string Player_Name = "";

	//load levels

	private string Disconnected_Level = "LobbyEmpty";
	private string[] Supported_Levels = { "MainScene" };
	private int Last_Level_Prefix = 0;



	void Awake()
	{
		// Network level loading is done in a seperate channel.
		DontDestroyOnLoad(this);
		networkView.group = 1;
		Application.LoadLevel(Disconnected_Level);

		Window_Rect = new Rect(Screen.width - 280, 0, 280, 10);
		ServerList_Rect = new Rect(0, 0, Screen.width - Window_Rect.width, 100);

		Connection_Test();

		// What kind of IP does this machine have? TestConnection also indicates this in the
		// test results
		if (Network.HavePublicAddress())
		{
			Debug.Log("This machine has a public IP address");
		}
		else
		{
			Debug.Log("This machine has a private IP address");
		}
	}

	void Update()
	{
		// If test is undetermined, keep running
		if (!doneTesting)
		{
			Connection_Test();
		}
	}

	void OnGUI()
	{

		Window_Rect = GUILayout.Window(0, Window_Rect, MakeServerWindow, "Server Controls");
		if (Network.peerType == NetworkPeerType.Disconnected && MasterServer.PollHostList().Length != 0)
		{
			ServerList_Rect = GUILayout.Window(1, ServerList_Rect, MakeExistingGamesWindow, "Server List");
		}

		//can possibly stick the following into a script that executes when user presses tab to see gameinfo

		else if (Network.peerType == NetworkPeerType.Connecting)
		{
			GUILayout.Label("Connection Status: Connecting..");
		}
		else if (Network.peerType == NetworkPeerType.Client)
		{
			GUILayout.Label("Connection Status: Client");
			GUILayout.Label("Ping To Server: " + Network.GetAveragePing(Network.connections[0]));
		}
		else if (Network.peerType == NetworkPeerType.Server)
		{
			GUILayout.Label("Connection Status: Server");

			if (Network.connections.Length >= 1)
			{
				GUILayout.Label("Ping To Server: " + Network.GetAveragePing(Network.connections[0]));
			}
			GUILayout.Label("Number of Connections: " + Network.connections.Length);
		}
	}

	void MakeServerWindow(int id)
	{

		if (Network.peerType == NetworkPeerType.Disconnected)
		{

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			GUILayout.Label("Server name:");
			Server_Name = GUILayout.TextField(Server_Name, 20, GUILayout.Width(130));

			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			GUILayout.Label("Player name:");
			Player_Name = GUILayout.TextField(Player_Name, 20, GUILayout.Width(130));

			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			// Start a new server
			if (GUILayout.Button("Start Server") && Check_Player_Name())
			{
				if (Server_Name == "")
				{
					Popup_Script.Popup_Box("Please enter the server's name", "Problem starting server...", 5.0f);
				}
				else
				{
					audio.PlayOneShot(AudioJoinGame);
					Network.InitializeServer(5, Connection_Port, Use_Nat);
					Network.Instantiate(GameDirector, transform.position, transform.rotation, 0);
					MasterServer.RegisterHost(Game_Name, Server_Name, "Host: " + Player_Name);
				}
			}

			// Refresh hosts
			if (GUILayout.Button("Refresh available Servers") || Time.realtimeSinceStartup > Last_Host_List_Request + Host_List_Refresh_Timeout)
			{
				MasterServer.ClearHostList();
				MasterServer.RequestHostList(Game_Name);
				Last_Host_List_Request = Time.realtimeSinceStartup;
			}

			GUILayout.FlexibleSpace();
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
		}
		else
		{
			if (!Is_Ingame && Network.peerType == NetworkPeerType.Server)
			{
				if (GUILayout.Button("Start Game"))
				{
					Is_Ingame = true;
					// register as an in progress game
					Network.maxConnections = 0;
					MasterServer.RegisterHost(Game_Name, Server_Name, "In Progress");
					//MasterServer.UnregisterHost();

					//LOAD LEVEL 1!
					// Make sure no old RPC calls are buffered and then send load level command
					Network.RemoveRPCsInGroup(0);
					Network.RemoveRPCsInGroup(1);

					// Load level with incremented level prefix (for view IDs)
					networkView.RPC("Load_Level", RPCMode.AllBuffered, Supported_Levels[0], Last_Level_Prefix + 1);
				}
			}
			if (GUILayout.Button("Disconnect"))
			{
				Realm.GameDirector.RemovePlayer(Network.player);
				GameObject gameDirector = GameObject.FindGameObjectWithTag("GameDirector");
				Destroy(gameDirector);

				audio.PlayOneShot(AudioLeaveGame);
				Network.Disconnect();
				MasterServer.UnregisterHost();
			}
			GUILayout.FlexibleSpace();

		}
		GUI.DragWindow(new Rect(0, 0, 1000, 1000));
	}

	void MakeExistingGamesWindow(int id)
	{


		HostData[] data = MasterServer.PollHostList();
		string connections;
		bool inprogress;

		GUILayout.Space(5);

		GUILayout.BeginHorizontal();

		GUILayout.EndHorizontal();

		foreach (HostData element in data)
		{
			GUILayout.BeginHorizontal();

			// Do not display NAT enabled games if we cannot do NAT punchthrough
			if (!(Filter_NAT_Hosts && element.useNat))
			{
				connections = element.connectedPlayers + "/" + element.playerLimit;
				if (element.comment == "In Progress")
				{
					inprogress = true;
				}
				else
				{
					inprogress = false;
				}
				GUILayout.Label(element.gameName);
				GUILayout.Space(5);
				GUILayout.Label(connections);
				GUILayout.Space(5);

				// Indicate if NAT punchthrough will be performed, omit showing GUID
				if (element.useNat)
				{
					GUILayout.Label("NAT");
					GUILayout.Space(5);
				}
				/*string hostInfo = "";
				// Here we display all IP addresses, there can be multiple in cases where
				// internal LAN connections are being attempted. In the GUI we could just display
				// the first one in order not confuse the end user, but internally Unity will
				// do a connection check on all IP addresses in the element.ip list, and connect to the
				// first valid one.
				foreach (string hostIP in element.ip)
				{
					hostInfo = hostInfo + hostIP + ":" + element.port + " ";
					break;
				}

				//GUILayout.Label("[" + element.ip + ":" + element.port + "]");	
				GUILayout.Label(hostInfo);
				GUILayout.Space(5);*/
				GUILayout.Label(element.comment);
				GUILayout.Space(5);
				GUILayout.FlexibleSpace();
				if (!inprogress)
				{
					if (GUILayout.Button("Connect") && Check_Player_Name())
					{
						Network.Connect(element);
					}
				}
			}
			GUILayout.EndHorizontal();
		}
		GUI.DragWindow(new Rect(0, 0, 1000, 1000));
	}

	bool Check_Player_Name()
	{
		if (Player_Name != "")
		{
			Chat_Script.PlayerName = Player_Name;
			return true;
		}
		else
		{
			Popup_Script.Popup_Box("Please enter your player name", "Player name required", 5.0f);
			return false;
		}
	}

	void Resize_Window_Rect()
	{
		Window_Rect.height = 10;
	}

	// Connection tests and debugging

	void Connection_Test()
	{
		// Start/Poll the connection test, report the results in a label and react to the results accordingly
		connectionTestResult = Network.TestConnection();
		switch (connectionTestResult)
		{
			case ConnectionTesterStatus.Error:
				testMessage = "Problem determining NAT capabilities";
				doneTesting = true;
				break;

			case ConnectionTesterStatus.Undetermined:
				testMessage = "Undetermined NAT capabilities";
				doneTesting = true;
				break;

			case ConnectionTesterStatus.PublicIPIsConnectable:
				testMessage = "Directly connectable public IP address.";
				Use_Nat = false;
				doneTesting = true;
				break;

			// This case is a bit special as we now need to check if we can 
			// circumvent the blocking by using NAT punchthrough
			case ConnectionTesterStatus.PublicIPPortBlocked:
				testMessage = "Non-connectble public IP address (port " + Connection_Port + " blocked), running a server is impossible.";
				Use_Nat = false;
				// If no NAT punchthrough test has been performed on this public IP, force a test
				if (!probingPublicIP)
				{
					Debug.Log("Testing if firewall can be circumvented");
					connectionTestResult = Network.TestConnectionNAT();
					probingPublicIP = true;
					IP_Probe_Timer = Time.time + 10;
				}
				// NAT punchthrough test was performed but we still get blocked
				else if (Time.time > IP_Probe_Timer)
				{
					probingPublicIP = false; 		// reset
					Use_Nat = true;
					doneTesting = true;
				}
				break;
			case ConnectionTesterStatus.PublicIPNoServerStarted:
				testMessage = "Public IP address but server not initialized, it must be started to check server accessibility. Restart connection test when ready.";
				break;

			case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
				Debug.Log("LimitedNATPunchthroughPortRestricted");
				testMessage = "Limited NAT punchthrough capabilities. Cannot connect to all types of NAT servers.";
				Use_Nat = true;
				doneTesting = true;
				break;

			case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
				Debug.Log("LimitedNATPunchthroughSymmetric");
				testMessage = "Limited NAT punchthrough capabilities. Cannot connect to all types of NAT servers. Running a server is ill adviced as not everyone can connect.";
				Use_Nat = true;
				doneTesting = true;
				break;

			case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
				goto case ConnectionTesterStatus.NATpunchthroughFullCone;
			case ConnectionTesterStatus.NATpunchthroughFullCone:
				Debug.Log("NATpunchthroughAddressRestrictedCone || NATpunchthroughFullCone");
				testMessage = "NAT punchthrough capable. Can connect to all servers and receive connections from all clients. Enabling NAT punchthrough functionality.";
				Use_Nat = true;
				doneTesting = true;
				break;

			default:
				testMessage = "Error in test routine, got " + connectionTestResult;
				break;
		}
		//Debug.Log(connectionTestResult + " " + probingPublicIP + " " + doneTesting);
		Debug.Log(testMessage);
	}

	void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
		Debug.Log(info);
	}

	void OnFailedToConnect(NetworkConnectionError info)
	{
		Debug.Log(info);
	}

	[RPC]

	void Load_Level(string level, int levelPrefix)
	{
		Debug.Log("Loading level " + level + " with prefix " + levelPrefix);
		Last_Level_Prefix = levelPrefix;

		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);

		// We need to stop receiving because first the level must be loaded.
		// Once the level is loaded, RPC's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;

		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(levelPrefix);
		Application.LoadLevel(level);

		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data
		Network.SetSendingEnabled(0, true);

		// Notify our objects that the level and the network is ready
		/*Debug.Log("sending Message");
		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
		}*/
	}

	void OnConnectedToServer()
	{
		Resize_Window_Rect();
	}

	void OnServerInitialized()
	{
		Resize_Window_Rect();
	}
	void OnPlayerConnected(NetworkPlayer player)
	{
		if (Network.connections.Length > Network.maxConnections)
		{
			Network.CloseConnection(Network.connections[Network.connections.Length - 1], true);
		}
	}

	void OnDisconnectedFromServer()
	{
		Resize_Window_Rect();
		audio.PlayOneShot(AudioLeaveGame);
		Is_Ingame = false;
		Application.LoadLevel(Disconnected_Level);
	}
}