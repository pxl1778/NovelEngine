using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

public class PropertyFoldout : Foldout
{
}

/// <summary>
/// Foldout to show a list of characters who will flip their sprite on the canvas.
/// </summary>
public class FlipFoldout : Foldout {
    public DialogueGraphView graphView;

    public FlipFoldout(DialogueNode dialogueNode, DialogueGraphView graphView, FlipSpriteProperty property = null) {
        this.graphView = graphView;
        text = "Flip Sprite";

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        // Add first element
        if (property == null) {
            property = new FlipSpriteProperty();
            nodeData.PropertiesList.Add(property);
            AddItem(dialogueNode, graphView, property);
        }

        // Setup Existing data
        property.characterIds.ForEach((characterId) => {
            AddItem(dialogueNode, graphView, property, characterId);
        }); 

        // Add Button callback
        var addButton = new Button(() => { AddItem(dialogueNode, graphView, property); }) { text = "+" };
        Insert(0, addButton);

        style.borderTopWidth = 1;
        style.borderTopColor = new Color(0, 0, 0, 1);
        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);
    }

    public void AddItem(DialogueNode dialogueNode, DialogueGraphView graphView, FlipSpriteProperty property, string characterId = "") {
        Add(new FlipFoldoutItem(dialogueNode, graphView, property, () => {
            int numChildren = 0;
            List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null" && !property.characterIds.Contains(x)).ToList();
            Children().ToList().ForEach((element) => {
                if(element is FlipFoldoutItem item) {
                    numChildren++;
                    item.UpdateCharacters(characters);
                }
            });
            if (numChildren == 0) {
                DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
                nodeData.PropertiesList.Remove(property);
                parent.Remove(this);
            }
        }, characterId));
    }
}

public class FlipFoldoutItem : VisualElement {
    public DropdownField characterDropdown;
    public FlipFoldoutItem(DialogueNode dialogueNode, DialogueGraphView graphView, FlipSpriteProperty property, Action callback, string characterId = "") {
        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;

        List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null" && !property.characterIds.Contains(x)).ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        characterDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoFlipSpriteCharacter:" + dialogueNode.GUID);
            property.characterIds.Remove(evt.previousValue);
            property.characterIds.Add(evt.newValue);
            callback.Invoke();
            EditorUtility.SetDirty(graphView.containerCache);
        });
        characterDropdown.SetValueWithoutNotify("");
        Add(characterDropdown);

        if (characterId != "") {
            characterDropdown.SetValueWithoutNotify(characterId);
        }

        var deleteButton = new Button(() => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoFlipSpriteRemove:" + dialogueNode.GUID);
            nodeData.PropertiesList.Remove(property);
            property.characterIds.Remove(characterDropdown.value);
            EditorUtility.SetDirty(graphView.containerCache);
            parent.contentContainer.Remove(this);
            callback.Invoke();
        }) { text = "X" };
        deleteButton.style.width = 20;
        characterDropdown.Add(deleteButton);
    }

    public void UpdateCharacters(List<string> characters) {
        characterDropdown.choices = characters;
    }
}

/// <summary>
/// Section to have a list of sprites that move their position horizontally on the canvas.
/// </summary>
public class MoveFoldout : Foldout {
    public DialogueGraphView graphView;

    public MoveFoldout(DialogueNode dialogueNode, DialogueGraphView graphView, MoveSpriteProperty property = null) {
        this.graphView = graphView;
        text = "Move Sprite";

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        // Add first element
        if (property == null) {
            property = new MoveSpriteProperty();
            nodeData.PropertiesList.Add(property);
            AddItem(dialogueNode, graphView, property);
        }

        // Setup Existing data
        property.characterPositions.Keys.ToList().ForEach((characterId) => {
            AddItem(dialogueNode, graphView, property, characterId, property.characterPositions[characterId]);
        });

        // Add Button callback
        var addButton = new Button(() => { AddItem(dialogueNode, graphView, property); }) { text = "+" };
        Insert(0, addButton);

        style.borderTopWidth = 1;
        style.borderTopColor = new Color(0, 0, 0, 1);
        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);
    }

    public void AddItem(DialogueNode dialogueNode, DialogueGraphView graphView, MoveSpriteProperty property, string characterId = "", float position = 3) {
        Add(new MoveFoldoutItem(dialogueNode, graphView, property, () => {
            int numChildren = 0;
            List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null" && !property.characterPositions.Keys.ToList().Contains(x)).ToList();
            Children().ToList().ForEach((element) => {
                if (element is MoveFoldoutItem item) {
                    numChildren++;
                    item.UpdateCharacters(characters);
                }
            });
            if (numChildren == 0) {
                DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
                nodeData.PropertiesList.Remove(property);
                parent.Remove(this);
            }
        }, characterId, position));
    }
}

