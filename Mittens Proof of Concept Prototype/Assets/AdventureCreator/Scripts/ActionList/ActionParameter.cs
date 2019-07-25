/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2019
 *	
 *	"ActionParameter.cs"
 * 
 *	This defines a parameter that can be used by ActionLists
 * 
 */

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A data container for an ActionList parameter. A parameter can change the value of an Action's public variables dynamically during gameplay, allowing the same Action to be repurposed for different tasks.
	 */
	[System.Serializable]
	public class ActionParameter
	{

		/** The display name in the Editor */
		public string label = "";
		/** A unique identifier */
		public int ID = 0;
		/** The type of variable it overrides (GameObject, InventoryItem, GlobalVariable, LocalVariable, String, Float, Integer, Boolean, Vector3, Document, ComponentVariable, Parameter) */
		public ParameterType parameterType = ParameterType.GameObject;
		/** The new value or ID number, if parameterType = ParameterType.Integer / Boolean / LocalVariable / GlobalVariable / InventoryItem / Document / ComponentVariable.  If parameterType = ParameterType.GameObject, it is the ConstantID number of the GameObject if it is not currently accessible */
		public int intValue = -1;
		/** The new value, if parameterType = ParameterType.Float */
		public float floatValue = 0f;
		/** The new value, if parameterType = ParameterType.String */
		public string stringValue = "";
		/** The new value, if parameterType = ParameterType.GameObject */
		public GameObject gameObject;
		/** The new value, if parameterType = ParameterType.UnityObject */
		public Object objectValue;
		/** The new value, if parameterType = ParameterType.Vector3 */
		public Vector3 vector3Value;
		/** If a prefab is passed between assets as a GameObject parameter, what should ultimately be referenced (ReferencePrefab, ReferenceSceneInstance) */
		public GameObjectParameterReferences gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;

		public Variables variables = null;


		/**
		 * <summary>A Constructor that generates a unique ID number.</summary>
		 * <param name = "idArray">An array of previously-used ID numbers, to ensure its own ID is unique.</param>
		 */
		public ActionParameter (int[] idArray)
		{
			label = string.Empty;
			ID = 0;
			intValue = -1;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			parameterType = ParameterType.GameObject;
			vector3Value = Vector3.zero;
			gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
			variables = null;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID ++;
			}
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		/**
		 * <summary>A Constructor that sets the ID number explicitly.</summary>
		 * <param name = "id">The unique identifier to assign</param>
		 */
		public ActionParameter (int id)
		{
			label = string.Empty;
			ID = id;
			intValue = -1;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			parameterType = ParameterType.GameObject;
			vector3Value = Vector3.zero;
			gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
			variables = null;

			label = "Parameter " + (ID + 1).ToString ();
		}


		/**
		 * <summary>A Constructor that duplicates another ActionParameter.</summary>
		 */
		public ActionParameter (ActionParameter _actionParameter, bool alsoCopyValues = false)
		{
			label = _actionParameter.label;
			ID = _actionParameter.ID;
			parameterType = _actionParameter.parameterType;

			if (alsoCopyValues)
			{
				intValue = _actionParameter.intValue;
				floatValue = _actionParameter.floatValue;
				stringValue = _actionParameter.stringValue;
				gameObject = _actionParameter.gameObject;
				objectValue = _actionParameter.objectValue;
				vector3Value = _actionParameter.vector3Value;
				gameObjectParameterReferences = _actionParameter.gameObjectParameterReferences;
				variables = _actionParameter.variables;
			}
			else
			{
				intValue = -1;
				floatValue = 0f;
				stringValue = string.Empty;
				gameObject = null;
				objectValue = null;
				vector3Value = Vector3.zero;
				gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
				variables = null;
			}
		}


		/**
		 * <summary>Copies the "value" variables from another ActionParameter, without changing the type, ID, or label.</summary>
		 * <parameter name = "otherParameter">The ActionParameter to copy from</param>
		 */
		public void CopyValues (ActionParameter otherParameter)
		{
			intValue = otherParameter.intValue;
			floatValue = otherParameter.floatValue;
			stringValue = otherParameter.stringValue;
			gameObject = otherParameter.gameObject;
			objectValue = otherParameter.objectValue;
			vector3Value = otherParameter.vector3Value;
			gameObjectParameterReferences = otherParameter.gameObjectParameterReferences;
			variables = otherParameter.variables;
		}


		/**
		 * Resets the value that the parameter assigns.
		 */
		public void Reset ()
		{
			intValue = -1;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			gameObjectParameterReferences = GameObjectParameterReferences.ReferencePrefab;
			variables = null;
		}


		/**
		 * <summary>Checks if the parameter's value is an integer. This is the case if parameterType = ParameterType.GameObject, GlobalVariable, Integer, InventoryItem or LocalVariable.</summary>
		 * <returns>True if the parameter's value is an integer.</returns>
		 */
		public bool IsIntegerBased ()
		{
			if (parameterType == ParameterType.GameObject ||
			    parameterType == ParameterType.GlobalVariable ||
			    parameterType == ParameterType.Integer ||
				parameterType == ParameterType.Boolean ||
			    parameterType == ParameterType.InventoryItem ||
			    parameterType == ParameterType.Document ||
			    parameterType == ParameterType.LocalVariable ||
			    parameterType == ParameterType.ComponentVariable)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Sets the intValue that the parameter assigns</summary>
		 * <param name = "_value">The new value or ID number, if parameterType = ParameterType.Integer / Boolean / LocalVariable / GlobalVariable / InventoryItem.  If parameterType = ParameterType.GameObject, it is the ConstantID number of the GameObject if it is not currently accessible</param>
		 */
		public void SetValue (int _value)
		{
			intValue = _value;
			floatValue = 0f;
			stringValue = string.Empty;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}


		/**
		 * <summary>Sets the floatValue that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.Float</param>
		 */
		public void SetValue (float _value)
		{
			floatValue = _value;
			stringValue = string.Empty;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}


		/**
		 * <summary>Sets the stringValue that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.String</param>
		 */
		public void SetValue (string _value)
		{
			stringValue = AdvGame.ConvertTokens (_value);
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}


		/**
		 * <summary>Sets the vector3Value that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.Vector3</param>
		 */
		public void SetValue (Vector3 _value)
		{
			stringValue = string.Empty;
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = _value;
			variables = null;
		}


		/**
		 * <summary>Sets the gameObject that the parameter assigns</summary>
		 * <param name = "_object">The new GameObject, if parameterType = ParameterType.GameObject</param>
		 */
		public void SetValue (GameObject _object)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = -1;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}


		/**
		 * <summary>Sets the objectValue that the parameter assigns</summary>
		 * <param name = "_object">The new Unity Object, if parameterType = ParameterType.UnityObject</param>
		 */
		public void SetValue (Object _object)
		{
			gameObject = null;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = -1;
			objectValue = _object;
			vector3Value = Vector3.zero;
			variables = null;
		}


		/**
		 * <summary>Sets the gameObject that the parameter assigns</summary>
		 * <param name = "_object">The new GameObject, if parameterType = ParameterType.GameObject</param>
		 * <param name = "_value">The GameObject's ConstantID number, which is used to find the GameObject if it is not always in the same scene as the ActionParameter class</param>
		 */
		public void SetValue (GameObject _object, int _value)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = _value;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = null;
		}


		public void SetValue (Variables _variables, int _value)
		{
			gameObject = null;
			floatValue = 0f;
			stringValue = string.Empty;
			intValue = _value;
			objectValue = null;
			vector3Value = Vector3.zero;
			variables = _variables;
		}


		public GVar GetVariable ()
		{
			switch (parameterType)
			{
				case ParameterType.GlobalVariable:
					return GlobalVariables.GetVariable (intValue, true);

				case ParameterType.LocalVariable:
					return LocalVariables.GetVariable (intValue);

				case ParameterType.ComponentVariable:
					if (variables != null)
					{
						return variables.GetVariable (intValue);
					}
					break;
			}
			return null;
		}


		/**
		 * <summary>Generates a label that represents the name of the parameter's value, if appropriate<summary>
		 * <returns>A label that represents the name of the parameter's value<summary>
		 */
		public string GetLabel ()
		{
			switch (parameterType)
			{
				case ParameterType.GameObject:
					if (gameObject != null)
					{
						Hotspot _hotspot = gameObject.GetComponent <Hotspot>();
						if (_hotspot) return _hotspot.GetName (Options.GetLanguage ());

						Char _char = gameObject.GetComponent <Char>();
						if (_char) return _char.GetName (Options.GetLanguage ());

						return gameObject.name;
					}
					return string.Empty;

				case ParameterType.InventoryItem:
					InvItem invItem = KickStarter.inventoryManager.GetItem (intValue);
					if (invItem != null)
					{
						return invItem.GetLabel (Options.GetLanguage ());
					}
					return GetSaveData ();

				case ParameterType.Document:
					Document document = KickStarter.inventoryManager.GetDocument (intValue);
					if (document != null)
					{
						return KickStarter.runtimeLanguages.GetTranslation (document.title,
																			document.titleLineID,
																			Options.GetLanguage ());
					}
					return GetSaveData ();

				case ParameterType.GlobalVariable:
					GVar gVar = GetVariable ();
					if (gVar != null)
					{
						return gVar.label;
					}
					return GetSaveData ();

				case ParameterType.LocalVariable:
					GVar lVar = GetVariable ();
					if (lVar != null)
					{
						return lVar.label;
					}
					return GetSaveData ();

				case ParameterType.ComponentVariable:
					GVar cVar = GetVariable ();
					if (cVar != null)
					{
						return cVar.label;
					}
					return GetSaveData ();

				default:
					return GetSaveData ();
			}
		}


		/**
		 * <summary>Generates a string that represents the parameter's saveable data</summar>
		 * <returns>The data string</returns>
		 */
		public string GetSaveData ()
		{
			switch (parameterType)
			{
				case ParameterType.Float:
					return floatValue.ToString ();

				case ParameterType.String:
					return AdvGame.PrepareStringForSaving (stringValue);

				case ParameterType.GameObject:
					if (gameObject != null)
					{
						if (gameObject.GetComponent <ConstantID>())
						{
							return gameObject.GetComponent <ConstantID>().constantID.ToString ();
						}
						ACDebug.LogWarning ("Could not save parameter data for '" + gameObject.name + "' as it has no Constant ID number.", gameObject);
					}
					return string.Empty;

				case ParameterType.UnityObject:
					if (objectValue != null)
					{
						return objectValue.name;
					}
					return string.Empty;

				case ParameterType.Vector3:
					string vector3Val = vector3Value.x.ToString () + "," + vector3Value.y.ToString () + "," + vector3Value.z.ToString ();
					vector3Val = AdvGame.PrepareStringForSaving (vector3Val);
					return vector3Val;

				default:
					return intValue.ToString ();
			}
		}


		/**
		 * <summary>Restores data from a data string</summary>
		 * <param name="dataString">The data</param>
		 */
		public void LoadData (string dataString)
		{
			switch (parameterType)
			{
				case ParameterType.Float:
					floatValue = 0f;
					float.TryParse (dataString, out floatValue);
					break;

				case ParameterType.String:
					stringValue = AdvGame.PrepareStringForLoading (dataString);
					break;

				case ParameterType.GameObject:
					gameObject = null;
					int constantID = 0;
					if (int.TryParse (dataString, out constantID))
					{
						ConstantID _constantID = Serializer.returnComponent <ConstantID> (constantID);
						if (_constantID != null)
						{
							gameObject = _constantID.gameObject;
						}
					}
					break;

				case ParameterType.UnityObject:
					if (string.IsNullOrEmpty (dataString))
					{
						objectValue = null;
					}
					else
					{
						Object[] objects = (Object[]) Resources.LoadAll ("");
						foreach (Object _object in objects)
						{
							if (_object.name == dataString)
							{
								objectValue = _object;
								return;
							}
						}
					}
					break;

				case ParameterType.Vector3:
					if (!string.IsNullOrEmpty (dataString))
					{
						dataString = AdvGame.PrepareStringForLoading (dataString);

						Vector3 _value = Vector3.zero;
						string[] valuesArray = dataString.Split (","[0]);
						if (valuesArray != null && valuesArray.Length == 3)
						{
							float xValue = 0f;
							float.TryParse (valuesArray[0], out xValue);

							float yValue = 0f;
							float.TryParse (valuesArray[1], out yValue);

							float zValue = 0f;
							float.TryParse (valuesArray[2], out zValue);

							_value = new Vector3 (xValue, yValue, zValue);
						}

						vector3Value = _value;
					}
					break;

				default:
					intValue = 0;
					int.TryParse (dataString, out intValue);
					break;
			}
		}

	}

}