/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionRename.cs"
 * 
 *	This action renames Hotspots. A "Remember Name" script needs to be
 *	attached to said hotspot if the renaming is to carry across saved games.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionRename : Action, ITranslatable
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public Hotspot hotspot;
		protected Hotspot runtimeHotspot;

		public string newName;
		public int lineID = -1;
		
		
		public ActionRename ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Rename";
			lineID = -1;
			description = "Renames a Hotspot, or an NPC with a Hotspot component.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}
		
		
		override public float Run ()
		{
			if (runtimeHotspot && !string.IsNullOrEmpty (newName))
			{
				runtimeHotspot.SetName (newName, lineID);
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Hotspot to rename:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				hotspot = null;
			}
			else
			{
				hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to rename:", hotspot, typeof (Hotspot), true);
				
				constantID = FieldToID <Hotspot> (hotspot, constantID);
				hotspot = IDToField <Hotspot> (hotspot, constantID, false);
			}
			
			newName = EditorGUILayout.TextField ("New label:", newName);
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberHotspot> (hotspot);
			}

			AssignConstantID <Hotspot> (hotspot, constantID, parameterID);
		}
		
		
		override public string SetLabel ()
		{
			if (hotspot != null && !string.IsNullOrEmpty (newName))
			{
				return hotspot.name + " to " + newName;
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
			return false;
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