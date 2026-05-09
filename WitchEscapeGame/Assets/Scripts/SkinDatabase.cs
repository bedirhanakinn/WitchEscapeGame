using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "WitchEscape/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    [Tooltip("Drag every SkinData asset here. Order = order shown in shop.")]
    public List<SkinData> skins = new List<SkinData>();

    [Tooltip("Fallback skin if save data is missing or corrupt. Usually the starter skin.")]
    public SkinData defaultSkin;

    public SkinData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return skins.Find(s => s != null && s.skinId == id);
    }
}
