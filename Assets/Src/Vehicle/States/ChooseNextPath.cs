using System.Collections.Generic;
using System.Linq;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
	public class ChooseNextPath : VehicleState
	{
		public override string Name => "ChooseNextPath";

		private MapNode CurrentNode;

		public ChooseNextPath(VehicleController vehicle) : base(vehicle)
		{
			this.CurrentNode = this.Vehicle.NodeStack.Last();
		}


		private List<MapPath> GetNotVisitedPaths()
		{
			return this.CurrentNode.OutgoingPaths.Where(path => !path.IsVisited).ToList();
		}


		public override void Start()
		{
			List<MapPath> notVisitedPaths = this.GetNotVisitedPaths();

			// Handle start node
			if (this.Vehicle.NodeStack.Count == 1)
			{
				Debug.Log("Start");
				this.HandleStartNode();
			}

			// Handle first node of the graph
			else if (this.Vehicle.NodeStack.Count == 2)
			{
				Debug.Log("FirstNode");
				this.HandleFirstNodeOfGraph(notVisitedPaths);
			}
			// Check if we have visited this node -> turn around
			else if (this.Vehicle.NodeStack.IndexOf(this.CurrentNode) != this.Vehicle.NodeStack.Count - 1)
			{
				Debug.Log("Already visited. Total: " + this.Vehicle.NodeStack.Count + ", Index: " + this.Vehicle.NodeStack.IndexOf(this.CurrentNode));
				this.GoBack();
			}

			// Check if we have visited all paths of this node
			else if (notVisitedPaths.Count == 0)
			{
				Debug.Log("NoOtherOptions");
				this.GoBack();
			}
			else
			{
				this.ChoseNextPathBasedOnEndNode();
			}
		}

		private void ChoseNextPathBasedOnEndNode()
		{
			List<MapPath> notVisitedPaths = this.GetNotVisitedPaths();

			if (notVisitedPaths.Count == 0)
			{
				GoBack();
				return;
			}

			// Determine the path based on the end node's label
			string endNodeLabel = this.Vehicle.EndNode.GetLabel();

			switch (endNodeLabel)
			{
				case "A":
					FollowLeftmostPath(notVisitedPaths);
					break;
				case "B":
					FollowMiddlePath(notVisitedPaths);
					break;
				case "C":
					FollowRightmostPath(notVisitedPaths);
					break;
				default:
					Debug.Log("Random from " + notVisitedPaths.Count + " possible positions");
					// Default behavior: Randomly choose a path
					ChoseRandomPath(notVisitedPaths);
					break;
			}
		}


		private void FollowLeftmostPath(List<MapPath> paths)
		{
			// Sort paths by the angle to the reference direction in ascending order
			MapPath selectedPath = paths
				.OrderBy(path =>
				{
					Vector2 outgoingPosition = path.GetOutgoingPositionFor(this.CurrentNode).Value;
					// Calculate the angle relative to the reference direction
					float angle = Vector2.SignedAngle(this.Vehicle.Drive.Forward, outgoingPosition) - 180;
					return Mathf.Abs(angle);
				})
				.First();
			
			// Choose the selected path
			ChoosePath(selectedPath);
		}

		private void FollowMiddlePath(List<MapPath> paths)
		{
			if (this.Vehicle.PreferLeft)
			{
				this.FollowLeftmostPath(paths);
			}
			else
			{
				this.FollowRightmostPath(paths);
			}
			this.Vehicle.PreferLeft = !this.Vehicle.PreferLeft;
		}

		private void FollowRightmostPath(List<MapPath> paths)
		{
			// Sort paths by the angle to the reference direction in ascending order
			MapPath selectedPath = paths
				.OrderBy(path =>
				{
					Vector2 outgoingPosition = path.GetOutgoingPositionFor(this.CurrentNode).Value;
					// Calculate the angle relative to the reference direction
					float angle = Vector2.SignedAngle(this.Vehicle.Drive.Forward, outgoingPosition) + 360;
					return Mathf.Abs(angle);
				})
				.First();

			// Choose the selected path
			ChoosePath(selectedPath);
		}


		private void ChoseRandomPath(List<MapPath> paths)
		{
			int randomIndex = Random.Range(0, paths.Count);
			ChoosePath(paths[randomIndex]);
		}
		private void HandleStartNode()
		{

			// There is only one way to go -> choose it
			this.ChoosePosition(this.CurrentNode.OutgoingPaths[0].GetOutgoingPositionFor(this.CurrentNode).Value);
		}


		private void HandleFirstNodeOfGraph(List<MapPath> notVisitedPaths)
		{

			// Check if we have explored everything
			if (notVisitedPaths.Count == 0)
			{
				this.Vehicle.SetState(new ExploredEverything(this.Vehicle));
				return;
			}

			// Randomly choose the next one
			this.ChoseNextPathBasedOnEndNode();
		}

		private void ChoosePath(MapPath path)
		{
			if (path.GetOutgoingPositionFor(this.CurrentNode).Value != null)
			{
				this.ChoosePosition(path.GetOutgoingPositionFor(this.CurrentNode).Value);
			}
			else
			{
				Debug.Log("selectedPath.Value is null");
			}

		}
		private void ChoosePosition(Vector2 linePosition)
		{
			// Turn to that line
			this.Vehicle.Drive.RotateFacing(linePosition - this.Vehicle.Position, () =>
			{

				// Start to following that line
				this.Vehicle.SetState(new FollowLine(this.Vehicle));
			});
		}

		private void GoBack()
		{
			// Remove the current node from the stack and return to the previous node
			this.Vehicle.NodeStack.RemoveAt(this.Vehicle.NodeStack.Count - 1);
			MapNode previousNode = this.Vehicle.NodeStack.Last();

			MapPath backtrackPath = CurrentNode.OutgoingPaths
				.FirstOrDefault(path => path.Start == previousNode || path.End == previousNode);

			if (backtrackPath != null)
			{
				ChoosePath(backtrackPath);
			}
		}
	}
}