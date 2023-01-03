using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class nay phu trach viec lay ra nhung diem thuoc polygon cua cac vat can tren scene
public class Obstacle : MonoBehaviour
{
    private static Obstacle ins;
    private List<Vector2> listPointPolygon = new List<Vector2>();
    private PolygonCollider2D[] arrayPolygon;

    public static Obstacle Ins { get => ins; }
    public List<Vector2> ListPointPolygon { get => listPointPolygon; }

    void Awake()
    {
        MakeSingleton();
        GetArrayPolygon();
        GetListPointPolygon();


    }
    void Start()
    {

    }

    void MakeSingleton()
    {
        if (ins == null)
        {
            ins = this as Obstacle;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void GetArrayPolygon()
    {
        arrayPolygon = gameObject.transform.GetComponentsInChildren<PolygonCollider2D>();
        //lay ra 1 mang cac polygon ung voi cac vat can( barrier)
        //luu y cac phan tu con chi duoc gan polygon collider2D
    }
    public void SetIsTriggerBarrier(bool value)
    {
        if (arrayPolygon == null || arrayPolygon.Length <= 0) return;
        for (int i = 0; i < arrayPolygon.Length; i++)
        {
            arrayPolygon[i].GetComponent<PolygonCollider2D>().isTrigger = value;
        }
    }
    private void GetListPointPolygon()
    {
        if (arrayPolygon == null || arrayPolygon.Length <= 0) return;
        for (int i = 0; i < arrayPolygon.Length; i++)
        {
            GameObject barrier = arrayPolygon[i].gameObject;
            barrier.layer = LayerMask.NameToLayer("Barrier");
            var points = arrayPolygon[i].points;
            Transform polygonTransform = arrayPolygon[i].transform;
            //var points = arrayPolygon[i].GetPath(0);
            for (int j = 0; j < points.Length; j++)
            {
                Vector2 point = polygonTransform.transform.TransformPoint(points[j]);
                listPointPolygon.Add(point);
            }
        }
    }
    public Vector2 GetPointNearInList(Vector2 point)//lay diem gan voi diem point nhat trong list,ko phai diem point
    {
        float distance = 100f;
        Vector2 result = new Vector2(100f, 100f);
        for (int i = 0; i < listPointPolygon.Count; i++)
        {
            if (listPointPolygon[i] != point && Vector2.Distance(listPointPolygon[i], point) < distance)
            {
                distance = Vector2.Distance(listPointPolygon[i], point);
                result = listPointPolygon[i];
            }
        }
        return result;
    }



}
