using UnityEngine;

public class Actor_Bullet : MonoBehaviour
{
    public GameObject MissEffect, HitEffect;
    public GameObject ShootSound, HitSound;

    [Header("Visual Model")]
    public string VisualModelResourcePath = "Models/SniperBullet/source/Bala";
    public string VisualModelBaseColorTextureResourcePath = "";
    public string VisualModelHiddenNameFragments = "";
    public Vector3 VisualModelLocalPosition = Vector3.zero;
    public Vector3 VisualModelLocalEulerAngles = Vector3.zero;
    public Vector3 VisualModelForwardFlipEulerAngles = new Vector3(0f, 180f, 0f);
    public bool AutoAlignVisualModel = true;
    public bool AutoFitVisualModel = true;
    public float VisualModelTargetLength = 0.28f;
    public bool HideDefaultVisuals = true;

    private float bulletDamage = 5f;    
    private bool isHit = false;  

    private Rigidbody rb;
    private Vector3 lastVelocity; // [추가] 물리 엔진이 속도를 0으로 만들기 전의 속도를 기억할 변수
    private GameObject activeVisualModel;
    private static readonly Vector3[] VisualAlignmentRotations =
    {
        Vector3.zero,
        new Vector3(90f, 0f, 0f),
        new Vector3(-90f, 0f, 0f),
        new Vector3(0f, 90f, 0f),
        new Vector3(0f, -90f, 0f),
        new Vector3(0f, 0f, 90f),
        new Vector3(0f, 0f, -90f),
        new Vector3(180f, 0f, 0f),
        new Vector3(0f, 180f, 0f),
        new Vector3(0f, 0f, 180f)
    };

