using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

	#region Singleton
	public static LevelManager instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		DontDestroyOnLoad(gameObject);
	}
	#endregion

	public Grid grid;
	public Transform bubblesArea;
	public List<GameObject> bubblesPrefabs;
	public GameObject specialBubblePrefab;
	public List<GameObject> bubblesInScene;
	public List<GameObject> levels;
	public List<string> colorsInScene;
	public int currentLevel = 0;
	public GameObject levelText;

	private void Start()
	{
		grid = GetComponent<Grid>();
	}

	public void NextLevel()
	{
		GameManager.instance.WinMenu.SetActive(false);
		StartLevel(currentLevel + 1);
	}

	public void RestartLevel()
	{
		GameManager.instance.LoseMenu.SetActive(false);
		StartLevel(currentLevel);
	}

	public void StartNewGame()
	{
		GameManager.instance.startUI.SetActive(false);
		GameManager.instance.levelsUI.SetActive(true);
	}

	System.Collections.IEnumerator LoadLevel(int level)
	{
		yield return new WaitForSeconds(0.1f);

		ScoreManager.GetInstance().Reset();
		GameObject levelToLoad = Instantiate(levels[level]);
		FillWithBubbles(levelToLoad, bubblesPrefabs);

		SnapChildrensToGrid(bubblesArea);
		InsertSpecialBubbles();
		UpdateListOfBubblesInScene();

		GameManager.instance.shootScript.CreateNewBubbles();
	}

	public void StartLevel(int level)
	{
		GameManager.instance.levelsUI.SetActive(false);
		if (level >= levels.Count)
			level = 0;

		currentLevel = level;
		levelText.GetComponent<UnityEngine.UI.Text>().text = "Level " + (level + 1);
		StartCoroutine(LoadLevel(level));
	}

	public void InsertSpecialBubbles()
	{
		int specialCount = Random.Range(1, 6);
		List<Transform> specials = new List<Transform>();
		for (int i = 0; i < specialCount; i++)
		{
			int randomBubble = Random.Range(0, bubblesArea.childCount);
			Transform bubble = bubblesArea.GetChild(randomBubble);

			if (!specials.Contains(bubble))
			{
				specials.Add(bubble);
				Instantiate(specialBubblePrefab, bubble.position, Quaternion.identity, bubblesArea);
				Destroy(bubble.gameObject);
			}
		}
	}

	public void ClearLevel()
	{
		foreach (Transform t in bubblesArea)
			Destroy(t.gameObject);
	}

	public int GetBubbleAreaChildCount()
	{
		return bubblesArea.childCount;
	}

	#region Snap to Grid
	private void SnapChildrensToGrid(Transform parent)
	{
		foreach (Transform t in parent)
		{
			SnapToNearestGripPosition(t);
		}
	}

	public void SnapToNearestGripPosition(Transform t)
	{
		Vector3Int cellPosition = grid.WorldToCell(t.position);
		t.position = grid.GetCellCenterWorld(cellPosition);
		t.rotation = Quaternion.identity;

	}
	#endregion

	private void FillWithBubbles(GameObject go, List<GameObject> _prefabs)
	{
		foreach (Transform t in go.transform)
		{
			var bubble = Instantiate(_prefabs[Random.Range(0, _prefabs.Count)], bubblesArea);
			bubble.transform.position = t.position;
		}

		Destroy(go);
	}

	public void UpdateListOfBubblesInScene()
	{
		List<string> colors = new List<string>();
		List<GameObject> newListOfBubbles = new List<GameObject>();

		foreach (Transform t in bubblesArea)
		{
			Bubble bubbleScript = t.GetComponent<Bubble>();
			if (colors.Count < bubblesPrefabs.Count && !colors.Contains(bubbleScript.bubbleColor.ToString()))
			{
				string color = bubbleScript.bubbleColor.ToString();

				foreach (GameObject prefab in bubblesPrefabs)
				{
					if (color.Equals(prefab.GetComponent<Bubble>().bubbleColor.ToString()))
					{
						colors.Add(color);
						newListOfBubbles.Add(prefab);
					}
				}
			}
		}

		colorsInScene = colors;
		bubblesInScene = newListOfBubbles;
	}

	public void SetAsBubbleAreaChild(Transform bubble)
	{
		SnapToNearestGripPosition(bubble);
		bubble.SetParent(bubblesArea);
	}
}
