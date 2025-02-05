using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float damage = 10;
    [SerializeField] private float fireRate = 1;
    [SerializeField] private float range = 30;
    [SerializeField] private int maxAmmo = 30;
    public int ammoLoaded = 30;
    private bool _ammo;
    public bool canShoot;
    public static Action <int,int> AmmoChanged;
    public static Action IsReloading;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fire;
    [SerializeField] private AudioClip reload;
    [SerializeField] private AudioClip noAmmo;
    [SerializeField] private Light flashLight;
    private AudioSource _weaponAudioSource;
    private Camera _camera;
    public bool CanShoot { get; set;}
    private EnemyBehavior _currentEnemy;
    private Light flash;
    private bool _shootCooling;
    private bool _canReload = true;

    private void Awake()
    {
        _camera = FindObjectOfType<Camera>();
        flash = GetComponentInChildren<Light>();
        _weaponAudioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        if (canShoot)
        {
            if (ammoLoaded !=maxAmmo && Input.GetKeyDown(KeyCode.R) && _canReload)
                Reload();
            else if(ammoLoaded == 0 && !_shootCooling && Input.GetKey(KeyCode.Mouse0))
                NoAmmo();
            else if(Input.GetKey(KeyCode.Mouse0) && !_shootCooling)
                Shoot();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void Shoot()
    {
        ammoLoaded -= 1;
        AmmoChanged?.Invoke(ammoLoaded,maxAmmo);
        _shootCooling = true;
        _weaponAudioSource.PlayOneShot(fire,3);
        muzzleFlash.Play();
        StartCoroutine(GunShotFlash());
        
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit, range))
        {
            if (hit.collider.gameObject.layer == 8)
            {
                hit.collider.TryGetComponent(out _currentEnemy);
                _currentEnemy.TakeDamage(damage);
            }
        }
    }

    private IEnumerator GunShotFlash()
    {
        //_camera.transform.Rotate(_camera.transform.forward,20);
        flash.enabled = true;
        yield return new WaitForSeconds(fireRate);
        flash.enabled = false;
        _shootCooling = false;
    }

    private void NoAmmo()
    {
        StartCoroutine(NoAmmoCoroutine());
    }

    IEnumerator NoAmmoCoroutine()
    {
        _shootCooling = true;
        _weaponAudioSource.PlayOneShot(noAmmo);
        yield return new WaitForSeconds(fireRate * 8);
        _shootCooling = false;
    }

    private void Reload()
    {
        IsReloading?.Invoke();
        _shootCooling = true;
        _canReload = false;
        StartCoroutine(ReloadGun());
    }

    private IEnumerator ReloadGun()
    {
        _weaponAudioSource.PlayOneShot(reload);
        yield return new WaitForSeconds(reload.length);
        ammoLoaded = maxAmmo;
        flashLight.enabled = true;
        _shootCooling = false;
        AmmoChanged?.Invoke(ammoLoaded,maxAmmo);
        _canReload = true;
    }
}
