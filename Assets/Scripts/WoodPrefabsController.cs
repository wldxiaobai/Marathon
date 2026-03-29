using UnityEngine;

/// <summary>
/// 木头对象自动注册到WoodPool的脚本
/// 挂载在木头预制体上，用于自动管理对象池的归属
/// </summary>
[RequireComponent(typeof(Collider2D))] // 确保木头有碰撞体（根据实际需求可移除）
public class WoodPoolAutoRegister : MonoBehaviour
{
    // 标记是否已经完成注册，避免重复处理
    private bool isRegistered = false;

    private void OnEnable()
    {
        if (WoodPool.Instance == null)
        {
            Debug.LogError("WoodPoolAutoRegister: WoodPool单例未找到，无法注册木头对象！");
            return;
        }

        if (!isRegistered)
        {
            // 调用新增函数，将自身纳入管理（标记为活跃态）
            WoodPool.Instance.AddWoodToPoolManagement(gameObject, true);
            isRegistered = true;
        }
    }


    /// <summary>
    /// 当对象被失活时调用（用于自动归还到对象池）
    /// </summary>
    private void OnDisable()
    {
        if (WoodPool.Instance == null || !isRegistered)
            return;

        // 自动归还到对象池
        WoodPool.Instance.ReturnWood(gameObject);
        isRegistered = false;
    }

    /// <summary>
    /// 可选：当木头对象触发碰撞/生命周期结束时，主动失活以归还到池
    /// 可根据你的业务逻辑调整触发条件（比如碰撞到地面、超时等）
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 示例：碰撞到标签为"Ground"的物体时，自动失活归还到池
        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 防止对象被直接销毁，强制归还到对象池
    /// </summary>
    private void OnDestroy()
    {
        if (WoodPool.Instance != null && isRegistered)
        {
            WoodPool.Instance.ReturnWood(gameObject);
        }
    }

    /// <summary>
    /// 手动触发归还到对象池（可通过外部调用）
    /// </summary>
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}