using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class SceneSelectMenu : MonoBehaviour
{
    [SerializeField]
    private Button SceneSelectPrefab;
    [SerializeField]
    private GameObject ButtonGrid;
    private string[] files;
    DialogueContainer[] dialogues;

    // Start is called before the first frame update
    void Start()
    {
        dialogues = Resources.LoadAll<DialogueContainer>("Dialogue");
        dialogues.ToList().ForEach(dialogue => {
            string name = dialogue.name;
            Button button = GameObject.Instantiate(SceneSelectPrefab, ButtonGrid.transform);
            button.GetComponentInChildren<Text>().text = name;
            button.onClick.AddListener(delegate {
                GameManager.instance.LoadScene(dialogues[button.transform.GetSiblingIndex()]);
            });
        });
    }
}
