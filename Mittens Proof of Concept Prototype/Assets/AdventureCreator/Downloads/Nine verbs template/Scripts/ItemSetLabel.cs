using UnityEngine;
using System.Collections;
using AC;

public class ItemSetLabel : MonoBehaviour
{

	public string inventoryMenuName = "Verbs";
	public string inventoryElementName = "Inventory";
	public int interactionIndex = 0;

	private int tempInvID = -1;
	private Menu verbsMenu;
	private MenuInventoryBox inventoryElement;


	private void Start ()
	{
		verbsMenu = PlayerMenus.GetMenuWithName (inventoryMenuName);
		inventoryElement = verbsMenu.GetElementWithName (inventoryElementName) as MenuInventoryBox;
	}


	private void Update ()
	{
		bool isOver = false;

		if (KickStarter.runtimeInventory.SelectedItem == null)
		{
			for (int _slot=0; _slot<inventoryElement.GetNumSlots (); _slot++)
			{
				if (verbsMenu.IsPointerOverSlot (inventoryElement, _slot, KickStarter.playerInput.GetInvertedMouse ()))
				{
					isOver = true;
					InvItem hoverItem = inventoryElement.GetItem (_slot);

					if (hoverItem.id != tempInvID && tempInvID >= 0)
					{
						KickStarter.playerCursor.ResetSelectedCursor ();
						tempInvID = -1;
					}

					int cursorID = KickStarter.playerCursor.GetSelectedCursorID ();

					if (cursorID == -1)
					{
						cursorID = hoverItem.interactions[interactionIndex].icon.id;
						KickStarter.playerCursor.SetCursorFromID (cursorID);
						tempInvID = hoverItem.id;
						KickStarter.playerMenus.UpdateAllMenus ();
						return;
					}
				}
			}
		}

		if (tempInvID >= 0 && !isOver)
		{
			KickStarter.playerCursor.ResetSelectedCursor ();
			tempInvID = -1;
		}
	}
	
}
