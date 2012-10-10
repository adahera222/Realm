using UnityEngine;
using System.Collections;
using System;

public class Popup_Script : MonoBehaviour
{

	static private Rect Popup_Rect;
	static private Rect Orig_Popup_Rect;
	static private string Popup_Message = "";
	static private string Popup_Title = "";
	static private float Popup_Timeout = 0.0f;
	static private bool Use_Timeout = true;
	
	
	void Awake()
	{
		Popup_Rect = new Rect(100, 100, 200, 10);
		Orig_Popup_Rect = Popup_Rect;
	}

	void OnGUI()
	{
		if (!Use_Timeout || Time.realtimeSinceStartup < Popup_Timeout)
		{
			Popup_Rect = GUILayout.Window (2, Popup_Rect, MakePopupWindow, Popup_Title);
		}
	}

	public static void Popup_Box(string msg, string title, float timeout)
	{
		Popup_Message = msg;
		Popup_Title = title;
		Popup_Rect = Orig_Popup_Rect;
		if(timeout > 0)
		{
			Use_Timeout = true;
			Popup_Timeout = Time.realtimeSinceStartup + timeout;
		} else {
			Use_Timeout = false;
		}
	}
			
	void MakePopupWindow(int id)
	{
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.Label(Popup_Message);
		GUILayout.Space(5);
		if(GUILayout.Button("Close"))
		{
			Popup_Timeout = Time.realtimeSinceStartup;
		}
		GUILayout.FlexibleSpace();
		GUILayout.Space(10);
		GUILayout.EndVertical();
		GUI.DragWindow (new Rect (0,0,1000,1000));
	}
	
}