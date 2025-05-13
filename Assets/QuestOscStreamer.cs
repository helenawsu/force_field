using UnityEngine;
using extOSC;
using Oculus.Interaction.HandGrab;

[RequireComponent(typeof(OSCTransmitter))]
public class dOscStreamer : MonoBehaviour
{
    [Header("Reference to your spawner")]
    public ForceField forceField;

    [Header("OSC Settings")]
    [Tooltip("Your Mac’s LAN IP (or use 255.255.255.255 for broadcast)")]
    public string remoteHost = "192.168.4.26";
    [Tooltip("Port matching your listener")]
    public int    remotePort = 7400;

    [Header("Cooldown")]
    [Tooltip("Minimum seconds between OSC sends")]
    public float sendCooldown = 0.5f;

    private OSCTransmitter _transmitter;
    private float _lastSendTime = -Mathf.Infinity;

    void Start()
    {
        // 1) Set up OSC
        _transmitter = gameObject.GetComponent<OSCTransmitter>();
        _transmitter.RemoteHost = remoteHost;
        _transmitter.RemotePort = remotePort;

        // 2) Attach a leave-only handler to each cube
        for (int i = 0; i < forceField.bodies.Length; i++)
        {
            var go      = forceField.bodies[i].gameObject;
            var handler = go.AddComponent<LeaveOnlyHandler>();
            handler.Init(i, forceField.bodies[i], this);
        }
    }

    // Called by LeaveOnlyHandler when hand stops touching cube
    internal void SendLeaveEvent(int cubeId, float mag)
{
    // cooldown
    if (Time.time - _lastSendTime < sendCooldown) return;
    _lastSendTime = Time.time;

    // 1) find the cube’s position
    Vector3 pos = forceField.bodies[cubeId].position;

    // 2) sample div & curl
    float div      = forceField.SampleDivergence(pos);
    Vector3 curl   = forceField.SampleCurl(pos);

    // 3) build and send OSC
    var msg = new OSCMessage("/cube/leave");
    msg.AddValue(OSCValue.Int  (cubeId));
    msg.AddValue(OSCValue.Float(mag));
    msg.AddValue(OSCValue.Float(div));
    msg.AddValue(OSCValue.Float(curl.magnitude));
    msg.AddValue(OSCValue.Float(curl.x));
    msg.AddValue(OSCValue.Float(curl.y));
    msg.AddValue(OSCValue.Float(curl.z));

    _transmitter.Send(msg);
    Debug.Log(
      $"[extOSC] /cube/leave  id={cubeId}  y={mag:F3}  div={div:F3}  curl=({curl.x:F3},{curl.y:F3},{curl.z:F3})"
    );
}


    // ---------------------------------
    // Component that only fires on exit
    // ---------------------------------
    private class LeaveOnlyHandler : MonoBehaviour
    {
        int               _id;
        Rigidbody         _rb;
        dOscStreamer      _parent;

        public void Init(int id, Rigidbody rb, dOscStreamer parent)
        {
            _id     = id;
            _rb     = rb;
            _parent = parent;
        }

        void OnTriggerExit(Collider other)
        {
            // detect any of the auto-generated hand colliders
            if (other.GetComponentInParent<HandGrabInteractor>() != null)
            {
                float mag = _rb.position.magnitude;
                _parent.SendLeaveEvent(_id, mag);
            }
        }
    }
}
