# Terrain Heightmap Manual Interpolation

This repository contains jobified and bursted manual implementation of Terrain.SampleHeight() function. Getting interpolated terrain heights is essential when placing objects on the surface of terrain. Applications of this ranges between spawning trees, grass, detail objects as well as adjusting heights for NavMeshAgents in order to walk on the terrain at the correct height (i.e. preventing walking in the air or having sinked legs where NavMesh does not match terrain).

Project includes correctness tests in terms of maximum error between Terrain.SampleHeight() and function being tested over a set of random points. Tests also include Stopwatch measures for how long tests run in order to better compare performance.

Correctness tests show that bilinear interpolation (i.e. https://forum.unity.com/threads/terrain-interpolation-algorithm.163935/) widely used to estimate interpolated heights is not the correct approach to estimate heights. Instead barycentric (http://wiki.unity3d.com/index.php?title=Barycentric&oldid=19264) interpolation appears to be the one which matches Terrain.SampleHeight() close to floating point precision. This can be explained since terrain heightmap is not just a grid of quads. Instead, each quad is represented as two triangles with the diagonal going through the middle in (0,0) => (1,1) direction. As a result, terrain can bend through this diagonal producing errors and barycentric interpolation accounts for it.

Performance tests show that once jobified and bursted, calculation over a list of points is nearly two orders of magnitude (100x) faster than non-jobified calculations. When running first time, Burst performs asynchronous runtime compilation and result is not visible. Instead, press "T" to rerun the test second time in order to see true results.

In order to run the test, open Main.scene and simply enter play mode.
