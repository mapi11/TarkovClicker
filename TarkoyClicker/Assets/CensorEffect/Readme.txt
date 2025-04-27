== Usage ==
Enable the "Opaque Texture" option on the currently active pipeline asset. Without this, the censor effect appears gray!

To add the effect to your scene:

- Go to GameObject -> 3D Object -> Quad
- Assign the "CensorEffectMat" material to the object

You can of course apply this to any kind of mesh.

== Known issues ==
Anti-aliasing may induce some flickering. Though if the censored object is slightly moving, this isn't noticable.