using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class AnimationExporter : ExporterBase<UnityEngine.AnimationClip>
    {
        public AnimationExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        public override string EvaluateResourcePath(UnityEngine.AnimationClip asset)
        {
            return EvaluateResourcePath(asset, ".ani");
        }

        public override IEnumerable Export(UnityEngine.AnimationClip asset)
        {
            yield return asset.name;

            var resourcePath = EvaluateResourcePath(asset);

            var allBindings = AnimationUtility.GetCurveBindings(asset).Select(_=>new Track(asset, _)).ToList();

            var transform = allBindings.Where(_ => _.ObjectType == typeof(Transform)).GroupBy(_=>_.ObjectName);
            var boneTracks = new List<BoneAnimationTrack>();
            foreach (var objectTransforms in transform)
            {
                var transformProperties = objectTransforms.GroupBy(_ => _.PropertyName);
                List<Sample<Vector3>> position = null;
                List<Sample<Quaternion>> rotation = null;
                List<Sample<Vector3>> scale = null;
                foreach (var transformProperty in transformProperties)
                {
                    switch (transformProperty.Key)
                    {
                        case "m_LocalRotation":
                            rotation = MergeQuaternionTracks(transformProperty);
                            break;
                        case "m_LocalPosition":
                            position = MergeVec3Tracks(transformProperty);
                            break;
                        case "m_LocalScale":
                            scale = MergeVec3Tracks(transformProperty);
                            break;
                    }
                }

                if (position != null || rotation != null || scale != null)
                {
                    IEnumerable<Sample<BoneSample>> boneSamples = new[]
                        { new Sample<BoneSample>(0.0f, new BoneSample()) };
                    byte mask = 0;
                    if (position != null)
                    {
                        boneSamples = boneSamples.Merge(position, (s, pos) => new BoneSample() { Position = pos });
                        mask |= 1;
                    }

                    if (rotation != null)
                    {
                        boneSamples = boneSamples.Merge(rotation,
                            (s, rot) => new BoneSample { Position = s.Position, Rotation = rot });
                        mask |= 2;
                    }

                    if (scale != null)
                    {
                        boneSamples = boneSamples.Merge(scale,
                            (s, pos) => new BoneSample()
                                { Position = s.Position, Rotation = s.Rotation, Scale = s.Scale });
                        mask |= 4;
                    }

                    boneTracks.Add(new BoneAnimationTrack()
                        { BoneName = objectTransforms.Key, Mask = mask, Samples = boneSamples.ToList() });
                }
            }

            using (var stream = _settings.CreateFile(EvaluateResourcePath(asset)))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(new byte[] { 0x55, 0x41, 0x4e, 0x49 });
                    writer.WriteStringSZ(asset.name);
                    writer.Write(asset.length);

                    writer.Write(boneTracks.Count);
                    foreach (var track in boneTracks)
                    {
                        writer.WriteStringSZ(track.BoneName);
                        writer.Write(track.Mask);
                        writer.Write(track.Samples.Count);
                        for (var index = 0; index < track.Samples.Count; index++)
                        {
                            writer.Write(track.Samples[index].Time);

                            if (0 != (track.Mask & 1))
                            {
                                writer.Write(track.Samples[index].Value.Position);
                            }
                            if (0 != (track.Mask & 2))
                            {
                                writer.Write(track.Samples[index].Value.Rotation);
                            }
                            if (0 != (track.Mask & 4))
                            {
                                writer.Write(track.Samples[index].Value.Scale);
                            }
                        }
                    }
                }
            }

            var propertyAnimations = allBindings.Where(_ => _.ObjectType != typeof(Transform)).ToList();
        }

        private List<Sample<Vector3>> MergeVec3Tracks(IEnumerable<Track> transformProperty)
        {
            Track x = Track.ZeroTrack;
            Track y = Track.ZeroTrack;
            Track z = Track.ZeroTrack;

            foreach (var track in transformProperty)
            {
                switch (track.PropertyField)
                {
                    case "x": x = track; break;
                    case "y": y = track; break;
                    case "z": z = track; break;
                }
            }

            return x.Samples.Merge(y.Samples).Merge(z.Samples).ToList();
        }

        private List<Sample<Quaternion>> MergeQuaternionTracks(IEnumerable<Track> transformProperty)
        {
            Track x = Track.ZeroTrack;
            Track y = Track.ZeroTrack;
            Track z = Track.ZeroTrack;
            Track w = Track.ZeroTrack;

            foreach (var track in transformProperty)
            {
                switch (track.PropertyField)
                {
                    case "x": x = track; break;
                    case "y": y = track; break;
                    case "z": z = track; break;
                    case "w": w = track; break;
                }
            }

            return x.Samples.Merge(y.Samples).Merge(z.Samples).Merge(w.Samples).Select(_=>new Sample<Quaternion>(_.Time, new Quaternion(_.Value.x, _.Value.y, _.Value.z, _.Value.w))).ToList();
        }

        public class BoneAnimationTrack
        {
            public string BoneName { get; set; }
            public byte Mask { get; set; }
            public List<Sample<BoneSample>> Samples { get; set; }
        }

        public struct BoneSample
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        public class Track
        {
            public static Track ZeroTrack = new Track() {Samples = new List<Sample<float>>(){new Sample<float>(0,0)}};

            public Track()
            {
            }

            public Track(AnimationClip clip, EditorCurveBinding binding)
            {
                Path = binding.path ?? string.Empty;
                int lastPathSeparator = Path.LastIndexOf('/');
                if (lastPathSeparator >= 0)
                {
                    ObjectName = Path.Substring(lastPathSeparator + 1);
                    Path = Path.Substring(0, lastPathSeparator);
                }
                else
                {
                    ObjectName = Path;
                    Path = string.Empty;
                }

                PropertyName = binding.propertyName ?? string.Empty;
                int lastFieldSeparator = PropertyName.LastIndexOf('.');
                if (lastFieldSeparator >= 0)
                {
                    PropertyField = PropertyName.Substring(lastFieldSeparator + 1);
                    PropertyName = PropertyName.Substring(0, lastFieldSeparator);
                }

                ObjectType = binding.type;
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var curveKeys = curve.keys;
                if (curveKeys.Length > 0)
                {
                    if (curveKeys.Length == 1)
                    {
                        Samples.Add(new Sample<float>(curveKeys[0].time, curveKeys[0].value));
                    }
                    else
                    {
                        float step = 1.0f / MathF.Max(1e-6f, clip.frameRate);
                        for (var t = 0.0f; t < clip.length - 1e-3f; t += step)
                        {
                            Samples.Add(new Sample<float>(t, curve.Evaluate(t)));
                        }

                        if (clip.length > 0.0f)
                        {
                            Samples.Add(new Sample<float>(clip.length, curve.Evaluate(clip.length)));
                        }
                    }
                }
            }

            public override string ToString()
            {
                return $"{Path}/{ObjectName}.{PropertyName}.{PropertyField}";
            }

            public string Path { get; set; }
            public string ObjectName { get; set; }
            public string PropertyName { get; set; }
            public string PropertyField { get; set; }
            public System.Type ObjectType { get; set; }
            public List<Sample<float>> Samples { get; set; } = new List<Sample<float>>();
        }
    }
}