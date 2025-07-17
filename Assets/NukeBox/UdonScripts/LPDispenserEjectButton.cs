
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LPDispenserEjectButton : UdonSharpBehaviour
{
    public override void Interact()
    {
        transform.parent.GetComponent<LPDispenser>().DispenseLp();
    }

}
