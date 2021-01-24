This file will highlight what is being used from 3rd parties as well as links to relevant projects:

1. Barycentric interpolation algorithm was used from http://wiki.unity3d.com/index.php?title=Barycentric and was adjusted to meet project needs for terrain height interpolation.

2. Implementation of noise octaves, persistence and lacunarity for terrain generation (NoiseExtensions.cs file) is based on this post: https://www.reddit.com/r/proceduralgeneration/comments/6eubj7/how_can_i_add_octaves_persistence_lacunarity/did2awi?utm_source=share&utm_medium=web2x&context=3. The noise itself is calculated inside Unity.Mathematics package which is installed through Unity Package Manager.

3. All my other code can be used under MIT License.
