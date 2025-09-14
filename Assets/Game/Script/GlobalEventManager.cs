using System;
using UnityEngine.TextCore.Text;

public static class GlobalEventManager
{
    public static Action<DeathInfo> BodyDeath;


}

public struct DeathInfo
{
    public DeathInfo(CharacterBody attacker, CharacterBody target)
    {
        this.attacker = attacker;
        this.target = target;
    }
    public CharacterBody attacker;
    public CharacterBody target;
}