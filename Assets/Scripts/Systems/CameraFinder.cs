using Unity.Cinemachine;
using UnityEngine;

namespace Scripts.Systems
{
    public class CameraFinder : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        public static Camera MainCamera {get; private set;}
        [SerializeField] private CinemachineCamera virtualCamera;
        public static CinemachineCamera VirtualCamera {get; private set;}
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            MainCamera = mainCamera;
            VirtualCamera = virtualCamera;
        }
    }
}
