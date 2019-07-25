/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"LimitVisibility.cs"
 * 
 *	Attach this script to a GameObject to limit its visibility
 *	to a specific GameCamera in your scene.
 * 
 */

#if !UNITY_2017_2_OR_NEWER
#define ALLOW_LEGACY_UI
#endif

#if UNITY_5_6_OR_NEWER && !UNITY_SWITCH
#define ALLOW_VIDEO
#endif

using UnityEngine;
using System.Collections.Generic;

#if ALLOW_VIDEO
using UnityEngine.Video;
#endif

namespace AC
{

	/**
	 * This component limits the visibility of a GameObject so that it can only be viewed through a specific _Camera.
	 */
	[AddComponentMenu("Adventure Creator/Camera/Limit visibility to camera")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_limit_visibility.html")]
	#endif
	public class LimitVisibility : MonoBehaviour
	{

		/** The _Camera to limit the GameObject's visibility to (deprecated) */
		[HideInInspector] public _Camera limitToCamera;
		/** The _Cameras to limit the GameObject's visibility to */
		public List<_Camera> limitToCameras = new List<_Camera>();
		/** If True, then child GameObjects will be affected in the same way */
		public bool affectChildren = false;
		/** If True, then the object will not be visible even if the correct _Camera is active */
		[HideInInspector] public bool isLockedOff = false;

		private bool isVisible = false;
		private _Camera activeCamera = null;
		private _Camera transitionCamera = null;
		

		private void OnEnable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void OnDisable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Unregister (this);
		}


		private void Start ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);

			Upgrade ();

			if (limitToCameras.Count == 0 || KickStarter.mainCamera == null)
			{
				return;
			}

			activeCamera = KickStarter.mainCamera.attachedCamera;

			if (activeCamera != null && !isLockedOff)
			{
				if (limitToCameras.Contains (activeCamera))
				{
					SetVisibility (true);
				}
				else
				{
					SetVisibility (false);
				}
			}
			else
			{
				SetVisibility (false);
			}
		}


		/**
		 * <summary>Upgrades the component to make use of the limitToCameras List, rather than the singular limitToCamera variable.</summary>
		 */
		public void Upgrade ()
		{
			if (limitToCameras == null)
			{
				limitToCameras = new List<_Camera>();
			}

			if (limitToCamera != null)
			{
				if (!limitToCameras.Contains (limitToCamera))
				{
					limitToCameras.Add (limitToCamera);
				}
				limitToCamera = null;

				#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					ACDebug.Log ("LimitVisibility component on '" + gameObject.name + "' has been temporarily upgraded - please view its Inspector when the game ends and save the scene.", gameObject);
				}
				else
				{
					UnityVersionHandler.CustomSetDirty (this, true);
					ACDebug.Log ("Upgraded LimitVisibility on '" + gameObject.name + "', please save the scene.", gameObject);
				}
				#endif
			}
		}


		/**
		 * Updates the visibility based on the attached camera. This is public so that it can be called by StateHandler.
		 */
		public void _Update ()
		{
			if (limitToCameras.Count == 0 || KickStarter.mainCamera == null)
			{
				return;
			}

			activeCamera = KickStarter.mainCamera.attachedCamera;
			transitionCamera = KickStarter.mainCamera.GetTransitionFromCamera ();

			if (!isLockedOff)
			{
				if (activeCamera != null && limitToCameras.Contains (activeCamera))
				{
					SetVisibility (true);
				}
				else if (transitionCamera != null && limitToCameras.Contains (transitionCamera))
				{
					SetVisibility (true);
				}
				else
				{
					SetVisibility (false);
				}
			}
			else if (isVisible)
			{
				SetVisibility (false);
			}

			/*
			if (activeCamera != null && !isLockedOff)
			{
				if (limitToCameras.Contains (activeCamera) && !isVisible)
				{
					SetVisibility (true);
				}
				else if (!limitToCameras.Contains (activeCamera) && isVisible)
				{
					SetVisibility (false);
				}
			}
			else if (isVisible)
			{
				SetVisibility (false);
			}*/
		}



		private void SetVisibility (bool state)
		{
			Renderer _renderer = GetComponent <Renderer>();
			if (_renderer != null)
			{
				_renderer.enabled = state;
			}
			else
			{
				SpriteRenderer spriteRenderer = gameObject.GetComponent <SpriteRenderer>();
				if (spriteRenderer != null)
				{
					spriteRenderer.enabled = state;
				}
			}
			#if ALLOW_LEGACY_UI
			GUITexture guiTexture = gameObject.GetComponent <GUITexture>();
			if (guiTexture != null)
			{
				guiTexture.enabled = state;
			}
			#endif

			if (affectChildren)
			{
				Renderer[] _children = GetComponentsInChildren <Renderer>();
				foreach (Renderer child in _children)
				{
					child.enabled = state;
				}

				SpriteRenderer[] spriteChildren = GetComponentsInChildren <SpriteRenderer>();
				foreach (SpriteRenderer child in spriteChildren)
				{
					child.enabled = state;
				}

				#if ALLOW_LEGACY_UI
				GUITexture[] textureChildren = GetComponentsInChildren <GUITexture>();
				foreach (GUITexture child in textureChildren)
				{
					child.enabled = state;
				}
				#endif
			}

			#if ALLOW_VIDEO
			VideoPlayer videoPlayer = GetComponent <VideoPlayer>();
			if (videoPlayer != null)
			{
				videoPlayer.targetCameraAlpha = (state) ? 1f : 0f;
			}
			#endif

			isVisible = state;
		}

	}

}