using System.Collections.Generic;
using UnityEngine;
using MountainGoap;
using UnityEngine.AI;

public class EnemyChaser : MonoBehaviour
{
    public float Damage = 25;
    public float MoveSpeed = 2f;
    public float attackRange = 1.7f;
    public Transform attackPoint;
    public Transform player;
    public HeathSystem playerHeathSystem;
    public const float VISIBILITY_RANGE = 10f;
    public List<Vector3> PatrolPoints;
    private Agent agent;

    //private CharacterController controller;
    private Animator animator;
    private bool causedDamage;
    private bool performAttack;
    private int logCounter = 0; // Для уникальности сообщений
    private CharacterBody characterBody;
    private NavMeshAgent _navMeshAgent;
    private int _nextPatrolPoint = 0;
    //private Vector3 _targetPoint = new Vector3(36, 10, -24);

    public void Init(Transform player, CharacterBody playerBody)
    {
        this.player = player;
        playerHeathSystem = playerBody.heathSystem;
        this.characterBody = GetComponent<CharacterBody>();
        //controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        attackPoint = transform;
        PatrolPoints = new List<Vector3>
        {
            new Vector3(36, 10, -24),
            new Vector3(30, 10, -20)
        };
        _navMeshAgent.SetDestination(PatrolPoints[_nextPatrolPoint]);
    }

    void Start()
    {
        agent = new Agent(
            state: new()
            {
                { "position", transform.position },
                { "playerPosition", player.position },
                { "playerInAttackRange", false },
                { "playerHealth", playerHeathSystem.CurrentHeath },
                { "playerDead", false },
                { "atPatrolTarget", false },
                { "nextPatrolTarget", PatrolPoints[0] },
                //{ "playerInVisibilityRange", false }
            },
            goals: new()
            {
                new ExtremeGoal(
                    name: "Kill player",
                    weight: 5f,
                    desiredState: new()
                    {
                        { "playerHealth", false}
                    }
                ),
                new Goal(
                    name: "Patrol",
                    weight: 5f,
                    desiredState: new()
                    {
                        { "atPatrolTarget", true }
                    }
                )
            },
            actions: new List<Action>
            {
                new Action(
                    name: "Chase player",
                    executor: ChasePlayerExecutor,
                    preconditions: new Dictionary<string, object>
                    {
                        // { "playerInVisibilityRange", true },
                        { "playerInAttackRange", false } // отменить патруль???
                    },
                    postconditions: new Dictionary<string, object>
                    {
                        { "playerInAttackRange", true }
                    },
                    costCallback: (action, state) =>
                    {
                        agent.State["position"] = transform.position;
                        agent.State["playerPosition"] = player.transform.position;
                        var distance = Vector3.Distance(transform.position, player.transform.position);
                        agent.State["playerInAttackRange"] = distance <= attackRange;
                        var cost = distance > VISIBILITY_RANGE ? 1f : float.MaxValue;
                        return cost;
                    }
                    // cost: 1f
                ),
                new Action(
                    name: "Hit player",
                    executor: HitPlayerExecutor,
                    preconditions: new Dictionary<string, object>
                    {
                        { "playerDead", false },
                        // { "playerInVisibilityRange", true },
                        { "playerInAttackRange", true }
                    },
                    arithmeticPostconditions: new Dictionary<string, object>
                    {
                        { "playerHealth", -Damage }
                    },
                    cost: 1f
                ),
                new Action(
                    name: "Patrol",
                    executor: PatrolExecutor,
                    // preconditions: new Dictionary<string, object>
                    // {
                    //     { "playerInVisibilityRange", false }
                    // },
                    postconditions: new Dictionary<string, object>
                    {
                        { "atPatrolTarget", true }
                    },
                    costCallback: (action, state) =>
                    {
                        agent.State["position"] = transform.position;
                        agent.State["playerPosition"] = player.transform.position;
                        var distance = Vector3.Distance(transform.position, player.transform.position);
                        agent.State["playerInAttackRange"] = distance <= attackRange;
                        var cost = distance > VISIBILITY_RANGE ? 1f : float.MaxValue;
                        return cost;
                    }
                )
            },
            sensors: new List<Sensor>
            {
                new Sensor(UpdateGameStateSensor),
            }
        );
    }

