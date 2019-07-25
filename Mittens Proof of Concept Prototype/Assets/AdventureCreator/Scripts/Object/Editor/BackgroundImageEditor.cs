#if !UNITY_2017_3_OR_NEWER
#define ALLOW_LEGACY_UI
#endif

#if UNITY_5_6_OR_NEWER && !UNITY_SWITCH
#define ALLOW_VIDEO
#endif

using UnityEngine;
using UnityEditor;
#if ALLOW_VIDEO
using UnityEngine.Video;
#endif

namespace AC
{

	#if UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE)
	[CustomEditor (typeof (BackgroundImage))]
	public class BackgroundImageEditor : Editor
	{
		
		private BackgroundImage _target;


		private void OnEnable ()
		{
			_target = (BackgroundImage) target;
		}
		
		
		public override void OnInspectorGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");

			#if ALLOW_LEGACY_UI

			_target.backgroundMethod25D = (BackgroundImage.BackgroundMethod25D) CustomGUILayout.EnumPopup ("Method:", _target.backgroundMethod25D, string.Empty, "How 2.5D backgrounds are renderered");
			switch (_target.backgroundMethod25D)
			{
				case BackgroundImage.BackgroundMethod25D.GUITexture:
					if (Object.FindObjectOfType <BackgroundImageUI>() != null)
					{
						BackgroundImageUI.Instance.ClearTexture (null);
					}
					if (_target.GUITexture == null)
					{
						EditorGUILayout.HelpBox ("A GUITexture component must be attached to this object for the chosen method to work.", MessageType.Warning);
					}
					break;

				case BackgroundImage.BackgroundMethod25D.UnityUI:
					_target = ShowUnityUIMethod (_target);
					break;
			}

			#else

			_target = ShowUnityUIMethod (_target);

			#endif

			#if ALLOW_VIDEO
			if (_target.backgroundImageSource == BackgroundImage.BackgroundImageSource.VideoClip)
			{
				EditorGUILayout.EndVertical ();
				UnityVersionHandler.CustomSetDirty (_target);
				return;
			}
			#endif

			#if UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2018_2_OR_NEWER

			EditorGUILayout.LabelField ("When playing a MovieTexture:");
			_target.loopMovie = CustomGUILayout.Toggle ("Loop clip?", _target.loopMovie, string.Empty, "If True, then any MovieTexture set as the background will be looped");
			_target.restartMovieWhenTurnOn = CustomGUILayout.Toggle ("Restart clip each time?", _target.restartMovieWhenTurnOn, string.Empty, "If True, then any MovieTexture set as the background will start from the beginning when the associated Camera is activated");

			#endif

			EditorGUILayout.EndVertical ();
			UnityVersionHandler.CustomSetDirty (_target);
		}


		private BackgroundImage ShowUnityUIMethod (BackgroundImage _target)
		{
			#if ALLOW_VIDEO

			_target.backgroundImageSource = (BackgroundImage.BackgroundImageSource) CustomGUILayout.EnumPopup ("Background type:", _target.backgroundImageSource, string.Empty, "What type of asset is used as a background");
			switch (_target.backgroundImageSource)
			{
				case BackgroundImage.BackgroundImageSource.Texture:
					_target.backgroundTexture = (Texture) CustomGUILayout.ObjectField <Texture> ("Texture:", _target.backgroundTexture, false, string.Empty, "The texture to display full-screen");
					break;

				case BackgroundImage.BackgroundImageSource.VideoClip:
				_target.backgroundTexture = (Texture) CustomGUILayout.ObjectField <Texture> ("Placeholder texture:", _target.backgroundTexture, false, string.Empty, "The texture to display full-screen while the VideoClip is being prepared");
					_target.backgroundVideo = (VideoClip) CustomGUILayout.ObjectField <VideoClip> ("Video clip:", _target.backgroundVideo, false, string.Empty, "The VideoClip to animate full-screen");
					break;
			}

			#else

			_target.backgroundTexture = (Texture) CustomGUILayout.ObjectField <Texture> ("Background texture:", _target.backgroundTexture, false, string.Empty, "The texture to display full-screen");

			#endif

			return _target;
		}

	}
	#endif

}