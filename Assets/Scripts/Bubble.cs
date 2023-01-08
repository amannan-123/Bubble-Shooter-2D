using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bubble : MonoBehaviour
{
	public float raycastRange = 0.15f;
	public float raycastOffset = 0.6f;

	public bool isFixed;
	public bool isConnected;

	public BubbleColor bubbleColor;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if ((collision.gameObject.tag == "Bubble" && collision.gameObject.GetComponent<Bubble>().isFixed) || collision.gameObject.tag == "Limit")
		{
			if (!isFixed)
				HasCollided();
		}
	}

	private void HasCollided()
	{
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		Destroy(rb);
		isFixed = true;
		LevelManager.instance.SetAsBubbleAreaChild(transform);
		GameManager.instance.ProcessTurn(transform);
	}

	public List<Transform> GetNeighbours()
	{
		List<RaycastHit2D> hits = new List<RaycastHit2D>();
		List<Transform> neighbours = new List<Transform>();

		hits.Add(Physics2D.Raycast(new Vector2(transform.position.x - raycastOffset, transform.position.y), Vector3.left, raycastRange));
		hits.Add(Physics2D.Raycast(new Vector2(transform.position.x + raycastOffset, transform.position.y), Vector3.right, raycastRange));
		hits.Add(Physics2D.Raycast(new Vector2(transform.position.x - raycastOffset, transform.position.y + raycastOffset), new Vector2(-1f, 1f), raycastRange));
		hits.Add(Physics2D.Raycast(new Vector2(transform.position.x - raycastOffset, transform.position.y - raycastOffset), new Vector2(-1f, -1f), raycastRange));
		hits.Add(Physics2D.Raycast(new Vector2(transform.position.x + raycastOffset, transform.position.y + raycastOffset), new Vector2(1f, 1f), raycastRange));
		hits.Add(Physics2D.Raycast(new Vector2(transform.position.x + raycastOffset, transform.position.y - raycastOffset), new Vector2(1f, -1f), raycastRange));

		foreach (RaycastHit2D hit in hits)
		{
			if (hit.collider != null && hit.transform.tag.Equals("Bubble"))
			{
				neighbours.Add(hit.transform);
			}
		}

		return neighbours;
	}

	void OnBecameInvisible()
	{
		Destroy(gameObject);
	}

	public enum BubbleColor
	{
		Blue, Yellow, Red, Purple, Bomb
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		foreach (Transform tr in GetNeighbours())
		{
			Gizmos.DrawLine(transform.position, tr.position);
		}
	}
}
