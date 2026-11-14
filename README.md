# Unity To RebelFork

This is an experimental (work in progress) tool to convert Unity3D assets to a format that RebelFramework can read.

Exporting assets from Unity to other game engines is a common practice among game developers. This is because Unity provides a wide range of tools and features that make it easy to create high-quality assets. However, Unity may not be the best engine for every game development project. For example, some developers may prefer RebelFork for its open-source nature and ease of use.

Here is something you should read or watch on YouTube before proceeding:
[![using-asset-store-assets-in-other-engines-is-it-legal](Documentation~/Images/unity_is_it_legal.jpg)](https://gamefromscratch.com/using-asset-store-assets-in-other-engines-is-it-legal/)

# Documentation

[Documentation](Documentation~/README.md)
# Export targets

The exporter writes for one engine, chosen by `ExportSettings.Target`:

- **Rbfx** (default): plain rbfx content. Menu `Tools/RebelFork/Export assets`, or batch `-executeMethod UnityToRebelFork.Editor.BatchExport.RunRbfx`.
- **Toe**: additionally writes files only the Toe engine reads (`<model>.humanoid.json`, `humanBones` in `Characters/<name>.json`, `Effects/<name>.json`)
  and can normalize the baked humanoid clips with the toe CLI. Menu `Tools/Toe/Export assets`, or batch `BatchExport.RunToe`.

Batch arguments: `-exportTarget rbfx|toe` (for `BatchExport.Run`), `-normalize` and `-toe <path of toe.dll>` (Toe target only).

# Reflection probes

A scene export writes each Unity `ReflectionProbe` as a `ReflectionProbe` component (plus a `ReflectionProbeManager` on the scene) and puts the
scene's default reflection on the zone. The cube maps are exported as HDR: `<name>.dds` (RGBA16F, DX10 header, full mip chain) and `<name>.xml`
(`<cubemap>` with the image and an `<sh>` tag for the ambient light). Probes with Toe's attributes (`Mode`, `Intensity`, `Blend Distance`, ...) are
read by Toe; rbfx skips what it does not know. Cube maps used by materials are not exported.

Mip levels: for the **Rbfx** target Unity's own (roughness-blurred) mips are written as they are, because rbfx reflects with a probe texture's mips without
filtering them. For the **Toe** target the file has level 0 plus averaged mips and Toe makes the roughness chain itself when a game first uses the cube map;
a batch export with `-toe <path of toe.dll>` (setting `PrefilterCubemaps`, default on) has the Toe CLI bake that chain into the files instead
(`cubemap prefilter <xml>... --in-place`), so nothing is filtered at load.
