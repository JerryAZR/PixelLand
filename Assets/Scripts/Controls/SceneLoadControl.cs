using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using Scripts.Systems;

namespace Scripts.Controls
{
     public class SceneLoadControl : NetworkBehaviour
     {
          private NetworkObject _playerObject; 
          public override void OnStartClient() {
               base.OnStartClient();
               
               _playerObject = GetComponentInParent<NetworkTransform>().NetworkObject;
               
               if (!IsOwner) return;
               
               LoadScene(new SceneIdentifier("MainWorld"), new[]{_playerObject});
          }

          public void MoveToScene(SceneIdentifier scene) {
               LoadScene(scene, new[]{_playerObject});
          }

          [ServerRpc]
          private void LoadScene(SceneIdentifier scene, NetworkObject[] moves, NetworkConnection conn = null) {
               _ = SceneLoader.Instance.ServerLoadScene(scene, moves, conn!);
          }
     }
}
