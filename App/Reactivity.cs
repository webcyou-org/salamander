using System;
using System.Collections.Generic;
using System.ComponentModel;

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

