using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(DragTrack))]
	public class DragTrackEditor : Editor
	{

		protected void SharedGUI (bool useColliders)
		{
			DragTrack _target = (DragTrack) target;

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("End-colliders", EditorStyles.boldLabel);
			
			_target.discSize = CustomGUILayout.Slider ("Gizmo size:", _target.discSize, 0f, 2f, "", "The size of the track's end colliders, as seen in the Scene window");
			if (useColliders)
			{
				_target.colliderMaterial = (PhysicMaterial) CustomGUILayout.ObjectField <PhysicMaterial> ("Material:", _target.colliderMaterial, false, "", "Physics Material to give the track's end colliders");
			}
			
			EditorGUILayout.EndVertical ();
			
			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}