using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

public delegate void EffectFn();
public delegate T Getter<T>();

public interface IComputed<T>
{
    T Value { get; }
    EffectFn Effect { get; }
}

public interface IEffectOptions
{
    bool Computed { get; set; }
    bool Lazy { get; set; }
}

public class EffectOptions : IEffectOptions
{
    public bool Computed { get; set; }
    public bool Lazy { get; set; }
}

public class ReactiveObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, string propertyName)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        return false;
    }
}

public class DeepReactive : ReactiveObject
{
    private readonly object _target;
    private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();

    protected DeepReactive(object target)
    {
        _target = target;
        var type = target.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead && prop.CanWrite)
            {
                _properties[prop.Name] = prop;
                var value = prop.GetValue(target);
                if (value != null && IsComplexType(prop.PropertyType))
                {
                    var reactiveValue = Create(prop.PropertyType, value);
                    prop.SetValue(target, reactiveValue);
                }
            }
        }
    }

    public static object Create(Type type, object target)
    {
        var reactiveType = typeof(DeepReactive<>).MakeGenericType(type);
        return Activator.CreateInstance(reactiveType, target);
    }

    public static T Create<T>(T target) where T : class
    {
        return (T)Create(typeof(T), target);
    }

    private bool IsComplexType(Type type)
    {
        return type.IsClass && type != typeof(string);
    }

    public T GetProperty<T>(string propertyName)
    {
        if (_properties.ContainsKey(propertyName))
        {
            return (T)_properties[propertyName].GetValue(_target);
        }
        throw new ArgumentException($"Property {propertyName} not found on {_target.GetType().Name}");
    }

    public void SetProperty<T>(string propertyName, T value)
    {
        if (_properties.ContainsKey(propertyName))
        {
            var oldValue = _properties[propertyName].GetValue(_target);
            if (!Equals(oldValue, value))
            {
                if (value != null && IsComplexType(value.GetType()))
                {
                    var reactiveValue = Create(value.GetType(), value);
                    _properties[propertyName].SetValue(_target, reactiveValue);
                }
                else
                {
                    _properties[propertyName].SetValue(_target, value);
                }
                OnPropertyChanged(propertyName);
            }
        }
        else
        {
            throw new ArgumentException($"Property {propertyName} not found on {_target.GetType().Name}");
        }
    }
}

public class DeepReactive<T> : DeepReactive where T : class
{
    public DeepReactive(T target) : base(target) { }
}

public static class ReactiveSystem
{
    private static EffectFn activeEffect;
    private static readonly Dictionary<object, Dictionary<string, HashSet<EffectFn>>> targetMap = new();

    public static void Effect(Action fn, EffectOptions options = null)
    {
        EffectFn effectFn = null;
        effectFn = () =>
        {
            activeEffect = effectFn;
            fn();
            activeEffect = null;
        };
        if (options?.Computed == true)
        {
            effectFn();
        }
        else
        {
            effectFn.Invoke();
        }
    }

    public static void Track(object target, string propertyName)
    {
        if (activeEffect == null) return;

        if (!targetMap.ContainsKey(target))
        {
            targetMap[target] = new Dictionary<string, HashSet<EffectFn>>();
        }

        if (!targetMap[target].ContainsKey(propertyName))
        {
            targetMap[target][propertyName] = new HashSet<EffectFn>();
        }

        targetMap[target][propertyName].Add(activeEffect);
    }

    public static void Trigger(object target, string propertyName)
    {
        if (!targetMap.ContainsKey(target) || !targetMap[target].ContainsKey(propertyName)) return;

        foreach (var effect in targetMap[target][propertyName])
        {
            effect();
        }
    }
}

public class ReactiveProperty<T> : ReactiveObject
{
    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value, nameof(Value)))
            {
                ReactiveSystem.Trigger(this, nameof(Value));
            }
        }
    }

    public ReactiveProperty(T initialValue = default)
    {
        _value = initialValue;
    }
}

public class Computed<T> : IComputed<T>
{
    private T _value;
    private readonly Getter<T> _getter;
    private bool _dirty = true;

    public EffectFn Effect { get; }

    public T Value
    {
        get
        {
            if (_dirty)
            {
                _value = _getter();
                _dirty = false;
            }
            return _value;
        }
    }

    public Computed(Getter<T> getter)
    {
        _getter = getter;
        Effect = () =>
        {
            _dirty = true;
            ReactiveSystem.Effect(() => _getter());
        };
    }
}

