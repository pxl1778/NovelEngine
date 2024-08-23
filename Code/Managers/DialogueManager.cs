using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour, IPointerClickHandler {
    private DialogueContainer dialogue;
    [SerializeField]
    private DialogueContainer debugDialogue;
    [SerializeField]
    protected GameObject BackgroundsParent;
    [SerializeField]
    protected GameObject SpritesParent;
    [SerializeField]
    protected GameObject HitBox;
    [SerializeField]
    protected GameObject BackgroundPrefab;
    [SerializeField]
    protected GameObject SpritesPrefab;
    [SerializeField]
    protected DialogueBox DialogueBox;
    [SerializeField]
    protected Image Dimmer;
    [SerializeField]
    protected Image FadeImage;
    [SerializeField]
    protected AudioSource MusicTrack;
    [SerializeField]
    protected AudioSource SoundEffect;
    [SerializeField]
    protected CanvasGroup NovelCanvasGroup;
    [SerializeField]
    protected ChoiceList ChoicesList;
    [SerializeField]
    protected CanvasGroup SidePanelsGroup;
    [SerializeField]
    protected CanvasGroup MaskGroup;

    [Header("Settings")]
    [SerializeField]
    private bool beginOnStart = true;

    private float timeScale = 1.0f;
    private float delayTimer = 0.0f;
    public float delayTimerMax = 0.2f;
    public bool Active { get { return active; } set { active = value; } }
    private bool active = false;
    public bool MoveOn { get { return moveOn; } set { moveOn = value; } }
    private bool moveOn = false;
    private bool paused = false;
    private bool choosing = false;
    private Sequence tweenSequence;
    private Image currentBackground;
    private DialogueNodeData currentLine;
    private Dictionary<string, AnimatedSprite> characterDictionary = new Dictionary<string, AnimatedSprite>();
    private List<string> choices = new List<string>();
    private SaveObject currentSave;
    private float[] spritePositions = { -1500.0f, -600.0f, -350.0f, 0.0f, 350.0f, 600.0f, 1500.0f };
    private List<DialogueNodeData> historyList = new List<DialogueNodeData>();

    // Start is called before the first frame update
    void Start() {
        NovelCanvasGroup.alpha = 0;
        currentSave = NovelManager.instance.SaveManager.GetLoaded();
        if (currentSave != null) {
            //TODO use scene name to get text asset
            //DialogueFileName = currentSave.sceneName;
            choices = currentSave.choices.ToList();
        }
        NovelManager.instance.EventManager.onPause.AddListener(() => { paused = true; tweenSequence?.Pause(); timeScale = 0.0f; });
        NovelManager.instance.EventManager.onUnpause.AddListener(() => { paused = false; tweenSequence?.TogglePause(); timeScale = 1.0f; });


        // LoadDialogue(DebugDialogueTextAsset);
        dialogue = NovelManager.instance.NextScene;
        // Debug
        if (dialogue == null) {
            dialogue = debugDialogue;
        }
        if(!beginOnStart) { return; }
        NodeLinkData nextLink = dialogue.NodeLinks.Where(link => link.BaseNodeGuid == dialogue.StartingNodeGUID).FirstOrDefault();
        DialogueNodeData nextNode = dialogue.DialogueNodeDatas.Where(node => node.Guid == nextLink.TargetNodeGuid).FirstOrDefault();
        currentLine = nextNode;
        delayTimer = 2.0f;
        historyList.Add(nextNode);
        //ContinueDialogue(true); 

        //Old Check Done Loading 
        NovelCanvasGroup.DOFade(1.0f, 0.2f);
        NovelCanvasGroup.blocksRaycasts = true;
        NovelCanvasGroup.interactable = true;
        MaskGroup.alpha = 1;
        if (currentSave != null) {
            //CatchUp();
        } else {
            if (currentLine.Background != null) {
                //if (lines[0].Background == "None") {
                //    currentBackground.DOFade(0, 0);
                //} else {
                if (currentBackground == null) {
                    currentBackground = GameObject.Instantiate(BackgroundPrefab, BackgroundsParent.transform).GetComponent<Image>();
                }
                currentBackground.sprite = currentLine.Background;
                //}
            }
            Dimmer.color = new Color(0, 0, 0, 0);
            Dimmer.DOFade(0.8f, 1.0f);
            FadeImage.color = new Color(0, 0, 0, 1);
            FadeIn().onComplete = () => {
                DialogueBox.FadeBoxIn().onComplete = () => { ContinueDialogue(true); };
            };
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Jump") && !paused) {
            HandleInput();
        }
        if (delayTimer > 0) {
            delayTimer -= Time.deltaTime;
        }
        if (active) {
            bool shouldEndLine = DialogueBox.TextAnimation(timeScale);
            if (shouldEndLine) {
                EndLine();
            }
        }
    }

    public void LoadDialogue(DialogueContainer vnScene) {
        dialogue = vnScene;
        NodeLinkData nextLink = dialogue.NodeLinks.Where(link => link.BaseNodeGuid == dialogue.StartingNodeGUID).FirstOrDefault();
        DialogueNodeData nextNode = dialogue.DialogueNodeDatas.Where(node => node.Guid == nextLink.TargetNodeGuid).FirstOrDefault();
        currentLine = nextNode;
        delayTimer = 2.0f;
        historyList.Add(nextNode);

        NovelCanvasGroup.blocksRaycasts = true;
        NovelCanvasGroup.interactable = true;
        NovelCanvasGroup.DOFade(1.0f, 0.2f);
        if (currentLine.Background != null) {
            if (currentBackground == null) {
                currentBackground = GameObject.Instantiate(BackgroundPrefab, BackgroundsParent.transform).GetComponent<Image>();
            }
            currentBackground.sprite = currentLine.Background;
        }
        Dimmer.color = new Color(0, 0, 0, 0);
        Dimmer.DOFade(0.8f, 1.0f);
        FadeImage.color = new Color(0, 0, 0, 1);
        FadeIn().onComplete = () => {
            DialogueBox.FadeBoxIn().onComplete = () => { ContinueDialogue(true); };
        };
    }

    public void OnPointerClick(PointerEventData e) {
        if (e.pointerCurrentRaycast.gameObject == HitBox && !paused) {
            HandleInput();
        }
    }

    BaseNodeData GetNextNode(NodeLinkData linkChoice = null) {
        NodeLinkData nextLink = linkChoice;
        if (linkChoice == null) {
            nextLink = dialogue.NodeLinks.Where(link => link.BaseNodeGuid == currentLine.Guid).FirstOrDefault();
        }
        if (nextLink == null) return null;
        BaseNodeData nextNode = dialogue.AllNodeDatas.Where(node => node.Guid == nextLink.TargetNodeGuid).FirstOrDefault();
        return nextNode;
    }

    void HandleInput() {
        if (currentLine != null && delayTimer <= 0 && !choosing) {
            if (moveOn) {
                if (GetNextNode() != null) {
                    //Next line
                    if (currentLine.SpeakingCharacterId != "" && characterDictionary.ContainsKey(currentLine.SpeakingCharacterId)) {
                        characterDictionary[currentLine.SpeakingCharacterId].ToggleTalking(false);
                    }
                    ContinueDialogue();
                    moveOn = false;
                    delayTimer = delayTimerMax;
                } else {
                    SceneEnd();
                }
            } else if (!choosing) {
                //Skip animation.
                tweenSequence?.Complete();
                moveOn = true;
                DialogueBox.SkipAnimation();
                EndLine();
            }
        }
    }

    void CatchUp() {
        //currentLine = currentSave.line;
        //string catchupBackground = "";
        //Dictionary<string, float> currentSprites = new Dictionary<string, float>();
        //for (int i = 0; i <= currentLine; i++) {
        //    catchupBackground = lines[i].Background != "" ? lines[i].Background : catchupBackground;
        //    if (lines[i].FadeInList != null) {
        //        foreach (string sprite in lines[i].FadeInList) {
        //            string characterName = sprite.Split(' ')[0].Split('_')[0];
        //            string matchingKey = "";
        //            foreach (string spriteKey in currentSprites.Keys) {
        //                if (spriteKey.Split('_')[0] == characterName) {
        //                    matchingKey = spriteKey;
        //                    break;
        //                }
        //            }
        //            float position = 3;
        //            if (matchingKey == "") {
        //                if (sprite.Split(' ').Length > 1) {
        //                    position = float.Parse(sprite.Split(' ')[1]);
        //                }
        //            } else {
        //                position = currentSprites[matchingKey];
        //                currentSprites.Remove(matchingKey);
        //            }
        //            currentSprites.Add(sprite.Split(' ')[0], position);
        //        }
        //    }
        //    if (lines[i].FadeOutList != null) {
        //        foreach (string sprite in lines[i].FadeOutList) {
        //            string characterName = sprite.Split(' ')[0].Split('_')[0];
        //            string matchingKey = "";
        //            foreach (string spriteKey in currentSprites.Keys) {
        //                if (spriteKey.Split('_')[0] == characterName) {
        //                    matchingKey = spriteKey;
        //                    break;
        //                }
        //            }
        //            currentSprites.Remove(matchingKey);
        //        }
        //    }
        //}
        //DisplayBackground(catchupBackground);
        //ShowCharacters(currentSprites);
        //DialogueBox.FadeBoxIn();
        //moveOn = true;
        //DialogueBox.SetCompleteLine(lines[currentLine]);
    }

    float GetSpritePosition(float pos) {
        float newPos = 0.0f;
        int baseIndex = Mathf.FloorToInt(pos);
        if (baseIndex < spritePositions.Length - 1) {
            newPos = Mathf.Lerp(spritePositions[baseIndex], spritePositions[baseIndex + 1], pos % 1.0f);
        } else {
            newPos = spritePositions[baseIndex - 1];
        }
        return newPos;
    }

    void ShowCharacters(Dictionary<string, float> characters) {
        //foreach (string key in characters.Keys) {
        //    string characterName = key.Split('_')[0];
        //    string animName = key.Split('_')[1];
        //    GameObject characterObject = GameObject.Instantiate(prefabDictionary[key], SpritesParent.transform);
        //    Image currentSprite = characterObject.GetComponent<Image>();
        //    characterObject.GetComponent<AnimatedSprite>().PlayAnimation(animName);
        //    currentSprite.rectTransform.anchoredPosition = new Vector2(GetSpritePosition(characters[key]), 0);
        //    characterDictionary[key.Split('_')[0]] = characterObject.GetComponent<AnimatedSprite>();
        //}
    }

    void DisplayBackground(Sprite newBackground) {
        currentBackground = GameObject.Instantiate(BackgroundPrefab, BackgroundsParent.transform).GetComponent<Image>();
        currentBackground.sprite = newBackground;
        currentBackground.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        SidePanelsGroup.DOFade(1, 2.0f);
    }

    void ContinueDialogue(bool firstLine = false, NodeLinkData linkChoice = null) {
        if (tweenSequence != null) { tweenSequence.Kill(true); }
        tweenSequence = DOTween.Sequence();
        if (!firstLine && currentLine.ScreenFadeOut) {
            tweenSequence.Append(FadeOut());
        }
        if (!firstLine) {
            BaseNodeData nextNode = GetNextNode(linkChoice);
            if(nextNode is SceneNodeData nextSceneNode) {
                SceneEnd(nextSceneNode.NextScene);
                return;
            } else {
                currentLine = nextNode as DialogueNodeData;
                historyList.Add(currentLine);
            }
        }
        //DialogueBox.SetLine(currentLine);
        // Doesn't have the correct requirement key saved
        //TODO
        //if (currentLine.RequirementKey != "" && !choices.Contains(current.RequirementKey)) {
        //    ContinueDialogue();
        //    return;
        //}
        if (currentLine.ScreenFadeIn) {
            tweenSequence.Append(FadeIn());
        }
        if (currentLine.Background != null && !firstLine) {
            //change backgroundimage image
            //new image on top, .DOFade image on top
            Image prevBackground = currentBackground;
            DisplayBackground(currentLine.Background);
            tweenSequence.Append(currentBackground.DOFade(1, 2.0f).OnComplete(() => {
                if(prevBackground != null) {
                    GameObject.Destroy(prevBackground.gameObject);
                }
            }));
        }
        if (currentLine.StopMusic) {
            //music Stop
            Tween musicStopTween = Dimmer.DOFade(Dimmer.color.a, 0.1f);
            musicStopTween.onComplete = () => {
                MusicTrack.DOFade(0.0f, 1.0f);
            };
            tweenSequence.Append(musicStopTween);
        }
        if (currentLine.Music != null) {
            //music
            Tween musicTween = Dimmer.DOFade(Dimmer.color.a, 0.1f);
            musicTween.onComplete = () => {
                Debug.Log("ChangeMusic: " + currentLine.Music.name);
                    MusicTrack.Stop();
                    MusicTrack.clip = currentLine.Music;
                    MusicTrack.Play();
                    MusicTrack.DOFade(1.0f, 1.0f);
            };
            tweenSequence.Append(musicTween);
        }
        Tween soundTween = Dimmer.DOFade(Dimmer.color.a, 0.1f);
        soundTween.onComplete = () => {
            if (currentLine.Sound != null) {
                Debug.Log("ChangeSound: " + currentLine.Sound.name);
                SoundEffect.clip = currentLine.Sound;
                SoundEffect.Play();
            }
            DialogueBox.SetTextBoxImage(currentLine.ExclaimTextBox);
        };
        tweenSequence.Append(soundTween);
        // Fade In List
        if (currentLine.FadeInList != null && currentLine.FadeInList.Count > 0) {
            currentLine.FadeInList.Keys.ToList().ForEach(character => {
                if (!characterDictionary.ContainsKey(character)) {
                    CharacterInfoSO characterInfo = NovelData.instance.GetCharacterInfoSO(character);
                    AnimatedSprite currentCharacter = GameObject.Instantiate(characterInfo.characterPrefab, SpritesParent.transform).GetComponent<AnimatedSprite>();
                    Image currentImage = currentCharacter.GetComponent<Image>();
                    currentImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);                        
                    currentImage.rectTransform.anchoredPosition = new Vector2(GetSpritePosition(currentLine.FadeInList[character].position), 0);
                    if (currentLine.FadeInList[character].flipped) {
                        currentImage.rectTransform.DOScaleX(currentImage.rectTransform.localScale.x * -1, 0.1f);
                    }
                    characterDictionary[character] = currentCharacter.GetComponent<AnimatedSprite>();
                    currentCharacter.PlayAnimation(currentLine.FadeInList[character].animation);
                    tweenSequence.Join(currentImage.DOFade(1.0f, 1.0f));
                }
            });
        }
        AddSpecialActions();
        active = false;
        Tween nameTween = Dimmer.DOFade(Dimmer.color.a, 0.1f);
        nameTween.onComplete = () => {
            DialogueBox.NextLine(currentLine);
            if (currentLine.SpeakingCharacterId != "" && characterDictionary.ContainsKey(currentLine.SpeakingCharacterId)) {
                characterDictionary[currentLine.SpeakingCharacterId].ToggleTalking(DialogueBox.isLineComplete());
                characterDictionary[currentLine.SpeakingCharacterId].transform.SetAsLastSibling();
            }
            Canvas.ForceUpdateCanvases();
        };
        Tween reactionTween = Dimmer.DOFade(Dimmer.color.a, 0.1f);
        reactionTween.onComplete = () => {
            if (currentLine.ReactionList != null && currentLine.ReactionList.Count > 0) {
                foreach (ReactionListItemData effect in currentLine.ReactionList) {
                    characterDictionary[effect.characterId].PlayReaction(effect.reaction);
                }
            }
        };
        if (!firstLine && currentLine.FadeOutList != null && currentLine.FadeOutList.Count > 0) {
            foreach (string character in currentLine.FadeOutList) {
                if (characterDictionary.ContainsKey(character)) {
                    tweenSequence.Join(characterDictionary[character].GetComponent<Image>().DOFade(0.0f, 1.0f).OnComplete(() => {
                        GameObject.Destroy(characterDictionary[character].gameObject);
                        characterDictionary.Remove(character);
                    }));
                } else {
                    Debug.LogWarning("Character Name not found in Fade Out List on line " + currentLine + 2);
                }
                //find character image and start fade out tween
            }
        }
        tweenSequence.Append(nameTween);
        tweenSequence.Append(reactionTween);
        // Change Animation List
        if (currentLine.ChangeAnimationList != null && currentLine.ChangeAnimationList.Count > 0) {
            currentLine.ChangeAnimationList.Keys.ToList().ForEach(character => {
                if (characterDictionary.ContainsKey(character)) {
                    if(currentLine.SpeakingCharacterId == character) {
                        characterDictionary[character].QueueAnimation(currentLine.ChangeAnimationList[character]);
                    } else {
                        Tween changeAnimTween = Dimmer.DOFade(Dimmer.color.a, 0.1f);
                        changeAnimTween.onComplete = () => {
                            characterDictionary[character].PlayAnimation(currentLine.ChangeAnimationList[character]);
                        };
                        tweenSequence.Append(changeAnimTween);
                    }
                }
            });
        }
        DialogueBox.ResetText();
    }

    void AddSpecialActions() {
        if (currentLine.SpecialActionList != null && currentLine.SpecialActionList.Count > 0) {
            foreach (ActionListItemData action in currentLine.SpecialActionList) {
                switch (action.actionType) {
                    case SpecialAction.Flip:
                        tweenSequence.Join(characterDictionary[action.parameterOne].GetComponent<Image>().rectTransform.DOScaleX(characterDictionary[action.parameterOne].GetComponent<Image>().rectTransform.localScale.x * -1, 0.1f));
                        break;
                    case SpecialAction.Move:
                        tweenSequence.Join(characterDictionary[action.parameterOne].GetComponent<Image>().rectTransform.DOAnchorPosX(GetSpritePosition(action.parameterTwo), 1.0f));
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void MakeChoice(NodeLinkData linkChoice) {
        moveOn = false;
        choosing = false;
        ContinueDialogue(false, linkChoice);
    }

    void EndLine() {
        if (currentLine.SpeakingCharacterId != "" && characterDictionary.ContainsKey(currentLine.SpeakingCharacterId)) {
            characterDictionary[currentLine.SpeakingCharacterId].ToggleTalking(false);
        }
        //Populate Choices
        var choiceLinks = dialogue.NodeLinks.Where(link => link.BaseNodeGuid == currentLine.Guid).ToList();
        if (choiceLinks.Count > 1) {
            choosing = true;
            ChoicesList.SetData(choiceLinks);
        } else {
            moveOn = true;
        }
    }

    void SceneEnd(DialogueContainer nextContainer = null) {
        active = false;
        DialogueBox.ResetBox();
        DialogueBox.FadeBoxOut().onComplete = () => {
            MusicTrack.DOFade(0.0f, 1.0f);
            FadeOut().onComplete = () => {
                MaskGroup.DOFade(0.0f, 0.4f).onComplete = () => {
                    Reset();
                    // Next Scene
                    if (nextContainer) {
                        NovelManager.instance.EndScene(dialogue);
                        NovelManager.instance.LoadScene(nextContainer);
                    } else {
                        NovelCanvasGroup.DOFade(0, 0.2f).onComplete = () => {
                            NovelCanvasGroup.interactable = false;
                            NovelCanvasGroup.blocksRaycasts = false;
                            NovelManager.instance.EndScene(dialogue);
                        };
                    }
                };
            };
        };
    }

    void Reset() {
        currentLine = null;
        moveOn = false;
        choosing = false;
        DialogueBox.ResetBox();
        foreach (string k in characterDictionary.Keys) {
            Destroy(characterDictionary[k].gameObject);
        }
        characterDictionary = new Dictionary<string, AnimatedSprite>();
        currentSave = null;
        NovelManager.instance.EventManager.ResetVN();
    }

    public List<DialogueNodeData> GetHistory() {
        return historyList;
    }

    Tween FadeIn() {
        return FadeImage.DOFade(0, 1);
    }

    Tween FadeOut() {
        return FadeImage.DOFade(1, 1);
    }
}
