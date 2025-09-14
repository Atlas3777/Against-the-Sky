using System.Collections.Generic;
using UnityEngine;
using MountainGoap;
using System.Text;
using System.Collections.Concurrent;

public class EnemyChaser : MonoBehaviour, IEnemy
{
    public float Damage;
    public float moveSpeed = 2f;
    public float attackRange = 0.7f;
    public Transform attackPoint;
    public Transform player;
    public HeathSystem playerHeathSystem;
    private Agent agent;

    private CharacterController controller;
    private Animator animator;
    private bool causedDamage;
    private bool performAttack;
    private int logCounter = 0; // Для уникальности сообщений
    private CharacterBody characterBody;

    public void Init(Transform player, CharacterBody characterBody)
    {
        this.player = player;
        playerHeathSystem = characterBody.heathSystem;
    }

    void Start()
    {
        characterBody = GetComponent<CharacterBody>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        agent = new Agent(
            state: new()
            {
                { "position", transform.position },
                { "playerPosition", player.position },
                { "playerInAttackRange", false },
                { "playerHealth", playerHeathSystem.CurrentHeath },
                { "playerDead", false }
            },
            goals: new()
            {
                new ExtremeGoal(
                    name: "Minimize Player Health",
                    weight: 5f,
                    desiredState: new (){
                        { "playerHealth", false }
                    }
                )
            },
            actions: new List<Action>
            {
                new Action(
                    name: "Chase Player",
                    executor: ChasePlayerExecutor,
                    preconditions: new Dictionary<string, object>
                    {
                        { "playerInAttackRange", false }
                    },
                    postconditions: new Dictionary<string, object>
                    {
                        { "playerInAttackRange", true }
                    },
                    stateMutator: (action, state) =>
                    {
                        Vector3 currentPos = (Vector3)state["position"];
                        Vector3 playerPos = (Vector3)state["playerPosition"];
                        Vector3 direction = (playerPos - currentPos).normalized;
                        Vector3 predictedPos = currentPos + direction * moveSpeed * 0.1f;
                        state["position"] = predictedPos;
                        state["distanceToPlayer"] = Vector3.Distance(predictedPos, playerPos);
                    },
                    costCallback: (action, state) =>
                    {
                        float distance = (float)state["distanceToPlayer"];
                        return distance * 0.1f;
                    }
                ),
                new Action(
                    name: "Attack",
                    executor: AttackExecutor,
                    preconditions: new()
                    {
                        { "playerInAttackRange", true }
                    },
                    arithmeticPostconditions: new () {
                        {"playerHealth", -20f}
                    },
                    cost: 0.5f
                )
            },
            sensors: new List<Sensor>
            {
                new Sensor(UpdateGameStateSensor)
            }
        );

        //SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        // Подписываемся на события
        Agent.OnAgentActionSequenceCompleted += (Agent a) 
            =>  Debug.Log($"[{logCounter++}] Агент завершил выполнение плана.");
        //Agent.OnAgentStep += (Agent a)
        //  =>Debug.Log($"[{logCounter++}] Агент работает. Текущее состояние: " + GetStateString(new Dictionary<string, object>(a.State)));
        
        // Обработчики событий агента

        Agent.OnPlanningStarted += (Agent a) 
            => Debug.Log($"[{logCounter++}] Агент начал планирование.");
        Agent.OnPlanningFinished += (Agent a, BaseGoal? goal, float utility) 
            => Debug.Log($"[{logCounter++}] Агент завершил планирование. Цель: {goal?.Name}, Полезность: {utility}");
        Agent.OnPlanningFinishedForSingleGoal += (Agent a, BaseGoal goal, float utility) 
            => Debug.Log($"[{logCounter++}] Агент завершил планирование для одной цели: {goal.Name}, Полезность: {utility}");
        Agent.OnEvaluatedActionNode += (ActionNode node, ConcurrentDictionary<ActionNode, ActionNode> nodes)
            => Debug.Log($"[{logCounter++}] Агент оценил узел действия: {node.Action?.Name ?? "null"}");
        Agent.OnPlanUpdated += OnPlanUpdated;

        
        // Обработчики событий действий
        Action.OnBeginExecuteAction += (Agent a, Action action, Dictionary<string, object?> parameters) 
            => Debug.Log($"[{logCounter++}] Агент начал выполнение действия: {action.Name}");
        Action.OnFinishExecuteAction += (Agent a, Action action, ExecutionStatus status, Dictionary<string, object?> parameters) 
            => Debug.Log($"[{logCounter++}] Агент завершил выполнение действия: {action.Name}, Статус: {status}");
    }



