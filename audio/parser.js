// bin2float.js
inlets = 1;
outlets = 1;

// Expects a list of exactly four byte values (0–255)
function list() {
    var bytes = arrayfromargs(arguments);
    if (bytes.length < 4) return;
    
    // Pack into a 4-byte ArrayBuffer and read as big-endian float32
    var buf  = new ArrayBuffer(4),
        view = new DataView(buf);
    
    for (var i = 0; i < 4; i++) {
        view.setUint8(i, bytes[i]);
    }
    
    // false = big-endian
    var f = view.getFloat32(0, false);
    outlet(0, f);
}
