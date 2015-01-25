using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadarBlipManager : MonoBehaviour {

    private float appearsTime = 0.1f;
    private float disappearsTime = 0.8f;

    public void Trigger()
    {
        StartCoroutine("Appears");
    }

    IEnumerator Appears()
    {
        yield return new WaitForSeconds(appearsTime);
        
        GetComponent<Image>().enabled = true;        

        StartCoroutine("Disappears");
    }

    IEnumerator Disappears()
    {
        yield return new WaitForSeconds(disappearsTime);
        GetComponent<Image>().enabled = false;        
        Kathulhu.PoolsManager.Instance.Deactivate( gameObject );
    }
}
