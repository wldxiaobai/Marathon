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
        // 场景卸载时，Unity已经在处理失活，无需手动操作
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded)
            return;

        if (WoodPool.Instance == null || !isRegistered)
            return;

        // 自动归还到对象池
        WoodPool.Instance.ReturnWood(gameObject);
        isRegistered = false;
    }

    /// <summary>
    /// 防止对象被直接销毁，强制归还到对象池
    /// 仅在非场景卸载时生效
    /// </summary>
    private void OnDestroy()
    {
        // 场景卸载时，所有对象都会被销毁，无需归还到对象池
        // 只在运行时场景中手动销毁对象时才归还
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded)
            return;

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