using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityToRebelFork.Editor;

namespace UnityToRebelFork
{
    /// <summary>Rooms and portals for the Toe engine: the math the gizmos and checks use, and what the exporter writes for them.</summary>
    [TestFixture]
    public class VisibilityRoomTests
    {
        private readonly List<GameObject> _made = new List<GameObject>();

        [TearDown]
        public void Clean()
        {
            foreach (var go in _made)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            _made.Clear();
        }

        private GameObject Make(string name, Vector3 position, Transform parent = null)
        {
            var go = new GameObject(name);
            _made.Add(go);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        private static PrefabVisitor Visitor(ExportTarget target)
        {
            var settings = ScriptableObject.CreateInstance<ExportSettings>();
            settings.Target = target;
            return new PrefabVisitor(new Lazy<ExportOrchestrator>(() => null), settings);
        }

        private static GenericComponent Generic(Node node, string typeName) => node.Components.OfType<GenericComponent>().SingleOrDefault(c => c.TypeName == typeName);

        private static Variant Value(GenericComponent component, string name) => component.Values.Single(v => v.Key == name).Value;

        private static RoomShape Shape(string name, int priority, Vector3 center, Vector3 size)
        {
            var shape = new RoomShape { Name = name, Priority = priority };
            shape.Boxes.Add(VisibilityMath.WorldBox(new VisibilityFrame(center, Quaternion.identity, Vector3.one), Vector3.zero, size));
            return shape;
        }

        // ---- Math ----

        [Test]
        public void OfTwoRooms_TheHigherPriorityHoldsAPoint_ThenTheSmaller()
        {
            var big = Shape("big", 0, Vector3.zero, new Vector3(10, 10, 10));
            var small = Shape("small", 0, Vector3.zero, new Vector3(4, 4, 4));
            var rooms = new List<RoomShape> { big, small };
            Assert.AreSame(small, VisibilityMath.FindRoom(rooms, Vector3.zero));
            Assert.AreSame(big, VisibilityMath.FindRoom(rooms, new Vector3(4, 0, 0)));
            Assert.IsNull(VisibilityMath.FindRoom(rooms, new Vector3(6, 0, 0)));
            big.Priority = 5;
            Assert.AreSame(big, VisibilityMath.FindRoom(rooms, Vector3.zero));
        }

        [Test]
        public void RoomsThatShareAWall_Touch_ButDoNotOverlap()
        {
            var a = Shape("a", 0, Vector3.zero, new Vector3(4, 4, 4));
            var b = Shape("b", 0, new Vector3(4, 0, 0), new Vector3(4, 4, 4));
            var c = Shape("c", 0, new Vector3(3, 0, 0), new Vector3(4, 4, 4));
            Assert.IsFalse(a.Overlaps(b, VisibilityMath.WallTolerance));
            Assert.IsTrue(a.Overlaps(c, VisibilityMath.WallTolerance));
        }

        [Test]
        public void APortalLooksAlongItsZ_AndFindsTheRoomOnEachSide()
        {
            var hall = Shape("Hall", 0, new Vector3(0, 1.5f, 0), new Vector3(8, 3, 8));
            var kitchen = Shape("Kitchen", 0, new Vector3(0, 1.5f, 8), new Vector3(8, 3, 8));
            var rooms = new List<RoomShape> { hall, kitchen };

            var inner = VisibilityMath.SidesOf(new VisibilityFrame(new Vector3(0, 1.1f, 4), Quaternion.identity, Vector3.one));
            VisibilityMath.RoomsOf(rooms, inner, out var behind, out var front);
            Assert.AreSame(hall, behind);
            Assert.AreSame(kitchen, front);

            var outer = VisibilityMath.SidesOf(new VisibilityFrame(new Vector3(0, 1.1f, -4), Quaternion.identity, Vector3.one));
            VisibilityMath.RoomsOf(rooms, outer, out behind, out front);
            Assert.IsNull(behind);
            Assert.AreSame(hall, front);

            var turned = VisibilityMath.SidesOf(new VisibilityFrame(Vector3.zero, new Quaternion(0f, Mathf.Sin(Mathf.PI / 4f), 0f, Mathf.Cos(Mathf.PI / 4f)), Vector3.one));
            Assert.AreEqual(1f, turned.Normal.x, 1e-4f);
        }

        [Test]
        public void TheChecks_ReportWhatCannotWork_AndNothingForAGoodHouse()
        {
            var hall = Shape("Hall", 0, new Vector3(0, 1.5f, 0), new Vector3(8, 3, 8));
            var kitchen = Shape("Kitchen", 0, new Vector3(0, 1.5f, 8), new Vector3(8, 3, 8));
            PortalSides At(Vector3 p) => VisibilityMath.SidesOf(new VisibilityFrame(p, Quaternion.identity, Vector3.one));
            var good = VisibilityMath.Check(new List<RoomShape> { hall, kitchen }, new List<KeyValuePair<string, PortalSides>>
            {
                new KeyValuePair<string, PortalSides>("Inner", At(new Vector3(0, 1.1f, 4))),
                new KeyValuePair<string, PortalSides>("Front", At(new Vector3(0, 1.1f, -4))),
            });
            Assert.IsEmpty(good);

            var twin = Shape("Twin", 0, new Vector3(1, 1.5f, 0), new Vector3(8, 3, 8));
            var bad = VisibilityMath.Check(new List<RoomShape> { hall, twin }, new List<KeyValuePair<string, PortalSides>>
            {
                new KeyValuePair<string, PortalSides>("Lost", At(new Vector3(80, 1, 0))),
                new KeyValuePair<string, PortalSides>("Middle", At(new Vector3(0, 1, 0))),
            });
            Assert.IsTrue(bad.Any(w => w.Contains("'Hall'") && w.Contains("'Twin'") && w.Contains("same priority")));
            Assert.IsTrue(bad.Any(w => w.Contains("'Lost'") && w.Contains("no room")));
            Assert.IsTrue(bad.Any(w => w.Contains("'Middle'") && w.Contains("same room")));
        }

        // ---- The components ----

        [Test]
        public void ABoxBelowARoom_IsPartOfIt_AndIsExportedInTheRoomsSpaceBeforeItsScale()
        {
            var room = Make("Room", new Vector3(10, 0, 0));
            room.transform.localScale = new Vector3(2, 2, 2);
            var component = room.AddComponent<VisibilityRoom>();
            component.center = new Vector3(0, 1.5f, 0);
            component.size = new Vector3(4, 3, 4);
            var wing = Make("Wing", new Vector3(2, 0, 0), room.transform);
            var box = wing.AddComponent<VisibilityRoomBox>();
            box.size = new Vector3(2, 2, 2);

            Assert.AreSame(component, box.Room);
            var boxes = new List<RoomBoxData>();
            component.CollectExportBoxes(boxes, out int empty, out int inexact);

            Assert.AreEqual(2, boxes.Count);
            Assert.AreEqual(0, empty);
            Assert.AreEqual(0, inexact);
            Assert.AreEqual(new Vector3(0, 1.5f, 0), boxes[0].Center);
            Assert.AreEqual(2f, boxes[1].Center.x, 1e-4f);
            Assert.AreEqual(2f, boxes[1].Size.x, 1e-4f);

            var world = new List<OrientedBox>();
            component.CollectWorldBoxes(world, out _);
            Assert.AreEqual(2, world.Count);
            Assert.AreEqual(14f, world[1].Center.x, 1e-4f);
            Assert.AreEqual(2f, world[1].HalfSize.x, 1e-4f); // A 2 m box under a room scaled by 2 is 4 m wide in the world: half is 2.
        }

        [Test]
        public void ABoxBelowTwoRooms_BelongsToTheNearestOne()
        {
            var outer = Make("Outer", Vector3.zero);
            var outerRoom = outer.AddComponent<VisibilityRoom>();
            var inner = Make("Inner", Vector3.zero, outer.transform);
            var innerRoom = inner.AddComponent<VisibilityRoom>();
            var box = Make("Box", Vector3.zero, inner.transform).AddComponent<VisibilityRoomBox>();

            Assert.AreSame(innerRoom, box.Room);
            var outerBoxes = new List<VisibilityRoomBox>();
            outerRoom.CollectChildBoxes(outerBoxes);
            Assert.IsEmpty(outerBoxes);
        }

        // ---- The export ----

        [Test]
        public void ToeTarget_ExportsARoomWithItsBoxes_APortalWithAStableId_AndNothingForTheBox()
        {
            var house = Make("House", Vector3.zero);
            var hall = Make("Hall", new Vector3(0, 0, 0), house.transform);
            var room = hall.AddComponent<VisibilityRoom>();
            room.priority = 3;
            room.size = new Vector3(8, 3, 8);
            Make("Wing", new Vector3(0, 0, 6), hall.transform).AddComponent<VisibilityRoomBox>();
            var door = Make("Door", new Vector3(0, 1.1f, 4), house.transform);
            var portal = door.AddComponent<VisibilityPortal>();
            portal.size = new Vector2(1.5f, 2.2f);
            portal.enabled = false; // Exports as closed, so a game can open it.

            var node = Visitor(ExportTarget.Toe).Visit(house, isRoot: true);

            var hallNode = node.Children.Single(n => n.Name == "Hall");
            var roomComponent = Generic(hallNode, "VisibilityRoom");
            Assert.IsNotNull(roomComponent);
            Assert.AreEqual(3, Value(roomComponent, "Priority").Int);
            var boxes = Value(roomComponent, "Boxes").VariantList;
            Assert.AreEqual(1 + 3 * 2, boxes.Count);
            Assert.AreEqual(2, boxes[0].Int);
            Assert.AreEqual(new Vector3(8, 3, 8), boxes[2].Vector3);
            Assert.IsNull(Generic(hallNode.Children.Single(), "VisibilityRoomBox"), "the box exports nothing by itself");
            Assert.IsEmpty(hallNode.Children.Single().Components);

            var doorNode = node.Children.Single(n => n.Name == "Door");
            var portalComponent = Generic(doorNode, "Portal");
            Assert.IsNotNull(portalComponent);
            Assert.AreEqual(new Vector2(1.5f, 2.2f), Value(portalComponent, "Size").Vector2);
            Assert.IsFalse(Value(portalComponent, "Is Open").Bool);
            var stable = Generic(doorNode, "StableId");
            Assert.IsNotNull(stable);
            Assert.AreEqual(32, Value(stable, "Id").String.Length);
        }

        [Test]
        public void RbfxTarget_LeavesRoomsAndPortalsOut_WithOneWarning()
        {
            var house = Make("House", Vector3.zero);
            Make("Hall", Vector3.zero, house.transform).AddComponent<VisibilityRoom>();
            Make("Door", Vector3.zero, house.transform).AddComponent<VisibilityPortal>();

            LogAssert.Expect(LogType.Warning, new Regex(@"\[Rooms\].*Toe target only"));
            var node = Visitor(ExportTarget.Rbfx).Visit(house, isRoot: true);

            Assert.IsEmpty(node.Children.SelectMany(c => c.Components).OfType<GenericComponent>());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ARoomWithoutABoxOfAnySize_IsNotExported_AndSaysSo()
        {
            var hall = Make("Hall", Vector3.zero);
            var room = hall.AddComponent<VisibilityRoom>();
            room.size = new Vector3(4, 0, 4);

            LogAssert.Expect(LogType.Warning, new Regex(@"\[Rooms\].*no box of any size"));
            var node = Visitor(ExportTarget.Toe).Visit(hall, isRoot: true);

            Assert.IsNull(Generic(node, "VisibilityRoom"));
        }

        [Test]
        public void AVariantListIsWrittenAsElements_LikeRbfxWritesIt()
        {
            var list = new VariantList { new Variant(2), new Variant(new Vector3(1, 2, 3)), new Variant(Quaternion.identity) };
            var attribute = new GenericComponent("Thing").Set("Items", new Variant(list));

            var element = new PrefabSceneSerializer().Visit(new Scene
            {
                Children = { new Node { Components = { attribute } } },
            });

            var xml = element.Descendants("attribute").Single(a => (string)a.Attribute("name") == "Items");
            Assert.AreEqual("VariantVector", (string)xml.Attribute("type"));
            Assert.IsNull(xml.Attribute("value"));
            var items = xml.Element("value").Elements("element").ToList();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("Int", (string)items[0].Attribute("type"));
            Assert.AreEqual("1 2 3", (string)items[1].Attribute("value"));
            Assert.AreEqual("1 0 0 0", (string)items[2].Attribute("value"));
        }
    }
}
