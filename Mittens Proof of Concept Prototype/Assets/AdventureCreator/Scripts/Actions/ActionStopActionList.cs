/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionStopActionList.cs"
 * 
 *	This Action stops other ActionLists
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionStopActionList : Action
	{
		
		public enum ListSource { InScene, AssetFile };
		public ListSource listSource = ListSource.InScene;
		
		public ActionList actionList;
		protected ActionList runtimeActionList;

		public ActionListAsset invActionList;
		public int constantID = 0;
		public int parameterID = -1;


		public ActionStopActionList ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Kill";
			description = "Instantly stops a scene or asset-based ActionList from running.";
		}

		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				runtimeActionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
			}
		}
		
		
		override public float Run ()
		{
			if (listSource == ListSource.InScene && runtimeActionList != null)
			{
				KickStarter.actionListManager.EndList (runtimeActionList);
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				KickStarter.actionListAssetManager.EndAssetList (invActionList, this);
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			listSource = (ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
			if (listSource == ListSource.InScene)
			{
				parameterID = Action.ChooseParameterGUI ("ActionList:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					actionList = null;
				}
				else
				{
					actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
					
					constantID = FieldToID <ActionList> (actionList, constantID);
					actionList = IDToField <ActionList> (actionList, constantID, true);
				}
			}
			else if (listSource == ListSource.AssetFile)
			{
				invActionList = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", invActionList, typeof (ActionListAsset), true);
			}

			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			AssignConstantID <ActionList> (actionList, constantID, parameterID);
		}


		public override string SetLabel ()
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				return actionList.name;
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				return invActionList.name;
			}
			return string.Empty;
		}
		
		#endif
		
	}
	
}