using UnityEngine;

public class Actor_EnemyShooter : MonoBehaviour
{
    [Header("Shoot Options")]
    public Transform FirePoint;
    public GameObject Bullet;
    public float BulletSpeed = 100f;
    public float BulletDamage = 5f;
    public float FireRate = 0.2f;       // 플레이어용 itemData 대신 독립된 연사 속도 변수 추가

    private bool isFiring = false;
    private float lastFireTime;

    private void Update()
    {
        // AI에 의해 발사 상태(isFiring)가 true일 때 연사 처리
        if (isFiring)
        {
            if (Time.time >= lastFireTime + FireRate)
            {
                Fire();
                lastFireTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 에너미 AI 스크립트에서 호출할 발사 제어 함수
    /// </summary>
    /// <param name="shouldFire">true면 발사 시작, false면 발사 중지</param>
    public void SetFire(bool shouldFire)
    {
        isFiring = shouldFire;
    }

    void Fire()
    {
        if (FirePoint == null || Bullet == null) return;

        //Debug.Log($"{gameObject.name} 에너미: 탕! (라이플 연사)");
        
        Vector3 pos = FirePoint.position;
        Quaternion dir = FirePoint.rotation;

        // 총알 프리팹 생성
        GameObject bulletClone = Instantiate(Bullet, pos, dir);
        
        // 총알에 데미지 전달 (기존 Player 총알 재사용 가능)
        Actor_Bullet bulletScript = bulletClone.GetComponent<Actor_Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(BulletDamage);
        }
        
        // 물리 효과로 총구 정면(forward) 방향으로 날아가게 처리
        Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(FirePoint.forward * BulletSpeed, ForceMode.VelocityChange);
        }

        // 2초 뒤 총알 삭제
        Destroy(bulletClone, 2f);
    }
}