    void Awake()
    {
        SetupVisualModel();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void SetupVisualModel(string visualModelName = "SniperBullet_Model")
    {
        ClearActiveVisualModel();

        if (string.IsNullOrWhiteSpace(VisualModelResourcePath))
        {
            return;
        }

        GameObject visualPrefab = Resources.Load<GameObject>(VisualModelResourcePath);
        if (visualPrefab == null)
        {
            Debug.LogWarning($"Bullet visual model not found in Resources: {VisualModelResourcePath}", this);
            return;
        }

        if (HideDefaultVisuals)
        {
            foreach (Renderer defaultRenderer in GetComponentsInChildren<Renderer>(true))
            {
                defaultRenderer.enabled = false;
            }
        }

        GameObject visualModel = Instantiate(visualPrefab, transform);
        activeVisualModel = visualModel;
        visualModel.name = visualModelName;
        visualModel.transform.localPosition = VisualModelLocalPosition;
        visualModel.transform.localRotation = Quaternion.Euler(VisualModelLocalEulerAngles);
        visualModel.transform.localScale = Vector3.one;

        HideVisualModelNameFragments(visualModel);

        foreach (Collider visualCollider in visualModel.GetComponentsInChildren<Collider>())
        {
            visualCollider.enabled = false;
        }

        foreach (Rigidbody visualRigidbody in visualModel.GetComponentsInChildren<Rigidbody>())
        {
            Destroy(visualRigidbody);
        }

        ApplyVisualModelTexture(visualModel);

        if (AutoAlignVisualModel)
        {
            AlignVisualModelForward(visualModel);
        }

        if (VisualModelForwardFlipEulerAngles != Vector3.zero)
        {
            visualModel.transform.localRotation *= Quaternion.Euler(VisualModelForwardFlipEulerAngles);
        }

        if (AutoFitVisualModel)
        {
            FitVisualModelToBullet(visualModel);
        }
    }

    private void ClearActiveVisualModel()
    {
        if (activeVisualModel == null)
        {
            return;
        }

        foreach (Renderer visualRenderer in activeVisualModel.GetComponentsInChildren<Renderer>(true))
        {
            visualRenderer.enabled = false;
        }

        activeVisualModel.transform.SetParent(null, false);

        if (Application.isPlaying)
        {
            Destroy(activeVisualModel);
        }
        else
        {
            DestroyImmediate(activeVisualModel);
        }

        activeVisualModel = null;
    }

    private void HideVisualModelNameFragments(GameObject visualModel)
    {
        if (string.IsNullOrWhiteSpace(VisualModelHiddenNameFragments))
        {
            return;
        }

        string[] fragments = VisualModelHiddenNameFragments.Split(';', ',');
        foreach (Transform visualTransform in visualModel.GetComponentsInChildren<Transform>(true))
        {
            if (visualTransform == visualModel.transform)
            {
                continue;
            }

            string lowerName = visualTransform.name.ToLowerInvariant();
            for (int i = 0; i < fragments.Length; i++)
            {
                string fragment = fragments[i].Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(fragment) && lowerName.Contains(fragment))
                {
                    visualTransform.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private void ApplyVisualModelTexture(GameObject visualModel)
    {
        if (string.IsNullOrWhiteSpace(VisualModelBaseColorTextureResourcePath))
        {
            return;
        }

        Texture2D baseColorTexture = Resources.Load<Texture2D>(VisualModelBaseColorTextureResourcePath);
        if (baseColorTexture == null)
        {
            return;
        }

        foreach (Renderer visualRenderer in visualModel.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = visualRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_MainTex"))
                {
                    materials[i].mainTexture = baseColorTexture;
                }
            }
        }
    }

    public void SetVisualModel(
        string resourcePath,
        string baseColorTextureResourcePath,
        string hiddenNameFragments,
        Vector3 localPosition,
        Vector3 localEulerAngles,
        Vector3 forwardFlipEulerAngles,
        bool autoAlign,
        bool autoFit,
        float targetLength,
        bool hideDefaultVisuals,
        string visualModelName)
    {
        VisualModelResourcePath = resourcePath;
        VisualModelBaseColorTextureResourcePath = baseColorTextureResourcePath;
        VisualModelHiddenNameFragments = hiddenNameFragments;
        VisualModelLocalPosition = localPosition;
        VisualModelLocalEulerAngles = localEulerAngles;
        VisualModelForwardFlipEulerAngles = forwardFlipEulerAngles;
        AutoAlignVisualModel = autoAlign;
        AutoFitVisualModel = autoFit;
        VisualModelTargetLength = targetLength;
        HideDefaultVisuals = hideDefaultVisuals;

        SetupVisualModel(visualModelName);
    }

    private void AlignVisualModelForward(GameObject visualModel)
    {
        Quaternion baseRotation = Quaternion.Euler(VisualModelLocalEulerAngles);
        Quaternion bestRotation = baseRotation;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < VisualAlignmentRotations.Length; i++)
        {
            visualModel.transform.localRotation = baseRotation * Quaternion.Euler(VisualAlignmentRotations[i]);

            if (!TryGetVisualBoundsInBulletSpace(visualModel, out Bounds bounds))
            {
                continue;
            }

            float sideSize = Mathf.Max(bounds.size.x, bounds.size.y);
            float score = bounds.size.z - sideSize;
            if (score > bestScore)
            {
                bestScore = score;
                bestRotation = visualModel.transform.localRotation;
            }
        }

        visualModel.transform.localRotation = bestRotation;
    }

    private void FitVisualModelToBullet(GameObject visualModel)
    {
        if (!TryGetVisualBoundsInBulletSpace(visualModel, out Bounds bounds) || VisualModelTargetLength <= 0f)
        {
            return;
        }

        float longestSide = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (longestSide > 0.001f)
        {
            float scaleMultiplier = VisualModelTargetLength / longestSide;
            visualModel.transform.localScale *= scaleMultiplier;
        }

        if (TryGetVisualBoundsInBulletSpace(visualModel, out bounds))
        {
            visualModel.transform.localPosition += VisualModelLocalPosition - bounds.center;
        }
    }

    private bool TryGetVisualBoundsInBulletSpace(GameObject visualModel, out Bounds bounds)
    {
        Renderer[] renderers = visualModel.GetComponentsInChildren<Renderer>();
        bounds = new Bounds();

        if (renderers.Length == 0)
        {
            return false;
        }

        bool hasBounds = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled || !renderers[i].gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds rendererBounds = renderers[i].bounds;
            Vector3 min = rendererBounds.min;
            Vector3 max = rendererBounds.max;

            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, min.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, min.y, max.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, max.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, max.y, max.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, min.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, min.y, max.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, max.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateBulletLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, max.y, max.z)), ref bounds, ref hasBounds);
        }

        return hasBounds;
    }

    private void EncapsulateBulletLocalPoint(Vector3 point, ref Bounds bounds, ref bool hasBounds)
    {
        if (!hasBounds)
        {
            bounds = new Bounds(point, Vector3.zero);
            hasBounds = true;
            return;
        }

        bounds.Encapsulate(point);
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
