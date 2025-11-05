namespace Game.GOAP
{
    public interface IAgentBehaviour
    {
        public bool ShouldAgentInvestigateSound { get; set; }
        void SwitchAgentSoundInvestigation(bool state);
    }
}
