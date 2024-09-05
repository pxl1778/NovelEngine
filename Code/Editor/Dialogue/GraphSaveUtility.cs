using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<BaseNode> Nodes => _targetGraphView.nodes.ToList().Cast<BaseNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView) {
        return new GraphSaveUtility {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName = "", bool newFile = false) {
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for(var i = 0; i < connectedPorts.Length; i++) {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;
            dialogueContainer.NodeLinks.Add(new NodeLinkData {
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                Guid = connectedPorts[i].viewDataKey,
                TargetNodeGuid = inputNode.GUID
            });
        }

        foreach(var node in Nodes.Where(node => !node.EntryPoint)) {
            if(node is DialogueNode) {
                DialogueNodeData nodeData = node.NodeData as DialogueNodeData;
                dialogueContainer.DialogueNodeDatas.Add(new DialogueNodeData {
                    Guid = node.GUID,
                    Position = node.GetPosition().position,
                    DialogueText = nodeData.DialogueText,
                    SpeakingCharacterId = nodeData.SpeakingCharacterId,
                    FadeInList = nodeData.FadeInList,
                    FadeOutList = nodeData.FadeOutList,
                    Background = nodeData.Background,
                    StopMusic = nodeData.StopMusic,
                    Music = nodeData.Music,
                    Sound = nodeData.Sound,
                    ExclaimTextBox = nodeData.ExclaimTextBox,
                    ScreenFadeIn = nodeData.ScreenFadeIn,
                    ScreenFadeOut = nodeData.ScreenFadeOut,
                    SpecialActionList = nodeData.SpecialActionList
                });
            } else if(node is SceneNode) {
                SceneNodeData nodeData = node.NodeData as SceneNodeData;
                dialogueContainer.SceneNodeDatas.Add(new SceneNodeData {
                    Guid = node.GUID,
                    Position = node.GetPosition().position,
                    NextScene = nodeData.NextScene
                });
            } else if (node is MakeChoiceNode) {
                MakeChoiceNodeData nodeData = node.NodeData as MakeChoiceNodeData;
                dialogueContainer.MakeChoiceNodeDatas.Add(new MakeChoiceNodeData {
                    Guid = node.GUID,
                    Position = node.GetPosition().position,
                    ChoiceKey = nodeData.ChoiceKey,
                    Activate = nodeData.Activate
                });
            } else if (node is ChoiceBranchNode) {
                ChoiceBranchNodeData nodeData = node.NodeData as ChoiceBranchNodeData;
                dialogueContainer.ChoiceBranchNodeDatas.Add(new ChoiceBranchNodeData {
                    Guid = node.GUID,
                    Position = node.GetPosition().position,
                    ChoiceKey = nodeData.ChoiceKey
                });
            }
        }

        dialogueContainer.StartingNodeGUID = Nodes.Where(node => node.EntryPoint).FirstOrDefault().GUID;
        dialogueContainer.ShortLine = _targetGraphView.GetShortLine();

        if(fileName == "") { // Save As...
            string saveDirectory = EditorUtility.SaveFilePanel("Select Directory", Application.dataPath + "/Resources/Dialogue", "", "asset");
            if (saveDirectory != "") {
                string assetRelativePath = saveDirectory.Substring(saveDirectory.IndexOf("Assets"));
                AssetDatabase.CreateAsset(dialogueContainer, assetRelativePath);
                string newFileName = System.IO.Path.GetFileNameWithoutExtension(saveDirectory);
                DialogueGraph window = EditorWindow.GetWindow<DialogueGraph>();
                string resourcesPath = saveDirectory.Substring(saveDirectory.IndexOf("/Resources/") + "/Resources/".Length);
                window.SetFilePath(resourcesPath, newFileName);
                _containerCache = (DialogueContainer)AssetDatabase.LoadAssetAtPath($"Assets/Resources/{resourcesPath}", typeof(DialogueContainer));
                _targetGraphView.SetContainerCache(_containerCache);
            }
        } else { // Save
            DialogueContainer oldContainer = (DialogueContainer)AssetDatabase.LoadAssetAtPath($"Assets/Resources/{fileName}", typeof(DialogueContainer));
            dialogueContainer.name = oldContainer.name;
            EditorUtility.CopySerialized(dialogueContainer, oldContainer);
        }
    }

    public void LoadGraph(string fileName) {
        _containerCache = Resources.Load<DialogueContainer>(fileName);
        if(_containerCache == null) {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        _targetGraphView.SetShortLine(_containerCache.ShortLine);
        _targetGraphView.SetContainerCache(_containerCache);
    }

    private void CreateNodes() {
        _targetGraphView.GenerateEntryPointNode();
        // Dialogue Nodes
        foreach (var nodeData in _containerCache.DialogueNodeDatas) {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText, nodeData.Position, nodeData.SpeakingCharacterId, nodeData);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
            if(nodePorts.Count > 0) {
                var port = (Port)tempNode.outputContainer[0];
                DialogueNode.RemovePort(_targetGraphView, tempNode, port);
            }
            nodePorts.ForEach(x => {
                DialogueNode.AddChoicePort(_targetGraphView, tempNode, x.PortName);
            });
        }
        // Scene Nodes
        foreach (var nodeData in _containerCache.SceneNodeDatas) {
            var tempNode = _targetGraphView.CreateSceneNode(nodeData.Position, nodeData);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);
        }
        // Make Choice Nodes
        foreach (var nodeData in _containerCache.MakeChoiceNodeDatas) {
            var tempNode = _targetGraphView.CreateMakeChoiceNode(nodeData.Position, nodeData);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);
        }
        // Make Branch Nodes
        foreach (var nodeData in _containerCache.ChoiceBranchNodeDatas) {
            var tempNode = _targetGraphView.CreateChoiceBranchNode(nodeData.Position, nodeData);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);
        }
    }

    private void ConnectNodes() {
        for(var i = 0; i < Nodes.Count; i++) {
            var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
            for(var j=0; j < connections.Count; j++) {
                var targetNodeGuid = connections[j].TargetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0], connections[j]);

                Vector2 position = _containerCache.AllNodeDatas.First(x => x.Guid == targetNodeGuid).Position;
                targetNode.SetPosition(new Rect(
                    position,
                    BaseNode.defaultNodeSize
                    ));
            }
        }
    }

    private void LinkNodes(Port output, Port input, NodeLinkData data) {
        var tempEdge = new Edge {
            output = output,
            input = input
        };
        tempEdge.viewDataKey = data.Guid;
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

    private void ClearGraph() {
        Edges.ForEach(edge => _targetGraphView.RemoveElement(edge));
        foreach (var node in Nodes) {
            _targetGraphView.RemoveElement(node);
        }
    }
}
