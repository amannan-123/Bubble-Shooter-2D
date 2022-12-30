using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapBubble : MonoBehaviour
{

	public Shooter shootScript;
	public void OnMouseDown()
	{
		if (shootScript)
		{
			shootScript.SwapBubbles();
		}
	}

}
