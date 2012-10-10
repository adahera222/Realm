using UnityEngine;
using System.Collections;

public class Player_Motor : MonoBehaviour
{
	public static Player_Motor Instance;
	public int MoveSpeed = 5;
	public Vector3 MoveVector = Vector3.zero;
	public float GravityStrength = 50f;
	public float JumpStrength = 15f;

	private float VerticalVelocity = 0f; //{ get; set; }

	void Awake()
	{
		Instance = this;
	}

	public void UpdateMotor()
	{
		ProcessMotion();
	}

	private void ProcessMotion()
	{
		MoveVector = Vector3.Normalize(MoveVector);

		MoveVector *= MoveSpeed;
		MoveVector.y = VerticalVelocity;
		ApplyGravity();

		Player_Controller.Instance.CharacterController.Move(MoveVector * Time.deltaTime);

		if (Player_Controller.Instance.CharacterController.isGrounded &&
			((MoveVector.y < 0) || (VerticalVelocity < 0)))
		{
			MoveVector.y = 0;
			VerticalVelocity = 0;
		}
	}

	private void ApplyGravity()
	{
		MoveVector.y -= GravityStrength * Time.deltaTime;
	}

	public void Jump()
	{
		if (Player_Controller.Instance.CharacterController.isGrounded)
		{
			VerticalVelocity = JumpStrength;
		}
	}

	public void SetVerticalVelocity(float vel)
	{
		VerticalVelocity = vel;
	}
}
