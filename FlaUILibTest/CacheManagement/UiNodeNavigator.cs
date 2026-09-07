using System.Xml;
using System.Xml.XPath;

class UiNodeNavigator : XPathNavigator
{
    private const int NoAttributeValue = -1;

    UiNode _currentElement;
    readonly UiNode _rootElement;
    readonly UiNodeWalker _walker;
    readonly XmlNameTable _nameTable;
    private int _attributeIndex = NoAttributeValue;

    public UiNodeNavigator(UiNode root, UiNodeWalker walker)
    {
        _rootElement = root;
        _currentElement = root;
        _walker = walker;
        _nameTable = new NameTable();
    }

    UiNodeNavigator(UiNodeNavigator source)
    {
        _rootElement = source._rootElement;
        _currentElement = source._currentElement;
        _walker = source._walker;
        _attributeIndex = source._attributeIndex;
        _nameTable = source._nameTable;
    }

    public UiNode Current => _currentElement;

    private bool IsInAttribute => _attributeIndex != NoAttributeValue;

    public override object UnderlyingObject => _currentElement;
    public override string BaseURI => string.Empty;
    public override string NamespaceURI => string.Empty;
    public override string Prefix => string.Empty;
    public override bool IsEmptyElement => false;
    public override XmlNameTable NameTable => _nameTable;
    public override bool HasAttributes => !IsInAttribute;

    public override string LocalName
    {
        get
        {
            if (IsInAttribute)
                return GetAttributeName(_attributeIndex);

            return _currentElement.ControlType.ToString();
        }
    }

    public override string Name => LocalName;

    public override string Value => IsInAttribute
        ? GetAttributeValue(_attributeIndex)
        : _currentElement.Name ?? string.Empty;

    public override XPathNodeType NodeType
    {
        get
        {
            if (IsInAttribute)
                return XPathNodeType.Attribute;
            if (_currentElement.Equals(_rootElement))
                return XPathNodeType.Root;
            return XPathNodeType.Element;
        }
    }

    public override bool MoveToFirstChild()
    {
        if (IsInAttribute) { return false; }
        var child = _walker.MoveFirstChild(_currentElement);
        if (child == null) return false;
        _currentElement = child;
        return true;
    }

    public override bool MoveToNext()
    {
        if (IsInAttribute) { return false; }
        var sibling = _walker.MoveNextSibling(_currentElement);
        if (sibling == null) return false;
        _currentElement = sibling;
        return true;
    }

    public override bool MoveToPrevious()
    {
        if (IsInAttribute) { return false; }
        if (_currentElement.Parent == null) return false;
        var sibling = _walker.MovePrevSibling(_currentElement);
        if (sibling == null) return false;
        _currentElement = sibling;
        return true;
    }

    public override bool MoveToParent()
    {
        if (IsInAttribute)
        {
            _attributeIndex = NoAttributeValue;
            return true;
        }
        var parent = _walker.MoveParent(_currentElement);
        if (parent == null) return false;
        _currentElement = parent;
        return true;
    }

    public override void MoveToRoot()
    {
        _attributeIndex = NoAttributeValue;
        _currentElement = _rootElement;
    }

    public override bool MoveToId(string id) => false;

    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) => throw new NotImplementedException();
    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) => throw new NotImplementedException();

    public override string GetAttribute(string localName, string namespaceURI) => localName switch
    {
        "Name" => _currentElement.Name ?? string.Empty,
        "ControlType" => _currentElement.ControlType.ToString(),
        _ => string.Empty
    };

    public override bool MoveToFirstAttribute()
    {
        if (IsInAttribute)
            return false;
        _attributeIndex = 0;
        return true;
    }

    public override bool MoveToNextAttribute()
    {
        if (_attributeIndex >= Enum.GetNames(typeof(ElementAttributes)).Length - 1)
            return false;
        if (!IsInAttribute)
            return false;
        _attributeIndex++;
        return true;
    }

    public override bool MoveToAttribute(string localName, string namespaceUri)
    {
        if (IsInAttribute)
            return false;
        var attributeIndex = GetAttributeIndexFromName(localName);
        if (attributeIndex != NoAttributeValue)
        {
            _attributeIndex = attributeIndex;
            return true;
        }
        return false;
    }

    private string GetAttributeValue(int attributeIndex) => attributeIndex switch
    {
        0 => _currentElement.Name ?? string.Empty,
        1 => _currentElement.ControlType.ToString(),
        _ => string.Empty
    };

    private string GetAttributeName(int attributeIndex)
    {
        var name = Enum.GetName(typeof(ElementAttributes), attributeIndex);
        if (name == null)
            throw new ArgumentOutOfRangeException(nameof(attributeIndex));
        return name;
    }

    private int GetAttributeIndexFromName(string attributeName)
    {
        if (Enum.TryParse(attributeName, out ElementAttributes parsedValue))
            return (int)parsedValue;
        return NoAttributeValue;
    }

    public override XPathNavigator Clone() => new UiNodeNavigator(this);

    public override bool IsSamePosition(XPathNavigator other)
        => other is UiNodeNavigator navigator && ReferenceEquals(_currentElement, navigator._currentElement);

    public override bool MoveTo(XPathNavigator other)
    {
        var specificNavigator = other as UiNodeNavigator;
        if (specificNavigator == null)
            return false;
        if (!_rootElement.Equals(specificNavigator._rootElement))
            return false;
        _currentElement = specificNavigator._currentElement;
        _attributeIndex = specificNavigator._attributeIndex;
        return true;
    }
}
