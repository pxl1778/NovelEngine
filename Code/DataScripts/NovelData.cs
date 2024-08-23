using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public enum SpecialAction { Move, Flip }

[CreateAssetMenu(fileName = "NovelDataSO", menuName = "ScriptableObjects/NovelDataSO")]
public class NovelData : ScriptableObject {
    [SerializeField]
    private SerializableDictionary<string, CharacterInfoSO> CharacterList = new SerializableDictionary<string, CharacterInfoSO>();
    private static string assetName => nameof(NovelData) + "SO";
    private static NovelData s_instance;
    public static NovelData instance {
        get {
            if (s_instance != null) return s_instance;
#if UNITY_EDITOR
            string[] assetPaths = AssetDatabase.FindAssets(assetName);
            string fullPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
            s_instance = (NovelData)AssetDatabase.LoadAssetAtPath(fullPath, typeof(NovelData));
            if (s_instance != null) return s_instance;
#else
            UnityEngine.Object[] novelDatas = Resources.LoadAll("", typeof(NovelData));
            s_instance = (NovelData)novelDatas[0];
            if (s_instance != null) return s_instance;
#endif
            return null;
        }
    }

    public CharacterInfoSO GetCharacterInfoSO(string characterString) {
        if (CharacterList.ContainsKey(characterString)) {
            return CharacterList[characterString];
        } else {
            if(characterString != "") {
                Debug.LogWarning("Character Id '" + characterString + "' does not exist ");
            }
            return null;
        }
    }

    public IEnumerable<string> GetCharacterIds() {
        return CharacterList.Keys;
    }
}