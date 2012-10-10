using UnityEngine;
using System.Collections;
using Realm;

public abstract class Player_Controller : MonoBehaviour
{
	public static Player_Controller Instance;

	public CharacterController CharacterController;
	protected float TurnSpeed = 1f;
	protected int TurningConstant = 1000;

	private void Awake()
	{
		if (networkView.isMine)
		{
			CharacterController = GetComponent("CharacterController") as CharacterController;
			Instance = this;
		}
	}

	private void Start()
	{
		if (!networkView.isMine)
		{
			enabled = false;
		}
	}

	protected virtual void Update()
	{
		float deadZone = 0.05f;

		GetLocomotionInput();
		HandleActionInput();

		// Turn character
		if (Mathf.Abs(Player_Motor.Instance.MoveVector.x) > deadZone || Mathf.Abs(Player_Motor.Instance.MoveVector.z) > deadZone)
		{
			Quaternion q = Quaternion.LookRotation(transform.position + Player_Motor.Instance.MoveVector * TurningConstant);
			transform.rotation = Quaternion.Slerp(transform.rotation, q, 0.1f * TurnSpeed);
			transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);
		}

		Player_Motor.Instance.UpdateMotor();
	}

	protected void GetLocomotionInput()
	{
		float deadZone = 0.01f;

		InitializeMoveVector();
		
		if (Mathf.Abs(Input.GetAxis("Vertical")) > deadZone)
		{
			Player_Motor.Instance.MoveVector += new Vector3(0, 0, Input.GetAxis("Vertical"));
		}
		if (Mathf.Abs(Input.GetAxis("Horizontal")) > deadZone)
		{
			Player_Motor.Instance.MoveVector += new Vector3(Input.GetAxis("Horizontal"), 0, 0);
		}

		/*Vector3 mv = Vector3.zero;
		if (Mathf.Abs(Input.GetAxis("Vertical")) > deadZone)
		{
			mv += new Vector3(0, 0, Input.GetAxis("Vertical"));
		}
		if (Mathf.Abs(Input.GetAxis("Horizontal")) > deadZone)
		{
			mv += new Vector3(Input.GetAxis("Horizontal"), 0, 0);
		}

		Quaternion lookRot = Quaternion.LookRotation(mv * 1000 - transform.position);
		transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 10);

		CharacterController.Move(mv.normalized * 5 * Time.deltaTime);*/
	}

	public void Jump()
	{
		Player_Motor.Instance.Jump();
	}

	private void InitializeMoveVector()
	{
		Player_Motor.Instance.SetVerticalVelocity(Player_Motor.Instance.MoveVector.y);
		Player_Motor.Instance.MoveVector = Vector3.zero;
	}

	// Functions implemented by the inheriting Controllers
	public abstract void HandleActionInput();
	public abstract void OnNetworkInstantiate(NetworkMessageInfo info);
}