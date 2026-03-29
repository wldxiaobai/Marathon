using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 木头对象池管理器，用于复用木头对象以提高性能
/// </summary>
public class WoodPool : MonoBehaviour
{
    /// <summary>
    /// 对象池的初始大小
    /// </summary>
    [SerializeField]
    [Tooltip("对象池初始创建的木头对象数量")]
    private int initialPoolSize = 10;

    /// <summary>
    /// 木头预制体
    /// </summary>
    [SerializeField]
    [Tooltip("木头预制体")]
    private GameObject woodPrefab;

    /// <summary>
    /// 可用的木头对象栈
    /// </summary>
    private Stack<GameObject> availableWood = new Stack<GameObject>();

    /// <summary>
    /// 正在使用的木头对象集合
    /// </summary>
    private HashSet<GameObject> activeWood = new HashSet<GameObject>();

    /// <summary>
    /// 木头对象的父容器
    /// </summary>
    private Transform poolContainer;

    /// <summary>
    /// 单例实例
    /// </summary>
    private static WoodPool instance;
    /// <summary>
    /// 标记应用是否正在退出
    /// </summary>
    private static bool isApplicationQuitting = false;

    public static WoodPool Instance
    {
        get
        {
            // 应用退出时，不创建新实例
            if (isApplicationQuitting)
                return null;

            if (instance == null)
            {
                instance = FindObjectOfType<WoodPool>();
                // 只在游戏运行时创建新实例，不在场景加载/卸载时创建
                if (instance == null && Application.isPlaying)
                {
                    GameObject poolObject = new GameObject("WoodPool");
                    instance = poolObject.AddComponent<WoodPool>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Initialize();
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void Initialize()
    {
        // 创建容器以管理池中的对象
        poolContainer = new GameObject("WoodPool_Container").transform;
        poolContainer.parent = transform;

        // 预先创建对象池中的对象
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewWood();
        }
    }

    /// <summary>
    /// 创建新的木头对象并放入可用栈
    /// </summary>
    private void CreateNewWood()
    {
        if (woodPrefab == null)
        {
            return;
        }

        GameObject wood = Instantiate(woodPrefab, poolContainer);
        wood.name = "Wood_Pooled";
        wood.SetActive(false);
        availableWood.Push(wood);
    }

    /// <summary>
    /// 从对象池中获取木头对象
    /// </summary>
    /// <param name="position">木头的世界位置</param>
    /// <returns>返回获取的木头对象</returns>
    public GameObject GetWood(Vector3 position)
    {
        GameObject wood;

        // 如果池中有可用对象，直接使用；否则创建新对象
        if (availableWood.Count > 0)
        {
            wood = availableWood.Pop();
        }
        else
        {
            CreateNewWood();
            wood = availableWood.Pop();
        }

        // 激活前重置Rigidbody2D物理状态
        Rigidbody2D rb = wood.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 重置速度和角速度
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // 重新启用重力和碰撞检测
            rb.isKinematic = false;
        }

        // 设置位置并激活
        wood.transform.position = position;
        wood.SetActive(true);
        activeWood.Add(wood);

        return wood;
    }

    /// <summary>
    /// 将木头对象归还到对象池
    /// </summary>
    /// <param name="wood">要归还的木头对象</param>
    public void ReturnWood(GameObject wood)
    {
        if (wood == null)
            return;

        // 场景卸载时，不进行归还操作
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded)
            return;

        if (!activeWood.Contains(wood))
            return;

        activeWood.Remove(wood);
        wood.SetActive(false);
        wood.transform.parent = poolContainer;
        availableWood.Push(wood);
    }

    /// <summary>
    /// 将传入的木头物体纳入对象池管理范围
    /// </summary>
    /// <param name="externalWood">外部的木头物体（非池化创建的）</param>
    /// <param name="isActive">物体当前是否为「活跃使用中」状态（true=加入活跃池，false=加入可用池）</param>
    public void AddWoodToPoolManagement(GameObject externalWood, bool isActive = false)
    {
        // 空值校验
        if (externalWood == null)
        {
            Debug.LogError("WoodPool: 传入的木头物体为空，无法纳入管理！");
            return;
        }

        // 避免重复管理（如果已经在池里，直接返回）
        if (activeWood.Contains(externalWood) || availableWood.Contains(externalWood))
        {
            Debug.LogWarning($"WoodPool: 物体 {externalWood.name} 已在对象池管理中，无需重复添加！");
            return;
        }

        // 统一命名规范（便于识别池化对象）
        if (!externalWood.name.StartsWith("Wood_Pooled"))
        {
            externalWood.name = $"Wood_Pooled_{externalWood.name}";
        }

        // 将物体父节点挂载到池容器（统一管理）
        externalWood.transform.parent = poolContainer;

        // 根据状态分类加入池管理：活跃态/可用态
        if (isActive)
        {
            // 活跃态：加入activeWood集合，保持激活状态
            externalWood.SetActive(true);
            // 重置物理状态（避免外部物体带残留物理属性）
            Rigidbody2D rb = externalWood.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = false;
            }
            activeWood.Add(externalWood);
        }
        else
        {
            // 可用态：加入availableWood栈，失活并归位
            externalWood.SetActive(false);
            availableWood.Push(externalWood);
        }

        Debug.Log($"WoodPool: 物体 {externalWood.name} 已成功纳入对象池管理，当前状态：{(isActive ? "活跃使用中" : "可用待分配")}");
    }

    /// <summary>
    /// 获取当前池中可用的木头数量
    /// </summary>
    /// <returns>可用木头数量</returns>
    public int GetAvailableCount()
    {
        return availableWood.Count;
    }

    /// <summary>
    /// 获取当前使用中的木头数量
    /// </summary>
    /// <returns>使用中的木头数量</returns>
    public int GetActiveCount()
    {
        return activeWood.Count;
    }

    /// <summary>
    /// 检查指定的游戏对象是否是活跃的木头对象
    /// </summary>
    /// <param name="wood">要检查的游戏对象</param>
    /// <returns>如果是活跃的木头对象返回true，否则返回false</returns>
    public bool IsActiveWood(GameObject wood)
    {
        return wood != null && activeWood.Contains(wood);
    }

    /// <summary>
    /// 获取木头预制体的Collider2D
    /// </summary>
    /// <returns>返回木头预制体的Collider2D，如果不存在返回null</returns>
    public Collider2D GetWoodCollider()
    {
        if (woodPrefab == null)
            return null;

        return woodPrefab.GetComponent<Collider2D>();
    }

    /// <summary>
    /// 获取木头预制体的缩放
    /// </summary>
    /// <returns>返回木头预制体的transform.scale</returns>
    public Vector3 GetWoodScale()
    {
        if (woodPrefab == null)
            return Vector3.one;

        return woodPrefab.transform.localScale;
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearPool()
    {
        // 只在运行时销毁对象，编辑模式下由 Unity 自动处理
        if (Application.isPlaying)
        {
            foreach (var wood in availableWood)
            {
                Destroy(wood);
            }
            foreach (var wood in activeWood)
            {
                Destroy(wood);
            }
        }
        availableWood.Clear();
        activeWood.Clear();
    }

    /// <summary>
    /// 应用退出时调用
    /// </summary>
    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    /// <summary>
    /// 对象池销毁时清理所有资源
    /// </summary>
    private void OnDestroy()
    {
        // 场景卸载时清空对象池
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded)
        {
            // 清空集合，释放对象引用
            // 子对象会由 Unity 自动销毁，无需手动调用 Destroy
            availableWood.Clear();
            activeWood.Clear();
            poolContainer = null;

            // 清除单例引用
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}