
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class VolumeSlider : UdonSharpBehaviour
{
    private Slider slider;

    [UdonSynced] private float volume;

    private bool isUpdatingFromNetwork = false;

    void Start() {
        slider = GetComponent<Slider>();
    }

    public void OnValueChanged() {
        if (isUpdatingFromNetwork) return;
        if (!Networking.IsOwner(gameObject)) {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        volume = slider.value;
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        isUpdatingFromNetwork = true;
        slider.value = volume;
        isUpdatingFromNetwork = false;
    }
}
