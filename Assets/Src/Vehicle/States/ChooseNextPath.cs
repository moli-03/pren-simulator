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


		private List<MapPath> GetNotVisitedPaths() {
			return this.CurrentNode.OutgoingPaths.Where(path => !path.IsVisited).ToList();
		}

		public override void Start()
		{

			List<MapPath> notVisitedPaths = this.GetNotVisitedPaths();

			// Handle start node
			if (this.Vehicle.NodeStack.Count == 1) {
				Debug.Log("Start");
				this.HandleStartNode();
			}

			// Handle first node of the graph
			else if (this.Vehicle.NodeStack.Count == 2) {
				Debug.Log("FirstNode");
				this.HandleFirstNodeOfGraph(notVisitedPaths);
			}

			// Check if we have visited this node -> turn around
			else if (this.Vehicle.NodeStack.IndexOf(this.CurrentNode) != this.Vehicle.NodeStack.Count - 1) {
				Debug.Log("Already visited. Total: " + this.Vehicle.NodeStack.Count + ", Index: " + this.Vehicle.NodeStack.IndexOf(this.CurrentNode));
				this.GoBack();
			}

			// Check if we have visited all paths of this node
			else if (notVisitedPaths.Count == 0) {
				Debug.Log("NoOtherOptions");
				this.GoBack();
			}

			// Randomly choose the next path
			else {
				Debug.Log("Random from " + notVisitedPaths.Count + " possible positions");
				this.ChoseRandomPath(notVisitedPaths);
			}

		}

		private void ChoosePosition(Vector2 linePosition) {
			// Turn to that line
			this.Vehicle.Drive.RotateFacing(linePosition - this.Vehicle.Position, () => {
				
				// Start to following that line
				this.Vehicle.SetState(new FollowLine(this.Vehicle));
			});
		}


		private void ChoseRandomPath(List<MapPath> paths) {

			// Randomly select one of the not visited nodes
			int index = Random.Range(0, paths.Count);
			
			this.ChoosePosition(paths[index].GetOutgoingPositionFor(this.CurrentNode).Value);
		}


		private void HandleStartNode() {

			// There is only one way to go -> choose it
			this.ChoosePosition(this.CurrentNode.OutgoingPaths[0].GetOutgoingPositionFor(this.CurrentNode).Value);
		}


		private void HandleFirstNodeOfGraph(List<MapPath> notVisitedPaths) {

			// Check if we have explored everything
			if (notVisitedPaths.Count == 0) {
				this.Vehicle.SetState(new ExploredEverything(this.Vehicle));
				return;
			}

			// Randomly choose the next one
			this.ChoseRandomPath(notVisitedPaths);
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