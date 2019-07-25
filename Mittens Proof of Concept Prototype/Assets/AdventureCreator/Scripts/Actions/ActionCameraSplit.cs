﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionCameraSplit.cs"
 * 
 *	This Action splits the screen horizontally or vertically.
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
	public class ActionCameraSplit : Action
	{

		public int parameterID1 = -1;
		public int parameterID2 = -1;

		public int constantID1 = 0;
		public int constantID2 = 0;

		public float splitAmount1 = 0.49f;
		public float splitAmount2 = 0.49f;

		public _Camera cam1;
		public _Camera cam2;

		protected _Camera runtimeCam1;
		protected _Camera runtimeCam2;

		public Rect overlayRect = new Rect (0.5f, 0.5f, 0.5f, 0.5f);

		public bool turnOff;
		public CameraSplitOrientation orientation;
		public bool mainIsTopLeft;
		
		
		public ActionCameraSplit ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Split-screen";
			description = "Displays two cameras on the screen at once, arranged either horizontally or vertically. Which camera is the 'main' (i.e. which one responds to mouse clicks) can also be set.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			runtimeCam1 = AssignFile <_Camera> (parameters, parameterID1, constantID1, cam1);
			runtimeCam2 = AssignFile <_Camera> (parameters, parameterID2, constantID2, cam2);
		}
		
		
		override public float Run ()
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			mainCamera.RemoveSplitScreen ();

			if (turnOff || runtimeCam1 == null || runtimeCam2 == null)
			{
				return 0f;
			}

			if (orientation == CameraSplitOrientation.Overlay)
			{
				mainCamera.SetBoxOverlay (runtimeCam1, runtimeCam2, overlayRect);
			}
			else
			{
				if (mainIsTopLeft)
				{
					mainCamera.SetSplitScreen (runtimeCam1, runtimeCam2, orientation, mainIsTopLeft, splitAmount1, splitAmount2);
				}
				else
				{
					mainCamera.SetSplitScreen (runtimeCam2, runtimeCam1, orientation, mainIsTopLeft, splitAmount1, splitAmount2);
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			turnOff = EditorGUILayout.Toggle ("Just disable previous?", turnOff);
			if (!turnOff)
			{
				string label1 = string.Empty;
				string label2 = string.Empty;

				orientation = (CameraSplitOrientation) EditorGUILayout.EnumPopup ("Orientation:", orientation);

				switch (orientation)
				{
					case CameraSplitOrientation.Horizontal:
						label1 = "Top";
						label2 = "Bottom";
						break;

					case CameraSplitOrientation.Vertical:
						label1 = "Left";
						label2 = "Right";
						break;

					case CameraSplitOrientation.Overlay:
						label1 = "Underlay";
						label2 = "Overlay";
						break;
				}

				parameterID1 = Action.ChooseParameterGUI (label1 + " camera:", parameters, parameterID1, ParameterType.GameObject);
				if (parameterID1 >= 0)
				{
					constantID1 = 0;
					cam1 = null;
				}
				else
				{
					cam1 = (_Camera) EditorGUILayout.ObjectField (label1 + " camera:", cam1, typeof (_Camera), true);
					
					constantID1 = FieldToID <_Camera> (cam1, constantID1);
					cam1 = IDToField <_Camera> (cam1, constantID1, false);
				}

				if (orientation != CameraSplitOrientation.Overlay)
				{
					splitAmount1 = EditorGUILayout.Slider (label1 + " camera space:", splitAmount1, 0f, 1f);
				}

				parameterID2 = Action.ChooseParameterGUI (label2 + " camera:", parameters, parameterID2, ParameterType.GameObject);
				if (parameterID2 >= 0)
				{
					constantID2 = 0;
					cam2 = null;
				}
				else
				{
					cam2 = (_Camera) EditorGUILayout.ObjectField (label2 + " camera:", cam2, typeof (_Camera), true);
					
					constantID2 = FieldToID <_Camera> (cam2, constantID2);
					cam2 = IDToField <_Camera> (cam2, constantID2, false);
				}

				if (orientation == CameraSplitOrientation.Overlay)
				{
					overlayRect.x = EditorGUILayout.Slider ("Centre X:", overlayRect.x, 0f, 1f);
					overlayRect.y = EditorGUILayout.Slider ("Centre Y:", overlayRect.y, 0f, 1f);

					overlayRect.width = EditorGUILayout.Slider ("Size X:", overlayRect.width, 0f, 1f);
					overlayRect.height = EditorGUILayout.Slider ("Size Y:", overlayRect.height, 0f, 1f);
				}
				else
				{
					splitAmount2 = Mathf.Min (splitAmount2, 1f-splitAmount1);
					splitAmount2 = EditorGUILayout.Slider (label2 + " camera space:", splitAmount2, 0f, 1f);
					splitAmount1 = Mathf.Min (splitAmount1, 1f-splitAmount2);

					mainIsTopLeft = EditorGUILayout.Toggle ("Main Camera is " + label1.ToLower () + "?", mainIsTopLeft);
				}
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <ConstantID> (cam1);
				AddSaveScript <ConstantID> (cam2);
			}

			AssignConstantID <_Camera> (cam1, constantID1, parameterID1);
			AssignConstantID <_Camera> (cam2, constantID2, parameterID2);
		}
		
		
		public override string SetLabel ()
		{
			return orientation.ToString ();
		}
		
		#endif

	}

}