using UnityEngine;
using extOSC;

[RequireComponent(typeof(OSCTransmitter))]
public class dOscStreamer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag in the ForceField controller that spawned the cubes")]
    public ForceField forceField;

    [Header("OSC Settings")]
    [Tooltip("The IP to send to (or broadcast)")]
    public string remoteHost = "192.168.4.26";
    [Tooltip("The port your listener is on")]
    public int remotePort  = 7400;

    private OSCTransmitter _transmitter;

    void Awake()
    {
        _transmitter = GetComponent<OSCTransmitter>();
        _transmitter.RemoteHost = remoteHost;
        _transmitter.RemotePort = remotePort;
    }

    void Update()
    {
        if (forceField == null || forceField.bodies == null) return;
    
        // Create one message for all heights
        var msg = new OSCMessage("/cubes/heights");
    
        for (int i = 0; i < forceField.bodies.Length; i++)
        {
            float y = forceField.bodies[i].position.y;
            msg.AddValue(OSCValue.Float(y));
        }
    
        _transmitter.Send(msg);
    }
}
