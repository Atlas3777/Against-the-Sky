using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyRig : MonoBehaviour
{
    [Header("Rig Components")]
    public Rig bodyRig;
    public Rig weaponRig;
    public Rig handRig;

    [Header("Rig Settings")]
    public float RigWeightChangeRate = 8.0f;

    [SerializeField] private GameObject weapon;

    private float _bodyRigTargetWeight;
    private float _weaponRigTargetWeight;
    private float _handRigTargetWeight;

    public void Setup()
    {
        // Враг всегда "прицеливается" (вес рига = 1)
        // _bodyRigTargetWeight = 1f;
        // _weaponRigTargetWeight = 1f;
        // _handRigTargetWeight = 1f;

        if (weapon)
            weapon.SetActive(false);
        
    }

    void Update()
    {
        SetRigWeigths();
    }

    public void UpdateRigWeights(bool isWeaponActive)
    {
        if(!bodyRig) return;
        if (isWeaponActive)
        {
            _bodyRigTargetWeight = 1f;
            _weaponRigTargetWeight = 1f;
            _handRigTargetWeight = 1f;
            weapon.SetActive(true);
        }
        else
        {
            _bodyRigTargetWeight = 0;
            _weaponRigTargetWeight = 0;
            _handRigTargetWeight = 0;
            weapon.SetActive(false);
        }
    }
    
    private void SetRigWeigths()
    {
        bodyRig.weight = Mathf.Lerp(bodyRig.weight, _bodyRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
        weaponRig.weight = Mathf.Lerp(weaponRig.weight, _weaponRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
        handRig.weight = Mathf.Lerp(handRig.weight, _handRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
    }
}