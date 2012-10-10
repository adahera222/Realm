using UnityEngine;
using System.Collections;

public class Camera_Script : MonoBehaviour
{
	public static GameObject Player;
	public float Mouse_Wheel_Sensitivity = 5f;
	public float Camera_Vertical_Distance = 12.25f;
	public float Max_Camera_Vertical_Distance = 20f;
	public float Min_Camera_Vertical_Distance = 4f;
	public float Camera_Horizontal_Distance = 10f;
	//public float Frontal_Camera_Angle_View = 2f;

	void Awake()
	{
		if (Player == null) return;
	}

	void Update()
	{
		if (Player == null) return;

		Vector3 player_position = Player.transform.position;

		if (Camera_Horizontal_Distance < 4f)
		{
			Camera_Horizontal_Distance = 4f;
		}

		if (Camera_Vertical_Distance < Min_Camera_Vertical_Distance)
		{
			Camera_Vertical_Distance = Min_Camera_Vertical_Distance;
		}
		else if (Camera_Vertical_Distance > Max_Camera_Vertical_Distance)
		{
			Camera_Vertical_Distance = Max_Camera_Vertical_Distance;
		}

		transform.position = new Vector3(player_position.x, Camera_Vertical_Distance, player_position.z - Camera_Horizontal_Distance);
		transform.LookAt(Player.transform.position);

		//Vector3 camera_angles = transform.localEulerAngles;
		//transform.localEulerAngles = new Vector3(camera_angles.x - Frontal_Camera_Angle_View, camera_angles.y, camera_angles.z);

		Handle_Input();
	}

	void Handle_Input()
	{
		var deadZone = 0.01f;

		if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		{
			Camera_Vertical_Distance -= Input.GetAxis("Mouse ScrollWheel") * Mouse_Wheel_Sensitivity;
		}
	}
}