public class MoveFoldoutItem : VisualElement {
    public DropdownField characterDropdown;
    public FloatField positionField;
    public MoveFoldoutItem(DialogueNode dialogueNode, DialogueGraphView graphView, MoveSpriteProperty property, Action callback, string characterId = "", float position = 3) {
        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;

        List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null" && !property.characterPositions.Keys.ToList().Contains(x)).ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        characterDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoMoveSpriteCharacter:" + dialogueNode.GUID);
            property.characterPositions.Remove(evt.previousValue);
            property.characterPositions.Add(evt.newValue, positionField.value);
            callback.Invoke();
            EditorUtility.SetDirty(graphView.containerCache);
        });
        characterDropdown.SetValueWithoutNotify(characterId);
        Add(characterDropdown);

        positionField = new FloatField("Position");
        positionField.labelElement.style.minWidth = 100;
        positionField.tooltip = "Position is a number that signifies the location on screen that a sprite will be. Position 1 is on screen all the way to the left and Position 5 is on screen to the right. Beyond those numbers is off screen and 3 is in the middle. Feel free to use decimal values as well.";
        positionField.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoMoveSpritePosition:" + dialogueNode.GUID);
            if (property.characterPositions.ContainsKey(characterDropdown.value)) {
                property.characterPositions[characterDropdown.value] = evt.newValue;
            }
            EditorUtility.SetDirty(graphView.containerCache);
        });
        positionField.SetValueWithoutNotify(position);
        Add(positionField);

        var deleteButton = new Button(() => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoMoveSpriteRemove:" + dialogueNode.GUID);
            property.characterPositions.Remove(characterDropdown.value);
            EditorUtility.SetDirty(graphView.containerCache);
            parent.contentContainer.Remove(this);
            callback.Invoke();
        }) { text = "X" };
        deleteButton.style.width = 20;
        characterDropdown.Add(deleteButton);
    }

    public void UpdateCharacters(List<string> characters) {
        characterDropdown.choices = characters;
    }
}

/// <summary>
/// Foldout with a list of characters moving their sort orders
/// </summary>
public class OrderFoldout : Foldout {
    public DialogueGraphView graphView;

    public OrderFoldout(DialogueNode dialogueNode, DialogueGraphView graphView, OrderSpriteProperty property = null) {
        this.graphView = graphView;
        text = "Sort Order Sprite";

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
        // Add first element
        if (property == null) {
            property = new OrderSpriteProperty();
            nodeData.PropertiesList.Add(property);
            AddItem(dialogueNode, graphView, property);
        }

        // Setup Existing data
        property.characterOrders.Keys.ToList().ForEach((characterId) => {
            AddItem(dialogueNode, graphView, property, characterId, property.characterOrders[characterId]);
        });

        // Add Button callback
        var addButton = new Button(() => { AddItem(dialogueNode, graphView, property); }) { text = "+" };
        Insert(0, addButton);

        style.borderTopWidth = 1;
        style.borderTopColor = new Color(0, 0, 0, 1);
        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);
    }

    public void AddItem(DialogueNode dialogueNode, DialogueGraphView graphView, OrderSpriteProperty property, string characterId = "", SortOrder order = SortOrder.MOVE_TO_FRONT) {
        Add(new OrderFoldoutItem(dialogueNode, graphView, property, () => {
            int numChildren = 0;
            List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null" && !property.characterOrders.Keys.ToList().Contains(x)).ToList();
            Children().ToList().ForEach((element) => {
                if (element is OrderFoldoutItem item) {
                    numChildren++;
                    item.UpdateCharacters(characters);
                }
            });
            if (numChildren == 0) {
                DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;
                nodeData.PropertiesList.Remove(property);
                parent.Remove(this);
            }
        }, characterId, order));
    }
}

public class OrderFoldoutItem : VisualElement {
    public DropdownField characterDropdown;
    public DropdownField orderDropdown;
    public OrderFoldoutItem(DialogueNode dialogueNode, DialogueGraphView graphView, OrderSpriteProperty property, Action callback, string characterId = "", SortOrder order = SortOrder.MOVE_TO_FRONT) {
        style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
        style.borderBottomWidth = 1;
        style.borderBottomColor = new Color(0, 0, 0, 1);

        DialogueNodeData nodeData = dialogueNode.NodeData as DialogueNodeData;

        List<string> characters = NovelData.instance.GetCharacterIds().Where(x => x != "Null" && !property.characterOrders.Keys.ToList().Contains(x)).ToList();
        characterDropdown = new DropdownField("Character", characters, 0);
        characterDropdown.labelElement.style.minWidth = 20;
        characterDropdown.RegisterValueChangedCallback(evt => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoOrderSpriteCharacter:" + dialogueNode.GUID);
            property.characterOrders.Remove(evt.previousValue);
            property.characterOrders.Add(evt.newValue, (SortOrder)System.Enum.Parse(typeof(SortOrder), orderDropdown.value));
            callback.Invoke();
            EditorUtility.SetDirty(graphView.containerCache);
        });
        characterDropdown.SetValueWithoutNotify(characterId);
        Add(characterDropdown);

        orderDropdown = new DropdownField("Sort Order", Enum.GetNames(typeof(SortOrder)).ToList(), 0);
        orderDropdown.labelElement.style.minWidth = 20;
        orderDropdown.RegisterValueChangedCallback(evt => {
            //Reset dropdown
            object actionType;
            if (System.Enum.TryParse(typeof(SortOrder), evt.newValue, out actionType)) {
                Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoOrderSpriteOrder:" + dialogueNode.GUID);
                property.characterOrders[characterDropdown.value] = (SortOrder)actionType;
                EditorUtility.SetDirty(graphView.containerCache);
            }
        });
        orderDropdown.SetValueWithoutNotify(order.ToString());
        Add(orderDropdown);

        var deleteButton = new Button(() => {
            Undo.RegisterCompleteObjectUndo(graphView.containerCache, "NodeUndoOrderSpriteRemove:" + dialogueNode.GUID);
            property.characterOrders.Remove(characterDropdown.value);
            EditorUtility.SetDirty(graphView.containerCache);
            parent.contentContainer.Remove(this);
            callback.Invoke();
        }) { text = "X" };
        deleteButton.style.width = 20;
        characterDropdown.Add(deleteButton);
    }

    public void UpdateCharacters(List<string> characters) {
        characterDropdown.choices = characters;
    }
}
