/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionPauseActionList.cs"
 * 
 *	This action pauses and resumes ActionLists.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionPauseActionList : Action
	{

		public enum PauseResume { Pause, Resume };
		public PauseResume pauseResume = PauseResume.Pause;

		public ActionRunActionList.ListSource listSource = ActionRunActionList.ListSource.InScene;
		public ActionListAsset actionListAsset;

		public bool rerunPausedActions;

		public ActionList actionList;
		protected ActionList _runtimeActionList;

		public int constantID = 0;
		public int parameterID = -1;

		protected RuntimeActionList[] runtimeActionLists = new RuntimeActionList[0];

		
		public ActionPauseActionList ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Pause or resume";
			description = "Pauses and resumes ActionLists.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				_runtimeActionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
			}
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				runtimeActionLists = new RuntimeActionList[0];

				if (pauseResume == PauseResume.Pause)
				{
					if (listSource == ActionRunActionList.ListSource.AssetFile && actionListAsset != null && !actionListAsset.actions.Contains (this))
					{
						runtimeActionLists = KickStarter.actionListAssetManager.Pause (actionListAsset);

						if (willWait && runtimeActionLists.Length > 0)
						{
							return defaultPauseTime;
						}
					}
					else if (listSource == ActionRunActionList.ListSource.InScene && _runtimeActionList != null && !_runtimeActionList.actions.Contains (this))
					{
						_runtimeActionList.Pause ();

						if (willWait)
						{
							return defaultPauseTime;
						}
					}
				}
				else if (pauseResume == PauseResume.Resume)
				{
					if (listSource == ActionRunActionList.ListSource.AssetFile && actionListAsset != null && !actionListAsset.actions.Contains (this))
					{
						KickStarter.actionListAssetManager.Resume (actionListAsset, rerunPausedActions);
					}
					else if (listSource == ActionRunActionList.ListSource.InScene && _runtimeActionList != null && !_runtimeActionList.actions.Contains (this))
					{
						KickStarter.actionListManager.Resume (_runtimeActionList, rerunPausedActions);
					}
				}
			}
			else
			{
				if (listSource == ActionRunActionList.ListSource.AssetFile)
				{
					foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
					{
						if (runtimeActionList != null && KickStarter.actionListManager.IsListRunning (runtimeActionList))
						{
							return defaultPauseTime;
						}
					}
				}
				else if (listSource == ActionRunActionList.ListSource.InScene)
				{
					if (KickStarter.actionListManager.IsListRunning (_runtimeActionList))
					{
						return defaultPauseTime;
					}
				}

				isRunning = false;
				return 0f;
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			pauseResume = (PauseResume) EditorGUILayout.EnumPopup ("Method:", pauseResume);

			listSource = (ActionRunActionList.ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
				
				constantID = FieldToID <ActionList> (actionList, constantID);
				actionList = IDToField <ActionList> (actionList, constantID, true);

				if (actionList != null && actionList.actions.Contains (this))
				{
					EditorGUILayout.HelpBox ("An ActionList cannot " + pauseResume.ToString () + " itself - it must be performed indirectly.", MessageType.Warning);
				}
			}
			else if (listSource == ActionRunActionList.ListSource.AssetFile)
			{
				actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", actionListAsset, typeof (ActionListAsset), false);

				if (actionListAsset != null && actionListAsset.actions.Contains (this))
				{
					EditorGUILayout.HelpBox ("An ActionList Asset cannot " + pauseResume.ToString () + " itself - it must be performed indirectly.", MessageType.Warning);
				}
			}
			
			if (pauseResume == PauseResume.Pause)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				if (willWait)
				{
					EditorGUILayout.HelpBox ("The ActionList will complete any currently-running Actions before it pauses.", MessageType.Info);
				}
			}
			else if (pauseResume == PauseResume.Resume)
			{
				rerunPausedActions = EditorGUILayout.ToggleLeft ("Re-run Action(s) at time of pause?", rerunPausedActions);
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				AssignConstantID <ActionList> (actionList, constantID, parameterID);
			}
		}
		
		
		public override string SetLabel ()
		{
			if (listSource == ActionRunActionList.ListSource.InScene && actionList != null)
			{
				return pauseResume.ToString () + " " + actionList.name;
			}
			else if (listSource == ActionRunActionList.ListSource.AssetFile && actionList != null)
			{
				return pauseResume.ToString () + " " + actionList.name;
			}
			return string.Empty;
		}
		
		#endif
		
	}
	
}