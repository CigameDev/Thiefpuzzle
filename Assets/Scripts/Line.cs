using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class Line : MonoBehaviour
{
    public GameObject root;
    public GameObject hand;

    //[SerializeField] float max_speed_x = 0.075f;
    //[SerializeField]float max_speed_y = 0.075f;

    float max_speed_x = 0.075f;
    float max_speed_y = 0.075f;

    [SerializeField] GameObject obstacle;
    [SerializeField] float lineStartWith = 0.02f;
    [SerializeField] float lineEndWith = 0.02f;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float speedPullBack = 5f;
    private LineRenderer ropeRenderer;
    private PolygonCollider2D polygon;
    private Vector3 mouseDown;
    private Vector3 directionEachFrame;//huong di chuyen chuot moi khung frame
    private Vector3 oldMousePos;
    private Rigidbody2D rb;
    private bool ropeAttached = false;//trang thai co the them diem hay khong
    private Vector2[] pointPolygons;
    private List<Vector2> ropePositions = new List<Vector2>();//luu vi tri cac diem cua rope
    private Vector2 oldPosHand;
    private bool canPullBack = false;
    private PolygonCollider2D[] barriers;//mang cac chuong ngai vat
    private Camera mainCamera;
    private Collider2D collider12 = null;
    private Collider2D collider13 = null;
    private List<Collider2D> collider2Ds = new List<Collider2D>();//luu lai cac colider da duoc ket noi 
    void Awake()
    {
        ropeRenderer = GetComponent<LineRenderer>();
        rb = hand.gameObject.GetComponent<Rigidbody2D>();
        oldPosHand = (Vector2)hand.transform.position;
        obstacle = GameObject.Find("Obstacle");
        mainCamera = FindObjectOfType<Camera>();
        if (obstacle != null)
        {
            barriers = obstacle.transform.GetComponentsInChildren<PolygonCollider2D>();
        }

    }
    #region START
    void Start()
    {
        GetPointPolygon();
        SetColorRope(Color.gray);
        ropeRenderer.startWidth = lineStartWith;
        ropeRenderer.endWidth = lineEndWith;
        ropeRenderer.SetPosition(0, root.transform.position);
        ropeRenderer.SetPosition(1, hand.transform.position);
        ropePositions.Add(root.transform.position);
        ropePositions.Add(hand.transform.position);


    }
    #endregion
    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //barrier.GetComponent<PolygonCollider2D>().isTrigger = true;//chuyen chuong ngai vat thanh isTrigger de ban tay co the di xuyen qua khong bi can lai
            Obstacle.Ins.SetIsTriggerBarrier(true);
            canPullBack = true;
        }
        UpdateRopePositions();
        PullBackRope();
        hand.transform.Translate(directionEachFrame);
    }
    
    private void UpdateLastPoint()//update vi tri diem cuoi cung theo chuot
    {
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, hand.transform.position);
    }

    protected virtual Vector2 DirectionRaycast()//huong tu hand toi diem gan cuoi cung ket noi cua Line
    {
        int length = ropePositions.Count;
        Debug.DrawLine(ropePositions[length - 2], (Vector2)hand.transform.position);
        return ropePositions[length - 2] - (Vector2)hand.transform.position;
    }

    protected virtual Vector2 DirectionRayCast31()//huong vecto tu hand toi diem truoc truoc no(count-1 -> count -3 )
    {
        int length = ropePositions.Count;
        Debug.DrawLine(ropePositions[length - 3], (Vector2)hand.transform.position, Color.cyan);
        return ropePositions[length - 3] - (Vector2)hand.transform.position;
    }
    private void GetPointPolygon()//lay ra tat ca nhung diem thuoc tat ca cac polygon
    {
        pointPolygons = Obstacle.Ins.ListPointPolygon.ToArray();
    }

    
    #region HandleInput
    protected virtual void HandleInput()//xu ly dau vao
    {
        if (Input.GetMouseButtonDown(0))//khi nhan chuot xuong thi lay vi tri cua chuot (mouseDown ) va luu vao oldMouseDown
        {
            mouseDown = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            oldMousePos = mouseDown;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 curPosMouse = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            directionEachFrame = (curPosMouse - oldMousePos);
            directionEachFrame = new Vector2(Mathf.Abs(directionEachFrame.x) > Mathf.Abs(max_speed_x) ? (max_speed_x * directionEachFrame.x/Mathf.Abs(directionEachFrame.x)) : directionEachFrame.x, Mathf.Abs(directionEachFrame.y) > Mathf.Abs(max_speed_y) ? (max_speed_y * directionEachFrame.y / Mathf.Abs(directionEachFrame.y)) : directionEachFrame.y);
            oldMousePos = curPosMouse;
            UpdateLastPoint();
            if ((Vector2)directionEachFrame == Vector2.zero)
            {
                directionEachFrame = Vector2.zero;
            }
            if (ropeAttached == false)
            {
                var dirRaycast = DirectionRaycast();
                var hit = Physics2D.Raycast(hand.transform.position, dirRaycast, 10, 1 << LayerMask.NameToLayer(StringDefine.GAMETAG_BARRIER));
                collider12 = hit.collider;
                if (hit.collider != null)
                {
                    Vector2 nearPoint12 = GetPointNearOfPolygon(hit.collider, hit.point);
                    if (ropePositions[ropePositions.Count - 2] != nearPoint12)
                    {
                        if (isBetween(nearPoint12, hand.transform.position, ropePositions[ropePositions.Count - 2]))
                        {
                            ropeAttached = true;
                            ropePositions.Insert(ropePositions.Count - 1, nearPoint12);

                        }
                    }
                    else//ropePositions[ropePositions.Count - 2] == hit.point 
                    {
                        //day la doan xu ly remove diem gan cuoi

                        if (ropePositions.Count >= 3)
                        {
                            var dir31 = DirectionRayCast31();
                            var hit31 = Physics2D.Raycast(hand.transform.position, dir31, 10f, 1 << LayerMask.NameToLayer(StringDefine.GAMELAYER_BARRIER));
                            collider13 = hit31.collider;
                            if (hit31.collider != null)//sua o day
                            {
                                Vector2 nearPoint13 = GetPointNearOfPolygon(hit31.collider, hit31.point);
                                float distance = Vector2.Distance(hit31.point, ropePositions[ropePositions.Count - 3]);
                                //if (hit31.point == ropePositions[ropePositions.Count - 3])
                                if (distance <0.2f)
                                {
                                   if (hit.collider != hit31.collider)
                                    {
                                        Vector2 center = centerTriangle(ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3], (Vector2)hand.transform.position);
                                        List<Vector2> newList = GetPointOfAPolygon(collider12.GetComponent<PolygonCollider2D>());
                                        Vector2 randomPoint = RandomPointList(newList, (Vector2)hit.point);
                                        if (!SameSide(center, randomPoint, ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3]))
                                        {
                                            RemovePointNearEnd();
                                        }

                                    }
                                   else//hit.collider == hit31.collider
                                    {
                                        Vector2 center = centerTriangle(ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3], (Vector2)hand.transform.position);
                                        List<Vector2> newList = GetPointOfAPolygon(collider12.GetComponent<PolygonCollider2D>());
                                        Vector2 randomPoint = RandomPointList2(newList, (Vector2)nearPoint12, (Vector2)nearPoint13);
                                        if (!SameSide(center, randomPoint, ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3]))
                                        {
                                            RemovePointNearEnd();
                                        }
                                    }    

                                }

                            }
                            else//hit31.collider ==null
                            {
                                if (ropePositions.Count == 3)
                                {
                                    Vector2 center = centerTriangle(ropePositions[0], ropePositions[1], (Vector2)hand.transform.position);
                                    List<Vector2> newList = GetPointOfAPolygon(collider12.GetComponent<PolygonCollider2D>());
                                    Vector2 newRandomPoint = RandomPointList(newList, (Vector2)nearPoint12);
                                    if (SameSide(newRandomPoint, center, ropePositions[0], ropePositions[1]) == false)
                                    {
                                        RemovePointNearEnd();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            directionEachFrame = Vector2.zero;
            ropeAttached = false;
            HandController handController = hand.GetComponent<HandController>();
            if (handController.CanConnectWithItem)
            {
                handController.SetParent();
                canPullBack = true;
                Obstacle.Ins.SetIsTriggerBarrier(true);
            }
        }
    }
    #endregion
    //ham tra ve cac diem leo cua polygoncollider2D
    protected virtual List<Vector2> GetPointOfAPolygon(PolygonCollider2D poly)
    {
        List<Vector2> result = new List<Vector2>();
        var points = poly.points;
        Transform polyTransform = poly.transform;
        for(int i=0;i<points.Length;i++)
        {
            Vector2 point = polyTransform.transform.TransformPoint(points[i]);
            result.Add(point);
        }    
        return result;
    }    
    protected virtual Vector2 RandomPointList(List<Vector2> list,Vector2 Input)
    {
        // lay ra 1 diem khac voi Input trong mang list Vector2
        float maxDistance = 0f;
        Vector2 result = new Vector2();
        for(int i=0;i<list.Count;i++)
        {
            if(Vector2.Distance(Input, list[i]) > maxDistance)
            {
                maxDistance = Vector2.Distance(Input, list[i]);
                result = list[i];
            }    
        }
        return result;
       
    }    
    protected virtual Vector2 RandomPointList2(List<Vector2>list,Vector2 Input1 ,Vector2 Input2)
    {
        //lay ra 1 diem trong list khac ca 2 diem list 1 va list 2
        if(list[0] != Input1 && list[0]!=Input2) return list[0];
        if (list[1] != Input1 && list[1] != Input2) return list[1];
        return list[2];
    }    
    protected virtual Vector2 GetPointNearOfPolygon(Collider2D col,Vector2 Input)
    {
        PolygonCollider2D poly = col.GetComponent<PolygonCollider2D>();
        List<Vector2> result = GetPointOfAPolygon(poly);
        Vector2 resultVector = result[0];
        float minDistance = Vector2.Distance(result[0], Input);
        for(int i=1;i< result.Count;i++)
        {
            if(minDistance > Vector2.Distance(Input, result[i]))
            {
                minDistance = Vector2.Distance(Input, result[i]);
                resultVector = result[i];
            }    
        }    
        return resultVector;
    }    
    protected virtual void UpdateRopePositions()
    {
        HandleInput();

        if (ropeAttached == false) return;

        ropeRenderer.positionCount = ropeRenderer.positionCount + 1;

        ropeRenderer.SetPosition(ropeRenderer.positionCount - 2, ropePositions[ropePositions.Count - 2]);
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, hand.transform.position);
        ropeAttached = false;
    }
    protected virtual void RemovePointNearEnd()//delete diem gan cuoi 
    {
        int count = ropePositions.Count;
        if (count <= 2) return;
        ropePositions.RemoveAt(count - 2);//xoa o vi tri gan cuoi
        ropeRenderer.positionCount = ropeRenderer.positionCount - 1;
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, hand.transform.position);
    }
    protected virtual bool SameSide(Vector2 point1, Vector2 point2, Vector2 root1, Vector2 root2)
    {
        //kiem tra xem point1 va point2 co nam cung phia voi duong thang noi giua root1 va root2
        //ax +by +c =0
        //a = normal.x ,b = normal.y
        Vector2 direction = root1 - root2;//vecto chi phuong
        Vector2 normal = new Vector2(direction.y, -direction.x);//vecto phap tuyen
        float a = normal.x;
        float b = normal.y;
        float c = -(a * root1.x + b * root1.y);
        float dir1 = a * point1.x + b * point1.y + c;
        float dir2 = a * point2.x + b * point2.y + c;
        return (dir1 * dir2 > 0);
    }

    protected virtual Vector2 centerTriangle(Vector2 A, Vector2 B, Vector2 C)
    {
        float x = (A.x + B.x + C.x) / 3;
        float y = (A.y + B.y + C.y) / 3;
        return new Vector2(x, y);
    }

    protected virtual void PullBackRope()
    {
        if (!canPullBack) return;
        if (ropePositions.Count >= 3)
        {
            Vector2 dirMove = DirectionRaycast().normalized;
            hand.transform.Translate(dirMove * Time.deltaTime * speedPullBack);
            UpdateLastPoint();//update rope follow position of the hand
            if (Vector2.Distance((Vector2)hand.transform.position, ropePositions[ropePositions.Count - 2]) <= 0.1f)
            {
                hand.transform.position = ropePositions[ropePositions.Count - 2];
                RemovePointNearEnd();
            }
        }
        else if (ropePositions.Count == 2)//di chuyen theo huong ve vi tri ban tay ban dau 
        {
            hand.transform.position = Vector2.Lerp(hand.transform.position, oldPosHand, Time.deltaTime * 5f);
            UpdateLastPoint();
            if (Vector2.Distance((Vector2)hand.transform.position, oldPosHand) <= 0.05f)
            {
                hand.transform.position = oldPosHand;
                canPullBack = false;
                Obstacle.Ins.SetIsTriggerBarrier(false);
                //khi ma ban tay tro ve vi tri cu thi win game
                this.PostEvent(EventId.OnWinGame);
                return;
            }
        }
    }
    protected virtual bool isBetween(Vector2 center, Vector2 out1, Vector2 out2)
    {
        //kiem tra xem center co nam giua out1 va out2 mot cach tuong doi hay khong
        //tuc la center ,out1,out2 khong nhat thiet phai nam thang hang
        Vector2 vec1 = out1 - center;
        Vector2 vec2 = out2 - center;
        return Vector2.Dot(vec1, vec2) < 0;
    }
    protected virtual void SetColorRope(Color color)
    {
        ropeRenderer.startColor = color;
        ropeRenderer.endColor = color;
    }
    private float AngelOfTwovector(Vector2 hand,Vector2 vec2,Vector2 vec3)
    {
        Vector2 vec12 = vec2 - hand;
        Vector2 vec13 = vec3 - hand;
        return Vector2.SignedAngle(vec12, vec13);
    }
    #region HandleInput1
    protected virtual void HandleInput1()//xu ly dau vao
    {
        if (Input.GetMouseButtonDown(0))//khi nhan chuot xuong thi lay vi tri cua chuot (mouseDown ) va luu vao oldMouseDown
        {
            mouseDown = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            oldMousePos = mouseDown;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 curPosMouse = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            directionEachFrame = (curPosMouse - oldMousePos);
            directionEachFrame = new Vector2(Mathf.Abs(directionEachFrame.x) > Mathf.Abs(max_speed_x) ? (max_speed_x * directionEachFrame.x / Mathf.Abs(directionEachFrame.x)) : directionEachFrame.x, Mathf.Abs(directionEachFrame.y) > Mathf.Abs(max_speed_y) ? (max_speed_y * directionEachFrame.y / Mathf.Abs(directionEachFrame.y)) : directionEachFrame.y);
            oldMousePos = curPosMouse;
            UpdateLastPoint();
            if ((Vector2)directionEachFrame == Vector2.zero)
            {
                directionEachFrame = Vector2.zero;
            }
            if (ropeAttached == false)
            {
                var dirRaycast = DirectionRaycast();
                var hit = Physics2D.Raycast(hand.transform.position, dirRaycast, 10, 1 << LayerMask.NameToLayer(StringDefine.GAMETAG_BARRIER));
                collider12 = hit.collider;
                if (hit.collider != null)
                {
                    Vector2 nearPoint12 = GetPointNearOfPolygon(hit.collider, hit.point);
                    if (ropePositions[ropePositions.Count - 2] != nearPoint12)
                    {
                        collider2Ds.Add(hit.collider);//them collider vao trong listCollider
                        if (isBetween(nearPoint12, hand.transform.position, ropePositions[ropePositions.Count - 2]))
                        {
                            ropeAttached = true;
                            ropePositions.Insert(ropePositions.Count - 1, nearPoint12);

                        }
                    }
                    else//ropePositions[ropePositions.Count - 2] == hit.point 
                    {
                        //day la doan xu ly remove diem gan cuoi

                        if (ropePositions.Count >= 3)
                        {
                            var dir31 = DirectionRayCast31();
                            var hit31 = Physics2D.Raycast(hand.transform.position, dir31, 10f, 1 << LayerMask.NameToLayer(StringDefine.GAMELAYER_BARRIER));
                            collider13 = hit31.collider;
                            if (hit31.collider != null)//sua o day
                            {
                                Vector2 nearPoint13 = GetPointNearOfPolygon(hit31.collider, hit31.point);
                                float distance = Vector2.Distance(hit31.point, ropePositions[ropePositions.Count - 3]);
                                //if (hit31.point == ropePositions[ropePositions.Count - 3])
                                //if (distance < 0.2f)
                                if (hit31.collider == collider2Ds[collider2Ds.Count-2])
                                {
                                    if (hit.collider != hit31.collider)
                                    {
                                        Vector2 center = centerTriangle(ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3], (Vector2)hand.transform.position);
                                        List<Vector2> newList = GetPointOfAPolygon(collider12.GetComponent<PolygonCollider2D>());
                                        Vector2 randomPoint = RandomPointList(newList, (Vector2)hit.point);
                                        if (!SameSide(center, randomPoint, ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3]))
                                        {
                                            RemovePointNearEnd();
                                            collider2Ds.RemoveAt(collider2Ds.Count - 1);//remove phan tu cuoi cung
                                        }

                                    }
                                    else//hit.collider == hit31.collider
                                    {
                                        Vector2 center = centerTriangle(ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3], (Vector2)hand.transform.position);
                                        List<Vector2> newList = GetPointOfAPolygon(collider12.GetComponent<PolygonCollider2D>());
                                        Vector2 randomPoint = RandomPointList2(newList, (Vector2)nearPoint12, (Vector2)nearPoint13);
                                        if (!SameSide(center, randomPoint, ropePositions[ropePositions.Count - 2], ropePositions[ropePositions.Count - 3]))
                                        {
                                            RemovePointNearEnd();
                                            collider2Ds.RemoveAt(collider2Ds.Count - 1);
                                        }
                                    }

                                }

                            }
                            else//hit31.collider ==null
                            {
                                if (ropePositions.Count == 3)
                                {
                                    Vector2 center = centerTriangle(ropePositions[0], ropePositions[1], (Vector2)hand.transform.position);
                                    List<Vector2> newList = GetPointOfAPolygon(collider12.GetComponent<PolygonCollider2D>());
                                    Vector2 newRandomPoint = RandomPointList(newList, (Vector2)nearPoint12);
                                    if (SameSide(newRandomPoint, center, ropePositions[0], ropePositions[1]) == false)
                                    {
                                        RemovePointNearEnd();
                                        collider2Ds.RemoveAt(collider2Ds.Count - 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            directionEachFrame = Vector2.zero;
            ropeAttached = false;
            HandController handController = hand.GetComponent<HandController>();
            if (handController.CanConnectWithItem)
            {
                handController.SetParent();
                canPullBack = true;
                Obstacle.Ins.SetIsTriggerBarrier(true);
            }
        }
    }
    #endregion
}