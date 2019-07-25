/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionDirector.cs"
 * 
 *	This action plays and stops controls Playable Directors
 * 
 */

#if UNITY_2017_1_OR_NEWER
#define CAN_USE_TIMELINE
#endif

using UnityEngine;
using System.Collections.Generic;

#if CAN_USE_TIMELINE
using UnityEngine.Timeline;
using UnityEngine.Playables;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionTimeline : Action
	{

		public bool disableCamera;

		#if CAN_USE_TIMELINE
		public PlayableDirector director;
		protected PlayableDirector runtimeDirector;
		public TimelineAsset newTimeline;
		public int directorConstantID = 0;
		public int directorParameterID = -1;

		public enum ActionDirectorMethod { Play, Stop };
		public ActionDirectorMethod method = ActionDirectorMethod.Play;
		public bool restart = true;
		public bool pause = false;
		public bool updateBindings = false;
		[SerializeField] private BindingData[] newBindings = new BindingData[0];
		#endif

		
		public ActionTimeline ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Control Timeline";
			description = "Controls a Timeline.  This is only compatible with Unity 2017 or newer.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			#if CAN_USE_TIMELINE
			runtimeDirector = AssignFile <PlayableDirector> (parameters, directorParameterID, directorConstantID, director);

			if (newBindings != null)
			{
				for (int i=0; i<newBindings.Length; i++)
				{
					if (newBindings[i].isPlayer)
					{
						if (KickStarter.player != null)
						{
							newBindings[i].gameObject = KickStarter.player.gameObject;
						}
						else
						{
							ACDebug.LogWarning ("Cannot bind timeline track to Player, because no Player was found!", runtimeDirector);
						}
					}
					else
					{
						newBindings[i].gameObject = AssignFile (parameters, newBindings[i].parameterID, newBindings[i].constantID, newBindings[i].gameObject);
					}
				}
			}
			#endif
		}
		
		
		override public float Run ()
		{
			#if CAN_USE_TIMELINE
			if (!isRunning)
			{
				isRunning = true;

				if (runtimeDirector != null)
				{
					if (method == ActionDirectorMethod.Play)
					{
						isRunning = true;

						if (restart)
						{
							PrepareDirector ();

							runtimeDirector.time = 0f;
							runtimeDirector.Play ();
						}
						else
						{
							runtimeDirector.Resume ();
						}

						if (willWait)
						{
							if (disableCamera)
							{
								KickStarter.mainCamera.Disable ();
							}
							return ((float) runtimeDirector.duration - (float) runtimeDirector.time);
						}
					}
					else if (method == ActionDirectorMethod.Stop)
					{
						if (disableCamera)
						{
							KickStarter.mainCamera.Enable ();
						}

						if (pause)
						{
							runtimeDirector.Pause ();
						}
						else
						{
							PrepareDirectorEnd ();

							runtimeDirector.time = runtimeDirector.duration;
							runtimeDirector.Stop ();
						}
					}
				}
			}
			else
			{
				if (method == ActionDirectorMethod.Play && disableCamera)
				{
					KickStarter.mainCamera.Enable ();
				}

				PrepareDirectorEnd ();
				isRunning = false;
			}
			#endif
			
			return 0f;
		}


		override public void Skip ()
		{
			#if CAN_USE_TIMELINE
			if (runtimeDirector != null)
			{
				if (disableCamera)
				{
					KickStarter.mainCamera.Enable ();
				}

				if (method == ActionDirectorMethod.Play)
				{
					if (runtimeDirector.extrapolationMode == DirectorWrapMode.Loop)
					{
						PrepareDirector ();

						if (restart)
						{
							runtimeDirector.Play ();
						}
						else
						{
							runtimeDirector.Resume ();
						}
						return;
					}

					PrepareDirectorEnd ();

					runtimeDirector.Stop ();
					runtimeDirector.time = runtimeDirector.duration;
				}
				else if (method == ActionDirectorMethod.Stop)
				{
					if (pause)
					{
						runtimeDirector.Pause ();
					}
					else
					{
						runtimeDirector.Stop ();
					}
				}
			}
			#endif
		}


		public override void Reset (ActionList actionList)
		{
			if (isRunning)
			{
				isRunning = false;
				Skip ();
			}
		}

		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			#if CAN_USE_TIMELINE
			method = (ActionDirectorMethod) EditorGUILayout.EnumPopup ("Method:", method);

			directorParameterID = Action.ChooseParameterGUI ("Director:", parameters, directorParameterID, ParameterType.GameObject);
			if (directorParameterID >= 0)
			{
				directorConstantID = 0;
				director = null;
			}
			else
			{
				director = (PlayableDirector) EditorGUILayout.ObjectField ("Director:", director, typeof (PlayableDirector), true);
				
				directorConstantID = FieldToID <PlayableDirector> (director, directorConstantID);
				director = IDToField <PlayableDirector> (director, directorConstantID, false);
			}

			if (director != null || directorParameterID >= 0)
			{
				if (method == ActionDirectorMethod.Play)
				{
					restart = EditorGUILayout.Toggle ("Play from beginning?", restart);
					if (restart)
					{
						newTimeline = (TimelineAsset) EditorGUILayout.ObjectField ("Timeline (optional):", newTimeline, typeof (TimelineAsset), false);
						updateBindings = EditorGUILayout.Toggle ("Remap bindings?", updateBindings);
						if (updateBindings)
						{
							if (newTimeline)
							{
								ShowBindingsUI (newTimeline, parameters);
							}
							else if (director != null && director.playableAsset != null)
							{
								ShowBindingsUI (director.playableAsset as TimelineAsset, parameters);
							}
							else
							{
								EditorGUILayout.HelpBox ("A Director or Timeline must be assigned in order to update bindings.", MessageType.Warning);
							}
						}
						else if (newTimeline != null)
						{
							EditorGUILayout.HelpBox ("The existing bindings will be transferred onto the new Timeline.", MessageType.Info);
						}
					}
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

					if (willWait)
					{
						disableCamera = EditorGUILayout.Toggle ("Disable AC camera?", disableCamera);
					}
				}
				else if (method == ActionDirectorMethod.Stop)
				{
					pause = EditorGUILayout.Toggle ("Pause timeline?", pause);
					disableCamera = EditorGUILayout.Toggle ("Enable AC camera?", disableCamera);
				}
			}

			#else
			EditorGUILayout.HelpBox ("This Action is only compatible with Unity 5.6 or newer.", MessageType.Info);
			#endif

			AfterRunningOption ();
		}


		#if CAN_USE_TIMELINE

		private int rebindTrackIndex;
		private void ShowBindingsUI (TimelineAsset timelineAsset, List<ActionParameter> parameters)
		{
			if (timelineAsset == null) return;

			if (newBindings == null || timelineAsset.outputTrackCount != newBindings.Length)
			{
				BindingData[] tempBindings = new BindingData[newBindings.Length];
				for (int i=0; i<newBindings.Length; i++)
				{
					tempBindings[i] = new BindingData (newBindings[i]);
				}

				newBindings = new BindingData[timelineAsset.outputTrackCount];
				for (int i=0; i<newBindings.Length; i++)
				{
					if (i < tempBindings.Length)
					{
						newBindings[i] = new BindingData (tempBindings[i]);
					}
					else
					{
						newBindings[i] = new BindingData ();
					}
				}
			}

			string[] popUpLabels = new string[newBindings.Length];
			for (int i=0; i<newBindings.Length; i++)
			{
				string trackName = (timelineAsset.GetOutputTrack (i) != null) ? timelineAsset.GetOutputTrack (i).name : " Track";
				if (string.IsNullOrEmpty (trackName))
				{
					trackName = " Unnamed";
				}
				popUpLabels[i] = "#" + i.ToString () + ": " + trackName;
			}

			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			rebindTrackIndex = EditorGUILayout.Popup ("Select a track:", rebindTrackIndex, popUpLabels);
			ShowBindingUI (rebindTrackIndex, parameters);

			if (newBindings.Length > 1)
			{
				EditorGUILayout.HelpBox ("Note: All bindings will be affected - not just the one selected above.", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();
		}


		private void ShowBindingUI (int i, List<ActionParameter> parameters)
		{
			if (newBindings == null || newBindings.Length < i) return;
			
			newBindings[i].isPlayer = EditorGUILayout.Toggle ("Bind to Player?", newBindings[i].isPlayer);
			if (!newBindings[i].isPlayer)
			{
				newBindings[i].parameterID = Action.ChooseParameterGUI ("Bind to:", parameters, newBindings[i].parameterID, ParameterType.GameObject);
				if (newBindings[i].parameterID >= 0)
				{
					newBindings[i].constantID = 0;
					newBindings[i].gameObject = null;
				}
				else
				{
					newBindings[i].gameObject = (GameObject) EditorGUILayout.ObjectField ("Bind to:", newBindings[i].gameObject, typeof (GameObject), true);

					newBindings[i].constantID = FieldToID (newBindings[i].gameObject, newBindings[i].constantID);
					newBindings[i].gameObject = IDToField (newBindings[i].gameObject, newBindings[i].constantID, false);
				}
			}
		}


		public TimelineAsset GetTimelineAsset ()
		{
			if (method == ActionDirectorMethod.Play && restart)
			{
				return newTimeline;
			}
			return null;
		}

		#endif


		override public void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			#if CAN_USE_TIMELINE
			if (saveScriptsToo)
			{
				AddSaveScript <RememberTimeline> (director);
			}
			AssignConstantID <PlayableDirector> (director, directorConstantID, directorParameterID);

			if (updateBindings && newBindings != null && newBindings.Length > 0)
			{
				for (int i=0; i<newBindings.Length; i++)
				{
					if (newBindings[i].gameObject != null)
					{
						if (saveScriptsToo)
						{
							AddSaveScript <ConstantID> (newBindings[i].gameObject);
						}
						AssignConstantID (newBindings[i].gameObject, newBindings[i].constantID, newBindings[i].parameterID);
					}
				}
			}
			#endif
		}

		
		public override string SetLabel ()
		{
			#if CAN_USE_TIMELINE
			if (director != null)
			{
				return method.ToString () + " " + director.gameObject.name;
			}
			#endif
			return string.Empty;
		}
		
		#endif


		#if CAN_USE_TIMELINE

		private void PrepareDirector ()
		{
			if (newTimeline != null)
			{
				if (runtimeDirector.playableAsset != null && runtimeDirector.playableAsset is TimelineAsset)
				{
					TimelineAsset oldTimeline = (TimelineAsset) runtimeDirector.playableAsset;
					GameObject[] transferBindings = new GameObject[oldTimeline.outputTrackCount];
					for (int i=0; i<transferBindings.Length; i++)
					{
						TrackAsset trackAsset = oldTimeline.GetOutputTrack (i);
						transferBindings[i] = runtimeDirector.GetGenericBinding (trackAsset) as GameObject;
					}

					runtimeDirector.playableAsset = newTimeline;

					for (int i=0; i<transferBindings.Length; i++)
					{
						if (transferBindings[i] != null)
						{
							var track = newTimeline.GetOutputTrack (i);
							if (track != null)
							{
								runtimeDirector.SetGenericBinding (track, transferBindings[i].gameObject);
							}
		                }
					}
				}
				else
				{
					runtimeDirector.playableAsset = newTimeline;
				}
			}

			TimelineAsset timelineAsset = runtimeDirector.playableAsset as TimelineAsset;
			if (timelineAsset != null)
			{
				for (int i=0; i<timelineAsset.outputTrackCount; i++)
				{
					TrackAsset trackAsset = timelineAsset.GetOutputTrack (i);

					if (updateBindings && newBindings != null && i < newBindings.Length && newBindings[i] != null)
					{
						if (trackAsset != null && newBindings[i].gameObject != null)
						{
							runtimeDirector.SetGenericBinding (trackAsset, newBindings[i].gameObject);
						}
	                }

					GameObject bindingObject = runtimeDirector.GetGenericBinding (trackAsset) as GameObject;
					if (bindingObject == null)
					{
						Animator bindingAnimator = runtimeDirector.GetGenericBinding (trackAsset) as Animator;
						if (bindingAnimator != null)
						{
							bindingObject = bindingAnimator.gameObject;
						}
					}

					if (bindingObject != null)
					{
						Char bindingObjectChar = bindingObject.GetComponent <Char>();
						if (bindingObjectChar != null)
						{
							bindingObjectChar.OnEnterTimeline (runtimeDirector, i);
						}
					}
	            }
			}
		}


		private void PrepareDirectorEnd ()
		{
			TimelineAsset timelineAsset = runtimeDirector.playableAsset as TimelineAsset;
			if (timelineAsset != null)
			{
				for (int i=0; i<timelineAsset.outputTrackCount; i++)
				{
					TrackAsset trackAsset = timelineAsset.GetOutputTrack (i);

					GameObject bindingObject = runtimeDirector.GetGenericBinding (trackAsset) as GameObject;
					if (bindingObject == null)
					{
						Animator bindingAnimator = runtimeDirector.GetGenericBinding (trackAsset) as Animator;
						if (bindingAnimator != null)
						{
							bindingObject = bindingAnimator.gameObject;
						}
					}

					if (bindingObject != null)
					{
						Char bindingObjectChar = bindingObject.GetComponent <Char>();
						if (bindingObjectChar != null)
						{
							bindingObjectChar.OnExitTimeline (runtimeDirector, i);
						}
					}
	            }
			}
		}


		[System.Serializable]
		private class BindingData
		{

			public GameObject gameObject;
			public bool isPlayer;
			public int constantID;
			public int parameterID = -1;


			public BindingData ()
			{
				gameObject = null;
				isPlayer = false;
				constantID = 0;
				parameterID = -1;
			}


			public BindingData (BindingData bindingData)
			{
				gameObject = bindingData.gameObject;
				isPlayer = bindingData.isPlayer;
				constantID = bindingData.constantID;
				parameterID = bindingData.parameterID;
			}

		}

		#endif

	}
	
}