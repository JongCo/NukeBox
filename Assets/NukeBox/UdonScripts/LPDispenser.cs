
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class LPDispenser : UdonSharpBehaviour
{
    [SerializeField] private VRCUrlInputField inputField;
    [SerializeField] private GameObject lpPrefab;
    [SerializeField] private VRCObjectPool objectPool;

    public void DispenseLp() {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        Networking.SetOwner(Networking.LocalPlayer, objectPool.gameObject);

        // 여기에 LP판 뽑기
        GameObject lpObject = objectPool.TryToSpawn();
        Networking.SetOwner(Networking.LocalPlayer, lpObject);

        lpObject.GetComponent<LP>().SetUrl(inputField.GetUrl());
        
        lpObject.transform.position = this.transform.position + Vector3.up * 1;
    }
}