    private void OnPlanUpdated(Agent a, List<Action> actionList)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var action in actionList)
        {
            sb.Append(action.Name + " -> ");
        }
        Debug.Log($"[{logCounter++}] Агент сгенерировал последовательность действий: {sb.ToString()}");
    }


    private string GetStateString(Dictionary<string, object> state)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var kvp in state)
        {
            sb.Append($"{kvp.Key}: {kvp.Value} | ");
        }
        return sb.ToString();
    }

    private ExecutionStatus ChasePlayerExecutor(Agent agent, Action action)
    {
        Vector3 enemyPos = (Vector3)agent.State["position"];
        Vector3 playerPos = (Vector3)agent.State["playerPosition"];

        Vector3 direction = playerPos - enemyPos;
        float distance = direction.magnitude;

        if (distance <= attackRange*2)
        {
            return ExecutionStatus.Succeeded;
        }

        Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
        move.y = 0;
        controller.Move(move);
    
        // Поворот только по горизонтали (Y axis)
        Vector3 lookDirection = playerPos - transform.position;
        lookDirection.y = 0; // Обнуляем Y компоненту для горизонтального поворота
    
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = targetRotation;
        }

        return ExecutionStatus.Executing;
    }

    private ExecutionStatus AttackExecutor(Agent agent, Action action)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetTrigger("Attack");
            //Debug.Log($"[{logCounter++}] AttackExecutor: Запуск анимации атаки");
        }
        
        if (!performAttack)
        {
            //Debug.Log($"[{logCounter++}] AttackExecutor: Анимация атаки еще выполняется");
            return ExecutionStatus.Executing;
        }
        
        if (causedDamage)
        {
            causedDamage = false;
            performAttack = false;
            Debug.Log($"[{logCounter++}] AttackExecutor: Атака успешно завершена");
            return ExecutionStatus.Succeeded;
        }
        
        performAttack = false;
        Debug.Log($"[{logCounter++}] AttackExecutor: Атака провалена");
        return ExecutionStatus.Failed;
    }

    private void UpdateGameStateSensor(Agent agent)
    {
        agent.State["position"] = transform.position;

        if (player != null)
        {
            agent.State["playerPosition"] = player.position;
            agent.State["playerHealth"] = playerHeathSystem.CurrentHeath;
            agent.State["playerDead"] = player.gameObject.activeSelf;

            float distance = Vector3.Distance(
                (Vector3)agent.State["position"],
                player.position
            );

            bool wasInAttackRange = (bool)agent.State["playerInAttackRange"];
            agent.State["playerInAttackRange"] = distance <= attackRange;
            bool isInAttackRange = (bool)agent.State["playerInAttackRange"];

            if (wasInAttackRange != isInAttackRange)
            {
                //Debug.Log($"[{logCounter++}] Сенсор: Статус зоны атаки игрока изменился на: {isInAttackRange}. Расстояние: {distance:F2}");
            }
        }
    }

    void Update()
    {
        if (agent == null || player == null) return;

        agent.Step(StepMode.OneAction);
    }

    public void OnAttackHit()
    {
        Debug.Log($"[{logCounter++}] OnAttackHit: Событие удара из анимации сработало");

        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRange);
        bool playerHit = false;
        
        foreach (var col in hitColliders)
        {
            if(col.TryGetComponent(out CharacterBody playerBody))
            {
                playerHit = true;
                playerBody.TakeDamage(new DamageInfo(Damage, characterBody, playerBody));
                break;
            }
        }

        if (playerHit)
        {
            float damage = 25f;
            float currentHealth = (float)agent.State["playerHealth"];
            float newHealth = Mathf.Max(0, currentHealth - damage);

            agent.State["playerHealth"] = newHealth;

            Debug.Log($"[{logCounter++}] OnAttackHit: Нанесено урона {damage}. Здоровье игрока: {newHealth:F1}");

            causedDamage = true;
        }
        else
        {
            Debug.Log($"[{logCounter++}] OnAttackHit: Игрок не в зоне атаки, урон не нанесен");
            causedDamage = false;
        }

        performAttack = true;
    }

    // Дополнительный метод для отладки состояния агента
    private void OnGUI()
    {
        if (agent != null)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label("Состояние агента:", style);
            foreach (var kvp in agent.State)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value}", style);
            }
            GUILayout.EndArea();
        }
    }
}

