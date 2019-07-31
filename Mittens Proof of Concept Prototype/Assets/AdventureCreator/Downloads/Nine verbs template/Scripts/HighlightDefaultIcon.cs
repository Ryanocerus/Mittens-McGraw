using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AC;

public class HighlightDefaultIcon : MonoBehaviour
{

	public string menuName;
	public string interactionElementName;

	private UnityEngine.UI.Button button;
	private MenuInteraction menuInteraction;

	private ColorBlock normalColorBlock;
	private ColorBlock highlightedColorBlock;


	private void Start ()
	{
		menuInteraction = PlayerMenus.GetElementWithName (menuName, interactionElementName) as MenuInteraction;

		button = GetComponent <UnityEngine.UI.Button>();

		Color highlightedColor = button.colors.highlightedColor;
		normalColorBlock = highlightedColorBlock = button.colors;
		highlightedColorBlock.normalColor = highlightedColor;
	}


	private void Update ()
	{
		button.colors = (menuInteraction.IsDefaultIcon) ? highlightedColorBlock : normalColorBlock;
	}
	
}
