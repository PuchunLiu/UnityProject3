using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GUNTYPE
{
    SINGLESHOT,
    AUTO,
    SNIPING
}

public class PlayerController : MonoBehaviour
{
    private Animator ani;
    public float moveSpeed;
    public float rotateSpeed;
    public Vector2 vector = new Vector2(0f, 0f);
    public Rigidbody body;
    public float jumpSpeed;
    public Transform gunPoint;
    public GameObject bloodEffect;
    public GameObject groundEffect;
    private float attackCD;
    private float attacktimer;
    public Transform gunEffect;
    public GameObject gunEffectPrefab;
    public GUNTYPE gunType;

    public Dictionary<GUNTYPE, int> bulletsBag = new Dictionary<GUNTYPE, int>();
    public Dictionary<GUNTYPE, int> bulletsClip = new Dictionary<GUNTYPE, int>();
    public int maxSingleShotBullets;
    public int maxAutoShotBullets;
    public int maxSnipingleShotBullets;

    public bool isReloading;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ani = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        rotateSpeed = 5f;
        moveSpeed = 4f;
        jumpSpeed = 300f;
        maxSingleShotBullets = 25;
        maxAutoShotBullets = 25;
        maxSnipingleShotBullets = 5;
        bulletsBag.Add(GUNTYPE.SINGLESHOT, 50);
        bulletsBag.Add(GUNTYPE.AUTO, 100);
        bulletsBag.Add(GUNTYPE.SNIPING, 20);
        bulletsClip.Add(GUNTYPE.SINGLESHOT, maxSingleShotBullets);
        bulletsClip.Add(GUNTYPE.AUTO, maxAutoShotBullets);
        bulletsClip.Add(GUNTYPE.SNIPING, maxSnipingleShotBullets);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime + transform.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        vector.x = Mathf.Clamp(vector.x - Input.GetAxis("Mouse Y") * rotateSpeed, -60f, 60f);
        //vector.y = vector.y - Input.GetAxis("Mouse X") * rotateSpeed;
        transform.eulerAngles = new Vector3(vector.x, transform.eulerAngles.y + Input.GetAxis("Mouse X") * rotateSpeed, transform.eulerAngles.z);
        if (!isReloading)
        {
            switch (gunType)
            {
                case GUNTYPE.SNIPING:
                    attackCD = 0.5f;
                    SnipingShot();
                    break;
                case GUNTYPE.AUTO:
                    attackCD = 0.1f;
                    AutoShot();
                    break;
                case GUNTYPE.SINGLESHOT:
                    attackCD = 0.1f;
                    SingleShot();
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            body.AddForce(Vector3.up * jumpSpeed);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            gunType++;
        }
    }

    public void Recover()
    {
        isReloading = false;
    }

    public void Reload()
    {
        if(bulletsBag[gunType] > 0)
        {
            isReloading = true;
            Invoke("Recover", 2.667f);
            ani.SetTrigger("Reloading");
            switch (gunType)
            {
                case GUNTYPE.SNIPING:
                    if (bulletsBag[gunType] >= maxSnipingleShotBullets)
                    {
                        bulletsBag[gunType] -= maxSnipingleShotBullets;
                        bulletsClip[gunType] = maxSnipingleShotBullets;
                    }
                    else if (bulletsBag[gunType] > 0)
                    {
                        bulletsClip[gunType] = bulletsBag[gunType];
                        bulletsBag[gunType] = 0;
                    }
                    break;
                case GUNTYPE.AUTO:
                    if (bulletsBag[gunType] >= maxAutoShotBullets)
                    {
                        bulletsBag[gunType] -= maxAutoShotBullets;
                        bulletsClip[gunType] = maxAutoShotBullets;
                    }
                    else if (bulletsBag[gunType] > 0)
                    {
                        bulletsClip[gunType] = bulletsBag[gunType];
                        bulletsBag[gunType] = 0;
                    }
                    break;
                case GUNTYPE.SINGLESHOT:
                    if (bulletsBag[gunType] >= maxSingleShotBullets)
                    {
                        bulletsBag[gunType] -= maxSingleShotBullets;
                        bulletsClip[gunType] = maxSingleShotBullets;
                    }
                    else if (bulletsBag[gunType] > 0)
                    {
                        bulletsClip[gunType] = bulletsBag[gunType];
                        bulletsBag[gunType] = 0;
                    }
                    break;
            }
        }
        
    }

    private void SingleShot()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - attacktimer >= attackCD)
        {
            if(bulletsClip[gunType] > 0)
            {
                bulletsClip[gunType]--;
                GameObject obj = Instantiate(gunEffectPrefab, gunEffect);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localEulerAngles = Vector3.zero;
                attacktimer = Time.time;
                ani.SetTrigger("SingleShot");
                RaycastHit hit;
                if (Physics.Raycast(gunPoint.position, gunPoint.transform.forward, out hit, 200f))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        Instantiate(bloodEffect, hit.point, Quaternion.identity);
                    }
                    if (hit.collider.CompareTag("Ground"))
                    {
                        Instantiate(groundEffect, hit.point, Quaternion.identity);
                    }
                }
            }
            else
            {
                Reload();
            }
        }
    }

    private void AutoShot()
    {
        if (Input.GetMouseButton(0) && Time.time - attacktimer >= attackCD)
        {
            if (bulletsClip[gunType] > 0)
            {
                bulletsClip[gunType]--;
                Debug.Log(bulletsClip[gunType]);
                GameObject obj = Instantiate(gunEffectPrefab, gunEffect);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localEulerAngles = Vector3.zero;
                attacktimer = Time.time;
                ani.SetBool("AutoShot", true);
                RaycastHit hit;
                if (Physics.Raycast(gunPoint.position, gunPoint.transform.forward, out hit, 200f))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        Instantiate(bloodEffect, hit.point, Quaternion.identity);
                    }
                    if (hit.collider.CompareTag("Ground"))
                    {
                        Instantiate(groundEffect, hit.point, Quaternion.identity);
                    }
                }
            }
            else
            {
                Reload();
            }
        }
        else if (Input.GetMouseButtonUp(0) || isReloading)
        {
            ani.SetBool("AutoShot", false);
        }
    }

    private void SnipingShot()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - attacktimer >= attackCD)
        {
            if (bulletsClip[gunType] > 0)
            {
                bulletsClip[gunType]--;
                GameObject obj = Instantiate(gunEffectPrefab, gunEffect);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localEulerAngles = Vector3.zero;
                attacktimer = Time.time;
                ani.SetTrigger("SingleShot");
                RaycastHit hit;
                if (Physics.Raycast(gunPoint.position, gunPoint.transform.forward, out hit, 200f))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        Instantiate(bloodEffect, hit.point, Quaternion.identity);
                    }
                    if (hit.collider.CompareTag("Ground"))
                    {
                        Instantiate(groundEffect, hit.point, Quaternion.identity);
                    }
                }
            }
            else
            {
                Reload();
            }
        }
    }
}
