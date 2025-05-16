# Force Field Decomposition of Classical Symphonies

## Demo
[![Watch the demo](https://img.youtube.com/vi/WyP1FmfPp00/0.jpg)](https://youtu.be/WyP1FmfPp00)

## Purpose
To explore abstract mappings between musical structure and physical properties in an interactive VR installation.

## Description
Floating cubes, driven by curl and gradient based noise, mirror the swirling motifs and tension of symphony. Each cube represents a single instrument and plays its audio sample only when you touch it—transforming the force field itself into an instrument. You can grab a cube and move it elsewhere in the field; its new position then modulates playback parameters (e.g. rate, duration, offset), offering an immersive, interactive soundscape. The fourth movement of Dvořák’s *New World Symphony* is selected as the audio source.

## Implementation
1. Unity builds an app to Quest.
2. Quest sends cube information (position, divergence, curl) using OSC stream to ip address of my mac.
3. Max on my mac receives numbers and plays audio using o.granubuf.






