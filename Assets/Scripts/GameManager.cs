using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region Singleton
	public static GameManager instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		WinMenu.SetActive(false);
		LoseMenu.SetActive(false);
		levelsUI.SetActive(false);
		sequenceBubbles = new List<Transform>();
		connectedBubbles = new List<Transform>();
		bubblesToDestroy = new List<Transform>();
		DontDestroyOnLoad(gameObject);
	}
	#endregion

	private const int SEQUENCE_SIZE = 3;

	private List<Transform> sequenceBubbles;
	private List<Transform> connectedBubbles;
	private List<Transform> bubblesToDestroy;
	public float RayDistance = 200f;
	public Shooter shootScript;
	public GameObject explosionPrefab;
	public GameObject WinMenu;
	public GameObject LoseMenu;
	public GameObject winScore;
	public GameObject winThrows;
	public GameObject volBtn;
	public GameObject playBtn;
	public GameObject homeVolBtn;
	public GameObject startUI;
	public GameObject levelsUI;
	public GameObject LightObj;
	public Transform bottomLimit;
	public float gravityScale = 1f;
	public string gameState = "play";
	private bool hitABomb = false;

	public void ToggleGameState()
	{
		if (gameState == "play")
		{
			playBtn.GetComponent<Image>().color = Color.gray;
			gameState = "pause";
			LightObj.GetComponent<UnityEngine.Rendering.Universal.Light2D>().intensity = 0.4f;
			PauseGame();
		}
		else if (gameState == "pause")
		{
			playBtn.GetComponent<Image>().color = Color.white;
			gameState = "play";
			LightObj.GetComponent<UnityEngine.Rendering.Universal.Light2D>().intensity = 1;
			ResumeGame();
		}
	}

	public void RestartGame()
	{
		LevelManager.instance.ClearLevel();
		shootScript.canShoot = false;
		startUI.SetActive(true);
		gameState = "play";
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
	}

	public void ToggleMute()
	{
		AudioManager.instance.ToggleMute();
		if (AudioManager.instance.mute)
		{
			volBtn.GetComponent<Image>().color = Color.gray;
			homeVolBtn.GetComponent<Image>().color = Color.gray;
		}
		else
		{
			volBtn.GetComponent<Image>().color = Color.white;
			homeVolBtn.GetComponent<Image>().color = Color.white;
		}
	}

	IEnumerator CheckSequence(Transform currentBubble)
	{
		yield return new WaitForSeconds(0.1f);

		sequenceBubbles.Clear();
		CheckBubbleSequence(currentBubble);
		ProcessSpecialBubbles(currentBubble);

		if ((sequenceBubbles.Count >= SEQUENCE_SIZE) || hitABomb)
		{
			ProcessBubblesInSequence();
			ProcessDisconectedBubbles();
		}

		sequenceBubbles.Clear();
		LevelManager.instance.UpdateListOfBubblesInScene();
		hitABomb = false;

		if (LevelManager.instance.bubblesInScene.Count == 0)
		{
			ScoreManager man = ScoreManager.GetInstance();
			winScore.GetComponent<Text>().text = man.GetScore().ToString();
			winThrows.GetComponent<Text>().text = man.GetThrows().ToString();
			WinMenu.SetActive(true);
		}
		else
		{
			shootScript.CreateNextBubble();
			shootScript.canShoot = true;
		}

		ProcessBottomLimit();
	}

	public void ProcessTurn(Transform currentBubble)
	{
		StartCoroutine(CheckSequence(currentBubble));
	}

	private void ProcessBottomLimit()
	{
		foreach (Transform t in LevelManager.instance.bubblesArea)
		{
			if (t.GetComponent<Bubble>().isConnected
			&& t.position.y < bottomLimit.position.y)
			{
				LevelManager.instance.ClearLevel();
				LoseMenu.SetActive(true);
				shootScript.canShoot = false;
				break;
			}
		}
	}

	private void ProcessSpecialBubbles(Transform currentBubble)
	{
		Bubble bubbleScript = currentBubble.GetComponent<Bubble>();
		List<Transform> neighbors = bubbleScript.GetNeighbors();

		foreach (Transform t in neighbors)
		{
			Bubble bScript = t.GetComponent<Bubble>();

			if (bScript.bubbleColor == Bubble.BubbleColor.Bomb)
			{
				hitABomb = true;
				GameObject explosion = Instantiate(explosionPrefab, t.position, Quaternion.identity);
				explosion.transform.localScale = new Vector3(25f, 25f, 1f);
				Destroy(explosion, 0.5f);

				Destroy(t.gameObject);

				foreach (Transform t2 in bScript.GetNeighbors())
				{
					if (sequenceBubbles.Contains(t2)) sequenceBubbles.Remove(t2);
					Destroy(t2.gameObject);
				}
			}

		}
	}

	private void CheckBubbleSequence(Transform currentBubble)
	{
		sequenceBubbles.Add(currentBubble);

		Bubble bubbleScript = currentBubble.GetComponent<Bubble>();
		List<Transform> neighbors = bubbleScript.GetNeighbors();

		foreach (Transform t in neighbors)
		{
			if (!sequenceBubbles.Contains(t))
			{
				Bubble bScript = t.GetComponent<Bubble>();

				if (bScript.bubbleColor == bubbleScript.bubbleColor)
				{
					CheckBubbleSequence(t);
				}
			}
		}
	}

	private void ProcessBubblesInSequence()
	{
		if (hitABomb)
			AudioManager.instance.PlaySound("explosion");
		else
			AudioManager.instance.PlaySound("destroy");

		foreach (Transform t in sequenceBubbles)
		{
			if (!bubblesToDestroy.Contains(t))
			{
				ScoreManager.GetInstance().AddScore(1);
				t.tag = "Untagged";
				bubblesToDestroy.Add(t);
			}
		}
	}

	#region Drop Disconected Bubbles

	private void ProcessDisconectedBubbles()
	{
		SetAllBubblesConnectionToFalse();
		SetConnectedBubblesToTrue();
		CheckDisconectedBubbles();
		DropAll();
	}

	private void SetAllBubblesConnectionToFalse()
	{
		foreach (Transform bubble in LevelManager.instance.bubblesArea)
		{
			bubble.GetComponent<Bubble>().isConnected = false;
		}
	}

	private void SetConnectedBubblesToTrue()
	{
		connectedBubbles.Clear();

		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right, RayDistance);

		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].transform.gameObject.tag.Equals("Bubble"))
				SetNeighboursConnectionToTrue(hits[i].transform);
		}
	}

	private void SetNeighboursConnectionToTrue(Transform bubble)
	{
		connectedBubbles.Add(bubble);

		Bubble bubbleScript = bubble.GetComponent<Bubble>();
		bubbleScript.isConnected = true;

		foreach (Transform t in bubbleScript.GetNeighbors())
		{
			if (!connectedBubbles.Contains(t))
			{
				SetNeighboursConnectionToTrue(t);
			}
		}
	}

	private void CheckDisconectedBubbles()
	{
		foreach (Transform bubble in LevelManager.instance.bubblesArea)
		{
			Bubble bubbleScript = bubble.GetComponent<Bubble>();
			if (!bubbleScript.isConnected)
			{
				if (!bubblesToDestroy.Contains(bubble))
				{
					ScoreManager.GetInstance().AddScore(2);
					bubble.tag = "Untagged";
					bubblesToDestroy.Add(bubble);
				}
			}
		}
	}

	private void DropAll()
	{
		foreach (Transform bubble in bubblesToDestroy)
		{
			bubble.SetParent(null);
			//Destroy(bubble.gameObject);
			bubble.gameObject.GetComponent<CircleCollider2D>().enabled = false;
			if (!bubble.GetComponent<Rigidbody2D>())
			{
				Rigidbody2D rig = (Rigidbody2D)bubble.gameObject.AddComponent(typeof(Rigidbody2D));
				rig.gravityScale = gravityScale;
			}
		}
		bubblesToDestroy.Clear();
	}

	#endregion

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, Vector2.right * RayDistance);
	}
}