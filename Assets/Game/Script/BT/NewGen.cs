using System;
using System.Collections.Generic;
using UnityEngine;

// ====================================================================
// БАЗОВЫЕ ТИПЫ И ИНТЕРФЕЙСЫ
// ====================================================================

/// <summary>
/// Состояния узлов дерева поведения
/// </summary>
public enum NodeState
{
    Success,    // Успешное завершение
    Failure,    // Неудачное завершение  
    Running     // Выполняется (асинхронно)
}

/// <summary>
/// Интерфейс узла дерева поведения
/// </summary>
public interface IBehaviorNode
{
    /// <summary>
    /// Выполнить узел
    /// </summary>
    /// <param name="blackboard">Черная доска для обмена данными</param>
    /// <returns>Состояние выполнения</returns>
    NodeState Tick(Blackboard blackboard);
    
    /// <summary>
    /// Сбросить состояние узла
    /// </summary>
    /// <param name="blackboard">Черная доска</param>
    void Reset(Blackboard blackboard);
}

// ====================================================================
// BLACKBOARD - ЧЕРНАЯ ДОСКА ДЛЯ ОБМЕНА ДАННЫМИ
// ====================================================================

/// <summary>
/// Черная доска для обмена данными между узлами дерева поведения
/// </summary>
public class Blackboard
{
    // Используем object pool для ключей, чтобы уменьшить аллокации
    private static readonly Dictionary<string, string> KeyCache = new Dictionary<string, string>();
    
    private Dictionary<string, object> data = new Dictionary<string, object>();

    /// <summary>
    /// Получить или создать ключ (для уменьшения аллокаций строк)
    /// </summary>
    private static string GetKey(string key)
    {
        if (!KeyCache.TryGetValue(key, out var cachedKey))
        {
            cachedKey = key;
            KeyCache[key] = cachedKey;
        }
        return cachedKey;
    }

    /// <summary>
    /// Установить значение по ключу
    /// </summary>
    public void Set<T>(string key, T value) => data[GetKey(key)] = value;

    /// <summary>
    /// Получить значение по ключу
    /// </summary>
    public T Get<T>(string key) => data.TryGetValue(GetKey(key), out var val) ? (T)val : default(T);

    /// <summary>
    /// Проверить наличие ключа
    /// </summary>
    public bool Has(string key) => data.ContainsKey(GetKey(key));

    /// <summary>
    /// Удалить значение по ключу
    /// </summary>
    public void Remove(string key) => data.Remove(GetKey(key));

    /// <summary>
    /// Очистить все значения с указанным префиксом
    /// </summary>
    public void ClearPrefix(string prefix)
    {
        var keysToRemove = new List<string>();
        foreach (var kvp in data)
        {
            if (kvp.Key.StartsWith(prefix))
                keysToRemove.Add(kvp.Key);
        }
        
        foreach (var key in keysToRemove)
        {
            data.Remove(key);
        }
    }

    /// <summary>
    /// Получить ссылку на значение в черной доске для удобного доступа
    /// </summary>
    public BlackboardValue<T> Ref<T>(string key) => new BlackboardValue<T>(this, key);
    
    /// <summary>
    /// Очистить всю черную доску
    /// </summary>
    public void Clear() => data.Clear();
}

/// <summary>
/// Ссылка на значение в черной доске для типизированного доступа
/// </summary>
public class BlackboardValue<T>
{
    private readonly Blackboard _blackboard;
    private readonly string _key;

    public BlackboardValue(Blackboard blackboard, string key)
    {
        _blackboard = blackboard;
        _key = key;
    }

    /// <summary>
    /// Текущее значение
    /// </summary>
    public T Value => _blackboard.Get<T>(_key);

    /// <summary>
    /// Установить новое значение
    /// </summary>
    public void Set(T value) => _blackboard.Set(_key, value);

    /// <summary>
    /// Неявное преобразование к типу T
    /// </summary>
    public static implicit operator T(BlackboardValue<T> value) => value.Value;
}

// ====================================================================
// БАЗОВЫЕ УЗЛЫ
// ====================================================================

/// <summary>
/// Базовый класс для узлов с ID для сброса состояния
/// </summary>
public abstract class BaseBehaviorNode : IBehaviorNode
{
    protected readonly string id = Guid.NewGuid().ToString();
    
    public abstract NodeState Tick(Blackboard blackboard);
    
    public virtual void Reset(Blackboard blackboard)
    {
        // Очищаем состояние по префиксу ID
        blackboard.ClearPrefix(id + "_");
    }
}

