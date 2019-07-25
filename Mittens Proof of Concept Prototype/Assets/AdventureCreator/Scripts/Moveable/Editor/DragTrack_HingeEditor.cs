using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(DragTrack_Hinge))]
	public class DragTrack_HingeEditor : DragTrackEditor
	{
		
		public override void OnInspectorGUI ()
		{
			DragTrack_Hinge _target = (DragTrack_Hinge) target;
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Track shape:", EditorStyles.boldLabel);

			_target.radius = CustomGUILayout.FloatField ("Radius:", _target.radius, "", "The track's radius (for visualising in the Scene window)");
			_target.handleColour = CustomGUILayout.ColorField ("Handles colour:", _target.handleColour, "", "The colour of Scene window Handles");
			
			_target.doLoop = CustomGUILayout.Toggle ("Is looped?", _target.doLoop, "", "If True, then objects can be rotated a full revolution");
			if (!_target.doLoop)
			{
				_target.maxAngle = CustomGUILayout.FloatField ("Maximum angle:", _target.maxAngle, "", "How much an object can be rotated by");
				
				if (_target.maxAngle > 360f)
				{
					_target.maxAngle = 360f;
				}
			}
			else
			{
				_target.limitRevolutions = CustomGUILayout.Toggle ("Limit revolutions?", _target.limitRevolutions, "", "If True, then the number of revolutions an object can rotate is limited");
				if (_target.limitRevolutions)
				{
					_target.maxRevolutions = CustomGUILayout.IntField ("Max revolutions:", _target.maxRevolutions, "", "The maximum number of revolutions an object can be rotated by");
				}
			}

			_target.alignDragToFront = CustomGUILayout.Toggle ("Align drag vector to front?", _target.alignDragToFront, "", "If True, then the calculated drag vector will be based on the track's orientation, rather than the object being rotated, so that the input drag vector will always need to be the same direction");

			EditorGUILayout.EndVertical ();
			
			SharedGUI (false);
		}
		
		
		public void OnSceneGUI ()
		{
			DragTrack_Hinge _target = (DragTrack_Hinge) target;
			
			float _angle = _target.maxAngle;
			if (_target.doLoop)
			{
				_angle = 360f;
			}
			
			Handles.color = new Color (_target.handleColour.r / 2f, _target.handleColour.g / 2f, _target.handleColour.b / 2f, _target.handleColour.a);
			Vector3 startPosition = _target.transform.position + (_target.radius * _target.transform.right);
			Handles.DrawSolidDisc (startPosition, _target.transform.up, _target.discSize);
			
			Transform t = _target.transform;
			Vector3 originalPosition = _target.transform.position;
			Quaternion originalRotation = _target.transform.rotation;
			t.position = startPosition;
			t.RotateAround (originalPosition, _target.transform.forward, _angle);
			
			Handles.color = _target.handleColour;
			Handles.DrawSolidDisc (t.position, t.up, _target.discSize);
			
			_target.transform.position = originalPosition;
			_target.transform.rotation = originalRotation;
			
			Handles.color = _target.handleColour;
			Handles.DrawWireArc (_target.transform.position, _target.transform.forward, _target.transform.right, _angle, _target.radius);
		}
		
	}

}