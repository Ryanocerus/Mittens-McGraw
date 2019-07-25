/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionCharRender.cs"
 * 
 *	This Action overrides Character
 *	render settings.
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
	public class ActionCharRender : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public bool isPlayer;
		public Char _char;
		protected Char runtimeChar;

		public RenderLock renderLock_sorting;
		public SortingMapType mapType;

		public int sortingOrder;
		public int sortingOrderParameterID = -1;
		public string sortingLayer;
		public int sortingLayerParameterID = -1;

		public RenderLock renderLock_scale;
		public int scale;

		public RenderLock renderLock_direction;
		public CharDirection direction;

		public RenderLock renderLock_sortingMap;
		public SortingMap sortingMap;
		
		
		public ActionCharRender ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Change rendering";
			description = "Overrides a Character's scale, sorting order, sprite direction or Sorting Map. This is intended mainly for 2D games.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile <Char> (parameters, parameterID, constantID, _char);
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}

			sortingOrder = AssignInteger (parameters, sortingOrderParameterID, sortingOrder);
			sortingLayer = AssignString (parameters, sortingLayerParameterID, sortingLayer);
		}
		
		
		override public float Run ()
		{
			if (runtimeChar != null)
			{
				if (renderLock_sorting == RenderLock.Set)
				{
					if (mapType == SortingMapType.OrderInLayer)
					{
						runtimeChar.SetSorting (sortingOrder);
					}
					else if (mapType == SortingMapType.SortingLayer)
					{
						runtimeChar.SetSorting (sortingLayer);
					}
				}
				else if (renderLock_sorting == RenderLock.Release)
				{
					runtimeChar.ReleaseSorting ();
				}

				if (runtimeChar.GetAnimEngine () != null)
				{
					runtimeChar.GetAnimEngine ().ActionCharRenderRun (this);
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					_char = KickStarter.player;
				}
				else
				{
					_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Character:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					_char = null;
				}
				else
				{
					_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
					
					constantID = FieldToID <Char> (_char, constantID);
					_char = IDToField <Char> (_char, constantID, false);
				}
			}

			if (_char)
			{
				EditorGUILayout.Space ();
				renderLock_sorting = (RenderLock) EditorGUILayout.EnumPopup ("Sorting:", renderLock_sorting);
				if (renderLock_sorting == RenderLock.Set)
				{
					mapType = (SortingMapType) EditorGUILayout.EnumPopup ("Sorting type:", mapType);
					if (mapType == SortingMapType.OrderInLayer)
					{
						sortingOrderParameterID = Action.ChooseParameterGUI ("New order:", parameters, sortingOrderParameterID, ParameterType.Integer);
						if (sortingOrderParameterID < 0)
						{
							sortingOrder = EditorGUILayout.IntField ("New order:", sortingOrder);
						}

					}
					else if (mapType == SortingMapType.SortingLayer)
					{
						sortingLayerParameterID = Action.ChooseParameterGUI ("New layer:", parameters, sortingLayerParameterID, ParameterType.String);
						if (sortingLayerParameterID < 0)
						{
							sortingLayer = EditorGUILayout.TextField ("New layer:", sortingLayer);
						}
					}
				}

				if (_char.GetAnimEngine ())
				{
					_char.GetAnimEngine ().ActionCharRenderGUI (this);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
			}

			EditorGUILayout.Space ();
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (!isPlayer)
			{
				if (saveScriptsToo)
				{
					if (!isPlayer && _char != null && _char.GetComponent <NPC>())
					{
						AddSaveScript <RememberNPC> (_char);
					}
				}

				AssignConstantID <Char> (_char, constantID, parameterID);
			}
		}
		
		
		public override string SetLabel ()
		{
			if (isPlayer)
			{
				return "Player";
			}
			else if (_char != null)
			{
				return "_char.name";
			}
			return string.Empty;
		}
		
		#endif
		
	}

}