/// <summary>
/// Узел-условие с лямбда-выражением
/// </summary>
public class LambdaConditionNode : BaseBehaviorNode
{
    private readonly Func<Blackboard, bool> _condition;

    public LambdaConditionNode(Func<Blackboard, bool> condition)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        try
        {
            return _condition(blackboard) ? NodeState.Success : NodeState.Failure;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in condition node: {ex.Message}");
            return NodeState.Failure;
        }
    }
}

/// <summary>
/// Узел-действие с лямбда-выражением
/// </summary>
public class LambdaActionNode : BaseBehaviorNode
{
    private readonly Func<Blackboard, NodeState> _action;

    public LambdaActionNode(Func<Blackboard, NodeState> action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        try
        {
            return _action(blackboard);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in action node: {ex.Message}");
            return NodeState.Failure;
        }
    }
}

// ====================================================================
// КОМПОЗИТНЫЕ УЗЛЫ
// ====================================================================

/// <summary>
/// Последовательность - выполняет узлы по порядку до первого Failure
/// </summary>
public class SequenceNode : BaseBehaviorNode
{
    private readonly List<IBehaviorNode> _children;
    private int _current;
    private string _currentKey;

    public SequenceNode(List<IBehaviorNode> children)
    {
        _children = children ?? new List<IBehaviorNode>();
        _current = 0;
        _currentKey = id + "_current";
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        // Восстанавливаем состояние из blackboard
        _current = blackboard.Get<int>(_currentKey);
        
        while (_current < _children.Count)
        {
            var state = _children[_current].Tick(blackboard);
            if (state == NodeState.Running)
            {
                blackboard.Set(_currentKey, _current);
                return NodeState.Running;
            }

            if (state == NodeState.Failure)
            {
                Reset(blackboard);
                return NodeState.Failure;
            }

            _current++;
        }

        Reset(blackboard);
        return NodeState.Success;
    }

    public override void Reset(Blackboard blackboard)
    {
        _current = 0;
        blackboard.Set(_currentKey, _current);
        foreach (var child in _children)
        {
            child.Reset(blackboard);
        }
        base.Reset(blackboard);
    }
}

/// <summary>
/// Селектор - выполняет узлы по порядку до первого Success
/// </summary>
public class SelectorNode : BaseBehaviorNode
{
    private readonly List<IBehaviorNode> _children;
    private int _current;
    private string _currentKey;

    public SelectorNode(List<IBehaviorNode> children)
    {
        _children = children ?? new List<IBehaviorNode>();
        _current = 0;
        _currentKey = id + "_current";
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        // Восстанавливаем состояние из blackboard
        _current = blackboard.Get<int>(_currentKey);
        
        while (_current < _children.Count)
        {
            var state = _children[_current].Tick(blackboard);
            if (state == NodeState.Running)
            {
                blackboard.Set(_currentKey, _current);
                return NodeState.Running;
            }

            if (state == NodeState.Success)
            {
                Reset(blackboard);
                return NodeState.Success;
            }

            _current++;
        }

        Reset(blackboard);
        return NodeState.Failure;
    }

    public override void Reset(Blackboard blackboard)
    {
        _current = 0;
        blackboard.Set(_currentKey, _current);
        foreach (var child in _children)
        {
            child.Reset(blackboard);
        }
        base.Reset(blackboard);
    }
}

/// <summary>
/// Параллельный узел - выполняет все узлы одновременно
/// </summary>
public class ParallelNode : BaseBehaviorNode
{
    private readonly List<IBehaviorNode> _children;
    private readonly int _successThreshold;

