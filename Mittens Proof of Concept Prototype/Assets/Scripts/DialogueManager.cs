using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text nameText;
    public Text dialogueText;
    //public Animator animator;
    public Canvas canvas;
    public bool askedQuestion;
    public GameObject player;

    public Queue<string> sentences;
    

    
    void Start()
    {
        canvas.enabled = false;
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        canvas.enabled = true;
        //animator.SetBool("IsOpen", true);
        nameText.text = dialogue.name;
        Debug.Log("Starting conversation with" + dialogue.name);

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
       
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            GoToOptions();
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        Debug.Log(sentence);
    }

    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }
    public void EndDialogue()
    {
        canvas.enabled = false;
        //animator.SetBool("IsOpen", false);
    }

    public void GoToOptions()
    {
        nameText.enabled = false;
        dialogueText.enabled = false;

        //display button
    }

}
