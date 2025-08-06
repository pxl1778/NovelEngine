using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[Serializable]
public class DialogueNodeProperty
{
}

public enum ImageBoxAnchor { TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT };
[Serializable]
public class ImageBoxProperty : DialogueNodeProperty {
    public Sprite ImageBoxSprite;
    public bool showHideToggle;
    public ImageBoxAnchor anchor = ImageBoxAnchor.TOP_LEFT;
}

[Serializable]
public class FlipSpriteProperty : DialogueNodeProperty {
    public List<string> characterIds = new List<string>();
}

[Serializable]
public class MoveSpriteProperty : DialogueNodeProperty {
    public SerializableDictionary<string, float> characterPositions = new SerializableDictionary<string, float>();
}

public enum SortOrder { MOVE_TO_FRONT, MOVE_TO_BACK };
[Serializable]
public class OrderSpriteProperty : DialogueNodeProperty {
    public SerializableDictionary<string, SortOrder> characterOrders = new SerializableDictionary<string, SortOrder>();
}
