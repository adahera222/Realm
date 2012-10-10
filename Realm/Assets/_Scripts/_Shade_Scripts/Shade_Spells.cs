using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Shade_Spells : MonoBehaviour
{
	public Transform FrostBolt;
	public float FrostBoltCooldown = 3.0f;

	public AudioClip AudioCooldownViolation;
	public AudioClip AudioEthereal;
	private enum Audio { CooldownViolation = 0, Ethereal = 1 }
	private Dictionary<Audio, AudioClip> AudioTable;

	private float FrostBoltLastCast;
	public static Shade_Spells Instance;

	private void OnNetworkInstantiate(NetworkMessageInfo info)
	{
		// Set up audio
		AudioTable = new Dictionary<Audio, AudioClip>();
		AudioTable.Add(Audio.CooldownViolation, AudioCooldownViolation);
		AudioTable.Add(Audio.Ethereal, AudioEthereal);
	}

	private void Awake()
	{
		if (networkView.isMine)
		{
			Instance = this;
			FrostBoltLastCast = FrostBoltCooldown * -1f;
		}
	}

	public void CastFrostBolt(Vector3 pos, Quaternion rot)
	{
		if (networkView.isMine)
		{
			if ((Time.time - FrostBoltLastCast) >= FrostBoltCooldown)
			{
				//audio.PlayOneShot(AudioEthereal);
				PlayAudio(Audio.Ethereal);
				Network.Instantiate(FrostBolt, pos, rot, 0);
				FrostBoltLastCast = Time.time;
			}
			else
			{
				//audio.PlayOneShot(AudioCooldownViolation);
				PlayAudio(Audio.CooldownViolation);
			}
		}
	}

	public void Cast_Invincibility()
	{
	}

	private void PlayAudio(Audio audioType)
	{
		networkView.RPC("RPCPlayAudio", RPCMode.All, (int)audioType);
	}

	[RPC]
	private void RPCPlayAudio(int audioIndex)
	{
		Audio myAudio = (Audio)Enum.ToObject(typeof(Audio), audioIndex);
		audio.PlayOneShot(AudioTable[myAudio]);
	}
}
