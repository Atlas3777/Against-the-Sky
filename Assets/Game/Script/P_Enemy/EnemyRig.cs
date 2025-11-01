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

    private float _bodyRigTargetWeight = 1f;
    private float _weaponRigTargetWeight = 1f;
    private float _handRigTargetWeight = 1f;

    public void Start()
    {
        // Враг всегда "прицеливается" (вес рига = 1)
        _bodyRigTargetWeight = 1f;
        _weaponRigTargetWeight = 1f;
        _handRigTargetWeight = 1f;

        if (weapon) 
            weapon.SetActive(true);
    }

    private void Update()
    {
        // Плавное включение рига (если нужно)
        bodyRig.weight = Mathf.Lerp(bodyRig.weight, _bodyRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
        weaponRig.weight = Mathf.Lerp(weaponRig.weight, _weaponRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
        handRig.weight = Mathf.Lerp(handRig.weight, _handRigTargetWeight, Time.deltaTime * RigWeightChangeRate);
    }

}