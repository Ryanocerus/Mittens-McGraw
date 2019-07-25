/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionCharPortrait.cs"
 * 
 *	This action picks a new portrait for the chosen Character.
 *	Written for the AC community by Guran.
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
	public class ActionCharPortrait : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public bool isPlayer;
		public Char _char;
		protected Char runtimeChar;
		public Texture newPortraitGraphic;


		public ActionCharPortrait ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Switch Portrait";
			description = "Changes the 'speaking' graphic used by Characters. To display this graphic in a Menu, place a Graphic element of type Dialogue Portrait in a Menu of Appear type: When Speech Plays. If the new graphic is placed in a Resources folder, it will be stored in saved game files.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile <Char> (parameters, parameterID, constantID, _char);

			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
		}

		
		override public float Run ()
		{
			if (runtimeChar)
			{
				runtimeChar.portraitIcon.texture = newPortraitGraphic;
				runtimeChar.portraitIcon.ClearSprites ();
				runtimeChar.portraitIcon.ClearCache ();
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			// Action-specific Inspector GUI code here
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					_char = KickStarter.player;
				}
				else
				{
					_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Character:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					_char = null;
				}
				else
				{
					_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
					
					constantID = FieldToID <Char> (_char, constantID);
					_char = IDToField <Char> (_char, constantID, false);
				}
			}
			
			newPortraitGraphic = (Texture) EditorGUILayout.ObjectField ("New Portrait graphic:", newPortraitGraphic, typeof (Texture), true);
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (!isPlayer)
			{
				if (saveScriptsToo)
				{
					if (_char != null && _char.GetComponent <NPC>())
					{
						AddSaveScript <RememberNPC> (_char);
					}
				}

				AssignConstantID <Char> (_char, constantID, parameterID);
			}
		}


		public override string SetLabel ()
		{
			if (isPlayer)
			{
				return "Player";
			}
			else if (_char != null)
			{
				return _char.name;
			}
			return string.Empty;
		}

		#endif
		
	}

}