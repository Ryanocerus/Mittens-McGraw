using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string name;
    
    //General inquiry
    [TextArea(3,10)]
    public string[] sentences;

    //Replies to the question
    public string[] QuestionReplies_A;
    public string[] QuestionReplies_B;
    public string[] QuestionReplies_C;
    public string[] QuestionReplies_D;

    
}
