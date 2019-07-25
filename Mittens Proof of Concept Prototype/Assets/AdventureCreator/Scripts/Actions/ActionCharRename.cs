/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionCharRename.cs"
 * 
 *	This action renames Hotspots. A "Remember NPC" script needs to be
 *	attached to the Character unless it is a Player prefab.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionCharRename : Action, ITranslatable
	{
		
		public int _charID = 0;
		public Char _char;
		public bool isPlayer;
		protected Char runtimeChar;

		public string newName;
		public int lineID = -1;
		
		
		public ActionCharRename ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Rename";
			lineID = -1;
			description = "Changes the display name of a Character when subtitles are used.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile <Char> (_charID, _char);

			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
		}
		
		
		override public float Run ()
		{
			if (runtimeChar && !string.IsNullOrEmpty (newName))
			{
				runtimeChar.SetName (newName, lineID);
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (!isPlayer)
			{
				_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
				
				_charID = FieldToID <Char> (_char, _charID);
				_char = IDToField <Char> (_char, _charID, true);
			}
			
			newName = EditorGUILayout.TextField ("New name:", newName);
			
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

				AssignConstantID <Char> (_char, _charID, 0);
			}
		}

		
		override public string SetLabel ()
		{
			if (_char != null && !string.IsNullOrEmpty (newName))
			{
				return _char.name + " to " + newName;
			}
			return string.Empty;
		}

		#endif


		/** ITranslatable implementation */

		public string GetTranslatableString (int index)
		{
			return newName;
		}

		public int GetTranslationID (int index)
		{
			return lineID;
		}


		#if UNITY_EDITOR

		public int GetNumTranslatables ()
		{
			return 1;
		}


		public bool HasExistingTranslation (int index)
		{
			return (lineID > -1);
		}


		public void SetTranslationID (int index, int _lineID)
		{
			lineID = _lineID;
		}


		public string GetOwner ()
		{
			return string.Empty;
		}


		public bool OwnerIsPlayer ()
		{
			return isPlayer;
		}


		public AC_TextType GetTranslationType (int index)
		{
			return AC_TextType.Hotspot;
		}


		public bool CanTranslate (int index)
		{
			return (!string.IsNullOrEmpty (newName));
		}

		#endif

	}

}