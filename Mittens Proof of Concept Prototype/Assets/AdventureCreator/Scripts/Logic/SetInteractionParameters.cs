/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"SetInteractionParameters.cs"
 * 
 *	A component used to set all of an Interactions parameters when run as the result of interacting with a Hotspot.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A component used to set all of an Interactions parameters when run as the result of interacting with a Hotspot. */
	[AddComponentMenu("Adventure Creator/Hotspots/Set Interaction parameters")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_set_interaction_parameters.html")]
	#endif
	public class SetInteractionParameters : SetParametersBase
	{

		#region Variables

		[SerializeField] private Hotspot hotspot;
		[SerializeField] private InteractionType interactionType;
		private enum InteractionType { Use, Examine, Inventory, UnhandledInventory };

		[SerializeField] private int buttonIndex;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			EventManager.OnHotspotInteract += OnHotspotInteract;
		}


		private void OnDisable ()
		{
			EventManager.OnHotspotInteract -= OnHotspotInteract;
		}

		#endregion


		#region CustomEvents

		private void OnHotspotInteract (Hotspot hotspot, Button button)
		{
			if (this.hotspot == hotspot && button != null)
			{
				switch (interactionType)
				{
					case InteractionType.Use:
						if (hotspot.provideUseInteraction && hotspot.useButtons.Contains (button))
						{
							ProcessButton (button);
						}
						break;

					case InteractionType.Examine:
						if (hotspot.provideLookInteraction && hotspot.lookButton == button)
						{
							ProcessButton (button);
						}
						break;

					case InteractionType.Inventory:
						if (hotspot.provideInvInteraction && hotspot.invButtons.Contains (button))
						{
							ProcessButton (button);
						}
						break;

					case InteractionType.UnhandledInventory:
						if (hotspot.provideUnhandledInvInteraction && hotspot.unhandledInvButton == button)
						{
							ProcessButton (button);
						}
						break;
				}
			}
		}

		#endregion


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			if (hotspot == null)
			{
				hotspot = GetComponent <Hotspot>();
			}

			hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot:", hotspot, typeof (Hotspot), true);
			if (hotspot != null)
			{
				interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction type:", interactionType);

				if (interactionType == InteractionType.Use)
				{
					if (hotspot.provideUseInteraction && hotspot.useButtons != null && hotspot.useButtons.Count > 0)
					{
						string[] labelArray = new string[hotspot.useButtons.Count];
						for (int i=0; i<hotspot.useButtons.Count; i++)
						{
							string label = (KickStarter.cursorManager != null)
											? i.ToString () + ": " + KickStarter.cursorManager.GetLabelFromID (hotspot.useButtons[i].iconID, 0)
											: i.ToString ();
							labelArray[i] = label;
						}

						buttonIndex = EditorGUILayout.Popup ("Interaction:", buttonIndex, labelArray);
						ShowParametersGUI (hotspot.useButtons[buttonIndex], labelArray[buttonIndex]);
						return;
					}
				}
				else if (interactionType == InteractionType.Inventory)
				{
					if (hotspot.provideInvInteraction && hotspot.invButtons != null && hotspot.invButtons.Count > 0)
					{
						string[] labelArray = new string[hotspot.invButtons.Count];
						for (int i=0; i<hotspot.invButtons.Count; i++)
						{
							string label = (KickStarter.inventoryManager != null)
											? i.ToString () + ": " + KickStarter.inventoryManager.GetLabel (hotspot.invButtons[i].iconID)
											: i.ToString ();
							labelArray[i] = label;
						}

						buttonIndex = EditorGUILayout.Popup ("Interaction:", buttonIndex, labelArray);
						ShowParametersGUI (hotspot.invButtons[buttonIndex], labelArray[buttonIndex]);
						return;
					}
				}
				else if (interactionType == InteractionType.Examine && hotspot.provideLookInteraction && hotspot.lookButton != null)
				{
					ShowParametersGUI (hotspot.lookButton, "Examine");
					return;
				}
				else if (interactionType == InteractionType.UnhandledInventory && hotspot.provideUnhandledInvInteraction && hotspot.unhandledInvButton != null)
				{
					ShowParametersGUI (hotspot.unhandledInvButton, "Unhandled inventory");
					return;
				}

				EditorGUILayout.HelpBox ("No interactions of type '" + interactionType.ToString () + " available!", MessageType.Warning);
			}
		}


		private void ShowParametersGUI (Button button, string label)
		{
			if (hotspot.interactionSource == InteractionSource.InScene && button.interaction != null)
			{
				ShowParametersGUI (hotspot.useButtons[buttonIndex].interaction);
			}
			else if (hotspot.interactionSource == InteractionSource.AssetFile && button.assetFile != null)
			{
				ShowParametersGUI (hotspot.useButtons[buttonIndex].assetFile);
			}
			else
			{
				EditorGUILayout.HelpBox ("Cannot set parameters for Interaction '" + label + "', since it has no associated ActionList.", MessageType.Warning);
			}
		}


		private void ShowParametersGUI (Interaction interaction)
		{
			if (interaction == null) return;

			if (interaction.source == ActionListSource.AssetFile && interaction.assetFile != null && interaction.assetFile.useParameters && interaction.assetFile.parameters != null && interaction.assetFile.parameters.Count > 0)
			{
				ShowActionListReference (interaction.assetFile);
				ShowParametersGUI (interaction.assetFile.parameters, interaction.syncParamValues);
			}
			else if (interaction.source == ActionListSource.InScene && interaction.useParameters && interaction.parameters != null && interaction.parameters.Count > 0)
			{
				ShowActionListReference (interaction);
				ShowParametersGUI (interaction.parameters, false);
			}
			else
			{
				EditorGUILayout.HelpBox ("No parameters defined for Interaction '" + interaction.gameObject.name + "'.", MessageType.Warning);
			}
		}


		private void ShowParametersGUI (ActionListAsset actionListAsset)
		{
			if (actionListAsset == null) return;

			if (actionListAsset.useParameters && actionListAsset.parameters != null && actionListAsset.parameters.Count > 0)
			{
				ShowActionListReference (actionListAsset);
				ShowParametersGUI (actionListAsset.parameters, true);
			}
			else
			{
				EditorGUILayout.HelpBox ("No parameters defined for ActionList Assset '" + actionListAsset.name + "'.", MessageType.Warning);
			}
		}


		private void ShowActionListReference (Interaction interaction)
		{
			if (interaction != null)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Interaction: " + interaction);
				if (GUILayout.Button ("Ping", GUILayout.Width (50f)))
				{
					EditorGUIUtility.PingObject (interaction);
				}
				EditorGUILayout.EndHorizontal ();
			}
		}


		private void ShowActionListReference (ActionListAsset actionListAsset)
		{
			if (actionListAsset != null)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Asset file: " + actionListAsset);
				if (GUILayout.Button ("Ping", GUILayout.Width (50f)))
				{
					EditorGUIUtility.PingObject (actionListAsset);
				}
				EditorGUILayout.EndHorizontal ();
			}
		}

		#endif


		private void ProcessButton (AC.Button button)
		{
			if (hotspot.interactionSource == InteractionSource.AssetFile)
			{
				AssignParameterValues (button.assetFile);
			}
			else if (hotspot.interactionSource == InteractionSource.InScene)
			{
				AssignParameterValues (button.interaction);
			}
		}

	}

}