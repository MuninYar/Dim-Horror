using System;
using System.Collections;
using PlayerMovement;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Trigger : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    public GameObject spawner;
    [SerializeField] public bool canTrigger = true;
    [SerializeField] [Range(0f, 100f)] private float spawnChance = 50f;
    [SerializeField] [Range(0f, 1f)] private float flashLightOffMultiplier;
    [SerializeField] [Range(0f, 1f)] private float walkMultiplier;
    [SerializeField] [Range(0f, 1f)] private float crouchMultiplier;
    public static Action<float> SpawnChanceUpdate;

    private Collider _player;
    private FirstPersonController _playerScript;
    [FormerlySerializedAs("_finalSpawnChance")] public float finalSpawnChance;
    private float _speedMultiplier;
    public static Action MonsterTrigger;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider>();
        _playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        _speedMultiplier = _playerScript.IsSprinting && !_playerScript.isCrouching ? 1f : _playerScript.isCrouching ? crouchMultiplier : walkMultiplier;
        if (_playerScript.flashLightOn)
            finalSpawnChance = _speedMultiplier * spawnChance;
        else if (!_playerScript.flashLightOn)
            finalSpawnChance = _speedMultiplier * flashLightOffMultiplier * spawnChance;
        SpawnChanceUpdate?.Invoke(finalSpawnChance);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (canTrigger && other == _player)
        {
            if (Random.value < finalSpawnChance / 100f)
            {
                enemy.SetActive(true);
                Instantiate(enemy, spawner.transform.position, spawner.transform.rotation);
                EventManager.MonsterTrigger();
                SetInactive();
                StartCoroutine(TriggerCooling());
                MonsterTrigger?.Invoke();
            }
        }
    }
    private IEnumerator TriggerCooling()
    {
        yield return new WaitForSeconds(10);
        SetActive();
    }
    private void SetInactive()
    {
        canTrigger = false;
    }
    private void SetActive()
    {
        canTrigger = true;
    }
}
