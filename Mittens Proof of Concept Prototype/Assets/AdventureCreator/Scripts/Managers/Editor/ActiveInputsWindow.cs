using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AC
{
	
	public class ActiveInputsWindow : EditorWindow
	{
		
		private SettingsManager settingsManager;
		private Vector2 scrollPos;
		private ActiveInput selectedActiveInput;
		private int sideInput = -1;

		private bool showActiveInputsList = true;
		private bool showSelectedActiveInput = true;

		
		[MenuItem ("Adventure Creator/Editors/Active Inputs Editor", false, 0)]
		public static void Init ()
		{
			ActiveInputsWindow window = GetWindowWithRect <ActiveInputsWindow> (new Rect (0, 0, 450, 460), true, "Active inputs", true);
			UnityVersionHandler.SetWindowTitle (window, "Active Inputs");
			window.position = new Rect (300, 200, 450, 460);
		}
		
		
		private void OnEnable ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
		}
		
		
		private void OnGUI ()
		{
			if (settingsManager == null)
			{
				EditorGUILayout.HelpBox ("A Settings Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			ActiveInput.Upgrade ();
			ShowActiveInputsGUI ();

			UnityVersionHandler.CustomSetDirty (settingsManager);
		}
		

		private void ShowActiveInputsGUI ()
		{
			EditorGUILayout.HelpBox ("Active Inputs are used to trigger ActionList assets when an input key is pressed under certain gameplay conditions.", MessageType.Info);

			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showActiveInputsList = CustomGUILayout.ToggleHeader (showActiveInputsList, "Active inputs");
			if (showActiveInputsList)
			{
				scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (settingsManager.activeInputs.Count * 21, 185f)+5));
				foreach (ActiveInput activeInput in settingsManager.activeInputs)
				{
					EditorGUILayout.BeginHorizontal ();
					
					if (GUILayout.Toggle (selectedActiveInput == activeInput, activeInput.ID + ": " + activeInput.Label, "Button"))
					{
						if (selectedActiveInput != activeInput)
						{
							DeactivateAllInputs ();
							ActivateInput (activeInput);
						}
					}

					if (GUILayout.Button ("", CustomStyles.IconCog))
					{
						SideMenu (activeInput);
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				EditorGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				if (GUILayout.Button ("Create new Active Input"))
				{
					Undo.RecordObject (this, "Create new Active Input");

					if (settingsManager.activeInputs.Count > 0)
					{
						List<int> idArray = new List<int>();
						foreach (ActiveInput activeInput in settingsManager.activeInputs)
						{
							idArray.Add (activeInput.ID);
						}
						idArray.Sort ();

						ActiveInput newActiveInput = new ActiveInput (idArray.ToArray ());
						settingsManager.activeInputs.Add (newActiveInput);

						DeactivateAllInputs ();
						ActivateInput (newActiveInput);
					}
					else
					{
						ActiveInput newActiveInput = new ActiveInput (1);
						settingsManager.activeInputs.Add (newActiveInput);
					}
				}
				EditorGUILayout.EndVertical ();
			}

			EditorGUILayout.Space ();

			if (selectedActiveInput != null && settingsManager.activeInputs.Contains (selectedActiveInput))
			{
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);

				showSelectedActiveInput = CustomGUILayout.ToggleHeader (showSelectedActiveInput, "Input #" + selectedActiveInput.ID + ": " + selectedActiveInput.label);
				if (showSelectedActiveInput)
				{
					string defaultName = "ActiveInput_" + selectedActiveInput.Label;

					selectedActiveInput.label = CustomGUILayout.TextField ("Label:", selectedActiveInput.label, "", "An Editor-friendly name");
					selectedActiveInput.inputName = CustomGUILayout.TextField ("Input button:", selectedActiveInput.inputName, "", "The name of the Input button, as defined in the Input Manager");
					selectedActiveInput.inputType = (SimulateInputType) CustomGUILayout.EnumPopup ("Input type:", selectedActiveInput.inputType, "", "What type of input is expected");
					if (selectedActiveInput.inputType == SimulateInputType.Axis)
					{
						selectedActiveInput.axisThreshold = CustomGUILayout.Slider ("Axis threshold:", selectedActiveInput.axisThreshold, -1f, 1f, "", "The threshold value for the axis to trigger the ActionListAsset");
					}
					selectedActiveInput.enabledOnStart = CustomGUILayout.Toggle ("Enabled by default?", selectedActiveInput.enabledOnStart, "", "If True, the active input is enabled when the game begins");
					selectedActiveInput.gameState = (GameState) CustomGUILayout.EnumPopup ("Available when game is:", selectedActiveInput.gameState, "", "What state the game must be in for the actionListAsset to run");
					selectedActiveInput.actionListAsset = ActionListAssetMenu.AssetGUI ("ActionList when triggered:", selectedActiveInput.actionListAsset, defaultName, "", "The ActionListAsset to run when the input button is pressed");
				}
				EditorGUILayout.EndVertical ();
			}
		}


		private void SideMenu (ActiveInput activeInput)
		{
			GenericMenu menu = new GenericMenu ();
			sideInput = settingsManager.activeInputs.IndexOf (activeInput);
			
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (settingsManager.activeInputs.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideInput > 0 || sideInput < settingsManager.activeInputs.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideInput > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
			}
			if (sideInput < settingsManager.activeInputs.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
			}

			menu.ShowAsContext ();
		}


		private void Callback (object obj)
		{
			if (sideInput >= 0)
			{
				ActiveInput tempInput = settingsManager.activeInputs[sideInput];
				
				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (settingsManager, "Insert Active Input");
					settingsManager.activeInputs.Insert (sideInput+1, new ActiveInput (GetIDList ().ToArray ()));
					break;
					
				case "Delete":
					Undo.RecordObject (settingsManager, "Delete Active Input");
					if (tempInput == selectedActiveInput)
					{
						DeactivateAllInputs ();
					}
					settingsManager.activeInputs.RemoveAt (sideInput);
					break;
					
				case "Move up":
					Undo.RecordObject (settingsManager, "Move Active Input up");
					settingsManager.activeInputs.RemoveAt (sideInput);
					settingsManager.activeInputs.Insert (sideInput-1, tempInput);
					break;
					
				case "Move down":
					Undo.RecordObject (settingsManager, "Move Active Input down");
					settingsManager.activeInputs.RemoveAt (sideInput);
					settingsManager.activeInputs.Insert (sideInput+1, tempInput);
					break;

				case "Move to top":
					Undo.RecordObject (settingsManager, "Move Active Input to top");
					settingsManager.activeInputs.RemoveAt (sideInput);
					settingsManager.activeInputs.Insert (0, tempInput);
					break;

				case "Move to bottom":
					Undo.RecordObject (settingsManager, "Move Active Input to bottom");
					settingsManager.activeInputs.Add (tempInput);
					settingsManager.activeInputs.RemoveAt (sideInput);
					break;
				}
			}
			
			EditorUtility.SetDirty (settingsManager);
			AssetDatabase.SaveAssets ();
			
			sideInput = -1;
		}


		private void DeactivateAllInputs ()
		{
			selectedActiveInput = null;
		}


		private List<int> GetIDList ()
		{
			List<int> idList = new List<int>();
			foreach (ActiveInput activeInput in settingsManager.activeInputs)
			{
				idList.Add (activeInput.ID);
			}
			
			idList.Sort ();

			return idList;
		}


		private void ActivateInput (ActiveInput activeInput)
		{
			selectedActiveInput = activeInput;
		}
		
	}
	
}
