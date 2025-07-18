using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class LPPlayer : UdonSharpBehaviour
{
    [SerializeField] private VRCAVProVideoPlayer videoPlayer;
    [SerializeField] private AudioSource speakerL;
    [SerializeField] private AudioSource speakerR;
    [SerializeField] private JCLogger jcLogger;
    [SerializeField] private TMP_Text lpPlayerOwnerText;
    [SerializeField] private Slider volumeSlider;

    // wait for videoPlayer loaded URL
    // private bool isChangingVideoUrl = false;
    
    // Variables for video sync
    [SerializeField] private int syncInterval = 10;
    private float lastSyncTime = 0;
    private VRCUrl currentUrl;
    private int currentVideoId;

    // Video Player Synced Variables
    [UdonSynced] private Vector2 syncedVideoTime;
    [UdonSynced] private VRCUrl syncedVideoUrl;
    [UdonSynced] private int syncedVideoId;

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
            int videoId = Random.Range(int.MinValue, int.MaxValue);

            RequestSerialization();

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeVideo), url, videoId);
        }
    }

    [NetworkCallable]
    public void ChangeVideo(VRCUrl videoUrl, int id) {
        jcLogger.Print("changeVideo");
        // if (currentUrl == null || videoUrl.Get() != currentUrl.Get()) {
            videoPlayer.LoadURL(videoUrl);
            videoPlayer.Stop();
            currentUrl = videoUrl;
            currentVideoId = id;
            // isChangingVideoUrl = true;
            jcLogger.Print($"Loading video URL: {videoUrl.Get()}");
        // }
    }

    void Update() {
        // -- Synchronize video playback time at regular intervals.
        if (Time.time > lastSyncTime + syncInterval && videoPlayer.IsPlaying) {
            if (Networking.IsOwner(gameObject)) {
                // SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(Sync), videoPlayer.GetTime());
                // lastSyncTime = Time.time;
                syncedVideoTime.x = (float)Networking.GetServerTimeInSeconds();
                syncedVideoTime.y = videoPlayer.GetTime();
                syncedVideoUrl = currentUrl;
                syncedVideoId = currentVideoId;
                RequestSerialization();
            } else {
                jcLogger.Print($"Attemped video sync");
                if (currentUrl != null && syncedVideoUrl != null && currentVideoId == syncedVideoId) {
                    Sync();
                }
            }
            lastSyncTime = Time.time;
        }

        // -- Synchronize video volume
        speakerL.volume = volumeSlider.value;
        speakerR.volume = volumeSlider.value;
        
        if (Networking.GetOwner(gameObject) == null) {
            if (Networking.IsMaster) {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
        }
        
        lpPlayerOwnerText.text = (Networking.IsOwner(gameObject)
            ? "Master"
            : "Not Master") + $"Vol : {volumeSlider.value}";
    }

    public void Sync() {
        float currentServerTime = (float)Networking.GetServerTimeInSeconds();
        float deltaServerTime = currentServerTime - syncedVideoTime.x;
        float globalVideoTime = syncedVideoTime.y + deltaServerTime;
        if (Mathf.Abs(videoPlayer.GetTime() - globalVideoTime) > 2f) {
            videoPlayer.SetTime(syncedVideoTime.y + deltaServerTime);
            jcLogger.Print($"synced: {syncedVideoTime.y} + {deltaServerTime} = {syncedVideoTime.y + deltaServerTime}");
        }
    }


    private void OnTriggerEnter(Collider other) {
        LP lp = other.GetComponent<LP>();
        if (lp != null && lp.GetLastestPickupPlayerId() == Networking.LocalPlayer.playerId) {
            meshRenderer.material.color = Color.red;
            InjectLp(lp.GetUrl(), lp.GetLastestPickupPlayerId());
        }
    }

    public override void OnVideoReady()
    {
        meshRenderer.material.color = new Color(1f, 0.5f, 0f);
        videoPlayer.Play();
        if (!Networking.IsOwner(gameObject) && currentVideoId == syncedVideoId) {
            Sync();
        }
        jcLogger.Print($"video is ready and playing, current time:{videoPlayer.GetTime()}");
    }


    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if(player.isLocal) {
            if (syncedVideoUrl != null) {
                ChangeVideo(syncedVideoUrl, syncedVideoId);
            }
        }
    }

    // for test
    public void GoTo50Sec() {
        videoPlayer.SetTime(50);
    }
}
