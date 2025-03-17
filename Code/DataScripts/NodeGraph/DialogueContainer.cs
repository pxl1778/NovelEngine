using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class DialogueContainer : ScriptableObject {
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string LoadPath { get; private set; }

    public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
    public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
    public List<SceneNodeData> SceneNodeDatas = new List<SceneNodeData>();
    public List<SpawnPointNodeData> SpawnPointNodeDatas = new List<SpawnPointNodeData>();
    public List<MakeChoiceNodeData> MakeChoiceNodeDatas = new List<MakeChoiceNodeData>();
    public List<ChoiceBranchNodeData> ChoiceBranchNodeDatas = new List<ChoiceBranchNodeData>();
    public List<CommentNodeData> CommentNodeDatas = new List<CommentNodeData>();

    public string StartingNodeGUID = "";
    public string ShortLine = "";

    public List<BaseNodeData> AllNodeDatas { get {
            List<BaseNodeData> allNodes = new List<BaseNodeData>();
            allNodes.AddRange(DialogueNodeDatas);
            allNodes.AddRange(SceneNodeDatas);
            allNodes.AddRange(SpawnPointNodeDatas);
            allNodes.AddRange(MakeChoiceNodeDatas);
            allNodes.AddRange(ChoiceBranchNodeDatas);
            allNodes.AddRange(CommentNodeDatas);
            return allNodes;
        } }

    private void OnValidate() {
#if UNITY_EDITOR
        Id = this.name;
        string fullPath = AssetDatabase.GetAssetPath(GetInstanceID());
        LoadPath = fullPath.Replace("Assets/Resources/", "");
        UnityEditor.EditorUtility.SetDirty(this);
        EditorPrefs.SetString("LastDialogueSOChanged", LoadPath);
#endif
    }
}