    ExecutionStatus ChasePlayerExecutor(Agent agent, Action action)
    {
        var position = (Vector3)agent.State["position"];
        var playerPosition = (Vector3)agent.State["playerPosition"];
        var distance = Vector3.Distance(position, playerPosition);
        if (distance > VISIBILITY_RANGE)
            return ExecutionStatus.NotPossible;
        if (distance <= attackRange)
        {
            _navMeshAgent.isStopped = true;
            return ExecutionStatus.Succeeded;
        }
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(playerPosition);
        return ExecutionStatus.Executing;
        // var movingOffset = (playerPosition - position).normalized * MoveSpeed * Time.deltaTime;
        // movingOffset.y = 0;
        // controller.Move(movingOffset);
        // var lookDirection = playerPosition - transform.position;
        // lookDirection.y = 0;
        // if (lookDirection != Vector3.zero)
        //     transform.rotation = Quaternion.LookRotation(lookDirection);
        // return ExecutionStatus.Executing;
    }

    ExecutionStatus HitPlayerExecutor(Agent agent, Action action)
    {
        _navMeshAgent.isStopped = true;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            animator.SetTrigger("Attack");
        if (!performAttack)
            return ExecutionStatus.Executing;
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

    ExecutionStatus PatrolExecutor(Agent agent, Action action)
    {
        var position = (Vector3)agent.State["position"];
        var playerPosition = (Vector3)agent.State["playerPosition"];
        var distance = Vector3.Distance(position, playerPosition);
        if (distance <= VISIBILITY_RANGE)
            return ExecutionStatus.Failed;
        if (PatrolPoints.Count == 0)
        {
            Debug.Log("there are no points to patrol");
            return ExecutionStatus.NotPossible;
        }
        var target = PatrolPoints[_nextPatrolPoint];

        if (!_navMeshAgent.hasPath || Vector3.Distance(_navMeshAgent.destination, target) > 0.1f)
        {
            _navMeshAgent.SetDestination(target);
            Debug.Log("Setting new destination: " + target);
        }

        if (!_navMeshAgent.pathPending && Vector3.Distance(transform.position, target) <= _navMeshAgent.stoppingDistance)
        {
            Debug.Log("target reached");
            ++_nextPatrolPoint;
            _nextPatrolPoint %= PatrolPoints.Count;
            target = PatrolPoints[_nextPatrolPoint];
            _navMeshAgent.SetDestination(target);
            return ExecutionStatus.Succeeded;
        }
        return ExecutionStatus.Executing;
    }

    void UpdateGameStateSensor(Agent agent)
    {
        agent.State["position"] = transform.position;
        agent.State["playerPosition"] = player.transform.position;
        // var distance = Vector3.Distance(transform.position, player.transform.position);
        //agent.State["playerInAttackRange"] = distance <= attackRange;
        agent.State["playerDead"] = playerHeathSystem.CurrentHeath <= 0;
        //agent.State["playerInVisibilityRange"] = distance <= VISIBILITY_RANGE;
        agent.State["atPatrolTarget"] = _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending;
        agent.State["playerHealth"] = Mathf.Max(0, playerHeathSystem.CurrentHeath);
        // if (distance <= VISIBILITY_RANGE && !_navMeshAgent.isStopped)
        //     _navMeshAgent.isStopped = true;
        // if (distance > VISIBILITY_RANGE && _navMeshAgent.isStopped)
        //     _navMeshAgent.isStopped = false;
        //Debug.Log(agent.State["atPatrolTarget"] + " " + agent.State["playerInVisibilityRange"]);
    }

    void Update()
    {
        if (agent == null || player == null) return;
        agent.Step(StepMode.OneAction);
    }

    public void OnAttackHit()
    {
        Debug.Log($"[{logCounter++}] OnAttackHit: Событие удара из анимации сработало");

        var hitColliders = Physics.OverlapSphere(attackPoint.position, attackRange);
        var playerHit = false;
        
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
            var currentHealth = (float)agent.State["playerHealth"];
            var newHealth = Mathf.Max(0, currentHealth - Damage);

            agent.State["playerHealth"] = newHealth;

            Debug.Log($"[{logCounter++}] OnAttackHit: Нанесено урона {Damage}. Здоровье игрока: {newHealth:F1}");

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
            style.fontSize = 28;
            
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

