/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionOptionSet.cs"
 * 
 *	This Action allows you to set an Options variable to a specific value
 * 
 */

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionOptionSet : Action
	{

		[SerializeField] private int indexParameterID = -1;
		[SerializeField] private int index;

		[SerializeField] private int volumeParameterID = -1;
		[SerializeField] private float volume;

		[SerializeField] private OptionSetMethod method = OptionSetMethod.Language;
		private enum OptionSetMethod { Language=0, Subtitles=1, SFXVolume=2, SpeechVolume=3, MusicVolume=4 };

		[SerializeField] private SplitLanguageType splitLanguageType = SplitLanguageType.TextAndVoice;

		
		public ActionOptionSet ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Save;
			title = "Set Option";
			description = "Set an Options variable to a specific value";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			switch (method)
			{
				case OptionSetMethod.Language:
					index = AssignInteger (parameters, indexParameterID, index);
					break;

				case OptionSetMethod.Subtitles:
					BoolValue boolValue = (BoolValue) index;
					boolValue = AssignBoolean (parameters, indexParameterID, boolValue);
					index = (int) boolValue;
					break;

				case OptionSetMethod.SFXVolume:
				case OptionSetMethod.SpeechVolume:
				case OptionSetMethod.MusicVolume:
					volume = AssignFloat (parameters, volumeParameterID, volume);
					volume = Mathf.Clamp01 (volume);
					break;
			}
		}
		
		
		override public float Run ()
		{
			switch (method)
			{
				case OptionSetMethod.Language:
					if (index >= 0 && KickStarter.speechManager != null && index < KickStarter.speechManager.languages.Count)
					{
						if (KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages)
						{
							switch (splitLanguageType)
							{
								case SplitLanguageType.TextAndVoice:
									Options.SetLanguage (index);
									Options.SetVoiceLanguage (index);
									break;

								case SplitLanguageType.TextOnly:
									Options.SetLanguage (index);
									break;

								case SplitLanguageType.VoiceOnly:
									Options.SetVoiceLanguage (index);
									break;
							}
						}
						else
						{
							Options.SetLanguage (index);
						}
					}
					else
					{
						ACDebug.LogWarning ("Could not set language to index: " + index + " - does this language exist?");
					}
					break;

				case OptionSetMethod.Subtitles:
					Options.SetSubtitles ((index == 1));
					break;

				case OptionSetMethod.SpeechVolume:
					Options.SetSpeechVolume (volume);
					break;

				case OptionSetMethod.SFXVolume:
					Options.SetSFXVolume (volume);
					break;

				case OptionSetMethod.MusicVolume:
					Options.SetMusicVolume (volume);
					break;
			}

			return 0f;
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			method = (OptionSetMethod) EditorGUILayout.EnumPopup ("Option to set:", method);

			switch (method)
			{
				case OptionSetMethod.Language:
					if (KickStarter.speechManager == null)
					{
						EditorGUILayout.HelpBox ("No Speech Manager found! One must be assigned in order to change the language.", MessageType.Warning);
					}
					else if (KickStarter.speechManager.languages != null && KickStarter.speechManager.languages.Count > 1)
					{
						if (KickStarter.speechManager != null && KickStarter.speechManager.separateVoiceAndTextLanguages)
						{
							splitLanguageType = (SplitLanguageType) EditorGUILayout.EnumPopup ("Affect:", splitLanguageType);
						}

						indexParameterID = Action.ChooseParameterGUI ("Language:", parameters, indexParameterID, ParameterType.Integer);
						if (indexParameterID < 0)
						{
							index = EditorGUILayout.Popup ("Language:", index, KickStarter.speechManager.languages.ToArray ());
						}
					}
					else
					{
						index = 0;
						EditorGUILayout.HelpBox ("Multiple languages not found!.", MessageType.Warning);
					}
					break;

				case OptionSetMethod.Subtitles:
					indexParameterID = Action.ChooseParameterGUI ("Show subtitles:", parameters, indexParameterID, ParameterType.Boolean);
					if (indexParameterID < 0)
					{
						bool showSubtitles = (index == 1);
						showSubtitles = EditorGUILayout.Toggle ("Show subtitles?", showSubtitles);
						index = (showSubtitles) ? 1 : 0;
					}
					break;

				case OptionSetMethod.SFXVolume:
				case OptionSetMethod.SpeechVolume:
				case OptionSetMethod.MusicVolume:
					volumeParameterID = Action.ChooseParameterGUI ("New volume:", parameters, volumeParameterID, ParameterType.Float);
					if (volumeParameterID < 0)
					{
						volume = EditorGUILayout.Slider ("New volume:", volume, 0f, 1f);
					}
					break;
			}

			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			return method.ToString ();
		}

		#endif
		
	}

}