﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AnimatedSprite : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    Reactions reactionContainer;
    float blinkTimer = 3.0f;
    private string nextAnim = "";

    // Start is called before the first frame update
    void Start()
    {
        Addressables.InitializeAsync();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    animator.SetBool("Talking", false);
        //    animator.SetTrigger("Frown");
        //}
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    animator.SetBool("Talking", true);
        //}
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    animator.SetBool("Talking", false);
        //    blinkTimer = 3.0f;
        //}
        if(animator.GetBool("Talking") == false)
        {
            blinkTimer -= Time.deltaTime;
            if(blinkTimer <= 0)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(state.fullPathHash, -1, 0f);
                blinkTimer = (Random.value * 4.0f) + 3.0f;
            }
        }
    }

    //public void SetData(string pCharacterName, string pSpawnAnim = null)
    //{
    //    AsyncOperationHandle<Animator> animatorHandle = Addressables.LoadAssetAsync<Animator>("Assets/TestingFolder/" + pCharacterName + "/" + pCharacterName + "_Animator.controller");
    //    animatorHandle.Completed += AnimatorLoaded;
    //    spawnAnim = pSpawnAnim;
    //}

    //void AnimatorLoaded(AsyncOperationHandle<Animator> handleToCheck)
    //{
    //    if (handleToCheck.Status == AsyncOperationStatus.Succeeded)
    //    {
    //        Animator animatorArray = handleToCheck.Result;
    //        animator.runtimeAnimatorController = animatorArray.runtimeAnimatorController;
    //        if(spawnAnim != null)
    //        {
    //            PlayAnimation(spawnAnim);
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Issue with Loading Animator: " + handleToCheck.Status);
    //    }
    //}

    public void PlayReaction(GameObject reactionObject) {
        Instantiate(reactionObject, reactionContainer.transform);
    }

    public void ToggleTalking(bool isTalking)
    {
        if(nextAnim != "") {
            PlayAnimation(nextAnim);
            nextAnim = "";
        }
        animator.SetBool("Talking", isTalking);
    }

    public void PlayAnimation(string pAnimName)
    {
        bool exists = false;
        foreach(AnimatorControllerParameter param in animator.parameters)
        {
            if(param.name == pAnimName)
            {
                animator.SetTrigger(pAnimName);
                exists = true;
                break;
            }
        }
        if (!exists)
        {
            Debug.Log("Animation state doesn't exist: " + pAnimName);
        }
    }

    public void QueueAnimation(string pAnimName) {
        bool exists = false;
        foreach (AnimatorControllerParameter param in animator.parameters) {
            if (param.name == pAnimName) {
                nextAnim = pAnimName;
                exists = true;
                break;
            }
        }
        if (!exists) {
            Debug.Log("Animation state doesn't exist: " + pAnimName);
        }
    }
}
