/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionVarSequence.cs"
 * 
 *	This action reads a Popup Variable and performs
 *	different follow-up Actions based on its value.
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
	public class ActionVarPopup : ActionCheckMultiple
	{
		
		public int variableID;
		public int variableNumber;
		public VariableLocation location = VariableLocation.Global;

		private LocalVariables localVariables;

		public Variables variables;
		public int variablesConstantID = 0;

		private GVar runtimeVariable;

		
		public ActionVarPopup ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Pop Up switch";
			description = "Uses the value of a Pop Up Variable to determine which Action is run next. An option for each possible value the Variable can take will be displayed, allowing for different subsequent Actions to run.";
		}


		override public void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject (actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}

			runtimeVariable = GetVariable ();
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				ACDebug.LogWarning ("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd ();
			}
			
			if (variableID == -1)
			{
				return GenerateStopActionEnd ();
			}
			
			if (runtimeVariable != null)
			{
				if (runtimeVariable.type == VariableType.PopUp)
				{
					return ProcessResult (runtimeVariable.val, actions);
				}
				else
				{
					ACDebug.LogWarning ("Variable: Run sequence Action is referencing a Variable that does not exist!");
				}
			}
			
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			location = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", location);

			switch (location)
			{
				case VariableLocation.Global:
					if (AdvGame.GetReferences ().variablesManager != null)
					{
						variableID = AdvGame.GlobalVariableGUI ("PopUp variable:", variableID, VariableType.PopUp);
					}
					break;

				case VariableLocation.Local:
					if (isAssetFile)
					{
						EditorGUILayout.HelpBox ("Local variables cannot be accessed in ActionList assets.", MessageType.Info);
					}
					else if (localVariables != null)
					{
						variableID = AdvGame.LocalVariableGUI ("PopUp variable:", variableID, VariableType.PopUp);
					}
					else
					{
						EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
					}
					break;

				case VariableLocation.Component:
					variables = (Variables) EditorGUILayout.ObjectField ("Component:", variables, typeof (Variables), true);
					variablesConstantID = FieldToID <Variables> (variables, variablesConstantID);
					variables = IDToField <Variables> (variables, variablesConstantID, false);
					
					if (variables != null)
					{
						variableID = AdvGame.ComponentVariableGUI ("PopUp variable:", variableID, VariableType.PopUp, variables);	
					}
					break;
			}

			GVar _var = GetVariable ();
			if (_var != null)
			{
				numSockets = _var.popUps.Length;
				if (numSockets == 0)
				{
					EditorGUILayout.HelpBox ("The selected variable has no values!", MessageType.Warning);
				}
			}
		}


		override public string SetLabel ()
		{
			switch (location)
			{
				case VariableLocation.Global:
					if (AdvGame.GetReferences ().variablesManager != null)
					{
						return GetLabelString (AdvGame.GetReferences ().variablesManager.vars);
					}
					break;

				case VariableLocation.Local:
					if (!isAssetFile && localVariables != null)
					{
						return GetLabelString (localVariables.localVars);
					}
					break;

				case VariableLocation.Component:
					if (variables != null)
					{
						return GetLabelString (variables.vars);
					}
					break;
			}

			return string.Empty;
		}
		
		
		private string GetLabelString (List<GVar> vars)
		{
			if (vars != null)
			{
				foreach (GVar _var in vars)
				{
					if (_var.id == variableID && _var.type == VariableType.PopUp)
					{
						return _var.label;
					}
				}
			}

			return string.Empty;
		}


		override public void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (numSockets < 0)
			{
				numSockets = 0;
			}
		
			if (numSockets < endings.Count)
			{
				endings.RemoveRange (numSockets, endings.Count - numSockets);
			}
			else if (numSockets > endings.Count)
			{
				if (numSockets > endings.Capacity)
				{
					endings.Capacity = numSockets;
				}
				for (int i=endings.Count; i<numSockets; i++)
				{
					ActionEnd newEnd = new ActionEnd ();
					if (i > 0)
					{
						newEnd.resultAction = ResultAction.Stop;
					}
					endings.Add (newEnd);
				}
			}
			
			foreach (ActionEnd ending in endings)
			{
				if (showGUI)
				{
					EditorGUILayout.Space ();
					int i = endings.IndexOf (ending);

					GVar _var = GetVariable ();
					if (_var != null)
					{
						string[] popUpLabels = _var.GenerateEditorPopUpLabels ();
						ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If = '" + popUpLabels[i] + "':", (ResultAction) ending.resultAction);
					}
					else
					{
						ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If = '" + (i+1).ToString () + "':", (ResultAction) ending.resultAction);
					}
				}
				
				if (ending.resultAction == ResultAction.RunCutscene && showGUI)
				{
					if (isAssetFile)
					{
						ending.linkedAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", ending.linkedAsset, typeof (ActionListAsset), false);
					}
					else
					{
						ending.linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", ending.linkedCutscene, typeof (Cutscene), true);
					}
				}
				else if (ending.resultAction == ResultAction.Skip)
				{
					SkipActionGUI (ending, actions, showGUI);
				}
			}
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			if (location == VariableLocation.Local && variableID == oldLocalID)
			{
				location = VariableLocation.Global;
				variableID = newGlobalID;
				wasAmended = true;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool wasAmended = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (location == VariableLocation.Global && variableID == oldGlobalID)
			{
				wasAmended = true;
				if (isCorrectScene)
				{
					location = VariableLocation.Local;
					variableID = newLocalID;
				}
			}
			return wasAmended;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation _location, int varID, Variables _variables)
		{
			int thisCount = 0;

			if (location == _location && variableID == varID)
			{
				if (location != VariableLocation.Component || (variables != null && variables == _variables))
				{
					thisCount ++;
				}
			}

			thisCount += base.GetVariableReferences (parameters, _location, varID, _variables);
			return thisCount;
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (location == VariableLocation.Component)
			{
				AssignConstantID <Variables> (variables, variablesConstantID, -1);
			}
		}

		#endif


		private GVar GetVariable ()
		{
			GVar _var = null;

			switch (location)
			{
				case VariableLocation.Global:
					if (Application.isPlaying)
					{
						_var = GlobalVariables.GetVariable (variableID);
					}
					else if (AdvGame.GetReferences ().variablesManager)
					{
						_var = AdvGame.GetReferences ().variablesManager.GetVariable (variableID);
					}
					break;

				case VariableLocation.Local:
					if (!isAssetFile)
					{
						_var = LocalVariables.GetVariable (variableID, localVariables);
					}
					break;

				case VariableLocation.Component:
					Variables runtimeVariables = AssignFile <Variables> (variablesConstantID, variables);
					if (runtimeVariables != null)
					{
						_var = runtimeVariables.GetVariable (variableID);
					}
					break;
			}

			if (_var != null && _var.type == VariableType.PopUp)
			{
				return _var;
			}
			return null;
		}

	}
	
}