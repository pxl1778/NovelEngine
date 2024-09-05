using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveObject
{
    public int slot;
    public string date;
    public string sceneName;
    public string dialogueLoadPath;
    public string lineGuid;
    public List<string> completedDialogues;
    public List<string> keyChoices;
    public SerializableDictionary<string, string> choices;
}
