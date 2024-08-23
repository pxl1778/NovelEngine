using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    
    public EventManager EventManager { get; private set; }
    public UIUtility UIUtility { get; private set; }
    public SaveManager SaveManager { get; private set; }
    public DialogueManager DialogueManager { get { return dialogueManager; } }
    private DialogueManager dialogueManager;

    public DialogueContainer NextScene = null;

    private void Awake()
    {
        if (instance == null)
        {
            //Starting up the game
            instance = this;
            EventManager = this.GetComponent<EventManager>();
            UIUtility = this.GetComponent<UIUtility>();
            SaveManager = this.GetComponent<SaveManager>();
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
