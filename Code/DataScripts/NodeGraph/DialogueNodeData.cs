using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class DialogueNodeData : BaseNodeData {
    public string DialogueText = "DialogueNode";
    public string SpeakingCharacterId = "";
    public string NameplateOverride = "";
    public SerializableDictionary<string, string> ChangeAnimationList = new SerializableDictionary<string, string>();
    public SerializableDictionary<string, FadeInListItemData> FadeInList = new SerializableDictionary<string, FadeInListItemData>();
    public List<string> FadeOutList = new List<string>();
    public Sprite Background;
    public bool StopMusic;
    public AudioClip Music;
    public AudioClip Sound;
    public bool ExclaimTextBox;
    public bool ScreenFadeIn;
    public bool ScreenFadeOut;
    public bool CameraShake;
    public float ShakeAmplitude = 1;
    public float ShakeDuration = 1;
    public List<ReactionListItemData> ReactionList = new List<ReactionListItemData>();
    public List<ActionListItemData> SpecialActionList  = new List<ActionListItemData>();
}

[Serializable]
public class FadeInListItemData {
    public string animation = null;
    public float position = 1000;
    public bool flipped = false;
    public bool startInBack = false;

    public FadeInListItemData(string animation, float position = 3, bool flipped = false, bool startInBack = false) {
        this.animation = animation;
        this.position = position;
        this.flipped = flipped;
        this.startInBack = startInBack;
    }
}

[Serializable]
public class ActionListItemData {
    public SpecialAction actionType;
    public string parameterOne;
    public float parameterTwo;

    public ActionListItemData(SpecialAction actionType, string parameterOne = "", float parameterTwo = 0) {
        this.actionType = actionType;
        this.parameterOne = parameterOne;
        this.parameterTwo = parameterTwo;
    }
}

[Serializable]
public class ReactionListItemData {
    public string characterId;
    public GameObject reaction;

    public ReactionListItemData(string characterId, GameObject reaction) {
        this.characterId = characterId;
        this.reaction = reaction;
    }
}
