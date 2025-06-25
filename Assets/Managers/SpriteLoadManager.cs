using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpriteLoadManager : NetworkBehaviour
{
    [SerializeField] private string playersSpritePath;
    [SerializeField] private string basicCardsPath;
    [SerializeField] private string animatorControllersPath;
    [SerializeField] private string pathEnderSpritePath;

    public static SpriteLoadManager Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Sprite GetPathEnderSprite()
    {
        var temp = Resources.Load<Sprite>(pathEnderSpritePath + "chip");

        if (temp == null)
        {
            Debug.LogWarning("PathEnderSprite IS NULL!!!");
        }

        return Resources.Load<Sprite>(pathEnderSpritePath + "chip");
    }

    public Sprite GetBasicCardSprite(string cardName)
    {
        var fullpath = basicCardsPath + cardName;
        var sprite = Resources.Load<Sprite>(fullpath);
        Debug.LogWarning($"SpriteName: {sprite}");

        var texture2d = Resources.Load<Texture2D>(fullpath);
        Debug.LogWarning($"Texture2DName: {texture2d}");

        return sprite;
    }

    public RuntimeAnimatorController GetAnimatorController(string controllerName, bool isSprite = true)
    {
        if (isSprite)
            return Resources.Load<RuntimeAnimatorController>(animatorControllersPath + controllerName);
        else
            return Resources.Load<RuntimeAnimatorController>(animatorControllersPath + controllerName + "_RectT");
    }
}
