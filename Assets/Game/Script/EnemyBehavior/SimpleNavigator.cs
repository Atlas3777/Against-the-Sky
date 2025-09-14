using UnityEngine;
using MountainGoap;

public class SimpleNavigator : MonoBehaviour
{
    private Agent agent;
    private Vector3 targetPosition;
    public float moveSpeed = 5f;
    
    void Start()
    {
        // Устанавливаем случайную цель
        targetPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        
        // Создаем простого агента с одной целью
        agent = new Agent(
            name: "Navigator",
            state: new() {
                { "atTarget", false },
                { "position", new Vector2(transform.position.x, transform.position.z) }
            },
            goals: new() {
                new Goal(
                    name: "Reach Target",
                    weight: 1f,
                    desiredState: new() {
                        { "atTarget", true }
                    }
                )
            },
            actions: new() {
                new Action(
                    name: "Move To Target",
                    executor: MoveToTargetExecutor,
                    preconditions: new() {
                        { "atTarget", false }
                    },
                    postconditions: new() {
                        { "atTarget", true }
                    }
                )
            }
        );
    }
    
    void Update()
    {
        agent.Step(StepMode.OneAction);
    }
    
    private ExecutionStatus MoveToTargetExecutor(Agent agent, Action action)
    {
        Vector2 currentPos = (Vector2)agent.State["position"];
        Vector2 targetPos = new Vector2(targetPosition.x, targetPosition.z);
        
        // Рассчитываем направление движения
        Vector2 direction = targetPos - currentPos;
        
        if (direction.magnitude > 0.5f)
        {
            // Двигаемся к цели
            Vector2 newPos = currentPos + direction.normalized * moveSpeed * Time.deltaTime;
            agent.State["position"] = newPos;
            
            // Обновляем позицию Unity объекта
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.y);
            
            return ExecutionStatus.Executing;
        }
        else
        {
            // Достигли цели
            agent.State["atTarget"] = true;
            return ExecutionStatus.Succeeded;
        }
    }
}