/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionObjectCheck.cs"
 * 
 *	This action checks if an object is
 *	in the scene.
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
	public class ActionObjectCheck : ActionCheck
	{

		public GameObject gameObject;
		public int parameterID = -1;
		public int constantID = 0; 
		protected GameObject runtimeGameObject;


		public ActionObjectCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check presence";
			description = "Use to determine if a particular GameObject or prefab is present in the current scene.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeGameObject = AssignFile (parameters, parameterID, constantID, gameObject);
		}
		
		
		override public bool CheckCondition ()
		{
			if (runtimeGameObject != null && runtimeGameObject.activeInHierarchy)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Object to check:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				gameObject = null;
			}
			else
			{
				gameObject = (GameObject) EditorGUILayout.ObjectField ("Object to check:", gameObject, typeof (GameObject), true);
				
				constantID = FieldToID (gameObject, constantID);
				gameObject = IDToField (gameObject, constantID, false);
			}
		}


		public override string SetLabel ()
		{
			if (gameObject != null)
			{
				return gameObject.name;
			}
			return string.Empty;
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			AssignConstantID (gameObject, constantID, parameterID);
		}
		
		#endif
		
	}

}