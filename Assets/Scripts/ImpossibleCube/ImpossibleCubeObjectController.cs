using UnityEngine;

namespace ImpossibleCube
{
    public class ImpossibleCubeObjectController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 40f;
  
        protected void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
