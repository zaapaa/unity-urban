using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WayPointNode : MonoBehaviour {

    public struct Neighbour {
        public GameObject node;
        public float cost;
        public Vector3 distance;
        public float distanceAbsolute;
        public bool lineDrawn;
    }

    public List<Neighbour> neighbours = new List<Neighbour>();
    public bool highlighted;
    public bool selected;
    public bool highlightedNeighbour;

    private float maxCost = 0;


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        ResetLines();

        if (selected) {
            GetComponent<Renderer>().material.color = Color.red;
        } else if (highlighted) {
            GetComponent<Renderer>().material.color = Color.blue;
        } else if (highlightedNeighbour) {
            GetComponent<Renderer>().material.color = Color.yellow;
        } else {
            GetComponent<Renderer>().material.color = Color.white;
        }

        DrawLines();
        
    }

    public void MakeConnection(GameObject newNode) {

        bool exists = false;
        int existIndex=-1;

        foreach (Neighbour n in neighbours) {
            if (n.node == newNode) {
                exists=true;
                existIndex = neighbours.IndexOf(n);
                break;
            }
        }

        if (exists) {
            if (existIndex >= 0) {
                neighbours.RemoveAt(existIndex);
            }
        } else {
            Neighbour newNeighbour = new Neighbour();
            newNeighbour.node = newNode;
            newNeighbour.distance = new Vector3(
                                        transform.position.x - newNode.transform.position.x,
                                        transform.position.y - newNode.transform.position.y,
                                        transform.position.z - newNode.transform.position.z);
            newNeighbour.distanceAbsolute = Vector3.Distance(transform.position, newNode.transform.position);
            newNeighbour.cost = CalculateCost(newNeighbour);
            newNeighbour.lineDrawn = false;

            neighbours.Add(newNeighbour);
        }

        
    }

    private float CalculateCost(Neighbour newNeighbour) {
        float newCost = newNeighbour.distanceAbsolute + Mathf.Abs(newNeighbour.distance.y);
        if (newCost > maxCost) {
            maxCost = newCost;
        }
        return newCost;
    }

    private void DrawLines() {
        for(int i=0;i<neighbours.Count;i++) {
            Neighbour n = neighbours[i];

            foreach(Neighbour nn in n.node.GetComponent<WayPointNode>().neighbours) {
                if (nn.node == gameObject) {
                    n.lineDrawn = nn.lineDrawn;
                    
                }
            }

            if (!n.lineDrawn) {
                float t = n.cost / maxCost;
                Debug.Log("drawing line " + t);
                Debug.DrawLine(transform.position, n.node.transform.position, Color.Lerp(Color.green, Color.red, t),0f,false);
                n.lineDrawn = true;
            }
        }
    }

    private void ResetLines() {
        for (int i = 0; i < neighbours.Count; i++) {
            Neighbour n = neighbours[i];
            n.lineDrawn = false;
        }
    }


    public string getNeighbourInfos() {
        string info="";

        if (neighbours.Count == 0) {
            info += "No connections";
        }

        foreach (Neighbour n in neighbours) {
            info += "Neighbour:" + n.node.name;
            info += "\nDistance:" + n.distance;
            info += "\nabsoluteDistance: " + n.distanceAbsolute;
            info += "\ncost: " + n.cost;
            info += "\nline drawn: " + n.lineDrawn;
            info += "\n";
        }

        return info;
    }
}
