/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionVarCheck.cs"
 * 
 *	This action checks to see if a Variable has been assigned a certain value,
 *	and performs something accordingly.
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
	public class ActionVarCheck : ActionCheck
	{

		public int parameterID = -1;
		public int variableID;
		public int variableNumber;

		public int checkParameterID = -1;

		public GetVarMethod getVarMethod = GetVarMethod.EnteredValue;
		public int compareVariableID;

		public int intValue;
		public float floatValue;
		public IntCondition intCondition;
		public bool isAdditive = false;
		
		public BoolValue boolValue = BoolValue.True;
		public BoolCondition boolCondition;

		public string stringValue;
		public bool checkCase = true;

		public Vector3 vector3Value;
		public VectorCondition vectorCondition = VectorCondition.EqualTo;

		public VariableLocation location = VariableLocation.Global;
		private LocalVariables localVariables;

		public Variables variables;
		public int variablesConstantID = 0;

		public Variables compareVariables;
		public int compareVariablesConstantID = 0;

		private GVar runtimeVariable;
		private GVar runtimeCompareVariable;


		public ActionVarCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Check";
			description = "Queries the value of both Global and Local Variables declared in the Variables Manager. Variables can be compared with a fixed value, or with the values of other Variables.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			intValue = AssignInteger (parameters, checkParameterID, intValue);
			boolValue = AssignBoolean (parameters, checkParameterID, boolValue);
			floatValue = AssignFloat (parameters, checkParameterID, floatValue);
			vector3Value = AssignVector3 (parameters, checkParameterID, vector3Value);
			stringValue = AssignString (parameters, checkParameterID, stringValue);

			runtimeVariable = null;
			switch (location)
			{
				case VariableLocation.Global:
					variableID = AssignVariableID (parameters, parameterID, variableID);
					runtimeVariable = GlobalVariables.GetVariable (variableID, true);
					break;

				case VariableLocation.Local:
					if (!isAssetFile)
					{
						variableID = AssignVariableID (parameters, parameterID, variableID);
						runtimeVariable = LocalVariables.GetVariable (variableID, localVariables);
					}
					break;

				case VariableLocation.Component:
					Variables runtimeVariables = AssignFile <Variables> (variablesConstantID, variables);
					if (runtimeVariables != null)
					{
						runtimeVariable = runtimeVariables.GetVariable (variableID);
					}
					runtimeVariable = AssignVariable (parameters, parameterID, runtimeVariable);
					break;
			}

			runtimeCompareVariable = null;
			switch (getVarMethod)
			{
				case GetVarMethod.GlobalVariable:
					compareVariableID = AssignVariableID (parameters, checkParameterID, compareVariableID);
					runtimeCompareVariable = GlobalVariables.GetVariable (compareVariableID, true);
					break;

				case GetVarMethod.LocalVariable:
					compareVariableID = AssignVariableID (parameters, checkParameterID, compareVariableID);
					runtimeCompareVariable = LocalVariables.GetVariable (compareVariableID, localVariables);
					break;

				case GetVarMethod.ComponentVariable:
					Variables runtimeCompareVariables = AssignFile <Variables> (compareVariablesConstantID, compareVariables);
					if (runtimeCompareVariables != null)
					{
						runtimeCompareVariable = runtimeCompareVariables.GetVariable (compareVariableID);
					}
					runtimeCompareVariable = AssignVariable (parameters, checkParameterID, runtimeCompareVariable);
					break;

				default:
					break;
			}
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
		}

		
		override public ActionEnd End (List<AC.Action> actions)
		{
			if (variableID == -1)
			{
				return GenerateStopActionEnd ();
			}

			if (getVarMethod == GetVarMethod.GlobalVariable ||
				getVarMethod == GetVarMethod.LocalVariable ||
				getVarMethod == GetVarMethod.ComponentVariable)
			{
				if (runtimeCompareVariable == null)
				{
					return GenerateStopActionEnd ();
				}
			}

			if (runtimeVariable != null)
			{
				return ProcessResult (CheckCondition (runtimeVariable, runtimeCompareVariable), actions);
			}

			ACDebug.LogWarning ("The 'Variable: Check' Action halted the ActionList because it cannot find the " + location.ToString () + " Variable with an ID of " + variableID);
			return GenerateStopActionEnd ();
		}
		
		
		private bool CheckCondition (GVar _var, GVar _compareVar)
		{
			if (_var == null)
			{
				ACDebug.LogWarning ("Cannot check state of variable since it cannot be found!");
				return false;
			}

			if (_compareVar != null && _var != null && _compareVar.type != _var.type)
			{
				ACDebug.LogWarning ("Cannot compare " + _var.label + " and " + _compareVar.label + " as they are not the same type!");
				return false;
			}

			if (_var.type == VariableType.Boolean)
			{
				int fieldValue = _var.val;
				int compareValue = (int) boolValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.val;
				}

				switch (boolCondition)
				{
					case BoolCondition.EqualTo:
						return (fieldValue == compareValue);

					case BoolCondition.NotEqualTo:
						return (fieldValue != compareValue);
				}
			}

			else if (_var.type == VariableType.Integer || _var.type == VariableType.PopUp)
			{
				int fieldValue = _var.val;
				int compareValue = intValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.val;
				}

				switch (intCondition)
				{
					case IntCondition.EqualTo:
						return (fieldValue == compareValue);

					case IntCondition.NotEqualTo:
						return (fieldValue != compareValue);

					case IntCondition.LessThan:
						return (fieldValue < compareValue);

					case IntCondition.MoreThan:
						return (fieldValue > compareValue);
				}
			}

			else if (_var.type == VariableType.Float)
			{
				float fieldValue = _var.floatVal;
				float compareValue = floatValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.floatVal;
				}

				switch (intCondition)
				{
					case IntCondition.EqualTo:
						return Mathf.Approximately (fieldValue, compareValue);

					case IntCondition.NotEqualTo:
						return !Mathf.Approximately (fieldValue, compareValue);

					case IntCondition.LessThan:
						return (fieldValue < compareValue);

					case IntCondition.MoreThan:
						return (fieldValue > compareValue);
				}
			}

			else if (_var.type == VariableType.String)
			{
				string fieldValue = _var.textVal;
				string compareValue = AdvGame.ConvertTokens (stringValue);
				if (_compareVar != null)
				{
					compareValue = _compareVar.textVal;
				}

				if (!checkCase)
				{
					fieldValue = fieldValue.ToLower ();
					compareValue = compareValue.ToLower ();
				}

				switch (boolCondition)
				{
					case BoolCondition.EqualTo:
						return (fieldValue == compareValue);

					case BoolCondition.NotEqualTo:
						return (fieldValue != compareValue);
				}
			}

			else if (_var.type == VariableType.Vector3)
			{
				switch (vectorCondition)
				{
					case VectorCondition.EqualTo:
						return (_var.vector3Val == vector3Value);

					case VectorCondition.MagnitudeGreaterThan:
						return (_var.vector3Val.magnitude > floatValue);
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			location = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", location);

			if (isAssetFile && getVarMethod == GetVarMethod.LocalVariable)
			{
				EditorGUILayout.HelpBox ("Local Variables cannot be referenced by Asset-based Actions.", MessageType.Warning);
				return;
			}

			switch (location)
			{
				case VariableLocation.Global:
					if (AdvGame.GetReferences ().variablesManager)
					{
						VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;

						parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.GlobalVariable);
						if (parameterID >= 0)
						{
							SetPlaceholderVar (variablesManager.vars, variableID);
							ShowPlaceholderVar ();
							variableID = ShowVarGUI (parameters, variablesManager.vars, variableID, false);
						}
						else
						{
							setPlaceholder = false;
							variableID = ShowVarGUI (parameters, variablesManager.vars, variableID, true);
						}
					}
					break;

				case VariableLocation.Local:
					if (localVariables != null)
					{
						parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.LocalVariable);
						if (parameterID >= 0)
						{
							SetPlaceholderVar (localVariables.localVars, variableID);
							ShowPlaceholderVar ();
							variableID = ShowVarGUI (parameters, localVariables.localVars, variableID, false);
						}
						else
						{
							setPlaceholder = false;
							variableID = ShowVarGUI (parameters, localVariables.localVars, variableID, true);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
					}
					break;

				case VariableLocation.Component:
					parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.ComponentVariable);
					if (parameterID >= 0)
					{
						ShowPlaceholderVar ();
						variableID = ShowVarGUI (parameters, (variables != null) ? variables.vars : null, variableID, false);
					}
					else
					{
						variables = (Variables) EditorGUILayout.ObjectField ("Component:", variables, typeof (Variables), true);
						variablesConstantID = FieldToID <Variables> (variables, variablesConstantID);
						variables = IDToField <Variables> (variables, variablesConstantID, false);
						
						if (variables != null)
						{
							variableID = ShowVarGUI (parameters, variables.vars, variableID, true);
							setPlaceholder = false;
							SetPlaceholderVar (variables.vars, variableID);
						}
					}
					break;
			}
		}


		[SerializeField] private VariableType placeholderType;
		[SerializeField] private bool setPlaceholder;

		private void SetPlaceholderVar (List<GVar> vars, int ID)
		{
			if (!setPlaceholder)
			{
				foreach (GVar _var in vars)
				{
					if (_var.id == ID)
					{
						placeholderType = _var.type;
						setPlaceholder = true;
						break;
					}
				}
			}
		}


		private void ShowPlaceholderVar ()
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Placeholder type:");
			EditorGUILayout.LabelField (placeholderType.ToString (), EditorStyles.whiteLabel, GUILayout.MaxWidth (50f));
			EditorGUILayout.EndHorizontal ();
		}


		private int ShowVarSelectorGUI (List<GVar> vars, int ID, string label)
		{
			variableNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}

			variableNumber = GetVarNumber (vars, ID);

			if (variableNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				ACDebug.LogWarning ("Previously chosen variable no longer exists!");
				variableNumber = 0;
				ID = 0;
			}

			variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray());
			ID = vars[variableNumber].id;

			return ID;
		}


		private int ShowVarGUI (List<ActionParameter> parameters, List<GVar> _vars, int ID, bool changeID)
		{
			VariableType showType = VariableType.Boolean;

			if (changeID)
			{
				if (_vars != null && _vars.Count > 0)
				{
					ID = ShowVarSelectorGUI (_vars, ID, "Variable:");

					variableNumber = Mathf.Min (variableNumber, _vars.Count-1);
					getVarMethod = (GetVarMethod) EditorGUILayout.EnumPopup ("Compare with:", getVarMethod);

					showType = _vars[variableNumber].type;
				}
				else
				{
					EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
					ID = -1;
					variableNumber = -1;
					return ID;
				}
			}
			else
			{
				showType = placeholderType;
			}

			switch (showType)
			{
				case VariableType.Boolean:
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Condition:", boolCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Boolean:", parameters, checkParameterID, ParameterType.Boolean);
						if (checkParameterID < 0)
						{
							boolValue = (BoolValue) EditorGUILayout.EnumPopup ("Boolean:", boolValue);
						}
					}
					break;

				case VariableType.Integer:
					intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Integer:", parameters, checkParameterID, ParameterType.Integer);
						if (checkParameterID < 0)
						{
							intValue = EditorGUILayout.IntField ("Integer:", intValue);
						}
					}
					break;

				case VariableType.Float:
					intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Float:", parameters, checkParameterID, ParameterType.Float);
						if (checkParameterID < 0)
						{
							floatValue = EditorGUILayout.FloatField ("Float:", floatValue);
						}
					}
					break;

				case VariableType.PopUp:
					intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Condition:", intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Value:", parameters, checkParameterID, ParameterType.Integer);
						if (checkParameterID < 0)
						{
							if (changeID && _vars != null && _vars.Count > variableNumber)
							{
								string[] popUpLabels = _vars[variableNumber].GenerateEditorPopUpLabels ();
								intValue = EditorGUILayout.Popup ("Value:", intValue, popUpLabels);
							}
							else
							{
								intValue = EditorGUILayout.IntField ("Index value:", intValue);
							}
						}
					}
					break;

				case VariableType.String:
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup ("Condition:", boolCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("String:", parameters, checkParameterID, ParameterType.String);
						if (checkParameterID < 0)
						{
							stringValue = EditorGUILayout.TextField ("String:", stringValue);
						}
					}
					checkCase = EditorGUILayout.Toggle ("Case-senstive?", checkCase);
					break;

				case VariableType.Vector3:
					vectorCondition = (VectorCondition) EditorGUILayout.EnumPopup ("Condition:", vectorCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						if (vectorCondition == VectorCondition.MagnitudeGreaterThan)
						{
							checkParameterID = Action.ChooseParameterGUI ("Float:", parameters, checkParameterID, ParameterType.Float);
							if (checkParameterID < 0)
							{
								floatValue = EditorGUILayout.FloatField ("Float:", floatValue);
							}
						}
						else if (vectorCondition == VectorCondition.EqualTo)
						{
							checkParameterID = Action.ChooseParameterGUI ("Vector3:", parameters, checkParameterID, ParameterType.Vector3);
							if (checkParameterID < 0)
							{
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Vector3:", GUILayout.MaxWidth (60f));
								vector3Value = EditorGUILayout.Vector3Field ("", vector3Value);
								EditorGUILayout.EndHorizontal ();
							}
						}
					}
					break;

				default:
					break;
			}

			if (getVarMethod == GetVarMethod.GlobalVariable)
			{
				if (AdvGame.GetReferences ().variablesManager == null || AdvGame.GetReferences ().variablesManager.vars == null || AdvGame.GetReferences ().variablesManager.vars.Count == 0)
				{
					EditorGUILayout.HelpBox ("No Global variables exist!", MessageType.Info);
				}
				else
				{
					checkParameterID = Action.ChooseParameterGUI ("Global variable:", parameters, checkParameterID, ParameterType.GlobalVariable);
					if (checkParameterID < 0)
					{
						compareVariableID = ShowVarSelectorGUI (AdvGame.GetReferences ().variablesManager.vars, compareVariableID, "Global variable:");
					}
				}
			}
			else if (getVarMethod == GetVarMethod.LocalVariable)
			{
				if (localVariables == null || localVariables.localVars == null || localVariables.localVars.Count == 0)
				{
					EditorGUILayout.HelpBox ("No Local variables exist!", MessageType.Info);
				}
				else
				{
					checkParameterID = Action.ChooseParameterGUI ("Local variable:", parameters, checkParameterID, ParameterType.LocalVariable);
					if (checkParameterID < 0)
					{
						compareVariableID = ShowVarSelectorGUI (localVariables.localVars, compareVariableID, "Local variable:");
					}
				}
			}
			else if (getVarMethod == GetVarMethod.ComponentVariable)
			{
				checkParameterID = Action.ChooseParameterGUI ("Component variable:", parameters, checkParameterID, ParameterType.ComponentVariable);
				if (checkParameterID < 0)
				{
					compareVariables = (Variables) EditorGUILayout.ObjectField ("Component", compareVariables, typeof (Variables), true);
					compareVariablesConstantID = FieldToID <Variables> (compareVariables, compareVariablesConstantID);
					compareVariables = IDToField <Variables> (compareVariables, compareVariablesConstantID, false);
					
					if (compareVariables != null)
					{
						compareVariableID = ShowVarSelectorGUI (compareVariables.vars, compareVariableID, "Component variable:");
					}
				}
			}

			return ID;
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
			string labelAdd = string.Empty;

			if (parameterID < 0 && vars.Count > 0 && vars.Count > variableNumber && variableNumber > -1)
			{
				labelAdd = vars[variableNumber].label;

				switch (vars[variableNumber].type)
				{
					case VariableType.Boolean:
						labelAdd += " " + boolCondition.ToString () + " " + boolValue.ToString ();
						break;

					case VariableType.Integer:
						labelAdd += " " + intCondition.ToString () + " " + intValue.ToString ();
						break;

					case VariableType.Float:
						labelAdd += " " + intCondition.ToString () + " " + floatValue.ToString ();
						break;

					case VariableType.String:
						labelAdd += " " + boolCondition.ToString () + " " + stringValue;
						break;

					case VariableType.PopUp:
						labelAdd += " " + intCondition.ToString () + " " + vars[variableNumber].popUps[intValue];
						break;

					default:
						break;
				}
			}

			return labelAdd;
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

			if (getVarMethod == GetVarMethod.LocalVariable && compareVariableID == oldLocalID)
			{
				getVarMethod = GetVarMethod.GlobalVariable;
				compareVariableID = newGlobalID;
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

			if (getVarMethod == GetVarMethod.GlobalVariable && compareVariableID == oldGlobalID)
			{
				wasAmended = true;
				if (isCorrectScene)
				{
					getVarMethod = GetVarMethod.LocalVariable;
					compareVariableID = newLocalID;
				}
			}

			return wasAmended;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation _location, int varID, Variables _variables)
		{
			int thisCount = 0;
			if (location == _location && variableID == varID && parameterID < 0)
			{
				if (location != VariableLocation.Component || (variables != null && variables == _variables))
				{
					thisCount ++;
				}
			}

			if (getVarMethod == GetVarMethod.LocalVariable && _location == VariableLocation.Local && compareVariableID == varID)
			{
				thisCount ++;
			}
			else if (getVarMethod == GetVarMethod.GlobalVariable && _location == VariableLocation.Global && compareVariableID == varID)
			{
				thisCount ++;
			}

			thisCount += base.GetVariableReferences (parameters, _location, varID, _variables);
			return thisCount;
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (location == VariableLocation.Component)
			{
				AssignConstantID <Variables> (variables, variablesConstantID, parameterID);
			}
		}

		#endif


		private int GetVarNumber (List<GVar> vars, int ID)
		{
			int i = 0;
			foreach (GVar _var in vars)
			{
				if (_var.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}

	}

}