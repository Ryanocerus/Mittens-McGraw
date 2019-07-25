/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionPause.cs"
 * 
 *	This action pauses the game by a given amount.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionPause : Action
	{

		public int parameterID = -1;
		public float timeToPause;

		
		public ActionPause ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Wait";
			description = "Waits a set time before continuing.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			timeToPause = AssignFloat (parameters, parameterID, timeToPause);
		}

		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;

				if (timeToPause < 0f)
				{
					return defaultPauseTime;
				}
				return timeToPause;
			}
			else
			{
				isRunning = false;
				return 0f;
			}
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Wait time (s):", parameters, parameterID, ParameterType.Float);
			if (parameterID < 0)
			{
				timeToPause = EditorGUILayout.FloatField ("Wait time (s):", timeToPause);
				if (timeToPause < 0f)
				{
					EditorGUILayout.HelpBox ("A negative value will pause the ActionList by one frame.", MessageType.Info);
				}
			}
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			return timeToPause.ToString () + "s";
		}

		#endif
		
	}

}