using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
/// <summary>
/// Baseclass that allows persisting of scalar values as a collection (which is not supported by EF 4.3)
/// </summary>
/// <typeparam name="T">Type of the single collection entry that should be persisted.</typeparam>
[ComplexType]
public class EFIntCollection : EFPrimitiveCollection<int>
{
    public override int ConvertFromString(string value) => int.Parse(value);
    public override string ConvertToString(int value) => value.ToString();
}


[ComplexType]
public abstract class EFPrimitiveCollection<T> : IList<T>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual int Id { get; set; }

    const string DefaultValueSeperator = "|";
    readonly string[] DefaultValueSeperators = new string[] { DefaultValueSeperator };

    [NotMapped]
    private List<T> _data;

    [NotMapped]
    private string _value;

    [NotMapped]
    private bool _loaded;

    protected virtual string ValueSeparator => DefaultValueSeperator;
    protected virtual string[] ValueSeperators => DefaultValueSeperators;

    [MaxLength]
    public virtual string Value // Change this to public if you prefer not to use the ShadowColumnAttribute
    {
        get => _value;
        set
        {
            _data.Clear();
            _value = value;

            if (string.IsNullOrWhiteSpace(_value))
                return;

            _data = _value.Split(ValueSeperators, StringSplitOptions.None)
                        .Select(x => ConvertFromString(x)).ToList();

            if (!_loaded) _loaded = true;
        }
    }

    public EFPrimitiveCollection()
    {
        _data = new List<T>();
    }

    void UpdateValue()
    {
        _value = string.Join(ValueSeparator.ToString(),
                _data.Select(x => ConvertToString(x))
                .ToArray());
    }

    public abstract T ConvertFromString(string value);
    public abstract string ConvertToString(T value);

    #region IList Implementation
    public int Count
    {
        get
        {
            EnsureData();
            return _data.Count;
        }
    }

    public T this[int index]
    {
        get
        {
            EnsureData();
            return _data[index];
        }
        set
        {
            EnsureData();
            _data[index] = value;
        }
    }

    public bool IsReadOnly => false;

    void EnsureData()
    {
        if (_loaded)
            return;

        if (string.IsNullOrWhiteSpace(_value))
            return;

        if (_data.Count > 0) return;


        if (!_loaded) _loaded = true;
        _data = _value.Split(ValueSeperators, StringSplitOptions.None)
                        .Select(x => ConvertFromString(x)).ToList();
    }

    public void Add(T item)
    {
        EnsureData();

        _data.Add(item);
        UpdateValue();
    }

    public bool Remove(T item)
    {
        EnsureData();

        bool res = _data.Remove(item);
        UpdateValue();

        return res;
    }

    public void Clear()
    {
        _data.Clear();
        UpdateValue();
    }

    public bool Contains(T item)
    {
        EnsureData();
        return _data.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        EnsureData();
        _data.CopyTo(array, arrayIndex);
    }

    public int IndexOf(T item)
    {
        EnsureData();
        return _data.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        EnsureData();
        _data.Insert(index, item);
        UpdateValue();
    }

    public void RemoveAt(int index)
    {
        EnsureData();
        _data.RemoveAt(index);
        UpdateValue();
    }

    public void AddRange(IEnumerable<T> collection)
    {
        EnsureData();
        _data.AddRange(collection);
        UpdateValue();
    }

    public IEnumerator<T> GetEnumerator()
    {
        EnsureData();
        return _data.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        EnsureData();
        return _data.GetEnumerator();
    }
    #endregion
}