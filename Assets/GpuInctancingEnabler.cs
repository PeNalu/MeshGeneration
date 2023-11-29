using UnityEngine;

namespace RenownedGames.AuroraEngine.Experimental
{
    [DisallowMultipleComponent]
    public class GpuInctancingEnabler : MonoBehaviour
    {
        /// <summary>
        /// Called when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}
