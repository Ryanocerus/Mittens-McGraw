/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionCharAnim.cs"
 * 
 *	This action is used to control character animation.
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
	public class ActionCharAnim : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public AnimEngine editingAnimEngine;

		public bool isPlayer;
		public Char animChar;
		protected Char runtimeAnimChar;
		public AnimationClip clip;
		public string clip2D;
		public int clip2DParameterID = -1;

		public enum AnimMethodChar { PlayCustom, StopCustom, ResetToIdle, SetStandard };
		public AnimMethodChar method;
		
		public AnimationBlendMode blendMode;
		public AnimLayer layer = AnimLayer.Base;
		public AnimStandard standard;
		public bool includeDirection = false;

		public bool changeSound = false;
		public AudioClip newSound;
		public int newSoundParameterID = -1;

		public int layerInt;
		public bool idleAfter = true;
		public bool idleAfterCustom = false;

		public AnimPlayMode playMode;
		public AnimPlayModeBase playModeBase = AnimPlayModeBase.PlayOnceAndClamp;

		public float fadeTime = 0f;

		public bool changeSpeed = false;
		public float newSpeed = 0f;

		public AnimMethodCharMecanim methodMecanim;
		public MecanimCharParameter mecanimCharParameter;
		public MecanimParameterType mecanimParameterType;
		public string parameterName;
		public int parameterNameID = -1;
		public float parameterValue;
		public int parameterValueParameterID = -1;

		public bool hideHead = false;
		public bool doLoop; // Ignored by official animation engines

		
		public ActionCharAnim ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Animate";
			description = "Affects a Character's animation. Can play or stop a custom animation, change a standard animation (idle, walk or run), change a footstep sound, or revert the Character to idle.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeAnimChar = AssignFile <Char> (parameters, parameterID, constantID, animChar);
			newSound = (AudioClip) AssignObject <AudioClip> (parameters, newSoundParameterID, newSound);
			parameterName = AssignString (parameters, parameterNameID, parameterName);
			clip2D = AssignString (parameters, clip2DParameterID, clip2D);

			if (isPlayer)
			{
				runtimeAnimChar = KickStarter.player;
			}

			if (runtimeAnimChar != null && runtimeAnimChar.GetAnimEngine () != null)
			{
				runtimeAnimChar.GetAnimEngine ().ActionCharAnimAssignValues (this, parameters);
			}
		}

		
		override public float Run ()
		{
			if (runtimeAnimChar != null)
			{
				if (runtimeAnimChar.GetAnimEngine () != null)
				{
					return runtimeAnimChar.GetAnimEngine ().ActionCharAnimRun (this);
				}
				else
				{
					ACDebug.LogWarning ("Could not create animation engine for " + runtimeAnimChar.name, runtimeAnimChar);
				}
			}
			else
			{
				ACDebug.LogWarning ("Could not create animation engine!");
			}

			return 0f;
		}


		override public void Skip ()
		{
			if (runtimeAnimChar != null)
			{
				if (runtimeAnimChar.GetAnimEngine () != null)
				{
					runtimeAnimChar.GetAnimEngine ().ActionCharAnimSkip (this);
				}
			}
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					animChar = KickStarter.player;
				}
				else if (AdvGame.GetReferences ().settingsManager)
				{
					animChar = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Character:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					animChar = null;
				}
				else
				{
					animChar = (Char) EditorGUILayout.ObjectField ("Character:", animChar, typeof (Char), true);
					
					constantID = FieldToID <Char> (animChar, constantID);
					animChar = IDToField <Char> (animChar, constantID, true);
				}
			}

			if (animChar)
			{
				ResetAnimationEngine (animChar.animationEngine, animChar.customAnimationClass);
			}

			if (editingAnimEngine != null)
			{
				editingAnimEngine.ActionCharAnimGUI (this, parameters);
			}
			else
			{
				EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
			}

			AfterRunningOption ();
		}

		
		override public string SetLabel ()
		{
			if (isPlayer)
			{
				return "Player";
			}
			else if (animChar != null)
			{
				return animChar.name;
			}
			return string.Empty;
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (isPlayer)
			{
				if (!fromAssetFile && GameObject.FindObjectOfType <Player>() != null)
				{
					animChar = GameObject.FindObjectOfType <Player>();
				}

				if (animChar == null && AdvGame.GetReferences ().settingsManager != null)
				{
					animChar = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}

			if (animChar != null)
			{
				ResetAnimationEngine (animChar.animationEngine, animChar.customAnimationClass);

				if (saveScriptsToo && editingAnimEngine != null && editingAnimEngine.RequiresRememberAnimator (this))
				{
					editingAnimEngine.AddSaveScript (this, animChar.gameObject);
				}

				AssignConstantID <Char> (animChar, constantID, parameterID);
			}
		}


		private void ResetAnimationEngine (AnimationEngine animationEngine, string customClassName)
		{
			string className = "";
			if (animationEngine == AnimationEngine.Custom)
			{
				className = customClassName;
			}
			else
			{
				className = "AnimEngine_" + animationEngine.ToString ();
			}
				
			if (className != "" && (editingAnimEngine == null || editingAnimEngine.ToString () != className))
			{
				editingAnimEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
			}
		}

		
		#endif


		public Char RuntimeAnimChar
		{
			get
			{
				return runtimeAnimChar;
			}
		}

	}

}