# UnityObjParser

This <b>obj parser for Unity</b> allows you to load models from obj+mtl files <b>in runtime</b>. Requires C# 6. Tested with Unity 2017.3 on windows.

Key features:
* <b>Asynchronous</b>. And I don’t mean coroutines, I mean separate threads, so your application doesn’t hang when it reads geometry.
* <b>Fast.</b> Millions of polygons take tens of seconds to load on desktop cpu. Keep in mind that in build it’s about 3 times faster than in editor.
* <b>Features support</b>. You can load meshes with triangles or quads, with positive or negative vertex indexes, there’s support for different normals and uv coordinates on a vertex, white spaces in file names etc.
* <b>Simplicity</b>. Just call await ObjParser.Parse(path) and you’re good to go! Example scene is also included.

Limitations:
* Parser uses async/await, so you need C# 6 and a platform with threading support.
* Only loads from files. There’s no option to load from a url, so you’ll have to add that yourself if you need it.
* Limited materials support. I’m only loading albedo, normals and transparency. Again, you’ll have to add more features yourself if you need them, it should be simple enough.
* Polygons with more than 4 vertices are supported, but only if they are convex and vertices are in one plane. If you’re not sure if that’s the case, triangulate your meshes during export.
* Textures are cached, models are not. Textures cache can be cleared.
