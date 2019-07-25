using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(Moveable_Drag))]
	public class Moveable_DragEditor : DragBaseEditor
	{

		public override void OnInspectorGUI ()
		{
			Moveable_Drag _target = (Moveable_Drag) target;
			GetReferences ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Movment settings:", EditorStyles.boldLabel);
			_target.maxSpeed = CustomGUILayout.FloatField ("Max speed:", _target.maxSpeed, "", "The maximum force magnitude that can be applied to itself");
			_target.playerMovementReductionFactor = CustomGUILayout.Slider ("Player motion reduction:", _target.playerMovementReductionFactor, 0f, 1f, "", "How much player movement is reduced by when the object is being dragged");
			_target.playerMovementInfluence = CustomGUILayout.FloatField ("Player motion influence:", _target.playerMovementInfluence, "", "The influence that player movement has on the drag force");
			_target.invertInput = CustomGUILayout.Toggle ("Invert input?", _target.invertInput, "", "If True, input vectors will be inverted");
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");

			EditorGUILayout.LabelField ("Drag settings:", EditorStyles.boldLabel);
			_target.dragMode = (DragMode) CustomGUILayout.EnumPopup ("Drag mode:", _target.dragMode, "", "The way in which the object can be dragged");
			if (_target.dragMode == DragMode.LockToTrack)
			{
				_target.track = (DragTrack) CustomGUILayout.ObjectField <DragTrack> ("Track to stick to:", _target.track, true, "", "The DragTrack the object is locked to");

				if (_target.track != null && _target.track is DragTrack_Straight)
				{
					EditorGUILayout.HelpBox ("For best results, ensure the first collider on this GameObject is a Sphere Collider covering the breath of the mesh.\r\nIt can be disabled if necessary, but will be used to set correct limit boundaries.", MessageType.Info);
				}

				_target.setOnStart = CustomGUILayout.Toggle ("Set starting position?", _target.setOnStart, "", "If True, then the object will be placed at a specific point along the track when the game begins");
				if (_target.setOnStart)
				{
					_target.trackValueOnStart = CustomGUILayout.Slider ("Initial distance along:", _target.trackValueOnStart, 0f, 1f, "", "How far along its DragTrack that the object should be placed at when the game begins");
				}
				_target.retainOriginalTransform = CustomGUILayout.ToggleLeft ("Maintain original child transforms?", _target.retainOriginalTransform, "", "If True, then the position and rotation of all child objects will be maintained when the object is attached to the track");

				EditorGUILayout.BeginHorizontal ();
				_target.interactionOnMove = (Interaction) CustomGUILayout.ObjectField <Interaction> ("Interaction on move:", _target.interactionOnMove, true, "", "The Interaction to run whenever the object is moved by the player");
				
				if (_target.interactionOnMove == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						Undo.RecordObject (_target, "Create Interaction");
						Interaction newInteraction = SceneManager.AddPrefab ("Logic", "Interaction", true, false, true).GetComponent <Interaction>();
						newInteraction.gameObject.name = AdvGame.UniqueName ("Move : " + _target.gameObject.name);
						_target.interactionOnMove = newInteraction;
					}
				}
				EditorGUILayout.EndVertical ();
			}
			else if (_target.dragMode == DragMode.MoveAlongPlane)
			{
				_target.alignMovement = (AlignDragMovement) CustomGUILayout.EnumPopup ("Align movement:", _target.alignMovement, "", "What movement is aligned to");
				if (_target.alignMovement == AlignDragMovement.AlignToPlane)
				{
					_target.plane = (Transform) CustomGUILayout.ObjectField <Transform> ("Movement plane:", _target.plane, true, "", "The plane to align movement to");
				}
			}
			else if (_target.dragMode == DragMode.RotateOnly)
			{
				_target.rotationFactor = CustomGUILayout.FloatField ("Rotation factor:", _target.rotationFactor, "", "The speed by which the object can be rotated");
				_target.allowZooming = CustomGUILayout.Toggle ("Allow zooming?", _target.allowZooming, "", "If True, the object can be moved towards and away from the camera");
				if (_target.allowZooming)
				{
					_target.zoomSpeed = CustomGUILayout.FloatField ("Zoom speed:", _target.zoomSpeed, "", "The speed at which the object can be moved towards and away from the camera");
					_target.minZoom = CustomGUILayout.FloatField ("Closest distance:", _target.minZoom, "", "The minimum distance that there can be between the object and the camera");
					_target.maxZoom = CustomGUILayout.FloatField ("Farthest distance:", _target.maxZoom, "", "The maximum distance that there can be between the object and the camera");
				}
			}

			if (_target.dragMode != DragMode.LockToTrack)
			{
				_target.noGravityWhenHeld = CustomGUILayout.Toggle ("Disable gravity when held?", _target.noGravityWhenHeld, "", "If True, then gravity will be disabled on the object while it is held by the player");
			}

			if (Application.isPlaying && _target.dragMode == DragMode.LockToTrack && _target.track)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Distance along: " + _target.GetPositionAlong ().ToString (), EditorStyles.miniLabel);
			}

			EditorGUILayout.EndVertical ();

			if (_target.dragMode == DragMode.LockToTrack && _target.track is DragTrack_Hinge)
			{
				SharedGUI (_target, true);
			}
			else
			{
				SharedGUI (_target, false);
			}

			DisplayInputList (_target);

			UnityVersionHandler.CustomSetDirty (_target);
		}


		private void DisplayInputList (Moveable_Drag _target)
		{
			string result = "";

			if (_target.dragMode == DragMode.RotateOnly)
			{
				if (_target.allowZooming)
				{
					result += "\n";
					result += "- ZoomMoveable";
				}
			}

			if (result != "")
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
				EditorGUILayout.HelpBox ("The following input axes are available for the chosen settings:" + result, MessageType.Info);
			}
		}

	}

}