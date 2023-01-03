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
    private bool ropeAttached =false;//trang thai co the them diem hay khong
    private Vector2[] pointPolygons;
    private List<Vector2> ropePositions = new List<Vector2>();//luu vi tri cac diem cua rope
    private Vector2 oldPosHand;
    private bool canPullBack = false;
    private PolygonCollider2D[] barriers;//mang cac chuong ngai vat
    private Camera mainCamera;

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
        Debug.Log("Do dai "+Obstacle.Ins.ListPointPolygon.Count);
        
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
    }
    void FixedUpdate()
    {
        if ((Vector2)directionEachFrame != Vector2.zero)
        {
            rb.velocity = directionEachFrame * moveSpeed; //* Time.fixedDeltaTime;//truyen 1 van toc theo huong di chuyen cua mouse
            //rb.AddForce(directionEachFrame *moveSpeed * Time.fixedDeltaTime,ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
    private void UpdateLastPoint()//update vi tri diem cuoi cung theo chuot
    {
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, hand.transform.position);
    }
   
    protected virtual Vector2 DirectionRaycast()//huong tu hand toi diem gan cuoi cung ket noi cua Line
    {
        int length = ropePositions.Count;
        Debug.DrawLine(ropePositions[length - 2], (Vector2)hand.transform.position);
        return ropePositions[length -2] - (Vector2)hand.transform.position;
    }
    
    protected virtual Vector2 DirectionRayCast31()//huong vecto tu hand toi diem truoc truoc no(count-1 -> count -3 )
    {
        int length = ropePositions.Count;
        Debug.DrawLine(ropePositions[length - 3], (Vector2)hand.transform.position);
        return ropePositions[length - 3] - (Vector2)hand.transform.position;
    }    
    private void GetPointPolygon()
    {
        //pointPolygons = polygon.points;
        //for(int i=0;i<pointPolygons.Length;i++)
        //{
        //    pointPolygons[i] += (Vector2)barrier.transform.position;
        //}    

        pointPolygons = Obstacle.Ins.ListPointPolygon.ToArray();
    }
    
   protected virtual Vector2 GetPointNeareast(Vector2 point)
    {
        Vector2 result = pointPolygons[0];
        float minDistance = Vector2.Distance(result, point);
        for(int i=1;i<pointPolygons.Length;i++)
        {
            if (minDistance > Vector2.Distance(pointPolygons[i], point))
            {
                result = pointPolygons[i];
                minDistance = Vector2.Distance(result, point);
            }    
        }    
        return result;
    }    
    
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
                oldMousePos = curPosMouse;
                UpdateLastPoint();
                if ((Vector2)directionEachFrame == Vector2.zero)
                {
                    directionEachFrame = Vector2.zero;
                }
                if (ropeAttached == false)
                {
                    var dirRaycast = DirectionRaycast();
                    var hit = Physics2D.Raycast(hand.transform.position, dirRaycast, 10, 1 << LayerMask.NameToLayer("Barrier"));
                    if (hit.collider != null)
                    {
                        if (ropePositions[ropePositions.Count - 2] != hit.point )
                        {
                           //Vector2 temp = pointPolygons[ropePositions.Count - 2] - (Vector2)hit.point;
                        if (isBetween(hit.point, hand.transform.position, ropePositions[ropePositions.Count-2]))
                        {
                            ropeAttached = true;
                            Vector2 pointAdd = GetPointNeareast(hit.point);
                            ropePositions.Insert(ropePositions.Count - 1, pointAdd);
                            /*
                                + vi co the khi di chuyen chuot khong ban trung frame dan den
                            vi tri ban co the khong trung 1 trong cac diem thuoc polygons ,nen tu vi tri hit.point
                            ta can tim diem ngan nhat tu hit.point toi 1 trong cac diem thuoc polygons de add diem do vao
                             */
                        }
                        }
                        else
                        {
                            //day la doan xu ly remove diem gan cuoi
                            //nhung phai co 1 dieu kien gi do o day nua
                            if (ropePositions.Count >= 3)
                            {
                                var dir31 = DirectionRayCast31();
                                var hit31 = Physics2D.Raycast(hand.transform.position, dir31, 10f, 1 << LayerMask.NameToLayer("Barrier"));
                                if (hit31.collider != null)
                                {
                                    if (hit31.point == ropePositions[ropePositions.Count - 3])
                                    {
                                        RemovePointNearEnd();
                                    }

                                }
                                else//hit31.collider ==null
                                {
                                    if (ropePositions.Count == 3)
                                    {
                                        Vector2 center = centerTriangle(ropePositions[0], ropePositions[1], (Vector2)hand.transform.position);
                                        Vector2 pointNear = Obstacle.Ins.GetPointNearInList(ropePositions[1]);
                                        Vector2 pointRandom = Vector2.zero;
                                        do
                                        {
                                            int number = Random.Range(0, pointPolygons.Length);
                                            pointRandom = pointPolygons[number];
                                        } while (pointRandom == ropePositions[1]);
                                        if (SameSide(pointNear, center, ropePositions[0], ropePositions[1]) == false)
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
            if(handController.CanConnectWithItem)
            {
                handController.SetParent();
                canPullBack = true;
                Obstacle.Ins.SetIsTriggerBarrier(true);
            }    
        }
    }
    protected virtual void UpdateRopePositions()
    {
        HandleInput();
        
        if (ropeAttached == false) return;
        
        ropeRenderer.positionCount = ropeRenderer.positionCount + 1;
        
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 2, ropePositions[ropePositions.Count - 2]);
        ropeRenderer.SetPosition(ropeRenderer.positionCount -1, hand.transform.position);
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
    protected virtual bool SameSide(Vector2 point1, Vector2 point2, Vector2 root1,Vector2 root2)
    {
        //kiem tra xem point1 va point2 co nam cung phia voi duong thang noi giua root1 va root2
        //ax +by +c =0
        //a = normal.x ,b = normal.y
        Vector2 direction = root1 - root2;//vecto chi phuong
        Vector2 normal = new Vector2(direction.y, -direction.x);//vecto phap tuyen
        float a = normal.x;
        float b = normal.y;
        float c = -(a*root1.x + b*root1.y);
        float dir1 = a * point1.x + b * point1.y + c;
        float dir2 = a* point2.x + b * point2.y + c;
        return (dir1 * dir2 > 0);
    }    
   
    protected virtual Vector2 centerTriangle(Vector2 A,Vector2 B,Vector2 C)
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
        else if(ropePositions.Count == 2)//di chuyen theo huong ve vi tri ban tay ban dau 
        {
            hand.transform.position = Vector2.Lerp(hand.transform.position, oldPosHand, Time.deltaTime *5f);
            UpdateLastPoint();
            if (Vector2.Distance((Vector2)hand.transform.position, oldPosHand) <= 0.05f)
            {
                hand.transform.position = oldPosHand;
                canPullBack = false;
                Obstacle.Ins.SetIsTriggerBarrier(false);
                return;
            }
        }    
    }
    protected virtual bool isBetween(Vector2 center,Vector2 out1,Vector2 out2)
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
   
    
}