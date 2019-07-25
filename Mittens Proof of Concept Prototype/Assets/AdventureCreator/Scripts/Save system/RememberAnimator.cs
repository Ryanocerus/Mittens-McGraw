/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"RememberAnimator.cs"
 * 
 *	This script is attached to Animator components in the scene we wish to save the state of. (Unity 5-only)
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	#if UNITY_5 || UNITY_2017_1_OR_NEWER
	
	/**
	 * This script is attached to Animator components in the scene we wish to save the state of. (Unity 5-only)
	 */
	[RequireComponent (typeof (Animator))]
	[AddComponentMenu("Adventure Creator/Save system/Remember Animator")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_animator.html")]
	#endif
	public class RememberAnimator : Remember
	{

		[SerializeField] private bool saveController = false; 
		[SerializeField] private bool setDefaultParameterValues = false;
		[SerializeField] private List<DefaultAnimParameter> defaultAnimParameters = new List<DefaultAnimParameter>();

		private Animator _animator;
		private bool loadedData = false;

		
		private void Awake ()
		{
			if (loadedData) return;

			if (GameIsPlaying () && setDefaultParameterValues)
			{
				for (int i=0; i<Animator.parameters.Length; i++)
				{
					if (i < defaultAnimParameters.Count)
					{
						string parameterName = Animator.parameters[i].name;

						switch (Animator.parameters[i].type)
						{
							case AnimatorControllerParameterType.Bool:
								Animator.SetBool (parameterName, defaultAnimParameters[i].intValue == 1);
								break;

							case AnimatorControllerParameterType.Float:
								Animator.SetFloat (parameterName, defaultAnimParameters[i].floatValue);
								break;

							case AnimatorControllerParameterType.Int:
								Animator.SetInteger (parameterName, defaultAnimParameters[i].intValue);
								break;
						}
					}
				}
			}
		}
		
		
		public override string SaveData ()
		{
			AnimatorData animatorData = new AnimatorData ();
			animatorData.objectID = constantID;
			animatorData.savePrevented = savePrevented;

			if (saveController && _animator.runtimeAnimatorController != null)
			{
				animatorData.controllerID = AssetLoader.GetAssetInstanceID (_animator.runtimeAnimatorController);
			}
			
			animatorData.parameterData = ParameterValuesToString (Animator.parameters);
			animatorData.layerWeightData = LayerWeightsToString ();
			animatorData.stateData = StatesToString ();

			return Serializer.SaveScriptData <AnimatorData> (animatorData);
		}
		
		
		public override void LoadData (string stringData)
		{
			AnimatorData data = Serializer.LoadScriptData <AnimatorData> (stringData);
			if (data == null)
			{
				loadedData = false;
				return;
			}
			SavePrevented = data.savePrevented; if (savePrevented) return;

			if (!string.IsNullOrEmpty (data.controllerID))
			{
				RuntimeAnimatorController runtimeAnimatorController = AssetLoader.RetrieveAsset (_animator.runtimeAnimatorController, data.controllerID);
				if (runtimeAnimatorController != null)
				{
					_animator.runtimeAnimatorController = runtimeAnimatorController;
				}
			}

			StringToParameterValues (Animator.parameters, data.parameterData);
			StringToLayerWeights (data.layerWeightData);
			StringToStates (data.stateData);

			loadedData = true;
		}


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			setDefaultParameterValues = EditorGUILayout.Toggle ("Set default parameters?", setDefaultParameterValues);
			if (setDefaultParameterValues && Animator.runtimeAnimatorController)
			{
				GUILayout.Box (string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));

				int numParameters = Animator.parameters.Length;
				if (numParameters < defaultAnimParameters.Count)
				{
					defaultAnimParameters.RemoveRange (numParameters, defaultAnimParameters.Count - numParameters);
				}
				else if (numParameters > defaultAnimParameters.Count)
				{
					if (numParameters > defaultAnimParameters.Capacity)
					{
						defaultAnimParameters.Capacity = numParameters;
					}
					for (int i=defaultAnimParameters.Count; i<numParameters; i++)
					{
						defaultAnimParameters.Add (new DefaultAnimParameter ());
					}
				}

				for (int i=0; i<Animator.parameters.Length; i++)
				{
					AnimatorControllerParameter parameter = Animator.parameters[i];
					switch (parameter.type)
					{
						case AnimatorControllerParameterType.Bool:
							bool boolValue = (defaultAnimParameters[i].intValue == 1);
							boolValue = EditorGUILayout.Toggle (parameter.name, boolValue);
							defaultAnimParameters[i] = new DefaultAnimParameter ((boolValue) ? 1 : 0);
							break;

						case AnimatorControllerParameterType.Float:
							float floatValue = EditorGUILayout.FloatField (parameter.name, defaultAnimParameters[i].floatValue);
							defaultAnimParameters[i] = new DefaultAnimParameter (floatValue);
							break;

						case AnimatorControllerParameterType.Int:
							int intValue = EditorGUILayout.IntField (parameter.name, defaultAnimParameters[i].intValue);
							defaultAnimParameters[i] = new DefaultAnimParameter (intValue);
							break;
					}
				}
			}
			EditorGUILayout.EndVertical ();
		}

		#endif

		
		private string ParameterValuesToString (AnimatorControllerParameter[] parameters)
		{
			StringBuilder stateString = new StringBuilder ();
			
			foreach (AnimatorControllerParameter parameter in parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Bool:
						string value = (Animator.GetBool (parameter.name)) ? "1" : "0";
						stateString.Append (value);
						break;

					case AnimatorControllerParameterType.Float:
						stateString.Append (Animator.GetFloat (parameter.name).ToString ());
						break;

					case AnimatorControllerParameterType.Int:
						stateString.Append (Animator.GetInteger (parameter.name).ToString ());
						break;
				}
				
				stateString.Append (SaveSystem.pipe);
			}
			
			return stateString.ToString ();
		}


		private string LayerWeightsToString ()
		{
			StringBuilder stateString = new StringBuilder ();

			if (Animator.layerCount > 1)
			{
				for (int i=1; i<Animator.layerCount; i++)
				{
					float weight = Animator.GetLayerWeight (i);
					stateString.Append (weight.ToString ());
					stateString.Append (SaveSystem.pipe);
				}
			}

			return stateString.ToString ();
		}


		private string StatesToString ()
		{
			StringBuilder stateString = new StringBuilder ();

			for (int i=0; i<Animator.layerCount; i++)
			{
				if (Animator.IsInTransition (i))
				{
					stateString = ProcessState (stateString, Animator.GetNextAnimatorStateInfo (i));
				}
				else
				{
					stateString = ProcessState (stateString, Animator.GetCurrentAnimatorStateInfo (i));
				}

				stateString.Append (SaveSystem.pipe);
			}

			return stateString.ToString ();
		}


		private StringBuilder ProcessState (StringBuilder stateString, AnimatorStateInfo stateInfo)
		{
			int nameHash = stateInfo.shortNameHash;
			float timeAlong = stateInfo.normalizedTime;

			if (timeAlong > 1f)
			{
				if (stateInfo.loop)
				{
					timeAlong = (timeAlong % 1);
				}
				else
				{
					timeAlong = 1f;
				}
			}

			stateString.Append (nameHash + "," + timeAlong);
			return stateString;
		}
		
		
		private void StringToParameterValues (AnimatorControllerParameter[] parameters, string valuesString)
		{
			if (string.IsNullOrEmpty (valuesString))
			{
				return;
			}
			
			string[] valuesArray = valuesString.Split (SaveSystem.pipe[0]);
			
			for (int i=0; i<parameters.Length; i++)
			{
				if (i < valuesArray.Length && valuesArray[i].Length > 0)
				{
					string parameterName = parameters[i].name;
					
					if (parameters[i].type == AnimatorControllerParameterType.Bool)
					{
						Animator.SetBool (parameterName, (valuesArray[i] == "1") ? true : false);
					}
					else if (parameters[i].type == AnimatorControllerParameterType.Float)
					{
						float value = 0f;
						if (float.TryParse (valuesArray[i], out value))
						{
							Animator.SetFloat (parameterName, value);
						}
					}
					else if (parameters[i].type == AnimatorControllerParameterType.Int)
					{
						int value = 0;
						if (int.TryParse (valuesArray[i], out value))
						{
							Animator.SetInteger (parameterName, value);
						}
					}
				}
			}
		}


		private void StringToLayerWeights (string valuesString)
		{
			if (string.IsNullOrEmpty (valuesString) || Animator.layerCount <= 1)
			{
				return;
			}

			string[] valuesArray = valuesString.Split (SaveSystem.pipe[0]);

			for (int i=1; i<Animator.layerCount; i++)
			{
				if (i < (valuesArray.Length+1) && valuesArray[i-1].Length > 0)
				{
					float weight = 1f;
					if (float.TryParse (valuesArray[i-1], out weight))
					{
						Animator.SetLayerWeight (i, weight);
					}
				}
			}
		}


		private void StringToStates (string valuesString)
		{
			if (string.IsNullOrEmpty (valuesString))
			{
				return;
			}
			
			string[] valuesArray = valuesString.Split (SaveSystem.pipe[0]);
			
			for (int i=0; i<Animator.layerCount; i++)
			{
				if (i < (valuesArray.Length) && valuesArray[i].Length > 0)
				{
					string[] stateInfoArray = valuesArray[i].Split (","[0]);

					if (stateInfoArray.Length >= 2)
					{
						int nameHash = 0;
						float timeAlong = 0f;

						if (int.TryParse (stateInfoArray[0], out nameHash))
						{
							if (float.TryParse (stateInfoArray[1], out timeAlong))
							{
								Animator.Play (nameHash, i, timeAlong);
							}
						}
					}
				}
			}
		}


		private Animator Animator
		{
			get
			{
				if (_animator == null)
				{
					_animator = GetComponent <Animator>();
				}
				return _animator;
			}
		}


		[System.Serializable]
		private struct DefaultAnimParameter
		{

			public int intValue;
			public float floatValue;


			public DefaultAnimParameter (int _intValue)
			{
				intValue = _intValue;
				floatValue = 0f;
			}


			public DefaultAnimParameter (float _floatValue)
			{
				intValue = 0;
				floatValue = _floatValue;
			}

		}

	}
	

	/**
	 * A data container used by the RememberAnimator script.
	 */
	[System.Serializable]
	public class AnimatorData : RememberData
	{

		/** The unique identified of the Animator Controller */
		public string controllerID;
		/** The values of the parameters, separated by a pipe (|) character. */
		public string parameterData;
		/** The weights of each layer, separated by a pipe (|) character. */
		public string layerWeightData;
		/** Data for each layer's animation state. */
		public string stateData;

		/**
		 * The default Constructor.
		 */
		public AnimatorData () { }

	}

	#else

	public class RememberAnimator : MonoBehaviour
	{ }

	#endif

}
	