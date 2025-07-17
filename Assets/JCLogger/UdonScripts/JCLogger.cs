
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class JCLogger : UdonSharpBehaviour
{
    [SerializeField] private TMP_Text loggerText;

    public void Print(string message) {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PrintNetworkHandler), 
            $"{Networking.LocalPlayer.displayName}({Networking.LocalPlayer.playerId}) : {message}"             
        );
    }

    [NetworkCallable]
    public void PrintNetworkHandler(string message) {
        loggerText.text = loggerText.text + "\n" + message;
    }
}
