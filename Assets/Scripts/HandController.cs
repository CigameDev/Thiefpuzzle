using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private bool canConnectWithItem = false;
    public bool CanConnectWithItem => canConnectWithItem;
    public GameObject stolengoods;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag =="Stolen goods")
        {
            Debug.Log("va cham voi do an trom");
            canConnectWithItem = true;
            stolengoods = collision.gameObject;
            //collision.transform.parent = gameObject.transform;
        }    
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Thoat ra roi");
        canConnectWithItem = false;
        stolengoods = null;
    }
    public void SetParent()
    {
        if (!stolengoods) return;
        stolengoods.transform.parent = gameObject.transform;
    }    
}
