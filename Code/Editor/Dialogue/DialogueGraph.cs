using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "New Narrative";
    private string _filePath = "";
    private Label fileNameLabel;

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow() {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable() {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMiniMap();
        GenerateBlackboard();
        if(_filePath != "") {
            LoadGraphAssetViaFileName(_filePath, _fileName);
        } else {
            _filePath = "";
            _fileName = "New Narrative";
        }
        Undo.undoRedoEvent = (in UndoRedoInfo info) => {
            if (info.undoName.Contains("NodeUndo")) {
                string undoFilePath = EditorPrefs.GetString("LastDialogueSOChanged");
                //Quick reload of a dialogue node
                if (undoFilePath == _filePath && info.undoName.Contains(":")) {
                    string[] guids = info.undoName.Split(':');
                    for(int i = 1; i < guids.Count(); i++) {
                        _graphView.ReloadNode(guids[i]);
                    }
                } else if (undoFilePath != "") { // Reload entire file
                    FocusWindowIfItsOpen<DialogueGraph>();
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(undoFilePath);
                    LoadGraphAssetViaFileName(undoFilePath, fileName, true);
                    return;
                }else if (_filePath != "") {
                    FocusWindowIfItsOpen<DialogueGraph>();
                    LoadGraphAssetViaFileName(_filePath, _fileName, true);
                }
            }
        };
    }

    private void ConstructGraphView() {
        _graphView = new DialogueGraphView(this) { name = "Dialogue Graph", };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateBlackboard() {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection { title = "Exposed Properties" });

        var shortLineProperty = new BlackboardField { text = "Hello", typeText = "Short Line" };
        var container = new VisualElement();
        container.Add(shortLineProperty);
        blackboard.Add(container);

        blackboard.SetPosition(new Rect(220, 30, 400, 100));
        _graphView.Add(blackboard);
        _graphView.SetShortLineProperty(shortLineProperty);
    }

    private void GenerateMiniMap() {
        var miniMap = new MiniMap { anchored = true };
        miniMap.SetPosition(new Rect(10, 30, 200, 140));
        _graphView.Add(miniMap); 
    }

    private void GenerateToolbar() {
        var toolbar = new Toolbar();
        fileNameLabel = new Label(_fileName);
        toolbar.Add(fileNameLabel);

        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Save As..." });
        toolbar.Add(new Button(() => MakeNewFile()) { text = "New..." });

        rootVisualElement.Add(toolbar);
    }

    private void MakeNewFile() {
        if (EditorUtility.DisplayDialog("Make A New Graph", "Would you like to make a new graph?\n(Remember to save your current graph!)", "Yes", "No")) {
            // Clear graph
            List<DialogueNode> nodes = _graphView.nodes.ToList().Cast<DialogueNode>().ToList();
            List<Edge> edges = _graphView.edges.ToList();
            foreach (var node in nodes) {
                if (node.EntryPoint) continue;
                edges.Where(x => x.input.node == node).ToList().ForEach(edge => _graphView.RemoveElement(edge));

                _graphView.RemoveElement(node);
            }
            _fileName = "New Narrative";
            _filePath = "";
            fileNameLabel.text = _fileName;
            RequestDataOperation(false);
        }
    }

    private void RequestDataOperation(bool overwriteSave) {
        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (overwriteSave) {
            saveUtility.SaveGraph(_filePath);
        } else {
            saveUtility.SaveGraph();
        }
    }

    private void OnDisable() {
        rootVisualElement.Remove(_graphView);
    }

    public void LoadGraphAssetViaFileName(string assetPath, string fileName, bool isUndo = false) {
        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        _fileName = fileName;
        _filePath = assetPath;
        string assetPathShort = assetPath.Replace(".asset", "");
        //_graphView.ClearUndoRecords(isUndo);
        saveUtility.LoadGraph(assetPathShort);
        fileNameLabel.text = _fileName;
    }

    public void SetFilePath(string assetPath, string newFileName) {
        _fileName = newFileName;
        _filePath = assetPath;
        fileNameLabel.text = _fileName;
    }
}
