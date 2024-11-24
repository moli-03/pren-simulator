using System.Collections.Generic;
using System.Linq;
using Assets.Src.Vehicle.Graph;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class ChooseNextPath : VehicleState
	{
		public override string Name => "ChooseNextPath";

		private MapNode CurrentNode;

		public ChooseNextPath(VehicleController vehicle) : base(vehicle) {
			this.CurrentNode = this.Vehicle.NodeStack.Last();
		}


		private List<Vector2> GetNotVisitedPositions() {

			// Get all the directions we have already visited
			List<Vector2> visitedDirections = new List<Vector2>();
			foreach (MapPath path in this.CurrentNode.OutgoingPaths) {
				Vector2? position = path.Start == this.CurrentNode ? path.StartOutgoingPathPosition : path.EndOutgoingPathPosition;

				if (position.HasValue) {
					visitedDirections.Add(position.Value);
				}
			}

			// Get the positions we have not visited yet
			return this.CurrentNode.OutgoingPathScanPositions.Where(position => !visitedDirections.Contains(position)).ToList();
		}

		public override void Start()
		{

			List<Vector2> notVisitedPositions = this.GetNotVisitedPositions();

			// Handle start node
			if (this.Vehicle.NodeStack.Count == 1) {
				Debug.Log("Start");
				this.HandleStartNode();
			}

			// Handle first node of the graph
			else if (this.Vehicle.NodeStack.Count == 2) {
				Debug.Log("FirstNode");
				this.HandleFirstNodeOfGraph(notVisitedPositions);
			}

			// Check if we have visited this node -> turn around
			else if (this.Vehicle.NodeStack.IndexOf(this.CurrentNode) != this.Vehicle.NodeStack.Count - 1) {
				Debug.Log("Already visited. Total: " + this.Vehicle.NodeStack.Count + ", Index: " + this.Vehicle.NodeStack.IndexOf(this.CurrentNode));
				this.GoBack();
			}

			// Check if we have visited all paths of this node
			else if (notVisitedPositions.Count == 0) {
				Debug.Log("NoOtherOptions");
				this.GoBack();
			}

			// Randomly choose the next path
			else {
				Debug.Log("Random from " + notVisitedPositions.Count + " possible positions");
				this.ChoseRandomPosition(notVisitedPositions);
			}

		}

		private void ChoosePosition(Vector2 linePosition) {
			// Turn to that line
			this.Vehicle.Drive.RotateFacing(linePosition - this.Vehicle.Position, () => {
				
				// Start to following that line
				this.Vehicle.SetState(new FollowLine(this.Vehicle));
			});
		}


		private void ChoseRandomPosition(List<Vector2> linePositions) {

			// Randomly select one of the not visited nodes
			int index = (new System.Random()).Next(0, linePositions.Count - 1);
			
			this.ChoosePosition(linePositions[index]);
		}


		private void HandleStartNode() {

			// There is only one way to go -> choose it
			this.ChoosePosition(this.CurrentNode.OutgoingPathScanPositions[0]);
		}


		private void HandleFirstNodeOfGraph(List<Vector2> notVisitedPositions) {

			// Check if we have explored everything
			if (notVisitedPositions.Count == 0) {
				this.Vehicle.SetState(new ExploredEverything(this.Vehicle));
				return;
			}

			// Randomly choose the next one
			this.ChoseRandomPosition(notVisitedPositions);
		}


		private void GoBack() {

			// Remove the current node from the stack
			this.Vehicle.NodeStack.RemoveAt(this.Vehicle.NodeStack.Count - 1);

			MapNode previous = this.Vehicle.NodeStack.Last();

			Vector2 chosenPosition = Vector2.zero;

			// Get the path to that node
			foreach (MapPath path in this.CurrentNode.OutgoingPaths) {

				if (path.Start == previous) {
					chosenPosition = path.EndOutgoingPathPosition.Value;
					break;
				}
				else if (path.End == previous) {
					chosenPosition = path.StartOutgoingPathPosition.Value;
					break;
				}
			}

			this.ChoosePosition(chosenPosition);
		}

	}

}