    public ParallelNode(int successThreshold, List<IBehaviorNode> children)
    {
        _successThreshold = successThreshold;
        _children = children ?? new List<IBehaviorNode>();
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        int successCount = 0;
        foreach (var child in _children)
        {
            var state = child.Tick(blackboard);
            if (state == NodeState.Failure)
            {
                Reset(blackboard);
                return NodeState.Failure;
            }

            if (state == NodeState.Success) successCount++;
        }

        if (successCount >= _successThreshold)
        {
            Reset(blackboard);
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    public override void Reset(Blackboard blackboard)
    {
        foreach (var child in _children)
        {
            child.Reset(blackboard);
        }
        base.Reset(blackboard);
    }
}

// ====================================================================
// ДЕКОРАТОРЫ
// ====================================================================

/// <summary>
/// Декоратор с таймером охлаждения
/// </summary>
public class CooldownDecoratorNode : BaseBehaviorNode
{
    private readonly IBehaviorNode _child;
    private readonly float _cooldown;
    private string _lastKey;

    public CooldownDecoratorNode(IBehaviorNode child, float cooldown)
    {
        _child = child ?? throw new ArgumentNullException(nameof(child));
        _cooldown = cooldown;
        _lastKey = id + "_last";
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        float last = blackboard.Get<float>(_lastKey);
        if (Time.time - last < _cooldown)
            return NodeState.Failure;
            
        var result = _child.Tick(blackboard);
        if (result == NodeState.Success)
            blackboard.Set(_lastKey, Time.time);
        return result;
    }

    public override void Reset(Blackboard blackboard)
    {
        _child.Reset(blackboard);
        base.Reset(blackboard);
    }
}

/// <summary>
/// Декоратор повторения
/// </summary>
public class RepeaterDecoratorNode : BaseBehaviorNode
{
    private readonly IBehaviorNode _child;
    private readonly int _repeatCount;
    private string _countKey;

    public RepeaterDecoratorNode(IBehaviorNode child, int repeatCount)
    {
        _child = child ?? throw new ArgumentNullException(nameof(child));
        _repeatCount = repeatCount;
        _countKey = id + "_count";
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        int current = blackboard.Get<int>(_countKey);
        var state = _child.Tick(blackboard);
        
        if (state == NodeState.Running)
            return NodeState.Running;

        current++;
        _child.Reset(blackboard);
        
        if (_repeatCount < 0 || current < _repeatCount)
        {
            blackboard.Set(_countKey, current);
            return NodeState.Running;
        }

        blackboard.Set(_countKey, 0);
        return state;
    }

    public override void Reset(Blackboard blackboard)
    {
        blackboard.Set(_countKey, 0);
        _child.Reset(blackboard);
        base.Reset(blackboard);
    }
}

/// <summary>
/// Узел ожидания
/// </summary>
public class WaitNode : BaseBehaviorNode
{
    private readonly float _duration;
    private string _startKey;
    private string _startedKey;

    public WaitNode(float duration)
    {
        _duration = duration;
        _startKey = id + "_start";
        _startedKey = id + "_started";
    }

    public override NodeState Tick(Blackboard blackboard)
    {
        bool started = blackboard.Get<bool>(_startedKey);
        if (!started)
        {
            blackboard.Set(_startKey, Time.time);
            blackboard.Set(_startedKey, true);
        }

        if (Time.time - blackboard.Get<float>(_startKey) < _duration)
            return NodeState.Running;
            
        blackboard.Set(_startedKey, false);
        return NodeState.Success;
    }

    public override void Reset(Blackboard blackboard)
    {
        blackboard.Set(_startedKey, false);
        base.Reset(blackboard);
    }
}

// ====================================================================
// BUILDER PATTERN - СТРОИТЕЛЬ ДЕРЕВА ПОВЕДЕНИЯ
// ====================================================================

/// <summary>
/// Строитель дерева поведения с Fluent API
/// </summary>
public class BehaviorTreeBuilder
{
    private Blackboard _blackboard;
    private Stack<List<IBehaviorNode>> _nodeStack = new Stack<List<IBehaviorNode>>();
    private List<IBehaviorNode> _currentNodes = new List<IBehaviorNode>();
    private IBehaviorNode _root;

    /// <summary>
    /// Создать строитель дерева поведения
    /// </summary>
    /// <param name="blackboard">Черная доска (создается автоматически если не указана)</param>
    public BehaviorTreeBuilder(Blackboard blackboard = null)
    {
        _blackboard = blackboard ?? new Blackboard();
        _nodeStack.Push(_currentNodes);
    }

    #region УСЛОВИЯ (CONDITIONS)

    /// <summary>
    /// Условие: значение по ключу равно true
    /// </summary>
    public BehaviorTreeBuilder When(string key)
    {
        return Condition(bb => bb.Get<bool>(key));
    }

    /// <summary>
    /// Условие: значение по ключу равно false
    /// </summary>
    public BehaviorTreeBuilder WhenNot(string key)
    {
        return Condition(bb => !bb.Get<bool>(key));
    }

    /// <summary>
    /// Условие: значение по ключу равно указанному значению
    /// </summary>
    public BehaviorTreeBuilder WhenEquals<T>(string key, T value) where T : IEquatable<T>
    {
        return Condition(bb => bb.Get<T>(key).Equals(value));
    }

    /// <summary>
    /// Условие: числовое значение больше указанного
    /// </summary>
    public BehaviorTreeBuilder WhenGreaterThan(string key, float value)
    {
        return Condition(bb => bb.Get<float>(key) > value);
    }

    /// <summary>
    /// Условие: числовое значение меньше указанного
    /// </summary>
    public BehaviorTreeBuilder WhenLessThan(string key, float value)
    {
        return Condition(bb => bb.Get<float>(key) < value);
    }

    /// <summary>
    /// Добавить пользовательское условие
    /// </summary>
    public BehaviorTreeBuilder Condition(Func<Blackboard, bool> condition)
    {
        var node = new LambdaConditionNode(condition);
        _currentNodes.Add(node);
        return this;
    }

    #endregion

    #region ДЕЙСТВИЯ (ACTIONS)

    /// <summary>
    /// Выполнить действие
    /// </summary>
    public BehaviorTreeBuilder Do(Action<Blackboard> action)
    {
        return Action(bb => 
        { 
            action(bb); 
            return NodeState.Success; 
        });
    }

    /// <summary>
    /// Выполнить действие с возвратом состояния
    /// </summary>
    public BehaviorTreeBuilder Do(Func<Blackboard, NodeState> action)
    {
        return Action(action);
    }

    /// <summary>
    /// Установить значение в черной доске
    /// </summary>
    public BehaviorTreeBuilder Set<T>(string key, T value)
    {
        return Action(bb => 
        { 
            bb.Set(key, value);
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Установить значение true
    /// </summary>
    public BehaviorTreeBuilder SetTrue(string key)
    {
        return Set(key, true);
    }

    /// <summary>
    /// Установить значение false
    /// </summary>
    public BehaviorTreeBuilder SetFalse(string key)
    {
        return Set(key, false);
    }

    /// <summary>
    /// Вывести сообщение в лог
    /// </summary>
    public BehaviorTreeBuilder Log(string message)
    {
        return Action(bb => 
        { 
            Debug.Log($"[BehaviorTree] {message}");
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Вывести значение из черной доски в лог
    /// </summary>
    public BehaviorTreeBuilder LogValue<T>(string key)
    {
        return Action(bb => 
        { 
            Debug.Log($"[BehaviorTree] {key} = {bb.Get<T>(key)}");
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Увеличить числовое значение
    /// </summary>
    public BehaviorTreeBuilder Increment(string key, float amount = 1.0f)
    {
        return Action(bb => 
        { 
            var current = bb.Get<float>(key);
            bb.Set(key, current + amount);
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Уменьшить числовое значение
    /// </summary>
    public BehaviorTreeBuilder Decrement(string key, float amount = 1.0f)
    {
        return Action(bb => 
        { 
            var current = bb.Get<float>(key);
            bb.Set(key, current - amount);
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Добавить элемент в список
    /// </summary>
    public BehaviorTreeBuilder AddToList<T>(string listKey, T item)
    {
        return Action(bb => 
        { 
            var list = bb.Get<List<T>>(listKey) ?? new List<T>();
            list.Add(item);
            bb.Set(listKey, list);
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Удалить элемент из списка
    /// </summary>
    public BehaviorTreeBuilder RemoveFromList<T>(string listKey, T item)
    {
        return Action(bb => 
        { 
            var list = bb.Get<List<T>>(listKey);
            if (list != null)
            {
                list.Remove(item);
                bb.Set(listKey, list);
            }
            return NodeState.Success;
        });
    }

    /// <summary>
    /// Добавить пользовательское действие
    /// </summary>
    public BehaviorTreeBuilder Action(Func<Blackboard, NodeState> action)
    {
        var node = new LambdaActionNode(action);
        _currentNodes.Add(node);
        return this;
    }

    #endregion

    #region КОМПОЗИТЫ

    /// <summary>
    /// Создать последовательность (Sequence)
    /// </summary>
    public BehaviorTreeBuilder Sequence()
    {
        _nodeStack.Push(_currentNodes);
        _currentNodes = new List<IBehaviorNode>();
        return this;
    }

    /// <summary>
    /// Создать селектор (Selector)
    /// </summary>
    public BehaviorTreeBuilder Selector()
    {
        _nodeStack.Push(_currentNodes);
        _currentNodes = new List<IBehaviorNode>();
        return this;
    }

    /// <summary>
    /// Создать параллельный узел (Parallel)
    /// </summary>
    public BehaviorTreeBuilder Parallel(int successCount = 1)
    {
        _nodeStack.Push(_currentNodes);
        _currentNodes = new List<IBehaviorNode>();
        return this;
    }

    /// <summary>
    /// Завершить текущий композитный узел
    /// </summary>
    public BehaviorTreeBuilder End()
    {
        var children = _currentNodes;
        _currentNodes = _nodeStack.Pop();
        
        if (_nodeStack.Count > 0)
        {
            // Создаем композитный узел
            IBehaviorNode compositeNode;
            
            if (children.Count > 0)
            {
                // Определяем тип по последнему узлу в родителе (если есть)
                if (_currentNodes.Count > 0)
                {
                    var lastNodeName = _currentNodes[_currentNodes.Count - 1].GetType().Name;
                    if (lastNodeName.Contains("Selector"))
                    {
                        compositeNode = new SelectorNode(children);
                    }
                    else if (lastNodeName.Contains("Parallel"))
                    {
                        compositeNode = new ParallelNode(1, children);
                    }
                    else
                    {
                        compositeNode = new SequenceNode(children);
                    }
                }
                else
                {
                    compositeNode = new SequenceNode(children);
                }
            }
            else
            {
                compositeNode = new SequenceNode(children);
            }
            
            _currentNodes.Add(compositeNode);
        }
        else
        {
            // Это корень дерева
            if (children.Count == 1)
            {
                _root = children[0];
            }
            else
            {
                _root = new SequenceNode(children);
            }
        }
        
        return this;
    }

    #endregion

    #region ДЕКОРАТОРЫ

    /// <summary>
    /// Добавить ожидание
    /// </summary>
    public BehaviorTreeBuilder Wait(float seconds)
    {
        _currentNodes.Add(new WaitNode(seconds));
        return this;
    }

    /// <summary>
    /// Добавить таймер охлаждения к последнему узлу
    /// </summary>
    public BehaviorTreeBuilder Cooldown(float seconds)
    {
        if (_currentNodes.Count > 0)
        {
            var lastNode = _currentNodes[_currentNodes.Count - 1];
            _currentNodes[_currentNodes.Count - 1] = 
                new CooldownDecoratorNode(lastNode, seconds);
        }
        return this;
    }

    /// <summary>
    /// Добавить повторение к последнему узлу
    /// </summary>
    public BehaviorTreeBuilder Repeat(int count = -1)
    {
        if (_currentNodes.Count > 0)
        {
            var lastNode = _currentNodes[_currentNodes.Count - 1];
            _currentNodes[_currentNodes.Count - 1] = 
                new RepeaterDecoratorNode(lastNode, count);
        }
        return this;
    }

    #endregion

    /// <summary>
    /// Построить дерево поведения
    /// </summary>
    public IBehaviorNode Build()
    {
        return _root;
    }

    /// <summary>
    /// Установить черную доску для дерева
    /// </summary>
    public BehaviorTreeBuilder WithBlackboard(Blackboard blackboard)
    {
        _blackboard = blackboard;
        return this;
    }
}

/// <summary>
/// Статический класс для удобного создания деревьев поведения
/// </summary>
public static class BehaviorTree
{
    /// <summary>
    /// Создать строитель дерева поведения
    /// </summary>
    public static BehaviorTreeBuilder Create(Blackboard blackboard = null)
    {
        return new BehaviorTreeBuilder(blackboard);
    }
}

// ====================================================================
// RUNTIME - КЛАСС ДЛЯ ВЫПОЛНЕНИЯ ДЕРЕВА
// ====================================================================

/// <summary>
/// Runtime класс для выполнения дерева поведения в игровом цикле
/// </summary>
public class BehaviorTreeRunner
{
    private readonly IBehaviorNode _root;
    private readonly Blackboard _blackboard;
    private bool _isRunning = false;

    public BehaviorTreeRunner(IBehaviorNode root, Blackboard blackboard)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
    }

    /// <summary>
    /// Запустить выполнение дерева поведения
    /// </summary>
    public void Start()
    {
        _isRunning = true;
    }

    /// <summary>
    /// Остановить выполнение дерева поведения
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _root.Reset(_blackboard);
    }

    /// <summary>
    /// Выполнить один тик дерева поведения
    /// </summary>
    public NodeState Tick()
    {
        if (!_isRunning)
            return NodeState.Failure;

        return _root.Tick(_blackboard);
    }

    /// <summary>
    /// Состояние выполнения
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Получить черную доску
    /// </summary>
    public Blackboard Blackboard => _blackboard;
}