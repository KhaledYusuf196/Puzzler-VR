﻿using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour
{
	public GameObject player;
	public GameObject eventSystem;
	public GameObject startUI, restartUI;
	public GameObject startPoint, playPoint, restartPoint;

	// An array to hold the orbs.
	public GameObject[] puzzleSpheres;

	// How many times the orbs light up during the pattern display.
	public int puzzleLength = 5;

	// How many seconds between the orbs light up during the pattern display.
	public float puzzleSpeed = 1f;

	// Variable for storing the order of the pattern display.
	private int[] puzzleOrder;

	// Variable for storing the index during the pattern display.
	private int currentDisplayIndex = 0;

	// Variable for storing the index the player is trying to solve.
	private int currentSolveIndex = 0;

	public float moveTime = 5;

	/* Uncomment the line below during 'A Little More Feedback!' lesson.*/
	public GameObject failAudioHolder;

	public GameObject successAudioHolder;

	void Start()
	{
		// Update 'player' to be the camera's parent gameobject, i.e. 'GvrEditorEmulator' instead of the camera itself.
		// Required because GVR resets camera position to 0, 0, 0.
		player = player.transform.parent.gameObject;

		// Move the 'player' to the 'startPoint' position.
		player.transform.position = startPoint.transform.position;

		// Set the size of our array to the declared puzzle length.
		puzzleOrder = new int[puzzleLength];

		// Create a random puzzle sequence.
		GeneratePuzzleSequence();
	}

	void Update(){
		if (player.transform.position == startPoint.transform.position) {
			startUI.SetActive (true);
			return;
		} else {
			startUI.SetActive (false);
		}
		if (player.transform.position == restartPoint.transform.position) {
			restartUI.SetActive (true);
			return;
		} else {
			restartUI.SetActive (false);
		}

	}

	// Create a random puzzle sequence.
	public void GeneratePuzzleSequence()
	{
		// Variable for storing a random number.
		int randomInt;

		// Loop as many times as the puzzle length.
		for (int i = 0; i < puzzleLength; i++)
		{
			// Generate a random number.
			randomInt = Random.Range(0, puzzleSpheres.Length);

			// Set the current index to the randomly generated number.
			puzzleOrder[i] = randomInt;
		}
	}

	// Begin the puzzle sequence.
	public void StartPuzzle()
	{

		// Move the player to the play position.
		iTween.MoveTo(player,
			iTween.Hash(
				"position", playPoint.transform.position,
				"time", moveTime,
				"easetype", "linear"
			)
		);

		// Call the DisplayPattern() function repeatedly.
		CancelInvoke("DisplayPattern");
		InvokeRepeating("DisplayPattern", moveTime + 1, puzzleSpeed);

		// Reset the index the player is trying to solving.
		currentSolveIndex = 0;
	}

	// Reset the puzzle sequence.
	public void ResetPuzzle()
	{
		if(player.transform.position == restartPoint.transform.position){

			// Move the player to the start position.
			player.transform.position = startPoint.transform.position;

			// Create a random puzzle sequence.
			GeneratePuzzleSequence();
		}
	}

	// Disaplay the
	// Called from StartPuzzle() and invoked repeatingly.
	void DisplayPattern()
	{
		// If we haven't reached the end of the display pattern.
		if (currentDisplayIndex < puzzleOrder.Length)
		{
			Debug.Log("Display index " + currentDisplayIndex + ": Orb index " + puzzleOrder[currentDisplayIndex]);

			// Disable gaze input while displaying the pattern (prevents player from interacting with the orbs).
			eventSystem.SetActive(false);

			// Light up the orb at the current index.
			puzzleSpheres[puzzleOrder[currentDisplayIndex]].GetComponent<LightUp>().PatternLightUp(puzzleSpeed);

			// Move one to the next orb.
			currentDisplayIndex++;
		}
		// If we have reached the end of the display pattern.
		else
		{
			Debug.Log("End of puzzle display");

			// Renable gaze input when finished displaying the pattern (allows player to interacte with the orbs).
			eventSystem.SetActive(true);

			// Reset the index tracking the orb being lit up.
			currentDisplayIndex = 0;

			// Stop the pattern display.
			CancelInvoke();
		}
	}

	// Identify the index of the sphere the player selected.
	// Called from LightUp.PlayerSelection() method (see LightUp.cs script).
	public void PlayerSelection(GameObject sphere)
	{
		// Variable for storing the selected index.
		int selectedIndex = 0;

		// Loop throught the array to find the index of the selected sphere.
		for (int i = 0; i < puzzleSpheres.Length; i++)
		{
			// If the passed in sphere is the sphere at the index being checked.
			if (puzzleSpheres[i] == sphere)
			{
				Debug.Log("Looks like we hit sphere: " + i);

				// Update the index of the passed in sphere to be the same as the index being checked.
				selectedIndex = i;
			}
		}

		// Check if the sphere the player selected is correct.
		SolutionCheck(selectedIndex);
	}

	// Check if the sphere the player selected is correct.
	public void SolutionCheck(int playerSelectionIndex)
	{
		// If the sphere the player selected is the correct sphere.
		if (playerSelectionIndex == puzzleOrder[currentSolveIndex])
		{
			Debug.Log("Correct!  You've solved " + currentSolveIndex + " out of " + puzzleLength);

			// Update the tracker to check the next sphere.
			currentSolveIndex++;

			// If this was the last sphere in the pattern display...
			if (currentSolveIndex >= puzzleLength)
			{
				PuzzleSuccess();
			}
		}
		// If the sphere the player selected is the incorrect sphere.
		else
		{
			PuzzleFailure();
		}
	}

	// Do this when the player solves the puzzle.
	public void PuzzleSuccess()
	{
		
		// Move the player to the restart position.
		iTween.MoveTo(player,
			iTween.Hash(
				"position", restartPoint.transform.position,
				"time", moveTime,
				"easetype", "linear"
			)
		);

		//play a success sound.
		successAudioHolder.GetComponent<GvrAudioSource>().Play();
			
	}

	// Do this when the player selects the wrong sphere.
	public void PuzzleFailure()
	{
		Debug.Log("You failed, resetting puzzle");

		// Get the GVR audio source component on the failAudioHolder and play the audio.
		/* Uncomment the line below during 'A Little More Feedback!' lesson.*/
		failAudioHolder.GetComponent<GvrAudioSource>().Play();

		// Reset the index the player is trying to solving.
		currentSolveIndex = 0;

		// Begin the puzzle sequence.
		StartPuzzle();
	}
}