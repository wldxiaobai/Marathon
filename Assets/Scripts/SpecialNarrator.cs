using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialNarrator : NarratorTrigger
{
    [SerializeField]
    [Tooltip("实体碰撞箱出现位置")]
    private Vector2 pos = Vector2.zero;
    [SerializeField]
    [Tooltip("实体碰撞箱预制体")]
    private GameObject colliderPrefab;

    private List<GameObject> generatedColliders = new List<GameObject>();

    /// <summary>
    /// 旁白显示后触发，生成碰撞箱
    /// </summary>
    protected override void OnNarratorDisplayed()
    {
        SpawnCollider();
    }

    /// <summary>
    /// 生成碰撞箱预制体
    /// </summary>
    private void SpawnCollider()
    {
        if (colliderPrefab == null)
        {
            Debug.LogError("colliderPrefab 未设置");
            return;
        }

        // 在指定位置生成预制体
        GameObject instance = Instantiate(colliderPrefab, pos, Quaternion.identity);

        if (instance != null)
        {
            generatedColliders.Add(instance);
            Debug.Log($"碰撞箱在位置 {pos} 生成成功");
        }
        else
        {
            Debug.LogError("碰撞箱生成失败");
        }
    }

    /// <summary>
    /// 清除所有已生成的碰撞箱
    /// </summary>
    public void ClearGeneratedColliders()
    {
        foreach (GameObject collider in generatedColliders)
        {
            if (collider != null)
            {
                Destroy(collider);
            }
        }
        generatedColliders.Clear();
        Debug.Log("已清除所有生成的碰撞箱");
    }

    /// <summary>
    /// 获取已生成的碰撞箱列表
    /// </summary>
    public List<GameObject> GetGeneratedColliders()
    {
        return generatedColliders;
    }
}
