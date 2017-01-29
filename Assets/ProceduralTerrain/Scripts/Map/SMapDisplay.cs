using UnityEngine;

public class SMapDisplay : MonoBehaviour
{
	[SerializeField] private Renderer m_textureRender;
    [SerializeField] private MeshFilter m_meshFilter;
    [SerializeField] private MeshRenderer m_meshRenderer;

	public void DrawTexture(Texture2D texture)
    {
		m_textureRender.sharedMaterial.mainTexture = texture;
		m_textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height);
	}

	public void DrawMesh(MeshData meshData, Texture2D texture)
    {
		m_meshFilter.sharedMesh = meshData.CreateMesh ();
		m_meshRenderer.sharedMaterial.mainTexture = texture;
	}
}
