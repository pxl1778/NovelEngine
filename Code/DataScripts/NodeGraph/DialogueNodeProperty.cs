using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[Serializable]
public class DialogueNodeProperty
{
}

[Serializable]
public class ImageBoxProperty : DialogueNodeProperty {
    public Sprite ImageBoxSprite;
    public bool showHideToggle;
}
