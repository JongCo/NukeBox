using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class LPPlayer : UdonSharpBehaviour
{
    [SerializeField] private VRCAVProVideoPlayer videoPlayer;
    [SerializeField] private JCLogger jcLogger;
    [SerializeField] private TMP_Text lpPlayerOwnerText;

    // wait for videoPlayer loaded URL
    private bool isChangingVideoUrl = true;
    
    // Variables for video sync
    // TODO : 영상재생시간을 관리하는 udon synced된 변수 만들고
    // TODO : 해당 변수를 통해 다른사람이 영상을 sync할 수 있도록 처리
    [SerializeField] private int syncInterval = 10;
    private float lastSyncTime = 0;
    [UdonSynced] private Vector2 syncedVideoTime;
    [UdonSynced] private VRCUrl syncedVideoUrl;
    private string currentUrl;

    // for Test
    private MeshRenderer meshRenderer;

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void InjectLp(VRCUrl url, int playerId) {
        VRCPlayerApi currentPlayer = VRCPlayerApi.GetPlayerById(playerId);
        meshRenderer.material.color = Color.black;
        if (currentPlayer.playerId == Networking.LocalPlayer.playerId) {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            meshRenderer.material.color = Color.cyan;

            syncedVideoTime.x = (float)Networking.GetServerTimeInSeconds();
            syncedVideoTime.y = 0;
            syncedVideoUrl = url;
            RequestSerialization();

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeVideo), url);
        }
    }

    [NetworkCallable]
    public void ChangeVideo(VRCUrl videoUrl) {
        jcLogger.Print("changeVideo");
        if (videoUrl.Get() != currentUrl) {
            videoPlayer.LoadURL(videoUrl);
            videoPlayer.Stop();
            currentUrl = videoUrl.Get();
            isChangingVideoUrl = true;
        }
    }

    void Update() {
        // -- Check if the vidoe URL is ready, then start playback -- 
        if (videoPlayer.IsReady && isChangingVideoUrl) {
            meshRenderer.material.color = new Color(1f, 0.5f, 0f);
            videoPlayer.Play();
            isChangingVideoUrl = false;
            if (!Networking.IsOwner(gameObject) && currentUrl == syncedVideoUrl.Get()) {
                Sync();
            }
            jcLogger.Print("url is ready and play");
        }

        if (Time.time > lastSyncTime + syncInterval && videoPlayer.IsPlaying) {
            if (Networking.IsOwner(gameObject)) {
                // SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(Sync), videoPlayer.GetTime());
                // lastSyncTime = Time.time;
                syncedVideoTime.x = (float)Networking.GetServerTimeInSeconds();
                syncedVideoTime.y = videoPlayer.GetTime();
                RequestSerialization();
            } else {
                jcLogger.Print($"Attemped video sync");
                if (currentUrl == syncedVideoUrl.Get()) {
                    Sync();
                }
            }
            lastSyncTime = Time.time;
        }
        
        if (Networking.GetOwner(gameObject) == null) {
            if (Networking.IsMaster) {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
        }
        
        lpPlayerOwnerText.text = Networking.IsOwner(gameObject)
            ? "Master"
            : "Not Master";
    }

    public void Sync() {
        float currentServerTime = (float)Networking.GetServerTimeInSeconds();
        float deltaServerTime = currentServerTime - syncedVideoTime.x;
        videoPlayer.SetTime(syncedVideoTime.y + deltaServerTime);
        jcLogger.Print($"synced: {syncedVideoTime.y} + {deltaServerTime} = {syncedVideoTime.y + deltaServerTime}");
    }


    private void OnTriggerEnter(Collider other) {
        LP lp = other.GetComponent<LP>();
        if (lp != null && lp.GetLastestPickupPlayerId() == Networking.LocalPlayer.playerId) {
            meshRenderer.material.color = Color.red;
            InjectLp(lp.GetUrl(), lp.GetLastestPickupPlayerId());
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if(player.isLocal) {
            if (syncedVideoUrl != null) {
                ChangeVideo(syncedVideoUrl);
            }
        }
    }


    // for test
    public void GoTo50Sec() {
        videoPlayer.SetTime(50);
    }
}
