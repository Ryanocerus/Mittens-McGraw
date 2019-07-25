/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionVisible.cs"
 * 
 *	This action controls the visibilty of a GameObject and its children.
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
	public class ActionVisible : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public GameObject obToAffect;
		protected GameObject runtimeObToAffect;

		public bool affectChildren;
		public VisState visState = 0;
		
		
		public ActionVisible ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Visibility";
			description = "Hides or shows a GameObject. Can optionally affect the GameObject's children.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeObToAffect = AssignFile (parameters, parameterID, constantID, obToAffect);
		}
		
		
		override public float Run ()
		{
			bool state = false;
			if (visState == VisState.Visible)
			{
				state = true;
			}
			
			if (runtimeObToAffect != null)
			{
				if (runtimeObToAffect.GetComponent <LimitVisibility>())
				{
					runtimeObToAffect.GetComponent <LimitVisibility>().isLockedOff = !state;
				}
				else if (runtimeObToAffect.GetComponent <Renderer>())
				{
					runtimeObToAffect.GetComponent <Renderer>().enabled = state;
				}

				if (affectChildren)
				{
					foreach (Renderer _renderer in runtimeObToAffect.GetComponentsInChildren <Renderer>())
					{
						_renderer.enabled = state;
					}
				}
					
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Object to affect:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				obToAffect = null;
			}
			else
			{
				obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", obToAffect, typeof (GameObject), true);

				constantID = FieldToID (obToAffect, constantID);
				obToAffect = IDToField (obToAffect, constantID, false);
			}

			visState = (VisState) EditorGUILayout.EnumPopup ("Visibility:", visState);
			affectChildren = EditorGUILayout.Toggle ("Affect children?", affectChildren);
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberVisibility> (obToAffect);
			}
			AssignConstantID (obToAffect, constantID, parameterID);
		}
		
		
		override public string SetLabel ()
		{
			if (obToAffect != null)
			{
				return obToAffect.name;
			}
			return string.Empty;
		}

		#endif

	}

}