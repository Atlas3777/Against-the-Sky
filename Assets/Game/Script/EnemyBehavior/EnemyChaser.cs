using System.Collections.Generic;
using UnityEngine;
using MountainGoap;
using UnityEngine.AI;
using Unity.VisualScripting;

public class EnemyChaser : MonoBehaviour
{
    public float Damage = 25;
    public float MoveSpeed = 2f;
    public float attackRange = 1f;
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
        // attackPoint = transform;
        PatrolPoints = new List<Vector3>
        {
            new Vector3(36, 20, -24),
            new Vector3(30, 20, -20)
        };
        _navMeshAgent.SetDestination(PatrolPoints[_nextPatrolPoint]);
    }

    public void AAAAAAStart()
    {
        agent = new Agent(
            state: new()
            {
                { "position", transform.position },
                { "playerPosition", player.position },
                { "playerInAttackRange", false },
<<<<<<< refs/remotes/origin/fixed_enemy_refactor
                { "playerHealth", playerHeathSystem.CurrentHeath },
=======
                { "playerHealth", PlayerStats.health + PlayerStats.shield },
>>>>>>> local
                { "playerDead", false },
                { "nextPatrolTarget", PatrolPoints[0] },
                { "distanceToPlayer", float.MaxValue },
                { "visitedPatrolPoints", 0}
                //{ "atPatrolTarget", false },
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
                new ExtremeGoal(
                    name: "Patrol",
                    weight: 5f,
                    desiredState: new()
                    {
                        { "visitedPatrolPoints", true }
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
                        { "playerInAttackRange", false }
                    },
                    postconditions: new Dictionary<string, object>
                    {
                        { "playerInAttackRange", true }
                    },
                    costCallback: (action, state) =>
                    {
                        // var distance = CheckDistance(attackPoint.position, (Vector3)agent.State["playerPosition"]);
                        // agent.State["playerInAttackRange"] = distance <= attackRange;
                        var cost = (float)state["distanceToPlayer"] > VISIBILITY_RANGE ? 1f : float.MaxValue;
                        return cost;
                    }
                ),
                new Action(
                    name: "Hit player",
                    executor: HitPlayerExecutor,
                    preconditions: new Dictionary<string, object>
                    {
                        { "playerDead", false },
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
                    postconditions: new Dictionary<string, object>
                    {
                        { "atPatrolTarget", true }
                    },
                    costCallback: (action, state) =>
                    {
                        // var distance = CheckDistance(attackPoint.position, (Vector3)agent.State["playerPosition"]);
                        // agent.State["playerInAttackRange"] = distance <= attackRange;
                        var cost = (float)state["distanceToPlayer"] > VISIBILITY_RANGE ? 1f : float.MaxValue;
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
        // var distance = CheckAttackRange((Vector3)agent.State["playerPosition"]);
        if ((float)agent.State["distanceToPlayer"] > VISIBILITY_RANGE)
            return ExecutionStatus.NotPossible;
        if ((float)agent.State["distanceToPlayer"] <= attackRange)
        {
            _navMeshAgent.isStopped = true;
            return ExecutionStatus.Succeeded;
        }
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination((Vector3)agent.State["playerPosition"]);
        return ExecutionStatus.Executing;
    }

    ExecutionStatus HitPlayerExecutor(Agent agent, Action action)
    {
        _navMeshAgent.isStopped = true;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("MutantAttack"))
            animator.SetTrigger("Attack");
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MutantAttack"))
            animator.ResetTrigger("Attack");
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
        // var distance = CheckAttackRange((Vector3)agent.State["playerPosition"]);
        if ((float)agent.State["distanceToPlayer"] <= VISIBILITY_RANGE)
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
<<<<<<< refs/remotes/origin/fixed_enemy_refactor
        // var distance = Vector3.Distance(transform.position, player.transform.position);
        //agent.State["playerInAttackRange"] = distance <= attackRange;
        agent.State["playerDead"] = playerHeathSystem.CurrentHeath <= 0;
=======
        var distance = Vector3.Distance(attackPoint.position, player.transform.position);
        agent.State["playerInAttackRange"] = distance <= attackRange;
        agent.State["distanceToPlayer"] = Vector3.Distance(attackPoint.position, player.transform.position);
        agent.State["playerDead"] = PlayerStats.health <= 0;
>>>>>>> local
        //agent.State["playerInVisibilityRange"] = distance <= VISIBILITY_RANGE;
        agent.State["atPatrolTarget"] = _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending;
        agent.State["playerHealth"] = Mathf.Max(0, playerHeathSystem.CurrentHeath);
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
            if (col.TryGetComponent(out CharacterBody playerBody) && !(col.GameObject() == gameObject))
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

            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.Label("Состояние агента:", style);
            foreach (var kvp in agent.State)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value}", style);
            }
            GUILayout.Label($"distanve: {agent.State["distanceToPlayer"]}", style);
            GUILayout.EndArea();
        }
    }

    // private float CheckAttackRange(Vector3 playerPos) => Vector3.Distance(attackPoint.position, playerPos);
}

