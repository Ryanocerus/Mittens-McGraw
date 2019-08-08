using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public GameObject player;
    public Canvas canvas;
    
    public void Update()
    {
        if (Vector3.Distance(player.transform.position, this.transform.position) <= 1.5f)
        {
            Debug.Log("player within distance to talk to " + dialogue.name);
        }
        if (Input.GetKeyDown(KeyCode.F) && Vector3.Distance(player.transform.position, this.transform.position) <= 1.5f )
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        canvas.enabled = true;
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }

    // trigger questionare

    // trigger rewards
}
