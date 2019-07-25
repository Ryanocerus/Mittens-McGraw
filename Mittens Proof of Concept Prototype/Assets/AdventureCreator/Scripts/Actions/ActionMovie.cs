/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionMovie.cs"
 * 
 *	Plays movie clips either on a Texture, or full-screen on mobile devices.
 * 
 */

#if UNITY_5_6_OR_NEWER && !UNITY_SWITCH
#define ALLOW_VIDEO
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if ALLOW_VIDEO
using UnityEngine.Video;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionMovie : Action
	{

		#if ALLOW_VIDEO
		public MovieClipType movieClipType = MovieClipType.VideoPlayer;
		#else
		public MovieClipType movieClipType = MovieClipType.FullScreen;
		#endif
		public MovieMaterialMethod movieMaterialMethod = MovieMaterialMethod.PlayMovie;

		#if ALLOW_VIDEO
		public VideoPlayer videoPlayer;
		protected VideoPlayer runtimeVideoPlayer;
		public int videoPlayerParameterID = -1;
		public int videoPlayerConstantID;
		public bool prepareOnly = false;
		public bool pauseWithGame = false;

			#if UNITY_WEBGL
			public string movieURL = "http://";
			public int movieURLParameterID = -1;
			#else
			public VideoClip newClip;
			#endif
			private bool isPaused;

		#endif

		#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)

		public string filePath;

		#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER
		public Material material;
		public int materialParameterID = -1;

		public MovieTexture movieClip;
		public int movieClipParameterID = -1;

		public Sound sound;
		public int soundID = 0;

		public bool includeAudio;
		private GUITexture guiTexture;
		#endif

		public string skipKey;
		public bool canSkip;


		
		public ActionMovie ()
		{
			this.isDisplayed = true;
			title = "Play movie clip";
			category = ActionCategory.Engine;
			description = "Plays movie clips either on a Texture, or full-screen on mobile devices.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			#if ALLOW_VIDEO
			runtimeVideoPlayer = AssignFile <VideoPlayer> (parameters, videoPlayerParameterID, videoPlayerConstantID, videoPlayer);
			isPaused = false;

				#if UNITY_WEBGL
				movieURL = AssignString (parameters, movieURLParameterID, movieURL);
				#endif

			#endif

			#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)
			#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER
			material = (Material) AssignObject <Material> (parameters, materialParameterID, material);
			movieClip = (MovieTexture) AssignObject <MovieTexture> (parameters, movieClipParameterID, movieClip);
			sound = AssignFile (soundID, sound);
			#endif
		}
		

		override public float Run ()
		{
			if (movieClipType == MovieClipType.VideoPlayer)
			{
				#if ALLOW_VIDEO
				if (runtimeVideoPlayer != null)
				{
					if (!isRunning)
					{
						isRunning = true;

						if (movieMaterialMethod == MovieMaterialMethod.PlayMovie)
						{
							#if UNITY_WEBGL
							if (!string.IsNullOrEmpty (movieURL))
							{
								runtimeVideoPlayer.url = movieURL;
							}
							#else
							if (newClip != null)
							{
								runtimeVideoPlayer.clip = newClip;
							}
							#endif

							if (prepareOnly)
							{
								runtimeVideoPlayer.Prepare ();

								if (willWait)
								{
									return defaultPauseTime;
								}
							}
							else
							{
								KickStarter.playerInput.skipMovieKey = "";
								runtimeVideoPlayer.Play ();

								if (runtimeVideoPlayer.isLooping)
								{
									ACDebug.LogWarning ("Cannot wait for " + runtimeVideoPlayer.name + " to finish because it is looping!", runtimeVideoPlayer);
									return 0f;
								}

								if (canSkip && skipKey != "")
								{
									KickStarter.playerInput.skipMovieKey = skipKey;
								}

								if (willWait)
								{
									return defaultPauseTime;
								}
							}
						}
						else if (movieMaterialMethod == MovieMaterialMethod.PauseMovie)
						{
							runtimeVideoPlayer.Pause ();
						}
						else if (movieMaterialMethod == MovieMaterialMethod.StopMovie)
						{
							runtimeVideoPlayer.Stop ();
						}

						return 0f;
					}
					else
					{
						if (prepareOnly)
						{
							if (!runtimeVideoPlayer.isPrepared)
							{
								return defaultPauseTime;
							}
						}
						else
						{
							if (pauseWithGame)
							{
								if (KickStarter.stateHandler.gameState == GameState.Paused)
								{
									if (runtimeVideoPlayer.isPlaying && !isPaused)
									{
										runtimeVideoPlayer.Pause ();
										isPaused = true;
									}
									return defaultPauseTime;
								}
								else
								{
									if (!runtimeVideoPlayer.isPlaying && isPaused)
									{
										isPaused = false;
										runtimeVideoPlayer.Play ();
									}
								}
							}

							if (canSkip && skipKey != "" && KickStarter.playerInput.skipMovieKey == "")
							{
								runtimeVideoPlayer.Stop ();
								isRunning = false;
								return 0f;
							}

							if (!runtimeVideoPlayer.isPrepared || runtimeVideoPlayer.isPlaying)
							{
								return defaultPauseTime;
							}
						}

						runtimeVideoPlayer.Stop ();
						isRunning = false;
						return 0f;
					}
				}
				else
				{
					ACDebug.LogWarning ("Cannot play video - no Video Player found!");
				}
				#else
				ACDebug.LogWarning ("Use of the VideoPlayer for movie playback is only available in Unity 5.6 or later.");
				#endif
				return 0f;
			}

			#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)

			if (!isRunning && filePath != "")
			{
				isRunning = true;

				if (canSkip)
				{
					Handheld.PlayFullScreenMovie (filePath, Color.black, FullScreenMovieControlMode.CancelOnInput);
				}
				else
				{
					Handheld.PlayFullScreenMovie (filePath, Color.black, FullScreenMovieControlMode.Hidden);
				}
				return defaultPauseTime;
			}
			else
			{
				isRunning = false;
				return 0f;
			}

			#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER

			if (movieClip == null)
			{
				ACDebug.LogWarning ("Cannot play movie - no movie clip set!");
				return 0f;
			}
			if (movieClipType == MovieClipType.OnMaterial && material == null)
			{
				ACDebug.LogWarning ("Cannot play movie - no material has been assigned. A movie clip can only be played as a material's texture, so a material must be assigned.");
				return 0f;
			}
			if (includeAudio && sound == null)
			{
				ACDebug.LogWarning ("Cannot play movie audio - no Sound object has been assigned.");
			}

			if (!isRunning)
			{
				isRunning = true;
				guiTexture = null;

				KickStarter.playerInput.skipMovieKey = "";

				if (movieClipType == MovieClipType.FullScreen)
				{
					CreateFullScreenMovie ();
				}
				else if (movieClipType == MovieClipType.OnMaterial)
				{
					if (movieMaterialMethod == MovieMaterialMethod.PlayMovie)
					{
						material.mainTexture = movieClip;
					}
					else if (movieMaterialMethod == MovieMaterialMethod.PauseMovie)
					{
						if (material.mainTexture == movieClip)
						{
							movieClip.Pause ();
							isRunning = false;
							return 0f;
						}
					}
					else if (movieMaterialMethod == MovieMaterialMethod.StopMovie)
					{
						if (material.mainTexture == movieClip)
						{
							movieClip.Stop ();
							isRunning = false;
							return 0f;
						}
					}
				}

				movieClip.Play ();

				if (includeAudio && sound != null)
				{
					if (movieClipType == MovieClipType.OnMaterial && movieMaterialMethod != MovieMaterialMethod.PlayMovie)
					{
						if (movieMaterialMethod == MovieMaterialMethod.PauseMovie)
						{
							sound.GetComponent <AudioSource>().Pause ();
						}
						else if (movieMaterialMethod == MovieMaterialMethod.StopMovie)
						{
							sound.Stop ();
						}
					}
					else
					{
						sound.GetComponent <AudioSource>().clip = movieClip.audioClip;
						sound.Play (false);
					}
				}

				if (movieClipType == MovieClipType.FullScreen || willWait)
				{
					if (canSkip && skipKey != "")
					{
						KickStarter.playerInput.skipMovieKey = skipKey;
					}
					return defaultPauseTime;
				}
				return 0f;
			}
			else
			{
				if (movieClip.isPlaying)
				{
					if (!canSkip || KickStarter.playerInput.skipMovieKey != "")
					{
						return defaultPauseTime;
					}
				}

				OnComplete ();
				isRunning = false;
				return 0f;
			}

			#else

			ACDebug.LogWarning ("On non-mobile platforms, this Action is only available in Unity 5 or Unity Pro.");
			return 0f;

			#endif
		}


		override public void Skip ()
		{
			OnComplete ();
		}


		private void OnComplete ()
		{
			if (movieClipType == MovieClipType.VideoPlayer)
			{
				#if ALLOW_VIDEO
				if (runtimeVideoPlayer != null)
				{
					if (prepareOnly)
					{
						runtimeVideoPlayer.Prepare ();
					}
					else
					{
						runtimeVideoPlayer.Stop ();
					}
				}
				#endif
			}
			else if (movieClipType == MovieClipType.FullScreen || (movieClipType == MovieClipType.OnMaterial && movieMaterialMethod == MovieMaterialMethod.PlayMovie))
			{
				if (isRunning)
				{
					#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)
					#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER
					if (includeAudio)
					{
						sound.Stop ();
					}
					movieClip.Stop ();
					KickStarter.playerInput.skipMovieKey = "";

					if (movieClipType == MovieClipType.FullScreen)
					{
						EndFullScreenMovie ();
					}
					#endif
				}
			}
			else if (movieClipType == MovieClipType.OnMaterial && movieMaterialMethod != MovieMaterialMethod.PlayMovie)
			{
				Run ();
			}
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			movieClipType = (MovieClipType) EditorGUILayout.EnumPopup ("Play clip:", movieClipType);

			if (movieClipType == MovieClipType.VideoPlayer)
			{
				#if ALLOW_VIDEO

				videoPlayerParameterID = Action.ChooseParameterGUI ("Video player:", parameters, videoPlayerParameterID, ParameterType.GameObject);
				if (videoPlayerParameterID >= 0)
				{
					videoPlayerConstantID = 0;
					videoPlayer = null;
				}
				else
				{
					videoPlayer = (VideoPlayer) EditorGUILayout.ObjectField ("Video player:", videoPlayer, typeof (VideoPlayer), true);

					videoPlayerConstantID = FieldToID <VideoPlayer> (videoPlayer, videoPlayerConstantID);
					videoPlayer = IDToField <VideoPlayer> (videoPlayer, videoPlayerConstantID, false);
				}

				movieMaterialMethod = (MovieMaterialMethod) EditorGUILayout.EnumPopup ("Method:", movieMaterialMethod);

				if (movieMaterialMethod == MovieMaterialMethod.PlayMovie)
				{
					#if UNITY_WEBGL
					movieURLParameterID = Action.ChooseParameterGUI ("Movie URL:", parameters, movieURLParameterID, ParameterType.String);
					if (movieURLParameterID < 0)
					{
						movieURL = EditorGUILayout.TextField ("Movie URL:", movieURL);
					}
					#else
					newClip = (VideoClip) EditorGUILayout.ObjectField ("New Clip (optional):", newClip, typeof (VideoClip), true);
					#endif
            
					prepareOnly = EditorGUILayout.Toggle ("Prepare only?", prepareOnly);
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

					if (willWait && !prepareOnly)
					{
						pauseWithGame = EditorGUILayout.Toggle ("Pause when game does?", pauseWithGame);
						canSkip = EditorGUILayout.Toggle ("Player can skip?", canSkip);
						if (canSkip)
						{
							skipKey = EditorGUILayout.TextField ("Skip with Input Button:", skipKey);
						}
					}
				}

				#elif UNITY_SWITCH

				EditorGUILayout.HelpBox ("This option not available on Switch.", MessageType.Info);

				#else

				EditorGUILayout.HelpBox ("This option is only available when using Unity 5.6 or later.", MessageType.Info);

				#endif

				AfterRunningOption ();
				return;
			}

			#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)

			if (movieClipType == MovieClipType.OnMaterial)
			{
				EditorGUILayout.HelpBox ("This option is not available on the current platform.", MessageType.Info);
			}
			else
			{
				filePath = EditorGUILayout.TextField ("Path to clip file:", filePath);
				canSkip = EditorGUILayout.Toggle ("Player can skip?", canSkip);

				EditorGUILayout.HelpBox ("The clip must be placed in a folder named 'StreamingAssets'.", MessageType.Info);
			}

			#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER

			movieClipParameterID = Action.ChooseParameterGUI ("Movie clip:", parameters, movieClipParameterID, ParameterType.UnityObject);
			if (movieClipParameterID < 0)
			{
				movieClip = (MovieTexture) EditorGUILayout.ObjectField ("Movie clip:", movieClip, typeof (MovieTexture), false);
			}

			if (movieClipType == MovieClipType.OnMaterial)
			{
				movieMaterialMethod = (MovieMaterialMethod) EditorGUILayout.EnumPopup ("Method:", movieMaterialMethod);

				string label = "Material to play on:";
				if (movieMaterialMethod == MovieMaterialMethod.PauseMovie)
				{
					label = "Material to pause:";
				}
				else if (movieMaterialMethod == MovieMaterialMethod.StopMovie)
				{
					label = "Material to stop:";
				}

				materialParameterID = Action.ChooseParameterGUI (label, parameters, materialParameterID, ParameterType.UnityObject);
				if (materialParameterID < 0)
				{
					material = (Material) EditorGUILayout.ObjectField (label, material, typeof (Material), true);
				}
			}

			if (movieClipType == MovieClipType.OnMaterial && movieMaterialMethod != MovieMaterialMethod.PlayMovie)
			{ }
			else
			{
				includeAudio = EditorGUILayout.Toggle ("Include audio?", includeAudio);
				if (includeAudio)
				{
					sound = (Sound) EditorGUILayout.ObjectField ("'Sound' to play audio:", sound, typeof (Sound), true);

					soundID = FieldToID (sound, soundID);
					sound = IDToField (sound, soundID, false);
				}

				if (movieClipType == MovieClipType.OnMaterial && movieMaterialMethod == MovieMaterialMethod.PlayMovie)
				{
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				}
				if (movieClipType == MovieClipType.FullScreen || willWait)
				{
					canSkip = EditorGUILayout.Toggle ("Player can skip?", canSkip);
					if (canSkip)
					{
						skipKey = EditorGUILayout.TextField ("Skip with Input Button:", skipKey);
					}
				}
			}

			#else

			EditorGUILayout.HelpBox ("On standalone, this Action is only available in Unity 5 or Unity Pro.", MessageType.Warning);

			#endif

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			#if ALLOW_VIDEO
			if (movieClipType == MovieClipType.VideoPlayer && videoPlayer != null)
			{
				if (saveScriptsToo)
				{
					AddSaveScript <RememberVideoPlayer> (videoPlayer);
				}

				AssignConstantID (videoPlayer, videoPlayerConstantID, videoPlayerParameterID);
			}
			#endif
		}
		
		
		public override string SetLabel ()
		{
			if (movieClipType == MovieClipType.VideoPlayer)
			{
				#if ALLOW_VIDEO
				string labelAdd = movieMaterialMethod.ToString ();
				if (videoPlayer != null) labelAdd += " " + videoPlayer.name.ToString ();
				return labelAdd;
				#else
				return string.Empty;
				#endif
			}

			#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)

			if (!string.IsNullOrEmpty (filePath))
			{
				return filePath;
			}

			#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER

			if (movieClip != null)
			{
				return movieClip.name;
			}

			#endif
			return string.Empty;
		}
		
		#endif


		#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_TVOS)
		#elif UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE) && !UNITY_2017_2_OR_NEWER

		private void CreateFullScreenMovie ()
		{
			GameObject movieOb = new GameObject ("Movie clip");
			movieOb.transform.position = Vector3.zero;
			movieOb.transform.position = new Vector2 (0.5f, 0.5f);

			guiTexture = movieOb.AddComponent<GUITexture>();
			guiTexture.enabled = false;
			guiTexture.texture = movieClip;
			guiTexture.enabled = true;

			KickStarter.sceneSettings.SetFullScreenMovie (movieClip);
		}


		private void EndFullScreenMovie ()
		{
			KickStarter.sceneSettings.StopFullScreenMovie ();
			if (guiTexture != null)
			{
				guiTexture.enabled = false;
				Destroy (guiTexture.gameObject);
			}
		}

		#endif

	}
	
}