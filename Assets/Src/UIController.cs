using System;
using Assets.Src.Vehicle.States;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
	public static UIController Instance;
	
    public TMP_Text DestinationTMP;

	public TMP_Text StateTMP;

	public TMP_Text TimerTMP;

	private DateTime TimerStart;

	void Awake() {
		Instance = this;
	}

	public void UpdateTarget(Node node) {
		this.DestinationTMP.text = "Target: " + node.GetName();
	}

	public void UpdateState(VehicleState state) {
		this.StateTMP.text = "State: " + state.Name;
	}

	public void StartTimer() {

		this.TimerStart = DateTime.Now;

		InvokeRepeating(nameof(UpdateTimerUI), 1.0f, 1.0f);
	}

	private void UpdateTimerUI() {
		TimeSpan passedTime = DateTime.Now - this.TimerStart;
		this.TimerTMP.text = "Timer: " + passedTime.Minutes + "m " + passedTime.Seconds + "s";
	}

	void OnApplicationQuit() {
		CancelInvoke(nameof(UpdateTimerUI));
	}
}
