﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"OptionalMouseInputModule.cs"
 * 
 *	This script is an alternative to the Standalone Input Module that makes mouse input optional.
 *  Code adapted from Vodolazz: http://answers.unity3d.com/questions/1197380/make-standalone-input-module-ignore-mouse-input.html
 *  and OpticalOverride: https://forum.unity.com/threads/fake-mouse-position-in-4-6-ui-answered.283748
 */

using UnityEngine.EventSystems;
using UnityEngine;

namespace AC
{

	/**
	 * <summary>This script is an alternative to the Standalone Input Module that makes mouse input optional.
 	 * Code adapted from Vodolazz: http://answers.unity3d.com/questions/1197380/make-standalone-input-module-ignore-mouse-input.html
 	 * and OpticalOverride: https://forum.unity.com/threads/fake-mouse-position-in-4-6-ui-answered.283748/</summary>
	 */
	public class OptionalMouseInputModule : StandaloneInputModule
	{

		private bool allowMouseInput = true;
		private readonly MouseState m_MouseState = new MouseState ();


		public bool AllowMouseInput
		{
			get
			{
				return allowMouseInput;
			}
			set
			{
				allowMouseInput = value;
			}
		}


		private void Update ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
			{
				AllowMouseInput = !CanDirectlyControlMenus ();
			}
			else
			{
				AllowMouseInput = true;
			}
		}


		private bool CanDirectlyControlMenus ()
		{
			if ((KickStarter.stateHandler.gameState == GameState.Paused && KickStarter.menuManager.keyboardControlWhenPaused) ||
				(KickStarter.stateHandler.gameState == GameState.DialogOptions && KickStarter.menuManager.keyboardControlWhenDialogOptions) ||
				(KickStarter.stateHandler.IsInGameplay () && KickStarter.playerInput.canKeyboardControlMenusDuringGameplay))
			{
				return true;
			}
			return false;
		}


		protected override MouseState GetMousePointerEventData (int id = 0)
		{
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.inputMethod != InputMethod.KeyboardOrController)
			{
				return base.GetMousePointerEventData (id);
			}

			PointerEventData leftData;
			var created = GetPointerData (kMouseLeftId, out leftData, true );
	 
			leftData.Reset ();
	 
			Vector2 pos = KickStarter.playerInput.GetMousePosition ();
			if (created)
			{
				leftData.position = pos;
			}

			leftData.delta = pos - leftData.position;
			leftData.position = pos;
			leftData.scrollDelta = Input.mouseScrollDelta;
			leftData.button = PointerEventData.InputButton.Left;
			eventSystem.RaycastAll (leftData, m_RaycastResultCache);
			var raycast = FindFirstRaycast (m_RaycastResultCache);
			leftData.pointerCurrentRaycast = raycast;
			m_RaycastResultCache.Clear ();
	 
			PointerEventData rightData;
			GetPointerData (kMouseRightId, out rightData, true);
			CopyFromTo (leftData, rightData);
			rightData.button = PointerEventData.InputButton.Right;
	 
			PointerEventData middleData;
			GetPointerData (kMouseMiddleId, out middleData, true);
			CopyFromTo (leftData, middleData);
			middleData.button = PointerEventData.InputButton.Middle;
	 
			PointerEventData.FramePressState leftClickState = PointerEventData.FramePressState.NotChanged;
			if (KickStarter.playerInput.InputGetButtonDown ("InteractionA"))
			{
				leftClickState = PointerEventData.FramePressState.Pressed;
			}
			else if (KickStarter.playerInput.InputGetButtonUp ("InteractionA"))
			{
				leftClickState = PointerEventData.FramePressState.Released;
			}

			PointerEventData.FramePressState rightClickState = PointerEventData.FramePressState.NotChanged;
			if (KickStarter.playerInput.InputGetButtonDown ("InteractionB"))
			{
				rightClickState = PointerEventData.FramePressState.Pressed;
			}
			else if (KickStarter.playerInput.InputGetButtonUp ("InteractionB"))
			{
				rightClickState = PointerEventData.FramePressState.Released;
			}
	 
			m_MouseState.SetButtonState (PointerEventData.InputButton.Left, leftClickState, leftData);
			m_MouseState.SetButtonState (PointerEventData.InputButton.Right, rightClickState, rightData);
			m_MouseState.SetButtonState (PointerEventData.InputButton.Middle, StateForMouseButton (2), middleData);

			return m_MouseState;
		}


		#if !UNITY_5_0

		public override void Process ()
		{
			bool usedEvent = SendUpdateEventToSelectedObject ();
	 
			if (eventSystem.sendNavigationEvents)
			{
				if (!usedEvent)
				{
					usedEvent |= SendMoveEventToSelectedObject ();
				}
	 
				if (!usedEvent)
				{
					SendSubmitEventToSelectedObject ();
				}
			}

			if (allowMouseInput)
			{
				ProcessMouseEvent ();
			}
		}

		#endif

	}

}