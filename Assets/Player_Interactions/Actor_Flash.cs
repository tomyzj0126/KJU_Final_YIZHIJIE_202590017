using UnityEngine;

public class Actor_Flash : InterfaceBase_IItem
{
    [Header("Flashlight Settings")]
    public GameObject SpotLight;

    private void Start()
    {
        OnStopUse();
    }
    public override void OnUse()
    {
        base.OnUse();
        SpotLight.SetActive(true);
        // spotLight.enabled = true;
        Debug.Log($"플래시 켜짐 (On)");
        // Debug.Log($"플래시 {(spotLight.enabled ? "켜짐" : "꺼짐")}");
    }

    public override void OnStopUse()
    {
        base.OnStopUse();
        SpotLight.SetActive(false);
        Debug.Log($"플래시 꺼짐 (Off)");
        // spotLight.enabled = false;
        // Debug.Log($"플래시 {(spotLight.enabled ? "켜짐" : "꺼짐")}");
    }
}