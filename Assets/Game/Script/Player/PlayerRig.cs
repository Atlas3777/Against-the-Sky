using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRig : MonoBehaviour
{
    [Header("Rig Components")] public Rig bodyRig;
    public Rig weaponRig;
    public Rig handRig;

    [Header("Rig Settings")] public float RigWeightChangeRate = 5.0f;

    private Transform _aimTarget;
    private GameObject _weapon;

    private float _bodyRigTargetWeight;
    private float _weaponRigTargetWeight;
    private float _handRigTargetWeight;

    public void Setup(Transform aimTarget)
    {
        _aimTarget = aimTarget;
    }

    public void SetWeapon(GameObject weapon)
    {
        _weapon = weapon;
    }

    public void UpdateRigWeights(bool isAiming)
    {
        _bodyRigTargetWeight = isAiming ? 1f : 0f;
        _weaponRigTargetWeight = isAiming ? 1f : 0f;
        _handRigTargetWeight = isAiming ? 1f : 0f;

        if (_weapon != null)
        {
            _weapon.SetActive(isAiming);
        }

        bodyRig.weight = Mathf.Lerp(bodyRig.weight, _bodyRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
        weaponRig.weight = Mathf.Lerp(weaponRig.weight, _weaponRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
        handRig.weight = Mathf.Lerp(handRig.weight, _handRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
    }
}