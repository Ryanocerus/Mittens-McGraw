/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionSpeechWait.cs"
 * 
 *	This Action waits until a particular character has stopped speaking.
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
	public class ActionSpeechWait : Action
	{

		public int constantID = 0;
		public int parameterID = -1;

		public bool isPlayer;
		public Char speaker;
		protected Char runtimeSpeaker;

		
		public ActionSpeechWait ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Wait for speech";
			description = "Waits until a particular character has stopped speaking.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeSpeaker = AssignFile <Char> (parameters, parameterID, constantID, speaker);

			// Special case: Use associated NPC
			if (runtimeSpeaker != null &&
				runtimeSpeaker is Player &&
				KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow &&
				KickStarter.player != null)
			{
				// Make sure not the active Player
				ConstantID speakerID = speaker.GetComponent <ConstantID>();
				ConstantID playerID = KickStarter.player.GetComponent <ConstantID>();
				if ((speakerID == null && playerID != null) ||
					(speakerID != null && playerID == null) ||
					(speakerID != null && playerID != null && speakerID.constantID != playerID.constantID))
				{
					Player speakerPlayer = runtimeSpeaker as Player;
					foreach (PlayerPrefab playerPrefab in KickStarter.settingsManager.players)
					{
						if (playerPrefab != null && playerPrefab.playerOb == speakerPlayer)
						{
							if (speakerPlayer.associatedNPCPrefab != null)
							{
								ConstantID npcConstantID = speakerPlayer.associatedNPCPrefab.GetComponent <ConstantID>();
								if (npcConstantID != null)
								{
									runtimeSpeaker = AssignFile <Char> (parameters, parameterID, npcConstantID.constantID, runtimeSpeaker);
								}
							}
							break;
						}
					}
				}
			}

			if (isPlayer)
			{
				runtimeSpeaker = KickStarter.player;
			}
		}


		override public float Run ()
		{
			if (runtimeSpeaker == null)
			{
				ACDebug.LogWarning ("No speaker set.");
			}
			else if (!isRunning)
			{
				isRunning = true;

				if (KickStarter.dialog.CharacterIsSpeaking (runtimeSpeaker))
				{
					return defaultPauseTime;
				}
			}
			else
			{
				if (KickStarter.dialog.CharacterIsSpeaking (runtimeSpeaker))
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			return;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Player line?",isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					speaker = KickStarter.player;
				}
				else
				{
					speaker = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Speaker:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					speaker = null;
				}
				else
				{
					speaker = (Char) EditorGUILayout.ObjectField ("Speaker:", speaker, typeof(Char), true);
					
					constantID = FieldToID <Char> (speaker, constantID);
					speaker = IDToField <Char> (speaker, constantID, false);
				}
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			AssignConstantID <Char> (speaker, constantID, parameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (parameterID == -1)
			{
				if (isPlayer)
				{
					return "Player";
				}
				else if (speaker != null)
				{
					return speaker.gameObject.name;
				}
			}
			return string.Empty;
		}
		
		#endif
		
	}
	
}