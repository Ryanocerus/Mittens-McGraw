/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionSoundShot.cs"
 * 
 *	This action plays an AudioClip without the need for a Sound object.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionSoundShot : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public Transform origin;
		protected Transform runtimeOrigin;

		public AudioClip audioClip;
		public int audioClipParameterID = -1;

		
		public ActionSoundShot ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play one-shot";
			description = "Plays an AudioClip once without the need for a Sound object.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeOrigin = AssignFile (parameters, parameterID, constantID, origin);
			audioClip = (AudioClip) AssignObject <AudioClip> (parameters, audioClipParameterID, audioClip);
		}
		
		
		override public float Run ()
		{
			if (audioClip == null)
			{
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				Vector3 originPos = Camera.main.transform.position;
				if (runtimeOrigin != null)
				{
					originPos = runtimeOrigin.position;
				}

				float volume = Options.GetSFXVolume ();
				AudioSource.PlayClipAtPoint (audioClip, originPos, volume);

				if (willWait)
				{
					return audioClip.length;
				}
			}
		
			isRunning = false;
			return 0f;
		}
		
		
		override public void Skip ()
		{
			if (audioClip == null)
			{
				return;
			}

			AudioSource[] audioSources = FindObjectsOfType (typeof (AudioSource)) as AudioSource[];
			foreach (AudioSource audioSource in audioSources)
			{
				if (audioSource.clip == audioClip && audioSource.isPlaying && audioSource.GetComponent<Sound>() == null)
				{
					audioSource.Stop ();
					return;
				}
			}
		}

		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			audioClipParameterID = Action.ChooseParameterGUI ("Clip to play:", parameters, audioClipParameterID, ParameterType.UnityObject);
			if (audioClipParameterID < 0)
			{
				audioClip = (AudioClip) EditorGUILayout.ObjectField ("Clip to play:", audioClip, typeof (AudioClip), false);
			}

			parameterID = Action.ChooseParameterGUI ("Position (optional):", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				origin = null;
			}
			else
			{
				origin = (Transform) EditorGUILayout.ObjectField ("Position (optional):", origin, typeof (Transform), true);
				
				constantID = FieldToID (origin, constantID);
				origin = IDToField (origin, constantID, false);
			}

			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			AssignConstantID (origin, constantID, parameterID);
		}
		
		
		override public string SetLabel ()
		{
			if (audioClip != null)
			{
				return audioClip.name;
			}
			return string.Empty;
		}
		
		#endif
		
	}
	
}