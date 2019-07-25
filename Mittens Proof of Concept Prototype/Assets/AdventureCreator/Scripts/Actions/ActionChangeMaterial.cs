/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionChangeMaterial.cs"
 * 
 *	This Action allows you to change an object's material.
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
	public class ActionChangeMaterial : Action
	{

		public int constantID = 0;
		public int parameterID = -1;

		public bool isPlayer;
		public GameObject obToAffect;
		private GameObject runtimeObToAffect;
		public int materialIndex;
		public Material newMaterial;
		public int newMaterialParameterID = -1;
		
		
		public ActionChangeMaterial ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Change material";
			description = "Changes the material on any scene-based mesh object.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				runtimeObToAffect = GetPlayerRenderer (KickStarter.player);
			}
			else
			{
				runtimeObToAffect = AssignFile (parameters, parameterID, constantID, obToAffect);
			}

			newMaterial = (Material) AssignObject <Material> (parameters, newMaterialParameterID, newMaterial);
		}

		
		override public float Run ()
		{
			if (runtimeObToAffect && newMaterial)
			{
				Renderer _renderer = runtimeObToAffect.GetComponent <Renderer>();
				if (_renderer != null)
				{
					Material[] mats = _renderer.materials;
					mats[materialIndex] = newMaterial;
					runtimeObToAffect.GetComponent <Renderer>().materials = mats;
				}
			}
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Affect player?", isPlayer);
			if (!isPlayer)
			{
				parameterID = Action.ChooseParameterGUI ("Object to affect:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					obToAffect = null;
				}
				else
				{
					obToAffect = (GameObject) EditorGUILayout.ObjectField ("Renderer:", obToAffect, typeof (GameObject), true);
					
					constantID = FieldToID (obToAffect, constantID);
					obToAffect = IDToField (obToAffect, constantID, true, false);
				}
			}

			materialIndex = EditorGUILayout.IntSlider ("Material index:", materialIndex, 0, 10);

			newMaterialParameterID = Action.ChooseParameterGUI ("New material:", parameters, newMaterialParameterID, ParameterType.UnityObject);
			if (newMaterialParameterID < 0)
			{
				newMaterial = (Material) EditorGUILayout.ObjectField ("New material:", newMaterial, typeof (Material), false);
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			GameObject obToUpdate = obToAffect;

			if (isPlayer)
			{
				if (!fromAssetFile && GameObject.FindObjectOfType <Player>() != null)
				{
					obToUpdate = GetPlayerRenderer (GameObject.FindObjectOfType <Player>());
				}

				if (obToUpdate == null && AdvGame.GetReferences ().settingsManager != null)
				{
					Player player = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
					obToUpdate = GetPlayerRenderer (player);
				}
			}

			if (saveScriptsToo)
			{
				AddSaveScript <RememberMaterial> (obToUpdate);
			}
			AssignConstantID (obToUpdate, constantID, parameterID);
		}


		public override string SetLabel ()
		{
			if (obToAffect != null)
			{
				string labelAdd = obToAffect.gameObject.name;
				if (newMaterial != null)
				{
					labelAdd += " - " + newMaterial;
				}
				return labelAdd;
			}
			return string.Empty;
		}
		
		#endif


		private GameObject GetPlayerRenderer (Player player)
		{
			if (player == null)
			{
				return null;
			}

			if (player.spriteChild != null && player.spriteChild.GetComponent <Renderer>())
			{
			    return player.spriteChild.gameObject;
			}

			if (player.GetComponentInChildren <Renderer>())
			{
				return player.gameObject.GetComponentInChildren <Renderer>().gameObject;
			}
			else
			{
				return player.gameObject;
			}
		}

	}
	
}