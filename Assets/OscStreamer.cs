using UnityEngine;
using extOSC;

[RequireComponent(typeof(OSCTransmitter))]
public class dOscStreamer : MonoBehaviour
{
    [Header("Reference to your spawner")]
    public ForceField forceField;

    [Header("OSC Settings")]
    public string remoteHost = "192.168.4.26";
    public int    remotePort = 7400;

    private OSCTransmitter _transmitter;

    void Awake()
    {
        _transmitter = GetComponent<OSCTransmitter>();
        _transmitter.RemoteHost = remoteHost;
        _transmitter.RemotePort = remotePort;
    }

    void Start()
    {
        // Hook up touch‐handlers to each cube, with persistent IDs
        for (int i = 0; i < forceField.bodies.Length; i++)
        {
            var go      = forceField.bodies[i].gameObject;
            var handler = go.AddComponent<CubeTouchHandler>();
            handler.Init(i, this);
        }
    }

    void Update()
    {
        // Single OSC message: interleaved [id, height] pairs
        var msg = new OSCMessage("/cubes/withIDs");

        for (int i = 0; i < forceField.bodies.Length; i++)
        {
            float y = forceField.bodies[i].position.y;
            msg.AddValue(OSCValue.Int(i));    // cube ID
            msg.AddValue(OSCValue.Float(y));  // cube height
        }

        _transmitter.Send(msg);
    }

    // Called by CubeTouchHandler on enter/exit
    internal void SendTouchEvent(int cubeId, bool isExit)
    {
        var address = isExit ? "/cube/leave" : "/cube/enter";
        var msg     = new OSCMessage(address);
        msg.AddValue(OSCValue.Int(cubeId));
        _transmitter.Send(msg);

        Debug.Log($"[OSC] {address} id={cubeId}");
    }

    // -------------------------
    // Per-cube touch handler
    // -------------------------
    private class CubeTouchHandler : MonoBehaviour
    {
        int           _id;
        dOscStreamer _parent;

        public void Init(int id, dOscStreamer parent)
        {
            _id     = id;     // persistent ID on this component
            _parent = parent;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hand"))
                _parent.SendTouchEvent(_id, isExit: false);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Hand"))
                _parent.SendTouchEvent(_id, isExit: true);
        }
    }
}
