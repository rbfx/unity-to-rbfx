#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace UnityToRebelFork.Editor.Particles
{
    /// <summary>A value in a graph: the output pin of a node.</summary>
    public readonly struct Pin
    {
        public Pin(int node, string name, string type)
        {
            Node = node;
            Name = name;
            Type = type;
        }

        public int Node { get; }

        public string Name { get; }

        /// <summary>The value type (Float, Vector3, ...) as far as the converter knows it.</summary>
        public string Type { get; }

        public bool IsValid => Node != 0;
    }

    /// <summary>
    /// Writes the nodes of one graph of an rbfx particle layer (<c>&lt;nodes&gt;</c>): every call adds a node and returns its output as a <see cref="Pin"/> to
    /// connect to the next. Nodes only refer to earlier ones, as the layer compiler needs.
    /// </summary>
    public sealed class GraphBuilder
    {
        private readonly List<XElement> _nodes = new List<XElement>();

        public static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);

        public static string V3(float x, float y, float z) => F(x) + " " + F(y) + " " + F(z);

        public static string V4(float x, float y, float z, float w) => F(x) + " " + F(y) + " " + F(z) + " " + F(w);

        public int Count => _nodes.Count;

        public XElement ToXml(string graphName) => new XElement(graphName, new XElement("nodes", _nodes));

        public static XElement Property(string name, string type, string value)
            => new XElement("property", new XAttribute("name", name), new XAttribute("type", type), new XAttribute("value", value));

        /// <summary>Adds a node. <paramref name="inputs"/> are (input pin name, source); <paramref name="outputs"/> are (output pin name, type, may be null).</summary>
        public Pin[] Node(string name, IEnumerable<XElement> properties, IEnumerable<KeyValuePair<string, Pin>> inputs, params KeyValuePair<string, string>[] outputs)
        {
            int id = _nodes.Count + 1;
            var node = new XElement("node", new XAttribute("id", id), new XAttribute("name", name));
            var props = properties?.ToList();
            if (props != null && props.Count > 0) node.Add(new XElement("properties", props));
            var ins = inputs?.ToList();
            if (ins != null && ins.Count > 0)
            {
                node.Add(new XElement("in", ins.Select(i => new XElement("pin",
                    new XAttribute("name", i.Key), new XAttribute("node", i.Value.Node), new XAttribute("pin", i.Value.Name)))));
            }
            if (outputs.Length > 0)
            {
                node.Add(new XElement("out", outputs.Select(o =>
                {
                    var pin = new XElement("pin", new XAttribute("name", o.Key));
                    if (o.Value != null) pin.Add(new XAttribute("type", o.Value));
                    return pin;
                })));
            }
            _nodes.Add(node);
            return outputs.Select(o => new Pin(id, o.Key, o.Value)).ToArray();
        }

        private static KeyValuePair<string, Pin> In(string name, Pin pin) => new KeyValuePair<string, Pin>(name, pin);

        private static KeyValuePair<string, string> Out(string name, string type = null) => new KeyValuePair<string, string>(name, type);

        // ---- Values ----

        public Pin Const(float value) => Node("Constant", new[] { Property("Value", "Float", F(value)) }, null, Out("out", "Float"))[0];

        public Pin ConstInt(int value) => Node("Constant", new[] { Property("Value", "Int", value.ToString(CultureInfo.InvariantCulture)) }, null, Out("out", "Int"))[0];

        public Pin ConstBool(bool value) => Node("Constant", new[] { Property("Value", "Bool", value ? "true" : "false") }, null, Out("out", "Bool"))[0];

        public Pin Const(float x, float y, float z) => Node("Constant", new[] { Property("Value", "Vector3", V3(x, y, z)) }, null, Out("out", "Vector3"))[0];

        public Pin Const(float x, float y) => Node("Constant", new[] { Property("Value", "Vector2", F(x) + " " + F(y)) }, null, Out("out", "Vector2"))[0];

        public Pin ConstColor(float r, float g, float b, float a) => Node("Constant", new[] { Property("Value", "Color", V4(r, g, b, a)) }, null, Out("out", "Color"))[0];

        public Pin ConstQuaternion(float x, float y, float z, float w) => Node("Constant", new[] { Property("Value", "Quaternion", V4(w, x, y, z)) }, null, Out("out", "Quaternion"))[0];

        public Pin Random(float min, float max)
            => Node("Random", new[] { Property("Min", "Float", F(min)), Property("Max", "Float", F(max)) }, null, Out("out", "Float"))[0];

        // ---- Attributes ----

        public Pin Get(string attribute, string type) => Node("GetAttribute", null, null, Out(attribute, type))[0];

        public Pin Set(string attribute, string type, Pin value) => Node("SetAttribute", null, new[] { In("", value) }, Out(attribute, type))[0];

        // ---- Arithmetic ----

        private Pin Binary(string node, Pin x, Pin y) => Node(node, null, new[] { In("x", x), In("y", y) }, Out("out"))[0];

        public Pin Add(Pin x, Pin y) => Binary("Add", x, y);

        public Pin Subtract(Pin x, Pin y) => Binary("Subtract", x, y);

        public Pin Multiply(Pin x, Pin y) => Binary("Multiply", x, y);

        public Pin Divide(Pin x, Pin y) => Binary("Divide", x, y);

        public Pin Min(Pin x, Pin y) => Binary("Min", x, y);

        public Pin Max(Pin x, Pin y) => Binary("Max", x, y);

        public Pin Less(Pin x, Pin y) => Binary("Less", x, y);

        public Pin Greater(Pin x, Pin y) => Binary("Greater", x, y);

        public Pin And(Pin x, Pin y) => Binary("And", x, y);

        public Pin Repeat(Pin x, Pin length) => Node("Repeat", null, new[] { In("x", x), In("length", length) }, Out("out"))[0];

        public Pin Pow(Pin x, Pin y) => Binary("Pow", x, y);

        public Pin Lerp(Pin x, Pin y, Pin t) => Node("Lerp", null, new[] { In("x", x), In("y", y), In("t", t) }, Out("out"))[0];

        public Pin Select(Pin condition, Pin x, Pin y)
            => Node("Select", null, new[] { In("condition", condition), In("x", x), In("y", y) }, Out("out"))[0];

        public Pin Negate(Pin x) => Node("Negate", null, new[] { In("x", x) }, Out("out"))[0];

        public Pin Floor(Pin x) => Node("Floor", null, new[] { In("x", x) }, Out("out"))[0];

        public Pin Length(Pin x) => Node("Length", null, new[] { In("x", x) }, Out("out", "Float"))[0];

        public Pin Normalized(Pin x) => Node("Normalized", null, new[] { In("x", x) }, Out("out"))[0];

        public Pin TimeStep() => Node("TimeStep", null, null, Out("out", "Float"))[0];

        public Pin TimeStepScale(Pin x) => Node("TimeStepScale", null, new[] { In("x", x) }, Out("out"))[0];

        public Pin EffectTime() => Node("EffectTime", null, null, Out("out", "Float"))[0];

        public Pin NormalizedEffectTime(float duration, bool looped)
            => Node("NormalizedEffectTime", new[] { Property("Duration", "Float", F(duration)), Property("IsLooped", "Bool", looped ? "true" : "false") }, null, Out("out", "Float"))[0];

        public Pin Make(Pin x, Pin y) => Node("Make", null, new[] { In("x", x), In("y", y) }, Out("out", "Vector2"))[0];

        public Pin Make(Pin x, Pin y, Pin z) => Node("Make", null, new[] { In("x", x), In("y", y), In("z", z) }, Out("out", "Vector3"))[0];

        public Pin MakeTransform(Pin translation, Pin rotation, Pin scale)
            => Node("Make", null, new[] { In("translation", translation), In("rotation", rotation), In("scale", scale) }, Out("out", "Matrix4"))[0];

        /// <summary>A rotation from angles in degrees about x, y and z.</summary>
        public Pin MakeRotation(Pin pitch, Pin yaw, Pin roll)
            => Node("Make", null, new[] { In("pitch", pitch), In("yaw", yaw), In("roll", roll) }, Out("out", "Quaternion"))[0];

        public Pin[] Break(Pin vector3) => Node("Break", null, new[] { In("vec", vector3) }, Out("x", "Float"), Out("y", "Float"), Out("z", "Float"));

        public Pin TransformPoint(Pin matrix, Pin point)
            => Node("TransformPoint", null, new[] { In("matrix", matrix), In("point", point) }, Out("out", "Vector3"))[0];

        public Pin TransformDirection(Pin matrix, Pin direction)
            => Node("TransformDirection", null, new[] { In("matrix", matrix), In("direction", direction) }, Out("out", "Vector3"))[0];

        public Pin LocalToWorld() => Node("LocalToWorld", null, null, Out("out", "Matrix4"))[0];

        public Pin WorldToLocal() => Node("WorldToLocal", null, null, Out("out", "Matrix4"))[0];

        /// <summary>The Collide node: the outputs are the new position, the new velocity and whether the particle collided.</summary>
        public Pin[] Collide(IEnumerable<XElement> properties, Pin previousPosition, Pin position, Pin velocity, Pin radius)
            => Node("Collide", properties,
                new[] { In("prevPosition", previousPosition), In("position", position), In("velocity", velocity), In("radius", radius) },
                Out("newPosition", "Vector3"), Out("newVelocity", "Vector3"), Out("collided", "Bool"));

        public Pin EmitterPosition() => Node("EmitterPosition", null, null, Out("out", "Vector3"))[0];

        public Pin Rotate(Pin rotation, Pin vector)
            => Node("Rotate", null, new[] { In("rotation", rotation), In("vector", vector) }, Out("out", "Vector3"))[0];

        /// <summary>How fast the emitter's node moves in the world (units a second).</summary>
        public Pin EmitterVelocity() => Node("EmitterVelocity", null, null, Out("out", "Vector3"))[0];

        // ---- Motion ----

        public Pin ApplyForce(Pin velocity, Pin force)
            => Node("ApplyForce", null, new[] { In("velocity", velocity), In("force", force) }, Out("out", "Vector3"))[0];

        public Pin Move(Pin position, Pin velocity)
            => Node("Move", null, new[] { In("position", position), In("velocity", velocity) }, Out("newPosition", "Vector3"))[0];

        public Pin LimitVelocity(Pin velocity, Pin limit, float dampen)
            => Node("LimitVelocity", new[] { Property("Dampen", "Float", F(dampen)) }, new[] { In("velocity", velocity), In("limit", limit) }, Out("out", "Vector3"))[0];

        public Pin NoiseVector(Pin position, float frequency, float strength, float scrollSpeed)
            => Node("NoiseVector3D", new[]
            {
                Property("Frequency", "Float", F(frequency)), Property("Strength", "Float", F(strength)), Property("Scroll Speed", "Float", F(scrollSpeed)),
            }, new[] { In("x", position) }, Out("out", "Vector3"))[0];

        /// <summary>Expire the particles whose time reached their lifetime.</summary>
        public void Expire(Pin time, Pin lifetime) => Node("Expire", null, new[] { In("time", time), In("lifetime", lifetime) });

        public void Emit(Pin count) => Node("Emit", null, new[] { In("count", count) });

        public Pin BurstTimer(Pin count, float delay, float interval, int cycles)
            => Node("BurstTimer", new[]
            {
                Property("Delay", "Float", F(delay)), Property("Interval", "Float", F(interval)), Property("Cycles", "Int", cycles.ToString(CultureInfo.InvariantCulture)),
            }, new[] { In("count", count) }, Out("out", "Float"))[0];

        // ---- Shapes ----

        public Pin[] Shape(string node, IEnumerable<XElement> properties)
            => Node(node, properties, null, Out("position", "Vector3"), Out("velocity", "Vector3"));

        // ---- Curves ----

        /// <summary>A curve sampled by <paramref name="t"/>. <paramref name="type"/> is Float, Vector3 or Color; each key is time and then the components.</summary>
        public Pin Curve(Pin t, string type, IList<float[]> keys)
        {
            var frames = new XElement("keyframes");
            foreach (var key in keys)
                frames.Add(new XElement("keyframe", new XAttribute("time", F(key[0])), new XAttribute("value", string.Join(" ", key.Skip(1).Select(F)))));
            var curve = new XElement("property", new XAttribute("name", "Curve"), new XAttribute("type", "VariantCurve"),
                new XElement("value", new XAttribute("name", ""), new XAttribute("type", type), new XAttribute("interpolation", "linear"),
                    new XAttribute("splineTension", "0.5"), frames));
            return Node("Curve", new[] { curve }, new[] { In("t", t) }, Out("out", type))[0];
        }

        // ---- Rendering ----

        public void RenderBillboard(string material, int rows, int columns, int faceCameraMode, bool sort, bool worldspace,
            Pin position, Pin size, Pin frame, Pin color, Pin rotation, Pin direction)
        {
            Node("RenderBillboard", new[]
            {
                Property("Material", "ResourceRef", "Material;" + material), Property("Rows", "Int", rows.ToString(CultureInfo.InvariantCulture)),
                Property("Columns", "Int", columns.ToString(CultureInfo.InvariantCulture)),
                Property("Face Camera Mode", "Int", faceCameraMode.ToString(CultureInfo.InvariantCulture)),
                Property("Sort By Distance", "Bool", sort ? "True" : "False"), Property("Is Worldspace", "Bool", worldspace ? "True" : "False"),
            }, new[] { In("pos", position), In("size", size), In("frame", frame), In("color", color), In("rotation", rotation), In("direction", direction) });
        }

        public void RenderMesh(string model, string material, bool worldspace, Pin transform, Pin color = default)
        {
            var inputs = new List<KeyValuePair<string, Pin>> { In("transform", transform) };
            if (color.IsValid) inputs.Add(In("color", color));
            Node("RenderMesh", new[]
            {
                Property("Model", "ResourceRef", "Model;" + model), Property("Material", "ResourceRefList", "Material;" + material),
                Property("Is Worldspace", "Bool", worldspace ? "True" : "False"),
            }, inputs);
        }
    }
}
