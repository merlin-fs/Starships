using System;
using UnityEngine;
using Game.Views.Stats;
using UnityEngine.AddressableAssets;
using Game.Model.Units;

public class TestHealthView : MonoBehaviour
{
    public Canvas Canvas;

    public AssetReferenceT<UnitConfig> Player;

    public StatViewComponent View;

    [SerializeField]
    Enum m_Stat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
