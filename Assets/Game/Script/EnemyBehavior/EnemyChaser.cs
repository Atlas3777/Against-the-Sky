using System.Collections.Generic;
using UnityEngine;
using MountainGoap;
using UnityEngine.AI;
using cowsins;

public class EnemyChaser : MonoBehaviour, IEnemy
{
    [SerializeField] private Animator animator;
    public float Damage = 25;
    public float AttackRange = 2f;
    
    public PointOfInterest PointOfInterest;
    

    public Transform attackPoint;
    public GameObject target;

    public bool PlayerVisibilite; 

    private Agent _agent;
    private bool causedDamage;
    private bool performAttack;
    private int logCounter;
    private NavMeshAgent _navMeshAgent;

    public void Init(GameObject target)
    {
        this.target = target;
    }

    public void GetComponents()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }


    public void MyStart()
    {
        _agent = new Agent(
            state: new()
            {
                { ASS.myTransform, transform },
                { ASS.targetTransform, target.GetComponent<InteractManager>().AttachmentPoints[0] },
                { ASS.targetHealth, 10000 },
                { ASS.distanceToTarget, 10000 },
                { ASS.navMeshAgent, _navMeshAgent },
                { ASS.countVisitedPoints, 0 },
                { ASS.targetReached, false }
            },
            goals: new()
            {
                new ExtremeGoal(
                    name: "Kill player",
                    weight: 5f,
                    desiredState: new()
                    {
                        { ASS.targetHealth, false }
                    }
                ),
                // new ExtremeGoal(
                //     name: "Visited Spot",
                //     weight: 5f,
                //     desiredState: new()
                //     {
                //         { ASS.countVisitedPoints, true }
                //     }
                // ),
            },
            actions: new List<Action>
            {
                new Action(
                    name: "ChaseTarget",
                    executor: ChaseTargetExecutor,
                    preconditions: new Dictionary<string, object>
                    {
                        { ASS.targetReached, false }
                    },
                    stateChecker: (action, state) =>
                    {
                        var navMeshPath = new NavMeshPath();
                        return NavMesh.CalculatePath(((Transform)state[ASS.myTransform]).position,
                            ((Transform)state[ASS.targetTransform]).position, NavMesh.AllAreas, navMeshPath);
                    },
                    postconditions: new()
                    {
                        { ASS.targetReached, true }
                    },
                    costCallback: (action, state) =>
                    {
                        var cost = PlayerVisibilite ? 1f : float.MaxValue;
                        return cost;
                    }
                ),
                new Action(
                    name: "Hit",
                    executor: HitExecutor,
                    preconditions: new()
                    {
                        { ASS.targetReached, true }
                    },
                    arithmeticPostconditions: new()
                    {
                        { ASS.targetHealth, -1000f }
                    },
                    cost: 1f
                ),
                // new Action(
                //     name: "Define New Spot Point",
                //     executor: DefineNewPoint,
                //     preconditions: new()
                //     {
                //         
                //     },
                //     postconditions: new Dictionary<string, object>
                //     {
                //         { ASS.е, true }
                //     },
                //     cost: 0.1f
                //     // costCallback: (action, state) =>
                //     // {
                //     //     var distance = CheckDistance((Vector3)agent.State["position"],
                //     //         (Vector3)agent.State["playerPosition"]);
                //     //     agent.State["playerInAttackRange"] = distance <= attackRange;
                //     //     var cost = distance > VISIBILITY_RANGE ? 1f : float.MaxValue;
                //     //     return cost;
                //     // }
               // )
            },
            sensors: new List<Sensor>
            {
                new Sensor(UpdateGameStateSensor),
            }
        );
    }



    private ExecutionStatus DefineNewPoint(Agent agent, Action action)
    {
        return ExecutionStatus.Executing;
    }

    ExecutionStatus ChaseTargetExecutor(Agent agent, Action action)
    {
        //Debug.Log("Trying to chase target");
        _navMeshAgent.ResetPath();
        

        if ((bool)agent.State[ASS.targetReached])
        {
            return ExecutionStatus.Succeeded;
        }

        if (!_navMeshAgent.SetDestination(((Transform)agent.State[ASS.targetTransform]).position))
        {
            return ExecutionStatus.NotPossible;
        }

        return ExecutionStatus.Executing;
    }

    ExecutionStatus HitExecutor(Agent agent, Action action)
    {
        _navMeshAgent.isStopped = true;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("MutantAttack"))
            animator.SetTrigger("Attack");
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MutantAttack"))
            animator.ResetTrigger("Attack");

        if (!performAttack)
            return ExecutionStatus.Executing;
        
        performAttack = false;

        if (causedDamage)
        {
            causedDamage = false;
            Debug.Log($"[{logCounter++}] AttackExecutor: Атака успешно завершена");
            return ExecutionStatus.Succeeded;
        }

        Debug.Log($"[{logCounter++}] AttackExecutor: Атака провалена");
        return ExecutionStatus.Failed;
    }

    // ExecutionStatus PatrolExecutor(Agent agent, Action action)
    // {
    //     if (PatrolPoints.Count == 0)
    //     {
    //         Debug.Log("there are no points to patrol");
    //         return ExecutionStatus.NotPossible;
    //     }
    //     
    //     var target = PatrolPoints[_nextPatrolPoint];
    //     
    //     if (!_navMeshAgent.hasPath || Vector3.Distance(_navMeshAgent.destination, target) > 0.1f)
    //     {
    //         _navMeshAgent.SetDestination(target);
    //         Debug.Log("Setting new destination: " + target);
    //     }
    //     
    //     if (!_navMeshAgent.pathPending &&
    //         Vector3.Distance(transform.position, target) <= _navMeshAgent.stoppingDistance)
    //     {
    //         Debug.Log("target reached");
    //         ++_nextPatrolPoint;
    //         _nextPatrolPoint %= PatrolPoints.Count;
    //         target = PatrolPoints[_nextPatrolPoint];
    //         _navMeshAgent.SetDestination(target);
    //         return ExecutionStatus.Succeeded;
    //     }
    //
    //     return ExecutionStatus.Executing;
    // }

    void UpdateGameStateSensor(Agent agent)
    {
        agent.State[ASS.distanceToTarget] = Vector3.Distance(((Transform)agent.State[ASS.myTransform]).position,
            ((Transform)agent.State[ASS.targetTransform]).position);

        agent.State[ASS.targetHealth] = Mathf.Max(0, target.GetComponent<PlayerStats>().health);
        agent.State[ASS.targetReached] = (float)agent.State[ASS.distanceToTarget] <= 0.1f;
    }

    void Update()
    {
        if (_agent == null || target == null) return;

        _agent.Step(StepMode.OneAction);
    }

    public void OnAttackHit()
    {
        Debug.Log($"[{logCounter++}] OnAttackHit: Событие удара из анимации сработало");


        var results = Physics.OverlapSphere(attackPoint.position, AttackRange);
        var playerHit = false;

        foreach (var col in results)
        {
            if (col.TryGetComponent(out PlayerStats playerStats) && (col.gameObject != gameObject))
            {
                playerHit = true;
                //PlayerStats.Damage(Damage, false);
                Debug.Log("hit");
                break;
            }
        }

        if (playerHit)
        {
            Debug.Log($"[{logCounter++}] OnAttackHit: Нанесено урон");
            causedDamage = true;
        }
        else
        {
            Debug.Log($"[{logCounter++}] OnAttackHit: Игрок не в зоне атаки, урон не нанесен");
            causedDamage = false;
        }

        performAttack = true;
    }

    private void OnGUI()
    {
        if (_agent != null)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 28;

            GUILayout.BeginArea(new Rect(10, 10, 500, 700));
            GUILayout.Label("Состояние агента:", style);
            foreach (var kvp in _agent.State)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value}", style);
            }

            foreach (var actionSequence in _agent.CurrentActionSequences)
            {
                foreach (var action in actionSequence)
                {
                    GUILayout.Label($"{action.Name}", style);
                }
            }

            GUILayout.EndArea();
        }
    }
}