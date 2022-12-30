using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
	public bool canShoot;
	public float speed = 25f;

	public Transform nextBubblePosition;
	public GameObject currentBubble;
	public GameObject nextBubble;

	private Vector2 lookDirection;
	private float lookAngle;
	private GameObject line;
	private GameObject limit;

	public void Awake()
	{
		line = GameObject.FindGameObjectWithTag("Line");
		limit = GameObject.FindGameObjectWithTag("Limit");
	}

	public void Update()
	{
		if (GameManager.instance.gameState == "play")
		{
			lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

			if (Input.GetMouseButton(0)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > transform.position.y)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
			{
				line.transform.position = transform.position;
				line.transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90);

				if (LevelManager.instance != null
				&& LevelManager.instance.GetBubbleAreaChildCount() > 0)
				{
					line.SetActive(true);
				}
			}
			else
			{
				line.SetActive(false);
			}

			if (canShoot
				&& Input.GetMouseButtonUp(0)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > transform.position.y)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
			{
				canShoot = false;
				Shoot();
			}
		}
	}

	public void Shoot()
	{
		if (currentBubble == null) CreateNextBubble();
		ScoreManager.GetInstance().AddThrows();
		AudioManager.instance.PlaySound("shoot");
		transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
		currentBubble.transform.rotation = transform.rotation;
		currentBubble.GetComponent<CircleCollider2D>().enabled = true;
		currentBubble.GetComponent<Rigidbody2D>().AddForce(currentBubble.transform.up * speed, ForceMode2D.Impulse);
		currentBubble.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		currentBubble = null;
	}

	public void SwapBubbles()
	{
		List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
		if (bubblesInScene.Count < 1) return;

		currentBubble.transform.position = nextBubblePosition.position;
		nextBubble.transform.position = transform.position;
		GameObject temp = currentBubble;
		currentBubble = nextBubble;
		nextBubble = temp;
	}

	public void CreateNewBubbles()
	{
		if (nextBubble != null)
			Destroy(nextBubble);

		if (currentBubble != null)
			Destroy(currentBubble);

		nextBubble = null;
		currentBubble = null;
		CreateNextBubble();
		canShoot = true;
	}

	public void CreateNextBubble()
	{
		List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
		List<string> colors = LevelManager.instance.colorsInScene;

		if (bubblesInScene.Count < 1) return;

		if (nextBubble == null)
		{
			nextBubble = InstantiateNewBubble(bubblesInScene);
		}
		else
		{
			// if (!colors.Contains(nextBubble.GetComponent<Bubble>().bubbleColor.ToString()))
			// {
			// 	Destroy(nextBubble);
			// 	nextBubble = InstantiateNewBubble(bubblesInScene);
			// }
		}

		if (currentBubble == null)
		{
			currentBubble = nextBubble;
			currentBubble.transform.position = transform.position;
			nextBubble = InstantiateNewBubble(bubblesInScene);
		}
	}

	private GameObject InstantiateNewBubble(List<GameObject> bubblesInScene)
	{
		if (bubblesInScene.Count > 0)
		{
			GameObject newBubble = Instantiate(bubblesInScene[Random.Range(0, bubblesInScene.Count)]);
			newBubble.transform.position = nextBubblePosition.position;
			newBubble.GetComponent<Bubble>().isFixed = false;
			newBubble.GetComponent<CircleCollider2D>().enabled = false;
			Rigidbody2D rb2d = newBubble.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
			rb2d.gravityScale = 0f;
			return newBubble;
		}
		else
		{
			return null;
		}

	}
}
