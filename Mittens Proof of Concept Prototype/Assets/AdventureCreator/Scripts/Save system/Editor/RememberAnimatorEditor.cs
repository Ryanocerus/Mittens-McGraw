using UnityEditor;
using UnityEngine;

namespace AC
{

	[CustomEditor (typeof (RememberAnimator), true)]
	public class RememberAnimatorEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberAnimator _target = (RememberAnimator) target;
			_target.ShowGUI ();

			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			SharedGUI ();
			#else
			EditorGUILayout.HelpBox ("This component is only compatible with Unity 5 or later.", MessageType.Info);
			#endif
		}
		
	}
	
}