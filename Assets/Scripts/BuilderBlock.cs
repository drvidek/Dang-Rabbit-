using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderBlock : MonoBehaviour
{
    [SerializeField] private GameObject _stump;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit))
            {
                if (hit.collider.TryGetComponent<Node>(out Node n))
                {
                    PlaceObject(_stump,n.transform.position);
                    n.SetDisabled(true);
                }
            }
        }
            
    }

    void PlaceObject(GameObject prefab,Vector3 pos)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, null);
    }
}
