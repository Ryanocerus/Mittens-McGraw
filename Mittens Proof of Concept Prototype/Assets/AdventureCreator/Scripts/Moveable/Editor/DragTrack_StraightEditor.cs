using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(DragTrack_Straight))]
	public class DragTrack_StraightEditor : DragTrackEditor
	{
		
		public override void OnInspectorGUI ()
		{
			DragTrack_Straight _target = (DragTrack_Straight) target;
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Track shape:", EditorStyles.boldLabel);
			
			_target.maxDistance = CustomGUILayout.FloatField ("Length:", _target.maxDistance, "", "The track's length");
			_target.handleColour = CustomGUILayout.ColorField ("Handles colour:", _target.handleColour, "", "The colour of Scene window Handles");
			_target.rotationType = (DragRotationType) CustomGUILayout.EnumPopup ("Rotation type:", _target.rotationType, "", "The way in which the Moveable_Drag object rotates as it moves");

			if (_target.rotationType == DragRotationType.Screw)
			{
				_target.screwThread = CustomGUILayout.FloatField ("Screw thread:", _target.screwThread, "", "The 'thread' if the Moveable_Drag object rotates like a screw - effectively how fast the object rotates as it moves");
				_target.dragMustScrew = CustomGUILayout.Toggle ("Drag must rotate too?", _target.dragMustScrew, "", "If True, then the input drag vector must also rotate, so that it is always tangential to the dragged object");
			}

			EditorGUILayout.EndVertical ();

			SharedGUI (true);
		}
		
		
		public void OnSceneGUI ()
		{
			DragTrack_Straight _target = (DragTrack_Straight) target;
			
			Handles.color = _target.handleColour;
			Vector3 maxPosition = _target.transform.position + (_target.transform.up * _target.maxDistance);
			maxPosition = Handles.PositionHandle (maxPosition, Quaternion.identity);
			Handles.DrawSolidDisc (maxPosition, -_target.transform.up, _target.discSize);
			_target.maxDistance = Vector3.Dot (maxPosition - _target.transform.position, _target.transform.up);
			
			Handles.color = new Color (_target.handleColour.r / 2f, _target.handleColour.g / 2f, _target.handleColour.b / 2f, _target.handleColour.a);
			Vector3 minPosition = _target.transform.position;
			Handles.DrawSolidDisc (minPosition, _target.transform.up, _target.discSize);
			
			Handles.color = _target.handleColour;
			Handles.DrawLine (minPosition, maxPosition);

			UnityVersionHandler.CustomSetDirty (_target);
		}
		
	}

}