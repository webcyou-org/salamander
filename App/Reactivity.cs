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

public class DeepReactive<T> : DispatchProxy where T : class
{
    private T _target;
    private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        var methodName = targetMethod.Name;

        if (methodName.StartsWith("get_"))
        {
            var propertyName = methodName.Substring(4);
            if (_properties.ContainsKey(propertyName))
            {
                return _properties[propertyName].GetValue(_target);
            }
        }
        else if (methodName.StartsWith("set_"))
        {
            var propertyName = methodName.Substring(4);
            if (_properties.ContainsKey(propertyName))
            {
                var oldValue = _properties[propertyName].GetValue(_target);
                if (!Equals(oldValue, args[0]))
                {
                    _properties[propertyName].SetValue(_target, args[0]);
                    OnPropertyChanged(propertyName);
                }
            }
        }

        return targetMethod.Invoke(_target, args);
    }

    public static T Create(T target)
    {
        object proxy = Create<T, DeepReactive<T>>();
        ((DeepReactive<T>)proxy).SetParameters(target);
        return (T)proxy;
    }

    private void SetParameters(T target)
    {
        _target = target;
        var type = typeof(T);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead && prop.CanWrite)
            {
                _properties[prop.Name] = prop;
            }
        }
    }

    private event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(_target, new PropertyChangedEventArgs(propertyName));
    }

    public void AddPropertyChangedHandler(PropertyChangedEventHandler handler)
    {
        PropertyChanged += handler;
    }

    public void RemovePropertyChangedHandler(PropertyChangedEventHandler handler)
    {
        PropertyChanged -= handler;
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

