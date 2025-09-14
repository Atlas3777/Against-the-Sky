using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Параметры обнаружения")]
    public float detectionRadius = 10f; // Радиус обнаружения игрока
    public float attackRadius = 2f; // Радиус атаки
    
    [Header("Параметры патрулирования")]
    public float patrolRadius = 5f; // Радиус патрулирования
    public float patrolWaitTime = 2f; // Время ожидания между точками
    public float patrolSpeed = 1f; // Скорость патрулирования (0.5 для ходьбы)
    
    [Header("Параметры преследования")]
    public float chaseSpeed = 2f; // Скорость преследования (1 для бега)
    
    [Header("Параметры атаки")]
    public float attackCooldown = 1f; // Задержка между атаками
    public float attackDamage = 10f; // Урон от атаки
    public Transform attackPoint; // Точка атаки для физического воздействия
    public float attackRange = 1.5f; // Радиус действия атаки
    public LayerMask playerLayer = 1; // Маска слоя игрока
    
    [Header("Физика")]
    public LayerMask obstacleMask = 1; // Маска препятствий для рейкастов
    public float raycastHeight = 1f; // Высота рейкаста от позиции врага
    
    private CharacterController characterController;
    private Rigidbody rb;
    private Transform player;
    private Animator animator;
    private Vector3 targetPosition;
    private float waitTimer = 0f;
    private float attackTimer = 0f;
    private bool isWaiting = false;
    private bool isAttacking = false;
    private float currentSpeed = 0f;
    
    // Состояния врага
    private enum EnemyState
    {
        Patrolling,
        Chasing,
        Attacking
    }
    
    private EnemyState currentState = EnemyState.Patrolling;

    public void Init(Transform playerTransform)
    {
        player = playerTransform;
    }
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // Если точка атаки не назначена, создаем её
        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = new Vector3(0, 1, 1); // Сlightly in front of enemy
        }
        
        SelectNewPatrolPoint();
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Обновляем таймеры
        if (isWaiting) waitTimer -= Time.deltaTime;
        if (isAttacking) attackTimer -= Time.deltaTime;
        
        // Проверяем завершение ожидания
        if (isWaiting && waitTimer <= 0)
        {
            isWaiting = false;
            SelectNewPatrolPoint();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerDetected = distanceToPlayer <= detectionRadius;
        
        // Логика состояний
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling(playerDetected, distanceToPlayer);
                break;
                
            case EnemyState.Chasing:
                HandleChasing(playerDetected, distanceToPlayer);
                break;
                
            case EnemyState.Attacking:
                HandleAttacking(distanceToPlayer);
                break;
        }
        
        // Обновляем движение
        UpdateMovement();
        
        // Обновляем анимации
        UpdateAnimations();
    }
    
    void HandlePatrolling(bool playerDetected, float distanceToPlayer)
    {
        if (playerDetected)
        {
            if (distanceToPlayer <= attackRadius)
            {
                StartAttack();
            }
            else
            {
                StartChasing();
            }
            return;
        }
        
        // Если достигли точки назначения
        if (Vector3.Distance(transform.position, targetPosition) <= 0.2f)
        {
            if (!isWaiting)
            {
                StartWaiting();
            }
        }
        else
        {
            // Проверяем путь к точке
            if (IsPathClear(transform.position, targetPosition))
            {
                currentSpeed = patrolSpeed;
            }
            else
            {
                // Если путь заблокирован, выбираем новую точку
                SelectNewPatrolPoint();
            }
        }
    }
    
    void HandleChasing(bool playerDetected, float distanceToPlayer)
    {
        if (!playerDetected)
        {
            StartPatrolling();
            return;
        }
        
        if (distanceToPlayer <= attackRadius)
        {
            StartAttack();
            return;
        }
        
        // Преследуем игрока
        targetPosition = player.position;
        if (IsPathClear(transform.position, targetPosition))
        {
            currentSpeed = chaseSpeed;
        }
        else
        {
            currentSpeed = 0f; // Останавливаемся, если путь заблокирован
        }
    }
    
    void HandleAttacking(float distanceToPlayer)
    {
        currentSpeed = 0f; // Во время атаки стоим на месте
        
        // Проверяем, закончилась ли атака
        if (!isAttacking && attackTimer <= 0)
        {
            // Атака завершена, возвращаемся к преследованию или патрулированию
            if (distanceToPlayer <= attackRadius)
            {
                // Игрок все еще в зоне атаки, продолжаем атаковать
                if (attackTimer <= 0)
                {
                    StartAttack();
                }
            }
            else if (distanceToPlayer <= detectionRadius)
            {
                // Игрок в зоне обнаружения, начинаем преследование
                StartChasing();
            }
            else
            {
                // Игрок вне зоны, начинаем патрулирование
                StartPatrolling();
            }
        }
    }
    
    void StartWaiting()
    {
        isWaiting = true;
        waitTimer = patrolWaitTime;
        currentSpeed = 0f;
    }
    
    void SelectNewPatrolPoint()
    {
        // Выбираем новую точку патрулирования
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0;
        Vector3 newTarget = transform.position + randomDirection;
        
        // Проверяем доступность пути
        if (IsPathClear(transform.position, newTarget))
        {
            targetPosition = newTarget;
            currentSpeed = patrolSpeed;
        }
        else
        {
            // Если путь заблокирован, пробуем снова через короткое время
            StartWaiting();
        }
    }
    
    void StartChasing()
    {
        currentState = EnemyState.Chasing;
        currentSpeed = chaseSpeed;
        isWaiting = false;
        isAttacking = false;
    }
    
    void StartPatrolling()
    {
        currentState = EnemyState.Patrolling;
        isAttacking = false;
        SelectNewPatrolPoint();
    }
    
    void StartAttack()
    {
        currentState = EnemyState.Attacking;
        isAttacking = true;
        attackTimer = attackCooldown;
        currentSpeed = 0f;
        
        // Активируем анимацию атаки
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void AttackOnAnim() //From Animator
    {
        Debug.Log("ATTACK YES");
        // Физическая атака - наносим урон игроку в радиусе
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach (Collider playerCollider in hitPlayers)
        {
            // Здесь можно добавить нанесение урона игроку
            // playerCollider.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            
            // Добавляем физическое воздействие
            Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 forceDirection = (playerCollider.transform.position - transform.position).normalized;
                forceDirection.y = 0.5f; // Немного вверх, чтобы игрок подпрыгнул
                playerRb.AddForce(forceDirection * 500f, ForceMode.Impulse);
            }
        }
    }

    public void AttackEnd()
    {
        Debug.Log("ATTACK END");
        // Завершаем атаку
        isAttacking = false;
        attackTimer = attackCooldown;
        
        // Теперь враг может продолжить движение
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRadius)
            {
                // Остаемся в состоянии атаки, если игрок рядом
                return;
            }
            else if (distanceToPlayer <= detectionRadius)
            {
                StartChasing();
            }
            else
            {
                StartPatrolling();
            }
        }
    }
    
    void UpdateMovement()
    {
        if (currentSpeed <= 0) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        // Поворачиваем в направлении движения
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Двигаем врага
        Vector3 moveVector = direction * currentSpeed * 2f * Time.deltaTime; // Умножаем на 2 для соответствия blend tree
        moveVector.y = Physics.gravity.y * Time.deltaTime;
        
        if (characterController != null)
        {
            characterController.Move(moveVector);
        }
        else
        {
            transform.position += moveVector;
        }
    }
    
    // Проверка пути с помощью рейкаста
    bool IsPathClear(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        
        if (distance > 0.1f)
        {
            direction.Normalize();
            
            // Проверяем на наличие препятствий
            Vector3 rayStart = from + Vector3.up * raycastHeight;
            Vector3 rayDirection = to - from;
            rayDirection.y = 0;
            
            // Основной луч
            if (Physics.Raycast(rayStart, rayDirection.normalized, distance, obstacleMask))
            {
                return false;
            }
            
            // Дополнительная проверка немного выше
            if (Physics.Raycast(rayStart + Vector3.up * 0.5f, rayDirection.normalized, distance, obstacleMask))
            {
                return false;
            }
            
            // Проверка немного ниже
            if (Physics.Raycast(rayStart - Vector3.up * 0.3f, rayDirection.normalized, distance, obstacleMask))
            {
                return false;
            }
        }
        
        return true;
    }
    
    // Обновление анимаций
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }
    }
    
    // Визуализация радиусов в редакторе
    void OnDrawGizmos()
    {
        // Рисуем сферу радиуса обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Рисуем сферу радиуса атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        
        // Рисуем сферу радиуса патрулирования
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        
        // Рисуем линию к текущей цели
        if (targetPosition != Vector3.zero)
        {
            Gizmos.color = currentState == EnemyState.Patrolling ? Color.green : 
                          currentState == EnemyState.Chasing ? Color.magenta : Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.2f);
        }
        
        // Визуализация рейкаста
        if (targetPosition != Vector3.zero && currentSpeed > 0)
        {
            Gizmos.color = IsPathClear(transform.position, targetPosition) ? Color.green : Color.red;
            Vector3 rayStart = transform.position + Vector3.up * raycastHeight;
            Gizmos.DrawLine(rayStart, targetPosition + Vector3.up * raycastHeight);
        }
        
        // Визуализация точки атаки и радиуса атаки
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            Gizmos.DrawSphere(attackPoint.position, 0.1f);
        }
    }
}