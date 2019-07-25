/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"AC_Trigger.cs"
 * 
 *	This ActionList runs when the Player enters it.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * An ActionList that is run when the Player, or another object, comes into contact with it.
	 */
	[AddComponentMenu("Adventure Creator/Logic/Trigger")]
	[System.Serializable]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_a_c___trigger.html")]
	#endif
	public class AC_Trigger : ActionList
	{

		/** If detectionMethod = TriggerDetectionMethod.RigidbodyCollision, what the Trigger will react to (Player, SetObject, AnyObject, AnyObjectWithComponent) */
		public TriggerDetects detects = TriggerDetects.Player;
		/** The GameObject that the Trigger reacts to, if detectionMethod = TriggerDetectionMethod.RigidbodyCollision and detects = TriggerDetects.SetObject */
		public GameObject obToDetect;
		/** The component that must be attached to an object for the Trigger to react to, if detectionMethod = TriggerDetectionMethod.RigidbodyCollision and detects = TriggerDetects.AnyObjectWithComponent */
		public string detectComponent = "";

		/** What kind of contact the Trigger reacts to (0 = "On enter", 1 = "Continuous", 2 = "On exit") */
		public int triggerType;
		/** If True, then a Gizmo will be drawn in the Scene window at the Trigger's position */
		public bool showInEditor = false;
		/** If True, and the Player sets off the Trigger while walking towards a Hotspot Interaction, then the Player will stop, and the Interaction will be cancelled */
		public bool cancelInteractions = false;
		/** The state of the game under which the trigger reacts (OnlyDuringGameplay, OnlyDuringCutscenes, DuringCutscenesAndGameplay) */
		public TriggerReacts triggerReacts = TriggerReacts.OnlyDuringGameplay;
		/** The way in which objects are detected (RigidbodyCollision, TransformPosition) */
		public TriggerDetectionMethod detectionMethod = TriggerDetectionMethod.RigidbodyCollision;

		/** If True, and detectionMethod = TriggerDetectionMethod.TransformPosition, then the Trigger will react to the active Player */
		public bool detectsPlayer = true;
		/** The GameObjects that the Trigger reacts to, if detectionMethod = TriggerDetectionMethod.TransformPosition */
		public List<GameObject> obsToDetect = new List<GameObject>();

		private Collider2D _collider2D;
		private Collider _collider;
		private bool[] lastFrameWithins;


		private void OnEnable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);

			_collider2D = GetComponent <Collider2D>();
			_collider = GetComponent <Collider>();
			lastFrameWithins = (detectsPlayer) ? new bool[obsToDetect.Count + 1] : new bool[obsToDetect.Count];

			if (_collider == null && _collider2D == null)
			{
				ACDebug.LogWarning ("Trigger '" + gameObject.name + " cannot detect collisions because it has no Collider!", this);
			}
		}


		private void Start ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void OnDisable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Unregister (this);
		}


		public void _Update ()
		{
			if (detectionMethod == TriggerDetectionMethod.TransformPosition)
			{
				for (int i=0; i<obsToDetect.Count; i++)
				{
					ProcessObject (obsToDetect[i], i);
				}

				if (detectsPlayer && KickStarter.player != null)
				{
					ProcessObject (KickStarter.player.gameObject, lastFrameWithins.Length - 1);
				}
			}
		}


		private void Interact (GameObject collisionOb)
		{
			if (cancelInteractions)
			{
				KickStarter.playerInteraction.StopMovingToHotspot ();
			}
			
			if (actionListType == ActionListType.PauseGameplay)
			{
				KickStarter.playerInteraction.DeselectHotspot (false);
			}
			
			// Set correct parameter
			if (useParameters && parameters != null && parameters.Count >= 1)
			{
				if (parameters[0].parameterType == ParameterType.GameObject)
				{
					parameters[0].gameObject = collisionOb;
				}
				else
				{
					ACDebug.Log ("Cannot set the value of parameter 0 ('" + parameters[0].label + "') as it is not of the type 'Game Object'.", this);
				}
			}

			KickStarter.eventManager.Call_OnRunTrigger (this, collisionOb);

			Interact ();
		}
		
		
		private void OnTriggerEnter (Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay (Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay2D (Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit (Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit2D (Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision && triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}


		/**
		 * <summary>Checks if the Trigger is enabled.</summary>
		 * <returns>True if the Trigger is enabled.</summary>
		 */
		public bool IsOn ()
		{
			if (GetComponent <Collider>())
			{
				return GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				return GetComponent <Collider2D>().enabled;
			}
			return false;
		}
		

		/**
		 * <summary>Enables the Trigger.</summary>
		 */
		public void TurnOn ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = true;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = true;
			}
			else
			{
				ACDebug.LogWarning ("Cannot turn " + this.name + " on because it has no Collider component.", this);
			}
		}
		

		/**
		 * <summary>Disables the Trigger.</summary>
		 */
		public void TurnOff ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = false;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = false;
			}
			else
			{
				ACDebug.LogWarning ("Cannot turn " + this.name + " off because it has no Collider component.", this);
			}

			if (lastFrameWithins != null)
			{
				for (int i=0; i<lastFrameWithins.Length; i++)
				{
					lastFrameWithins[i] = false;
				}
			}
		}
		
		
		private bool IsObjectCorrect (GameObject obToCheck)
		{
			if (KickStarter.stateHandler == null || KickStarter.stateHandler.gameState == GameState.Paused || obToCheck == null)
			{
				return false;
			}

			if (KickStarter.saveSystem.loadingGame != LoadingGame.No)
			{
				return false;
			}

			if (triggerReacts == TriggerReacts.OnlyDuringGameplay && KickStarter.stateHandler.gameState != GameState.Normal)
			{
				return false;
			}
			else if (triggerReacts == TriggerReacts.OnlyDuringCutscenes && KickStarter.stateHandler.IsInGameplay ())
			{
				return false;
			}

			if (KickStarter.stateHandler != null && KickStarter.stateHandler.AreTriggersDisabled ())
			{
				return false;
			}

			if (detectionMethod == TriggerDetectionMethod.TransformPosition)
			{
				return true;
			}

			if (detects == TriggerDetects.Player)
			{
				if (obToCheck.CompareTag (Tags.player))
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.SetObject)
			{
				if (obToDetect != null && obToCheck == obToDetect)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithComponent)
			{
				if (!string.IsNullOrEmpty (detectComponent))
				{
					string[] allComponents = detectComponent.Split (";"[0]);
					foreach (string component in allComponents)
					{
						if (!string.IsNullOrEmpty (component) && obToCheck.GetComponent (component))
						{
							return true;
						}
					}
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithTag)
			{
				if (!string.IsNullOrEmpty (detectComponent))
				{
					string[] allComponents = detectComponent.Split (";"[0]);
					foreach (string component in allComponents)
					{
						if (!string.IsNullOrEmpty (component) && obToCheck.tag == component)
						{
							return true;
						}
					}
				}
			}
			else if (detects == TriggerDetects.AnyObject)
			{
				return true;
			}
			
			return false;
		}


		#if UNITY_EDITOR
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			Color gizmoColor = new Color (1f, 0.3f, 0f, 0.8f);

			if (GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), gizmoColor);
			}
			else if (GetComponent <MeshCollider>())
			{
				AdvGame.DrawMeshCollider (transform, GetComponent <MeshCollider>().sharedMesh, gizmoColor);
			}
			else if (GetComponent <SphereCollider>())
			{
				AdvGame.DrawSphereCollider (transform, GetComponent <SphereCollider>(), gizmoColor);
			}
			else if (GetComponent <BoxCollider2D>() != null || GetComponent <BoxCollider>() != null)
			{
				AdvGame.DrawCubeCollider (transform, gizmoColor);
			}
		}

		#endif


		private void ProcessObject (GameObject objectToCheck, int i)
		{
			if (objectToCheck != null)
			{
				bool isInside = CheckForPoint (objectToCheck.transform.position);
				if (DetermineValidity (isInside, i))
				{
					if (IsObjectCorrect (objectToCheck))
					{
						Interact (objectToCheck);
					}
				}
			}
		}


		private bool DetermineValidity (bool thisFrameWithin, int i)
		{
			bool isValid = false;

			switch (triggerType)
			{
				case 0:
					// OnEnter
					if (thisFrameWithin && !lastFrameWithins[i])
					{
						isValid = true;
					}
					break;

				case 1:
					// Continuous
					isValid = thisFrameWithin;
					break;

				case 2:
					// OnExit
					if (!thisFrameWithin && lastFrameWithins[i])
					{
						isValid = true;
					}
					break;

				default:
					break;
			}

			lastFrameWithins[i] = thisFrameWithin;
			return isValid;
		}


		private bool CheckForPoint (Vector3 position)
		{
			if (_collider2D != null)
			{
				if (_collider2D.enabled)
				{
					return _collider2D.OverlapPoint (position);
				}
				return false;
			}

			if (_collider != null && _collider.enabled)
			{
				return _collider.bounds.Contains (position);
			}

			return false;
		}

	}
	
}