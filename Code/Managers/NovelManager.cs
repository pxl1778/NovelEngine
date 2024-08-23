using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NovelManager : MonoBehaviour
{
    public static NovelManager instance = null;
    
    public EventManager EventManager { get; private set; }
    public UIUtility UIUtility { get; private set; }
    public SaveManager SaveManager { get; private set; }
    public DialogueManager DialogueManager { get { return dialogueManager; } }
    private DialogueManager dialogueManager;

    public DialogueContainer NextScene = null;
    public List<string> CompletedDialogues { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            //Starting up the game
            instance = this;
            EventManager = this.GetComponent<EventManager>();
            UIUtility = this.GetComponent<UIUtility>();
            SaveManager = this.GetComponent<SaveManager>();
            CompletedDialogues = new List<string>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else if (instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    public void EndScene(DialogueContainer dialogueContainer) {
        if (!CompletedDialogues.Contains(dialogueContainer.Id)) {
            CompletedDialogues.Add(dialogueContainer.Id);
        }
        EventManager.EndNovelScene();
    }

    public bool CheckIfDialogueCompleted(DialogueContainer vnScene) {
        return CompletedDialogues.Contains(vnScene.Id);
    }

    public void LoadScene(DialogueContainer nextDialogue) {
        NextScene = nextDialogue;
        SceneManager.LoadScene("TestScene");

        //LoadingScreen loadingScreen = GameObject.Instantiate(LoadingPrefab).GetComponent<LoadingScreen>();
        //Tween transitionTween = loadingScreen.TransitionIn();
        //transitionTween.OnComplete(() => {
        //    AsyncOperation loadOperation = SceneManager.LoadSceneAsync(spawnPointSO.sceneToLoad.Name);
        //    loadingScreen.StartLoading(loadOperation);
        //});
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (dialogueManager == null && GameObject.Find("NovelCanvas") != null) {
            dialogueManager = GameObject.Find("NovelCanvas").GetComponent<DialogueManager>();
        }
    }
}
