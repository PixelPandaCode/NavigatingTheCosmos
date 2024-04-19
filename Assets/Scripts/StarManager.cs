using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class StarManager : MonoBehaviour
{
    public GameObject Star;
    public TextAsset StarData;
    public GameObject Line;
    public List<Vector3> StarPoints;
    public float Radius = 5.0f;
    public StarGraphBuilder GraphBuilder = new StarGraphBuilder();
    public int KNeighbors = 2;
    public SpaceShip MySpaceShip = null;
    public List<Star> stars = new List<Star>();
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public List<Vector3> NavPath = null;
    // Start is called before the first frame update
    void Start()
    {
        // Split the CSV into rows
        string[] rows = StarData.text.Split(new char[] { '\n' });
        StarPoints = new List<Vector3> { };
        // Iterate through each row
        for (int i = 1; i < rows.Length; i++)
        {
            // Split the row into columns
            string[] columns = rows[i].Split(new char[] { ';' });
            if (columns.Length < 3)
            {
                continue;
            }

            // Check if there are enough columns
            if (columns.Length > 0)
            {
                // Parse the first column to get the 3D coordinates
                Vector3 position = ParseVector3(columns[0]) * Radius;

                // Instantiate the Star GameObject at the parsed position
                Star star = Instantiate(Star, position, Quaternion.identity).GetComponent<Star>();
                Vector3 color = ParseVector3(columns[1]);
                star.Intensity = Mathf.Clamp(1.0f, 5.0f, 2.0f * float.Parse(columns[2]));
                // star.Intensity = Mathf.Clamp(star.Intensity, 1.0f, 3.0f);
                star.StarColor = new Color(color.x / 255.0f, color.y / 255.0f, color.z / 255.0f, 1.0f);
                star.Init();
                stars.Add(star);
                // Debug.Log(star);
                StarPoints.Add(position);
            }
        }
        if (Line != null)
        {
            List<Vector3[]> graph = GraphBuilder.BuildGraph(StarPoints, KNeighbors);
            for (int i = 0; i < graph.Count; ++i)
            {
                Vector3[] edge = graph[i];
                GameObject newLine = Instantiate(Line);
                newLine.GetComponent<LineRenderer>().positionCount = 2;
                newLine.GetComponent<LineRenderer>().SetPositions(edge);
                lineRenderers.Add(newLine.GetComponent<LineRenderer>());
            }
        }
    }

    public void ShowPath(Vector3 start, Vector3 end)
    {
        List<Vector3> Path = GraphBuilder.FindPathDijkstra(start, end);
        for (int i = 0; i < lineRenderers.Count; ++i)
        {
            Vector3[] positions = new Vector3[2];
            lineRenderers[i].GetPositions(positions);
            for (int j = 0; j < Path.Count - 1; ++j)
            {
                if (positions.Contains(Path[j]) && positions.Contains(Path[j+1])) {
                    lineRenderers[i].material = new Material(lineRenderers[i].sharedMaterial);
                    float intensity = 5.0f;
                    lineRenderers[i].material.SetColor("_BaseColor", new Color(0, 0.435f, 1.0f) * intensity);
                }
            }
        }
        NavPath = Path;
    }

    public void ClearPath()
    {
        for (int i = 0; i < lineRenderers.Count; ++i)
        {
            Vector3[] positions = new Vector3[2];
            lineRenderers[i].GetPositions(positions);
            lineRenderers[i].material = new Material(lineRenderers[i].sharedMaterial);
            lineRenderers[i].material.SetColor("_BaseColor", new Color(1.0f, 1.0f, 1.0f));
        }
        NavPath = null;
    }


    public void ShowTraversalPath()
    {
        List<Vector3> Path = GraphBuilder.FindTraversalPathGreedy();
        for (int i = 0; i < lineRenderers.Count; ++i)
        {
            Vector3[] positions = new Vector3[2];
            lineRenderers[i].GetPositions(positions);
            for (int j = 0; j < Path.Count - 1; ++j)
            {
                if (positions.Contains(Path[j]) && positions.Contains(Path[j + 1]))
                {
                    lineRenderers[i].material = new Material(lineRenderers[i].sharedMaterial);
                    float intensity = 5.0f;
                    lineRenderers[i].material.SetColor("_BaseColor", new Color(0, 0.435f, 1.0f) * intensity);
                }
            }
        }
        NavPath = Path;
    }

    public float MoveSpaceShip()
    {
        if (NavPath != null)
        {
            return MySpaceShip.Move(NavPath);
        }
        return 0;
    }

    Vector3 ParseVector3(string vectorString)
    {
        // Remove the parentheses
        vectorString = vectorString.Trim(new char[] { '(', ')' });

        // Split the remaining string by commas
        string[] values = vectorString.Split(new char[] { ',' });

        // Parse the separated strings as floats
        float x = float.Parse(values[0]);
        float y = float.Parse(values[1]);
        float z = float.Parse(values[2]);

        // Return the new Vector3
        return new Vector3(x, y, z);
    }
}
