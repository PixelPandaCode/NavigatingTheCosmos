using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarGraphBuilder
{
    public List<Vector3[]> StarEdges = null;
    public List<Vector3> StarVertices = null;
    public Dictionary<Vector3, List<Vector3>> StarUvs = new Dictionary<Vector3, List<Vector3>>();
    public List<Vector3[]> BuildGraph(List<Vector3> points, int k)
    {
        List<Vector3[]> edges = new List<Vector3[]>();
        List<Vector3> vertices = new List<Vector3>();

        // Check if k is greater than or equal to the number of points
        if (k >= points.Count)
        {
            Debug.LogWarning("k is set to a value larger than the number of points. Adjusting k to points.Count - 1.");
            k = points.Count - 1; // Adjust k to ensure it's within bounds
        }
        List<int> connectedIndexes = new List<int>();
        for (int i = 0; i < points.Count; i++)
        {
            // Create a list to store distances and corresponding point indices
            List<(float distance, int index)> distances = new List<(float distance, int index)>();

            for (int j = 0; j < points.Count; j++)
            {
                if (i != j && !connectedIndexes.Contains(j)) // Avoid self-comparison
                {
                    // Calculate distance and add it to the list
                    float distance = Vector3.Distance(points[i], points[j]);

                    distances.Add((distance, j));
                }
            }
            vertices.Add(points[i]);

            // Sort the list by distance and take the first k elements
            List<int> nearestNeighborIndices = distances.OrderBy(d => d.distance)
                                                         .Take(k)
                                                         .Select(d => d.index)
                                                         .ToList();

            // Create edges to the k-nearest neighbors
            foreach (int neighborIndex in nearestNeighborIndices)
            {
                edges.Add(new Vector3[] { points[i], points[neighborIndex] });
                if (!StarUvs.ContainsKey(points[i]))
                {
                    StarUvs.Add(points[i], new List<Vector3>());
                }
                if (!StarUvs.ContainsKey(points[neighborIndex]))
                {
                    StarUvs.Add(points[neighborIndex], new List<Vector3>());
                }
                if (!StarUvs[points[i]].Contains(points[neighborIndex]))
                {
                    StarUvs[points[i]].Add(points[neighborIndex]);
                }
                if (!StarUvs[points[neighborIndex]].Contains(points[i]))
                {
                    StarUvs[points[neighborIndex]].Add(points[i]);
                }
                if (!connectedIndexes.Contains(neighborIndex) && FindPathDFS(points[0], points[neighborIndex]).Count > 0)
                {
                    connectedIndexes.Add(neighborIndex);
                }
            }
        }
        StarEdges = edges;
        StarVertices = vertices;
        return edges;
    }

    public List<Vector3> FindPathDFS(Vector3 start, Vector3 end)
    {
        var visited = new HashSet<Vector3>();
        var pathStack = new Stack<Vector3>();
        var path = new List<Vector3>();

        if (DFS(start, end, visited, pathStack))
        {
            path = pathStack.ToList();
            path.Reverse(); // Reverse to get the path from start to end
        }

        return path;
    }

    private bool DFS(Vector3 current, Vector3 end, HashSet<Vector3> visited, Stack<Vector3> pathStack)
    {
        visited.Add(current);
        pathStack.Push(current);

        if (current == end) return true;

        foreach (var neighbor in StarUvs[current])
        {
            if (!visited.Contains(neighbor))
            {
                if (DFS(neighbor, end, visited, pathStack)) return true;
            }
        }

        pathStack.Pop();
        return false;
    }

    public List<Vector3> FindPathBFS(Vector3 start, Vector3 end)
    {
        var visited = new HashSet<Vector3> { start };
        var queue = new Queue<Vector3>();
        var parents = new Dictionary<Vector3, Vector3?>();

        queue.Enqueue(start);
        parents[start] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end) break;

            foreach (var neighbor in StarUvs[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parents[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return ReconstructPath_BFS(parents, start, end);
    }

    private List<Vector3> ReconstructPath_BFS(Dictionary<Vector3, Vector3?> parents, Vector3 start, Vector3 end)
    {
        var path = new List<Vector3>();
        for (Vector3? at = end; at != null; at = parents[at.Value])
        {
            path.Add(at.Value);
        }

        path.Reverse();
        return path.Count > 1 ? path : new List<Vector3>(); // Return the path if it exists
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
    {
        var totalPath = new List<Vector3> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    public List<Vector3> FindPathAStar(Vector3 start, Vector3 end)
    {
        var openSet = new SortedList<float, Queue<Vector3>>();
        AddToOpenSet(openSet, 0, start);

        var cameFrom = new Dictionary<Vector3, Vector3>();
        var gScore = StarVertices.ToDictionary(v => v, v => Mathf.Infinity);
        var fScore = StarVertices.ToDictionary(v => v, v => Mathf.Infinity);

        gScore[start] = 0;
        fScore[start] = Vector3.Distance(start, end);

        while (openSet.Count > 0)
        {
            var current = Dequeue(openSet);

            if (current.Equals(end))
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var neighbor in StarUvs[current])
            {
                float tentativeGScore = gScore[current] + Vector3.Distance(current, neighbor);
                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Vector3.Distance(neighbor, end);
                    AddToOpenSet(openSet, fScore[neighbor], neighbor);
                }
            }
        }

        return new List<Vector3>(); // Return empty path if none found
    }

    public List<Vector3> FindPathDijkstra(Vector3 start, Vector3 end)
    {
        var openSet = new SortedList<float, Queue<Vector3>>();
        AddToOpenSet(openSet, 0, start);

        var cameFrom = new Dictionary<Vector3, Vector3>();
        var costSoFar = new Dictionary<Vector3, float>();

        foreach (var vertex in StarVertices)
        {
            costSoFar[vertex] = Mathf.Infinity;
        }
        costSoFar[start] = 0;

        while (openSet.Count > 0)
        {
            var current = Dequeue(openSet);

            if (current.Equals(end))
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var neighbor in StarUvs[current])
            {
                float newCost = costSoFar[current] + Vector3.Distance(current, neighbor);
                if (newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    AddToOpenSet(openSet, costSoFar[neighbor], neighbor);
                }
            }
        }

        return new List<Vector3>(); // Return empty path if none found
    }

    private void AddToOpenSet(SortedList<float, Queue<Vector3>> openSet, float priority, Vector3 node)
    {
        if (!openSet.ContainsKey(priority))
        {
            openSet[priority] = new Queue<Vector3>();
        }
        openSet[priority].Enqueue(node);
    }

    private Vector3 Dequeue(SortedList<float, Queue<Vector3>> openSet)
    {
        var firstPair = openSet.First();
        var node = firstPair.Value.Dequeue();
        if (firstPair.Value.Count == 0)
        {
            openSet.Remove(firstPair.Key);
        }
        return node;
    }

    private float CalculateTotalPathDistance(List<Vector3> path)
    {
        float totalDistance = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(path[i], path[i + 1]);
        }
        return totalDistance;
    }

    public List<Vector3> FindTraversalPathGreedy()
    {
        if (StarVertices == null || StarVertices.Count == 0)
        {
            Debug.LogError("StarVertices is null or empty.");
            return new List<Vector3>();
        }

        List<Vector3> path = new List<Vector3>();
        HashSet<Vector3> visited = new HashSet<Vector3>();

        // Select a random starting vertex
        Vector3 current = StarVertices[UnityEngine.Random.Range(0, StarVertices.Count)];
        visited.Add(current);
        path.Add(current);

        // Helper method to find the nearest unvisited neighbor
        Vector3? FindNearestUnvisitedNeighbor(Vector3 from)
        {
            if (!StarUvs.TryGetValue(from, out List<Vector3> neighbors))
                return null;

            Vector3? nearestNeighbor = null;
            float nearestDistance = float.MaxValue;

            foreach (var neighbor in neighbors.Where(n => !visited.Contains(n)))
            {
                float distance = Vector3.Distance(from, neighbor);
                if (distance < nearestDistance)
                {
                    nearestNeighbor = neighbor;
                    nearestDistance = distance;
                }
            }

            return nearestNeighbor;
        }

        while (visited.Count < StarVertices.Count)
        {
            var nearestNeighbor = FindNearestUnvisitedNeighbor(current);

            if (nearestNeighbor.HasValue)
            {
                // Move to the nearest unvisited neighbor
                current = nearestNeighbor.Value;
                visited.Add(current);
                path.Add(current);
            }
            else
            {
                Vector3? bestUnvisited = null;
                List<Vector3> bestPathToUnvisited = null;
                float bestPathDistance = float.MaxValue;

                // Calculate the total distance for each path and compare to find the shortest
                foreach (var vertex in StarVertices.Where(v => !visited.Contains(v)))
                {
                    List<Vector3> potentialPath = FindPathAStar(current, vertex);
                    float potentialPathDistance = CalculateTotalPathDistance(potentialPath);

                    if (potentialPathDistance < bestPathDistance)
                    {
                        bestPathDistance = potentialPathDistance;
                        bestUnvisited = vertex;
                        bestPathToUnvisited = potentialPath;
                    }
                }

                if (bestUnvisited.HasValue && bestPathToUnvisited != null && bestPathDistance < float.MaxValue)
                {
                    // Skip the first vertex if it's the current position
                    int startIndex = bestPathToUnvisited[0] == current ? 1 : 0;

                    for (int i = startIndex; i < bestPathToUnvisited.Count; i++)
                    {
                        Vector3 step = bestPathToUnvisited[i];
                        if (!visited.Contains(step))
                        {
                            visited.Add(step);
                            path.Add(step);
                        }
                    }
                    current = bestUnvisited.Value;
                }
                else
                {
                    Debug.LogWarning("No reachable unvisited vertices left.");
                    break;
                }
            }
        }

        return path;
    }
}
