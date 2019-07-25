/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionTagCheck.cs"
 * 
 *	This action checks which tag has been assigned to a given GameObject.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class ActionTagCheck : ActionCheck
	{
		
		public GameObject objectToCheck;
		public int objectToCheckConstantID;
		public int objectToCheckParameterID = -1;
		protected GameObject runtimeObjectToCheck;

		public string tagsToCheck;
		public int tagsToCheckParameterID = -1;
		
		
		public ActionTagCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check tag";
			description = "This action checks which tag has been assigned to a given GameObject.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeObjectToCheck = AssignFile (parameters, objectToCheckParameterID, objectToCheckConstantID, objectToCheck);
			tagsToCheck = AssignString (parameters, tagsToCheckParameterID, tagsToCheck);
		}


		public override bool CheckCondition ()
		{
			if (runtimeObjectToCheck != null && !string.IsNullOrEmpty (tagsToCheck))
			{
				if (!tagsToCheck.StartsWith (";"))
				{
					tagsToCheck = ";" + tagsToCheck;
				}
				if (!tagsToCheck.EndsWith (";"))
				{
					tagsToCheck += ";";
				}

				string objectTag = runtimeObjectToCheck.tag;
				return (tagsToCheck.Contains (";" + objectTag + ";"));
			}

			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			objectToCheckParameterID = Action.ChooseParameterGUI ("GameObject to check:", parameters, objectToCheckParameterID, ParameterType.GameObject);
			if (objectToCheckParameterID >= 0)
			{
				objectToCheckConstantID = 0;
				objectToCheck = null;
			}
			else
			{
				objectToCheck = (GameObject) EditorGUILayout.ObjectField ("GameObject to check:", objectToCheck, typeof (GameObject), true);
				
				objectToCheckConstantID = FieldToID (objectToCheck, objectToCheckConstantID);
				objectToCheck = IDToField (objectToCheck, objectToCheckConstantID, false);
			}

			tagsToCheckParameterID = Action.ChooseParameterGUI ("Check has tag(s):", parameters, tagsToCheckParameterID, ParameterType.String);
			if (tagsToCheckParameterID < 0)
			{
				tagsToCheck = EditorGUILayout.TextField ("Check has tag(s):", tagsToCheck);
			}
			EditorGUILayout.HelpBox ("Multiple character names should be separated by a colon ';'", MessageType.Info);
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			AssignConstantID (objectToCheck, objectToCheckConstantID, objectToCheckParameterID);
		}
		

		public override string SetLabel ()
		{
			if (objectToCheck != null)
			{
				return objectToCheck.name;
			}
			return string.Empty;
		}

		#endif
		
	}

}