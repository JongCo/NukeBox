
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LP : UdonSharpBehaviour
{
    [UdonSynced] private VRCUrl url; 
    [UdonSynced] private int lastestPickupPlayerId = -1;
    [SerializeField] private TMP_Text text;

    public void SetUrl(VRCUrl url) {
        if (Networking.IsOwner(gameObject)) {
            this.url = url;
            RequestSerialization();
        }
    }

    public VRCUrl GetUrl() {
        return url;
    }

    public override void OnPickup()
    {
        lastestPickupPlayerId = Networking.LocalPlayer.playerId;
        GetComponent<MeshRenderer>().material.color = Color.green;
        text.text = $"{lastestPickupPlayerId}";
        RequestSerialization();
    }

    public int GetLastestPickupPlayerId() {
        return lastestPickupPlayerId;
    }

    public override void OnDeserialization()
    {
        GetComponent<MeshRenderer>().material.color = Color.green;
        text.text = $"{lastestPickupPlayerId}";
    }


}
