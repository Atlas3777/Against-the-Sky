using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Game.GOAP;
using UnityEngine;
using UnityEngine.AI;

public class ShootingBehaviour : MonoBehaviour
{
    private AgentBehaviour agent;
    private GoapActionProvider provider;
    private GoapBehaviour goap;

    void Awake()
    {
        if(this.goap ==null) this.goap = FindObjectOfType<GoapBehaviour>();
        
        if(this.agent is null) agent = this.GetComponent<AgentBehaviour>();
        if (this.provider is null) provider = this.GetComponent<GoapActionProvider>();


        if (this.provider.AgentTypeBehaviour == null)
            this.provider.AgentType = this.goap.GetAgentType("ShootingAgent");
        
    }
    
    void Start()
    {
        this.provider.RequestGoal<ShootingGoal>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
