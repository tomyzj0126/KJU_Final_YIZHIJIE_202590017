using UnityEngine;

public class Actor_Bullet : MonoBehaviour
{
    public GameObject MissEffect, HitEffect;
    public GameObject ShootSound, HitSound;
    private float bulletDamage = 5f;    
    private bool isHit = false;  

    private Rigidbody rb;
    private Vector3 lastVelocity; // [추가] 물리 엔진이 속도를 0으로 만들기 전의 속도를 기억할 변수

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // [추가] 충돌하기 직전 프레임까지의 실제 속도를 매 프레임 안전하게 기록합니다.
        if (rb != null && rb.velocity.sqrMagnitude > 0.1f)
        {
            lastVelocity = rb.velocity;
        }
    }

    public void SetDamage(float amount)
    {
        bulletDamage = amount;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isHit) return;

        ContactPoint contactPoint = collision.contacts[0];
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"[OnTriggerEnter] Hit {collision.gameObject.name}! Damage: " + bulletDamage);
            isHit = true;
            ShowEffect(HitEffect, contactPoint); 
            //ScoreManager.Instance.AddScore(-10);    
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(bulletDamage);   
            Destroy(gameObject);
        } 
        else if(collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"[OnTriggerEnter] Hit {collision.gameObject.name}! Damage: " + bulletDamage);
            isHit = true;
            ShowEffect(HitEffect, contactPoint);
            //ScoreManager.Instance.AddScore(10);          
            collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(bulletDamage);
            Destroy(gameObject);
        }        
        else if(collision.gameObject.CompareTag("Shootable"))
        {
            // Debug.Log("[OnCollisionEnter] Miss Target! No Damage");
            ShowEffect(MissEffect, contactPoint);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, 2f);    
        }
        
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (isHit) return;

        Vector3 contactPoint = other.ClosestPoint(transform.position);
        if (other.CompareTag("Player")){
            isHit = true;
            Debug.Log($"[OnTriggerEnter] Hit {other.name}! Damage: " + bulletDamage);
            ShowEffect(HitEffect, contactPoint);

            //ScoreManager.Instance.AddScore(-10);
            other.GetComponent<PlayerHealth>().TakeDamage(bulletDamage);
            Destroy(gameObject);
        } 
        else if(other.CompareTag("Enemy"))
        {
            isHit = true;
            Debug.Log($"[OnTriggerEnter] Hit {other.name}! Damage: " + bulletDamage);
            ShowEffect(HitEffect, contactPoint);

            //ScoreManager.Instance.AddScore(10);
            other.GetComponent<EnemyHealth>().TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
        else if(other.CompareTag("Shootable"))
        {
            Debug.Log($"[OnTriggerEnter] Miss {other.name} ");
            ShowEffect(MissEffect, contactPoint);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, 2f);    
        }
        
        // Destroy(gameObject);
    }

    void ShowEffect(GameObject Effect, ContactPoint contactPoint)
    {
        // Vector3 pos = contactPoint.point; // 부딪힌 벽면/몸통 표면 좌표
        Vector3 pos = contactPoint.point + (contactPoint.normal * 0.05f); // 부딪힌 벽면/몸통 표면 좌표
        Quaternion dir = Quaternion.LookRotation(contactPoint.normal);
        GameObject hitEffectClone = Instantiate(Effect, pos, dir);
        Destroy(hitEffectClone, 2f);
    }

    void ShowEffect(GameObject Effect, Vector3 contactPoint)
    {
        Vector3 surfaceNormal = (transform.position - contactPoint).normalized;
        Vector3 pos = contactPoint + (surfaceNormal * 0.05f);        
        Quaternion dir = Quaternion.LookRotation(surfaceNormal);
        GameObject hitEffectClone = Instantiate(Effect, pos, dir);
        Destroy(hitEffectClone, 2f);
    }

    void ShowEffect(GameObject Effect, float calibTime)
    {
        // [수정] 현재 충돌해서 0이 되었을지도 모르는 rb.velocity 대신, 우리가 기록한 lastVelocity를 사용합니다.
        Vector3 bulletSpeed = lastVelocity; 

        // 예외 처리: 만약 기록된 속도마저 없다면 현재 Rigidbody 속도나마 대안으로 사용
        if (bulletSpeed.sqrMagnitude < 0.001f && rb != null)
        {
            bulletSpeed = rb.velocity;
        }

        // 만약 완전히 멈춰있는 상태라면 오프셋 없이 현재 위치에 생성
        Vector3 pos = transform.position;
        Quaternion dir = transform.rotation;

        if (bulletSpeed.sqrMagnitude > 0.001f)
        {
            // 현재 위치 - (직전 탄속 * 시간) = 정확히 속도에 비례한 뒤쪽 위치
            pos = transform.position - (bulletSpeed * calibTime);
            // 날아가던 방향 그대로 이펙트 정렬
            dir = Quaternion.LookRotation(bulletSpeed.normalized);
        }

        GameObject hitEffectClone = Instantiate(Effect, pos, dir);
        Destroy(hitEffectClone, 2f);
    }
    

    void ShowEffect(GameObject Effect)
    {
        // 만약 완전히 멈춰있는 상태라면 오프셋 없이 현재 위치에 생성
        Vector3 pos = transform.position;
        Quaternion dir = transform.rotation;

        GameObject hitEffectClone = Instantiate(Effect, pos, dir);
        Destroy(hitEffectClone, 2f);
    }
}