using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class Path_AStar {

    Queue<Tile> path;

	public Path_AStar(World world, Tile tileStart, Tile tileEnd) {

        //Check to see if we have a valid tile graph
        if(world.tileGraph == null) {
            world.tileGraph = new Path_TileGraph(world);
        }
        
        //A dictionary of all valid, wallkable nodes.
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

        //Make sure our start/end tiles our in the list of nodes.
        if(nodes.ContainsKey(tileStart) == false) {
            Debug.LogError("Path_AStar: The starting tile isn't in the list of nodes.");

            //FIXME: Right now, we're going to manually add the start tile into the list
            //of valid nodes.

            return;
        }

        if(nodes.ContainsKey(tileEnd) == false) {
            Debug.LogError("Path_AStar: The ending tile isn't in the list of nodes.");
            return;
        }

        Path_Node<Tile> start = nodes[tileStart];
        Path_Node<Tile> goal = nodes[tileEnd];
        
        //From here on following Wikipedia psuedocode

        // The set of nodes already evaluated
        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        // The set of currently discovered nodes that are not evaluated yet.
        // Initially, only the start node is known.
        /*List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
        OpenSet.Add(start);*/

        SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>>();
        OpenSet.Enqueue(start, 0);

        // For each node, which node it can most efficiently be reached from.
        // If a node can be reached from many nodes, cameFrom will eventually contain the
        // most efficient previous step.
        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        // For each node, the cost of getting from the start node to that node.
        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> n in nodes.Values) {
            g_score[n] = Mathf.Infinity;
        }

        // The cost of going from start to start is zero.
        g_score[start] = 0;

        // For each node, the total cost of getting from the start node to the goal
        // by passing by that node. That value is partly known, partly heuristic.
        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> n in nodes.Values) {
            f_score[n] = Mathf.Infinity;
        }

        // For the first node, that value is completely heuristic.
        f_score[start] = heuristic_cost_estimate(start, goal);

        while(OpenSet.Count > 0){
            Path_Node<Tile> current = OpenSet.Dequeue();

            if(current == goal) {
                //We have reached our goal!
                //Let's convert this into an actual sequence of tiles to walk on, then end this constructor.
                reconstruct_path(Came_From, current);
                return;
            }

            ClosedSet.Add(current);

            foreach(Path_Edge<Tile> edge_neighbor in current.edges) {
                Path_Node<Tile> neighbor = edge_neighbor.node;

                if(ClosedSet.Contains(neighbor) == true) {
                    continue; //ignore this already completed neighbor
                }

                float movement_cost_to_neighbor = neighbor.data.movementCost * dist_between(current, neighbor);

                float tentative_g_score = g_score[current] + movement_cost_to_neighbor;

                if (OpenSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                    continue;

                Came_From[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = g_score[neighbor] + heuristic_cost_estimate(neighbor, goal);

                if(OpenSet.Contains(neighbor) == false) {
                    OpenSet.Enqueue(neighbor, f_score[neighbor]);
                } else {
                    OpenSet.UpdatePriority(neighbor, f_score[neighbor]);
                }

            } //foreach neighbor
        } //while 

        //If we reached here, it means that we've burned through the entire OpenSet without ever reaching a point where current == goal.
        //This happens when there is no path from start to goal (so there's no path from start to goal).

        //We don't have a failure state, maybe? It's just that path list will be null.

    }

    float heuristic_cost_estimate(Path_Node<Tile> a, Path_Node<Tile> b) {
        return Mathf.Sqrt(Mathf.Pow(a.data.X - b.data.X, 2) + Mathf.Pow(a.data.Y - b.data.Y, 2));
    }

    float dist_between(Path_Node<Tile> a, Path_Node<Tile> b) {
        //We can make assumptions because we know we are on a grid.

        //Hori/Vert neighbours have a distance of 1
        if(Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1) {
            return 1f;
        }

        //Diag neighbours have a distance of 1.414121356
        if(Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1) {
            return 1.41421356237f;
        }

        //Otherwise, do the actiual math.
        return Mathf.Sqrt(Mathf.Pow(a.data.X - b.data.X, 2) + Mathf.Pow(a.data.Y - b.data.Y, 2));
    }

    void reconstruct_path(Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From, Path_Node<Tile> current) {
        //So at this point, current IS the goal.
        //So what we want to do is walk backwards through the Came_From map until we reach the "end" of that map... which will be our starting node!

        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data); //This "final" step in the path is the goal!

        while (Came_From.ContainsKey(current)) {
            //Came_From is a map, where the key => value relation is really saying some_node => we_got_there_from_this_node.

            current = Came_From[current];
            total_path.Enqueue(current.data);
        }

        //At this point, total_path is a queue that is running backwards from the END tile to the START tile, so let's reverse it.
        path = new Queue<Tile>(total_path.Reverse());

    }

    public Tile Dequeue() {
        return path.Dequeue();
    }

    public int Length() {
        if (path == null)
            return 0;

        return path.Count;
    }

}
