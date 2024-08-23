using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HistoryMenu : Graphic
{
    [SerializeField]
    private GameObject HistoryGroupPrefab;
    [SerializeField]
    private GameObject ScrollParent;
    [SerializeField]
    private DialogueManager DialogueManager;
    [SerializeField]
    private ScrollRect ScrollRect;
    [SerializeField]
    private Image BlockerPanel;

    private int previousLine = 0;
    private float originalY;

    private void OnEnable()
    {
        ScrollRect.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
        BlockerPanel.DOFade(0.5f, 0.2f);
        float endYPos = this.rectTransform.anchoredPosition.y;//y pos should be 90.0f in case things get messy
        this.rectTransform.anchoredPosition = new Vector2(this.rectTransform.anchoredPosition.x, endYPos + 50);
        this.rectTransform.DOAnchorPosY(endYPos, 0.2f);

        List<DialogueNodeData> historyList = NovelManager.instance.DialogueManager.GetHistory();
        for(int i = previousLine; i < historyList.Count; i++) {
            GameObject group = GameObject.Instantiate<GameObject>(HistoryGroupPrefab, ScrollParent.transform);
            Text[] texts = group.GetComponentsInChildren<Text>();
            Text nameText = group.GetComponentsInChildren<Text>()[0];
            Text dialogueText = group.GetComponentsInChildren<Text>()[1];
            nameText.text = NovelData.instance.GetCharacterInfoSO(historyList[i].SpeakingCharacterId).characterName;
            dialogueText.text = historyList[i].DialogueText;
        }
        previousLine = historyList.Count;

        ScrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        ScrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        ScrollRect.verticalNormalizedPosition = 0;
    }

    void Start()
    {
        originalY = this.rectTransform.anchoredPosition.y - 50;
    }

    public void BackButton()
    {
        ScrollRect.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        BlockerPanel.DOFade(0.0f, 0.2f);
        float endYPos = this.rectTransform.anchoredPosition.y-50;
        this.rectTransform.DOAnchorPosY(endYPos, 0.2f).OnComplete(() =>
        {
            this.rectTransform.anchoredPosition = new Vector2(this.rectTransform.anchoredPosition.x, originalY);
            this.gameObject.SetActive(false);
        });
    }
}
