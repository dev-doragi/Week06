using DG;
using DG.Tweening;
using JetBrains.Annotations;
using NUnit.Framework;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlacedPart : SerializedMonoBehaviour
{
    [SerializeField] public PartData data;
    public Vector2Int origin;
    public int rotation;
    public List<Vector2Int> occupiedCells = new();

    [SerializeField] private float currentHp;

    private readonly List<SpriteRenderer> cellRenderers = new();
    private Rigidbody2D rigid;
    private List<BoxCollider2D> cellCols = new();
    public float CurrentHp => currentHp;

    public void Initialize(PartData data, Vector2Int origin, int rotation, List<Vector2Int> occupiedCells)
    {
        this.data = data;
        this.origin = origin;
        this.rotation = rotation;
        this.occupiedCells = new List<Vector2Int>(occupiedCells);

        currentHp = data.health;
    }

    public void BuildVisual(GridRenderer gridRenderer, Transform visualParent, UnityEngine.Color color)
    {
        ClearVisual();

        foreach (var cell in occupiedCells)
        {
            GameObject cellObj = new GameObject($"Cell_{cell.x}_{cell.y}");
            cellObj.transform.SetParent(visualParent != null ? visualParent : transform);

            cellObj.transform.position = gridRenderer.GridToWorld(cell);
            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
            BoxCollider2D boxCol = cellObj.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(1, 1);
            sr.sprite = data.Icon;
            sr.color = color;

            float scale = gridRenderer.cellSize;
            cellObj.transform.localScale = new Vector3(scale, scale, 1f);

            cellRenderers.Add(sr);
            cellCols.Add(boxCol);
        }
        if (gameObject.GetComponent<Rigidbody2D>() == null)
        {
            rigid = gameObject.AddComponent<Rigidbody2D>();
            rigid.bodyType = RigidbodyType2D.Static;
        }
    }

    public void SetColor(UnityEngine.Color color)
    {
        foreach (var sr in cellRenderers)
        {
            if (sr != null)
                sr.color = color;
        }
    }

    public void ClearVisual()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        cellRenderers.Clear();
    }

    public void ShakeX()
    {
        if (DOTween.IsTweening(transform.parent))
            return;
        transform.parent.DOPunchPosition(new Vector3(0.3f, 0f, 0f), 0.25f, 12, 0.8f);
    }

    public void DecreaseHp(float damage)
    {
        currentHp -= damage;

        float value = .5f + ((currentHp / data.health) / 2);

        UnityEngine.Color color = new UnityEngine.Color(1, value, value, 1);
        SetColor(color);
        if (currentHp <= 0f)
        {
            BuildManager.Instance.BrokenPart(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //여기서 무기 충돌 판정
        //DecreaseHp(float damage)
    }

    public void DestroyAnim()
    {
        StartCoroutine(DropAnim());
    }

    IEnumerator DropAnim()
    {
        rigid.bodyType = RigidbodyType2D.Dynamic;
        Vector2 ExplosionVector = new Vector2(Random.Range(-.7f, .7f), 1);
        foreach (var col in cellCols)
        {
            if (col != null)
                col.isTrigger = true;
        }
        rigid.AddForce(ExplosionVector * 2, ForceMode2D.Impulse);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
