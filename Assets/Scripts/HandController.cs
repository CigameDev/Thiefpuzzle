using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private bool canConnectWithItem = false;
    public bool CanConnectWithItem => canConnectWithItem;
    public GameObject stolengoods;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == StringDefine.GAMETAG_STOLENGOODS)
        {

            canConnectWithItem = true;
            stolengoods = col.gameObject;
            //col.transform.parent = gameObject.transform;
        }    
    }
    private void OnTriggerExit2D(Collider2D col)
    {

        canConnectWithItem = false;
        stolengoods = null;
    }
    public void SetParent()
    {
        if (!stolengoods) return;
        stolengoods.transform.parent = gameObject.transform;
    }    
}
