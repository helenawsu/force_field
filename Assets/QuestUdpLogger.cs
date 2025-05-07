using UnityEngine;
using extOSC;

public class QuestOscLogger : MonoBehaviour
{
    [Tooltip("The Transform whose Y you want to stream")]
    public Transform cube;

    [Tooltip("Your Mac’s LAN IP (or use 255.255.255.255 for broadcast)")]
    public string remoteHost = "192.168.4.26";

    [Tooltip("Port matching your Max [udpreceive]")]
    public int remotePort = 6666;

    private OSCTransmitter _transmitter;

    void Start()
    {
        // Create and configure the extOSC transmitter
        _transmitter = gameObject.AddComponent<OSCTransmitter>();
        _transmitter.RemoteHost = remoteHost;
        _transmitter.RemotePort = remotePort;
    }

    void Update()
    {
        if (cube == null) return;

        // Build an OSC message at address /cube/height with one float
        var message = new OSCMessage("/cube/height");
        message.AddValue(OSCValue.Float(cube.position.y));

        // Send it off via extOSC
        _transmitter.Send(message);
        
        Debug.Log($"[extOSC] Sent /cube/height {cube.position.y:F3}");
    }
}
