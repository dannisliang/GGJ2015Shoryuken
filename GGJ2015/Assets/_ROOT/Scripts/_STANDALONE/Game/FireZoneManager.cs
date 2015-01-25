using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class FireZoneManager : MonoBehaviour {

    public GameObject m_LockableDoor;
    public List<GameObject> FireZones;
    public List<GameObject> Sprinklers;
    public GameObject LightGO;

	// Use this for initialization
	void LevierTriggered () {
        TriggerFires(true);
	}

    public void TriggerFires(bool isOn)
    {
        if (isOn)
        {
            m_LockableDoor.GetComponent<Door>().Locked = true;
        }
        else
        {
            m_LockableDoor.GetComponent<Door>().Locked = false;
        }

        List<AlarmLight> lights = LightGO.GetComponentsInChildren<AlarmLight>().ToList<AlarmLight>();
        foreach (AlarmLight light in lights)
        {
            if (isOn)
                light.Play();
            else
                light.Stop();
        }


        foreach (GameObject go in FireZones)
        {
            Debug.Log("GO");
            List<ParticleSystem> pss = go.GetComponentsInChildren<ParticleSystem>().ToList<ParticleSystem>();
            foreach (ParticleSystem ps in pss)
            {
                if (isOn)
                    ps.Play();
                else
                    ps.Stop();
            }

            //List<ParticleRenderer> lst = prs.OfType<ParticleRenderer>().ToList();
        }

        foreach (GameObject go in Sprinklers)
        {
            Debug.Log("GO");
            List<ParticleSystem> pss = go.GetComponentsInChildren<ParticleSystem>().ToList<ParticleSystem>();
            foreach (ParticleSystem ps in pss)
            {
                if (!isOn)
                    ps.Play();
            }

            //List<ParticleRenderer> lst = prs.OfType<ParticleRenderer>().ToList();
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
