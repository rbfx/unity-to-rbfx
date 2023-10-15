// <auto-generated />

using System.Collections.Generic;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public partial class UIElement: UnityToRebelFork.Editor.Animatable
    {
        protected static readonly RbfxAttribute<string> NameAttr = new ("Name", VariantType.VarString, new Variant(""), _=>((UIElement)_).Name, (_,v)=>((UIElement)_).Name = v);
        protected static readonly RbfxAttribute<IntVector2> PositionAttr = new ("Position", VariantType.VarIntVector2, new Variant(new IntVector2(0, 0)), _=>((UIElement)_).Position, (_,v)=>((UIElement)_).Position = v);
        protected static readonly RbfxAttribute<IntVector2> SizeAttr = new ("Size", VariantType.VarIntVector2, new Variant(new IntVector2(0, 0)), _=>((UIElement)_).Size, (_,v)=>((UIElement)_).Size = v);
        protected static readonly RbfxAttribute<IntVector2> MinSizeAttr = new ("Min Size", VariantType.VarIntVector2, new Variant(new IntVector2(0, 0)), _=>((UIElement)_).MinSize, (_,v)=>((UIElement)_).MinSize = v);
        protected static readonly RbfxAttribute<IntVector2> MaxSizeAttr = new ("Max Size", VariantType.VarIntVector2, new Variant(new IntVector2(2147483647, 2147483647)), _=>((UIElement)_).MaxSize, (_,v)=>((UIElement)_).MaxSize = v);
        protected static readonly RbfxAttribute<string> HorizAlignmentAttr = new ("Horiz Alignment", VariantType.VarString, new Variant(UnityToRebelFork.Editor.HorizAlignment.Left), _=>((UIElement)_).HorizAlignment, (_,v)=>((UIElement)_).HorizAlignment = v);
        protected static readonly RbfxAttribute<string> VertAlignmentAttr = new ("Vert Alignment", VariantType.VarString, new Variant(UnityToRebelFork.Editor.VertAlignment.Top), _=>((UIElement)_).VertAlignment, (_,v)=>((UIElement)_).VertAlignment = v);
        protected static readonly RbfxAttribute<Vector2> MinAnchorAttr = new ("Min Anchor", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((UIElement)_).MinAnchor, (_,v)=>((UIElement)_).MinAnchor = v);
        protected static readonly RbfxAttribute<Vector2> MaxAnchorAttr = new ("Max Anchor", VariantType.VarVector2, new Variant(new Vector2(0f, 0f)), _=>((UIElement)_).MaxAnchor, (_,v)=>((UIElement)_).MaxAnchor = v);
        protected static readonly RbfxAttribute<IntVector2> MinOffsetAttr = new ("Min Offset", VariantType.VarIntVector2, new Variant(new IntVector2(0, 0)), _=>((UIElement)_).MinOffset, (_,v)=>((UIElement)_).MinOffset = v);
        protected static readonly RbfxAttribute<IntVector2> MaxOffsetAttr = new ("Max Offset", VariantType.VarIntVector2, new Variant(new IntVector2(0, 0)), _=>((UIElement)_).MaxOffset, (_,v)=>((UIElement)_).MaxOffset = v);
        protected static readonly RbfxAttribute<Vector2> PivotAttr = new ("Pivot", VariantType.VarVector2, new Variant(new Vector2(3.4028235E+38f, 3.4028235E+38f)), _=>((UIElement)_).Pivot, (_,v)=>((UIElement)_).Pivot = v);
        protected static readonly RbfxAttribute<bool> EnableAnchorAttr = new ("Enable Anchor", VariantType.VarBool, new Variant(false), _=>((UIElement)_).EnableAnchor, (_,v)=>((UIElement)_).EnableAnchor = v);
        protected static readonly RbfxAttribute<IntRect> ClipBorderAttr = new ("Clip Border", VariantType.VarIntRect, new Variant(new IntRect(0, 0, 0, 0)), _=>((UIElement)_).ClipBorder, (_,v)=>((UIElement)_).ClipBorder = v);
        protected static readonly RbfxAttribute<int> PriorityAttr = new ("Priority", VariantType.VarInt, new Variant(0), _=>((UIElement)_).Priority, (_,v)=>((UIElement)_).Priority = v);
        protected static readonly RbfxAttribute<float> OpacityAttr = new ("Opacity", VariantType.VarFloat, new Variant(1f), _=>((UIElement)_).Opacity, (_,v)=>((UIElement)_).Opacity = v);
        protected static readonly RbfxAttribute<Color> ColorAttr = new ("Color", VariantType.VarColor, new Variant(new Color(1f, 1f, 1f, 1f)), _=>((UIElement)_).Color, (_,v)=>((UIElement)_).Color = v);
        protected static readonly RbfxAttribute<Color> TopLeftColorAttr = new ("Top Left Color", VariantType.VarColor, new Variant(new Color(1f, 1f, 1f, 1f)), _=>((UIElement)_).TopLeftColor, (_,v)=>((UIElement)_).TopLeftColor = v);
        protected static readonly RbfxAttribute<Color> TopRightColorAttr = new ("Top Right Color", VariantType.VarColor, new Variant(new Color(1f, 1f, 1f, 1f)), _=>((UIElement)_).TopRightColor, (_,v)=>((UIElement)_).TopRightColor = v);
        protected static readonly RbfxAttribute<Color> BottomLeftColorAttr = new ("Bottom Left Color", VariantType.VarColor, new Variant(new Color(1f, 1f, 1f, 1f)), _=>((UIElement)_).BottomLeftColor, (_,v)=>((UIElement)_).BottomLeftColor = v);
        protected static readonly RbfxAttribute<Color> BottomRightColorAttr = new ("Bottom Right Color", VariantType.VarColor, new Variant(new Color(1f, 1f, 1f, 1f)), _=>((UIElement)_).BottomRightColor, (_,v)=>((UIElement)_).BottomRightColor = v);
        protected static readonly RbfxAttribute<bool> IsEnabledAttr = new ("Is Enabled", VariantType.VarBool, new Variant(false), _=>((UIElement)_).IsEnabled, (_,v)=>((UIElement)_).IsEnabled = v);
        protected static readonly RbfxAttribute<bool> IsEditableAttr = new ("Is Editable", VariantType.VarBool, new Variant(true), _=>((UIElement)_).IsEditable, (_,v)=>((UIElement)_).IsEditable = v);
        protected static readonly RbfxAttribute<bool> IsSelectedAttr = new ("Is Selected", VariantType.VarBool, new Variant(false), _=>((UIElement)_).IsSelected, (_,v)=>((UIElement)_).IsSelected = v);
        protected static readonly RbfxAttribute<bool> IsVisibleAttr = new ("Is Visible", VariantType.VarBool, new Variant(true), _=>((UIElement)_).IsVisible, (_,v)=>((UIElement)_).IsVisible = v);
        protected static readonly RbfxAttribute<bool> BringToFrontAttr = new ("Bring To Front", VariantType.VarBool, new Variant(false), _=>((UIElement)_).BringToFront, (_,v)=>((UIElement)_).BringToFront = v);
        protected static readonly RbfxAttribute<bool> BringToBackAttr = new ("Bring To Back", VariantType.VarBool, new Variant(true), _=>((UIElement)_).BringToBack, (_,v)=>((UIElement)_).BringToBack = v);
        protected static readonly RbfxAttribute<bool> ClipChildrenAttr = new ("Clip Children", VariantType.VarBool, new Variant(false), _=>((UIElement)_).ClipChildren, (_,v)=>((UIElement)_).ClipChildren = v);
        protected static readonly RbfxAttribute<bool> UseDerivedOpacityAttr = new ("Use Derived Opacity", VariantType.VarBool, new Variant(true), _=>((UIElement)_).UseDerivedOpacity, (_,v)=>((UIElement)_).UseDerivedOpacity = v);
        protected static readonly RbfxAttribute<string> FocusModeAttr = new ("Focus Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.FocusMode.NotFocusable), _=>((UIElement)_).FocusMode, (_,v)=>((UIElement)_).FocusMode = v);
        protected static readonly RbfxAttribute<string> DragAndDropModeAttr = new ("Drag And Drop Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.DragAndDropMode.Disabled), _=>((UIElement)_).DragAndDropMode, (_,v)=>((UIElement)_).DragAndDropMode = v);
        protected static readonly RbfxAttribute<string> LayoutModeAttr = new ("Layout Mode", VariantType.VarString, new Variant(UnityToRebelFork.Editor.LayoutMode.Free), _=>((UIElement)_).LayoutMode, (_,v)=>((UIElement)_).LayoutMode = v);
        protected static readonly RbfxAttribute<int> LayoutSpacingAttr = new ("Layout Spacing", VariantType.VarInt, new Variant(0), _=>((UIElement)_).LayoutSpacing, (_,v)=>((UIElement)_).LayoutSpacing = v);
        protected static readonly RbfxAttribute<IntRect> LayoutBorderAttr = new ("Layout Border", VariantType.VarIntRect, new Variant(new IntRect(0, 0, 0, 0)), _=>((UIElement)_).LayoutBorder, (_,v)=>((UIElement)_).LayoutBorder = v);
        protected static readonly RbfxAttribute<Vector2> LayoutFlexScaleAttr = new ("Layout Flex Scale", VariantType.VarVector2, new Variant(new Vector2(1f, 1f)), _=>((UIElement)_).LayoutFlexScale, (_,v)=>((UIElement)_).LayoutFlexScale = v);
        protected static readonly RbfxAttribute<int> IndentAttr = new ("Indent", VariantType.VarInt, new Variant(0), _=>((UIElement)_).Indent, (_,v)=>((UIElement)_).Indent = v);
        protected static readonly RbfxAttribute<int> IndentSpacingAttr = new ("Indent Spacing", VariantType.VarInt, new Variant(16), _=>((UIElement)_).IndentSpacing, (_,v)=>((UIElement)_).IndentSpacing = v);
        protected static readonly RbfxAttribute<Dictionary<string,Variant>> VariablesAttr = new ("Variables", VariantType.VarStringvariantmap, new Variant(new Dictionary<string,Variant>() {  }), _=>((UIElement)_).Variables, (_,v)=>((UIElement)_).Variables = v);
        protected static readonly RbfxAttribute<IList<string>> TagsAttr = new ("Tags", VariantType.VarStringList, new Variant(new List<string>() {  }), _=>((UIElement)_).Tags, (_,v)=>((UIElement)_).Tags = v);

        public override IEnumerable<RbfxAttribute> GetTypeAttributes()
        {
            yield return NameAttr;
            yield return PositionAttr;
            yield return SizeAttr;
            yield return MinSizeAttr;
            yield return MaxSizeAttr;
            yield return HorizAlignmentAttr;
            yield return VertAlignmentAttr;
            yield return MinAnchorAttr;
            yield return MaxAnchorAttr;
            yield return MinOffsetAttr;
            yield return MaxOffsetAttr;
            yield return PivotAttr;
            yield return EnableAnchorAttr;
            yield return ClipBorderAttr;
            yield return PriorityAttr;
            yield return OpacityAttr;
            yield return ColorAttr;
            yield return TopLeftColorAttr;
            yield return TopRightColorAttr;
            yield return BottomLeftColorAttr;
            yield return BottomRightColorAttr;
            yield return IsEnabledAttr;
            yield return IsEditableAttr;
            yield return IsSelectedAttr;
            yield return IsVisibleAttr;
            yield return BringToFrontAttr;
            yield return BringToBackAttr;
            yield return ClipChildrenAttr;
            yield return UseDerivedOpacityAttr;
            yield return FocusModeAttr;
            yield return DragAndDropModeAttr;
            yield return LayoutModeAttr;
            yield return LayoutSpacingAttr;
            yield return LayoutBorderAttr;
            yield return LayoutFlexScaleAttr;
            yield return IndentAttr;
            yield return IndentSpacingAttr;
            yield return VariablesAttr;
            yield return TagsAttr;
            foreach (var a in base.GetTypeAttributes()) yield return a;
        }

        protected string _name = "";

        protected IntVector2 _position = new IntVector2(0, 0);

        protected IntVector2 _size = new IntVector2(0, 0);

        protected IntVector2 _minSize = new IntVector2(0, 0);

        protected IntVector2 _maxSize = new IntVector2(2147483647, 2147483647);

        protected string _horizAlignment = UnityToRebelFork.Editor.HorizAlignment.Left;

        protected string _vertAlignment = UnityToRebelFork.Editor.VertAlignment.Top;

        protected Vector2 _minAnchor = new Vector2(0f, 0f);

        protected Vector2 _maxAnchor = new Vector2(0f, 0f);

        protected IntVector2 _minOffset = new IntVector2(0, 0);

        protected IntVector2 _maxOffset = new IntVector2(0, 0);

        protected Vector2 _pivot = new Vector2(3.4028235E+38f, 3.4028235E+38f);

        protected bool _enableAnchor = false;

        protected IntRect _clipBorder = new IntRect(0, 0, 0, 0);

        protected int _priority = 0;

        protected float _opacity = 1f;

        protected Color _color = new Color(1f, 1f, 1f, 1f);

        protected Color _topLeftColor = new Color(1f, 1f, 1f, 1f);

        protected Color _topRightColor = new Color(1f, 1f, 1f, 1f);

        protected Color _bottomLeftColor = new Color(1f, 1f, 1f, 1f);

        protected Color _bottomRightColor = new Color(1f, 1f, 1f, 1f);

        protected bool _isEnabled = false;

        protected bool _isEditable = true;

        protected bool _isSelected = false;

        protected bool _isVisible = true;

        protected bool _bringToFront = false;

        protected bool _bringToBack = true;

        protected bool _clipChildren = false;

        protected bool _useDerivedOpacity = true;

        protected string _focusMode = UnityToRebelFork.Editor.FocusMode.NotFocusable;

        protected string _dragAndDropMode = UnityToRebelFork.Editor.DragAndDropMode.Disabled;

        protected string _layoutMode = UnityToRebelFork.Editor.LayoutMode.Free;

        protected int _layoutSpacing = 0;

        protected IntRect _layoutBorder = new IntRect(0, 0, 0, 0);

        protected Vector2 _layoutFlexScale = new Vector2(1f, 1f);

        protected int _indent = 0;

        protected int _indentSpacing = 16;

        protected Dictionary<string,Variant> _variables = new Dictionary<string,Variant>() {  };

        protected IList<string> _tags = new List<string>() {  };

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public IntVector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public IntVector2 Size
        {
            get => _size;
            set => _size = value;
        }

        public IntVector2 MinSize
        {
            get => _minSize;
            set => _minSize = value;
        }

        public IntVector2 MaxSize
        {
            get => _maxSize;
            set => _maxSize = value;
        }

        public string HorizAlignment
        {
            get => _horizAlignment;
            set => _horizAlignment = value;
        }

        public string VertAlignment
        {
            get => _vertAlignment;
            set => _vertAlignment = value;
        }

        public Vector2 MinAnchor
        {
            get => _minAnchor;
            set => _minAnchor = value;
        }

        public Vector2 MaxAnchor
        {
            get => _maxAnchor;
            set => _maxAnchor = value;
        }

        public IntVector2 MinOffset
        {
            get => _minOffset;
            set => _minOffset = value;
        }

        public IntVector2 MaxOffset
        {
            get => _maxOffset;
            set => _maxOffset = value;
        }

        public Vector2 Pivot
        {
            get => _pivot;
            set => _pivot = value;
        }

        public bool EnableAnchor
        {
            get => _enableAnchor;
            set => _enableAnchor = value;
        }

        public IntRect ClipBorder
        {
            get => _clipBorder;
            set => _clipBorder = value;
        }

        public int Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public float Opacity
        {
            get => _opacity;
            set => _opacity = value;
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public Color TopLeftColor
        {
            get => _topLeftColor;
            set => _topLeftColor = value;
        }

        public Color TopRightColor
        {
            get => _topRightColor;
            set => _topRightColor = value;
        }

        public Color BottomLeftColor
        {
            get => _bottomLeftColor;
            set => _bottomLeftColor = value;
        }

        public Color BottomRightColor
        {
            get => _bottomRightColor;
            set => _bottomRightColor = value;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public bool IsEditable
        {
            get => _isEditable;
            set => _isEditable = value;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public bool BringToFront
        {
            get => _bringToFront;
            set => _bringToFront = value;
        }

        public bool BringToBack
        {
            get => _bringToBack;
            set => _bringToBack = value;
        }

        public bool ClipChildren
        {
            get => _clipChildren;
            set => _clipChildren = value;
        }

        public bool UseDerivedOpacity
        {
            get => _useDerivedOpacity;
            set => _useDerivedOpacity = value;
        }

        public string FocusMode
        {
            get => _focusMode;
            set => _focusMode = value;
        }

        public string DragAndDropMode
        {
            get => _dragAndDropMode;
            set => _dragAndDropMode = value;
        }

        public string LayoutMode
        {
            get => _layoutMode;
            set => _layoutMode = value;
        }

        public int LayoutSpacing
        {
            get => _layoutSpacing;
            set => _layoutSpacing = value;
        }

        public IntRect LayoutBorder
        {
            get => _layoutBorder;
            set => _layoutBorder = value;
        }

        public Vector2 LayoutFlexScale
        {
            get => _layoutFlexScale;
            set => _layoutFlexScale = value;
        }

        public int Indent
        {
            get => _indent;
            set => _indent = value;
        }

        public int IndentSpacing
        {
            get => _indentSpacing;
            set => _indentSpacing = value;
        }

        public Dictionary<string,Variant> Variables
        {
            get => _variables;
            set => _variables = value;
        }

        public IList<string> Tags
        {
            get => _tags;
            set => _tags = value;
        }
    }
}
