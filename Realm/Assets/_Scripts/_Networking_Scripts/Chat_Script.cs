using UnityEngine;
using System.Collections;

public class Chat_Script : MonoBehaviour
{
	public static string PlayerName;

	public float Chat_Box_Height_Ratio = 0.3f;
	public float Chat_Box_Width_Ratio = 0.4f;

	bool usingChat = false;
	bool showChat = false;
	string inputField = "";
	float Width;
	float Height;
	float Last_Unfocus_Time = 0;
	int Max_Message_Count = 100;

	GUIStyle Font_Style;
	Vector2 scrollposition;
	Rect Window;

	ArrayList Player_List = new ArrayList();
	class GamePlayer
	{
		public string Name;
		public NetworkPlayer NetPlayer;
	}

	ArrayList ChatMessages = new ArrayList();
	class ChatMessage
	{
		public string Sender = "";
		public string Message = "";
	}

	void Awake()
	{
		Font_Style = new GUIStyle();
		Font_Style.fontSize = gameObject.GetComponent<GUIText>().fontSize;
		Font_Style.fontStyle = gameObject.GetComponent<GUIText>().fontStyle;
		Font_Style.normal.textColor = Color.white;
	}

	void Update()
	{
		Height = Screen.height * Chat_Box_Height_Ratio;
		Width = Screen.width * Chat_Box_Width_Ratio;
		Window = new Rect(0, Screen.height - Height, Width, Height);
	}

	void OnConnectedToServer()
	{
		showChatWindow();
		networkView.RPC("TellServerOurName", RPCMode.Server, PlayerName);
		addGameChatMessage(PlayerName + " has just joined the chat!");
	}

	void OnServerInitialized()
	{
		showChatWindow();
		GamePlayer newEntry = new GamePlayer();
		newEntry.Name = PlayerName;
		newEntry.NetPlayer = Network.player;
		Player_List.Add(newEntry);
		addGameChatMessage(PlayerName + " has just joined the chat!");
	}

	GamePlayer GetPlayerNode(NetworkPlayer netPlay)
	{
		
		foreach (GamePlayer entry in Player_List)
		{
			if (entry.NetPlayer == netPlay)
			{
				return entry;
			}
		}
		Debug.LogError("GetPlayerNode: Requested a playernode of non-existing player");
		return null;
	}

	void OnPlayerDisconnected(NetworkPlayer netPlayer)
	{
		Player_List.Remove(GetPlayerNode(netPlayer));
	}

	void OnDisconnectedFromServer()
	{
		addGameChatMessage(PlayerName + " has disconnected");
		CloseChatWindow();
	}

	[RPC]
	void TellServerOurName(string name, NetworkMessageInfo info)
	{
		GamePlayer newEntry = new GamePlayer();
		newEntry.Name = PlayerName;
		newEntry.NetPlayer = info.sender;
		Player_List.Add(newEntry);
	}

	void CloseChatWindow()
	{
		showChat = false;
		inputField = "";
		ChatMessages = new ArrayList();
	}

	void showChatWindow()
	{
		showChat = true;
		inputField = "";
		ChatMessages = new ArrayList();
	}

	void OnGUI()
	{
		if (!showChat) return;

		if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length <= 0)
		{
			if (Last_Unfocus_Time + .25f < Time.time)
			{
				usingChat = true;
				GUI.FocusWindow(5);
				GUI.FocusControl("Chat input field");
			}
		}

		Window = GUILayout.Window(5, Window, GlobalChatWindow, "");
	}

	void GlobalChatWindow(int id)
	{
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.EndVertical();

		scrollposition = GUILayout.BeginScrollView(scrollposition);

		foreach (ChatMessage entry in ChatMessages)
		{
			GUILayout.BeginHorizontal();
			if (entry.Sender == " - ")
			{
				GUILayout.Label(entry.Sender + entry.Message, Font_Style);
			}
			else
			{
				GUILayout.Label(entry.Sender + ": " + entry.Message, Font_Style);
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(2);
		}

		GUILayout.EndScrollView();

		if (Event.current.type == EventType.keyDown && Event.current.character == '\n')
		{
			HitEnter(inputField);
			usingChat = false;
			GUI.UnfocusWindow();
			Last_Unfocus_Time = Time.time;
		}

		GUI.SetNextControlName("Chat input field");
		inputField = GUILayout.TextField(inputField, Font_Style);

		if (Input.GetKeyDown("mouse 0"))
		{
			if (usingChat)
			{
				usingChat = false;
				GUI.UnfocusWindow();
				Last_Unfocus_Time = Time.time;
			}
		}
	}

	public void HitEnter(string msg)
	{
		if (msg.Length > 0)
		{
			msg = msg.Replace('\n', ' ');
			networkView.RPC("ApplyGlobalChatText", RPCMode.All, PlayerName, msg);
		}
	}

	[RPC]
	void ApplyGlobalChatText(string name, string msg)
	{
		ChatMessage entry = new ChatMessage();
		entry.Sender = name;
		entry.Message = msg;

		ChatMessages.Add(entry);
		if (ChatMessages.Count > Max_Message_Count)
		{
			ChatMessages.RemoveAt(0);
		}

		scrollposition.y = 1000000;
		inputField = "";
	}

	void addGameChatMessage(string str)
	{
		ApplyGlobalChatText(" - ", str);
		if (Network.connections.Length > 0)
		{
			networkView.RPC("ApplyGlobalChatText", RPCMode.Others, " - ", str);
		}
	}
}
