using UnityEngine;
using System.Collections;

public class RadarBlipManager : MonoBehaviour {

    private float appearsTime = 0.1f;
    private float disappearsTime = 1.0f;

    public void Trigger()
    {
        StartCoroutine("Appears");
    }

    IEnumerator Appears()
    {
        yield return new WaitForSeconds(appearsTime);
        GetComponent<MeshRenderer>().enabled = true;
        StartCoroutine("Disappears");
    }

    IEnumerator Disappears()
    {
        yield return new WaitForSeconds(disappearsTime);
        GetComponent<MeshRenderer>().enabled = false;
    }
}
