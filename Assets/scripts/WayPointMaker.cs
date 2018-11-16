using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class WayPointMaker : MonoBehaviour {

    float horizontal;
    float vertical;
    float altitude;

    float origMoveSpeed = 50;
    float moveSpeed;

    public GameObject wayPointNode;
    int counter = 0;

    GameObject selectedNode = null;
    GameObject highlightedNode = null;

    List<GameObject> nodes = new List<GameObject>();

    public Text highlightedNeighbourInfo;
    public Text selectedNeighbourInfo;

    void Start() {
        moveSpeed = origMoveSpeed;
    }

    void Update() {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        altitude = Input.GetAxis("Altitude");

        if (Input.GetKey(KeyCode.LeftShift)) {
            moveSpeed = origMoveSpeed * 3;
        } else if (Input.GetKey(KeyCode.LeftControl)) {
            moveSpeed = origMoveSpeed / 3;
        } else {
            moveSpeed = origMoveSpeed;
        }

        handleMovement();


        Ray ray = new Ray(transform.position, Vector3.down);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.tag == "node") {
                if (hit.collider.gameObject != selectedNode) {
                    if (highlightedNode != null) {
                        highlightedNode.GetComponent<WayPointNode>().highlighted = false;
                        highlightedNode.GetComponent<WayPointNode>().highlightedNeighbour = false;
                    }
                    highlightedNode = hit.collider.gameObject;
                    if (selectedNode != null) {
                        highlightedNode.GetComponent<WayPointNode>().highlightedNeighbour = true;
                    } else {
                        highlightedNode.GetComponent<WayPointNode>().highlighted = true;
                    }

                }
            } else if (highlightedNode != null) {
                highlightedNode.GetComponent<WayPointNode>().highlighted = false;
                highlightedNode.GetComponent<WayPointNode>().highlightedNeighbour = false;
                highlightedNode = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (hit.collider.tag == "terrain") {
                Vector3 summonPoint = hit.point;
                summonPoint.y += 5;
                //Debug.Log("summonPoint: " + summonPoint + "hitpoint: " + hit.point);
                GameObject newObject = (GameObject)Instantiate(wayPointNode, summonPoint, Quaternion.identity);
                newObject.name = wayPointNode.name + counter;
                Debug.Log("Summoned " + newObject.name);
                nodes.Add(newObject);
                counter++;
            } else if (hit.collider.tag == "node") {
                if (selectedNode != null) {
                    selectedNode.GetComponent<WayPointNode>().selected = false;
                }
                selectedNode = hit.collider.gameObject;
                selectedNode.GetComponent<WayPointNode>().highlighted = false;
                selectedNode.GetComponent<WayPointNode>().selected = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            if (selectedNode != null) {
                selectedNode.GetComponent<WayPointNode>().selected = false;
                selectedNode = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            if (selectedNode != null && highlightedNode != null) {
                selectedNode.GetComponent<WayPointNode>().MakeConnection(highlightedNode);
                highlightedNode.GetComponent<WayPointNode>().MakeConnection(selectedNode);
            }
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            if (hit.collider.tag == "node") {
                if (hit.collider.gameObject == selectedNode) {
                    selectedNode = null;
                }
                highlightedNode = null;
                nodes.Remove(hit.collider.gameObject);
                hit.collider.gameObject.GetComponent<WayPointNode>().neighbours.Clear();
                foreach(GameObject node in nodes) {
                    int indexToRemove=-1;
                    List<WayPointNode.Neighbour> neighbours = node.GetComponent<WayPointNode>().neighbours;
                    foreach (WayPointNode.Neighbour n in neighbours) {
                        if(n.node==hit.collider.gameObject){
                            indexToRemove = neighbours.IndexOf(n);
                            break;
                        }
                    }
                    if (indexToRemove >= 0) {
                        neighbours.RemoveAt(indexToRemove);
                    }

                }

                Destroy(hit.collider.gameObject);
            }

        }

        updateInfos();
    }

    private void updateInfos() {
        if (highlightedNode != null) {
            highlightedNeighbourInfo.text = highlightedNode.GetComponent<WayPointNode>().getNeighbourInfos();
        } else {
            highlightedNeighbourInfo.text = "No node highlighted";
        }
        if (selectedNode != null) {
            selectedNeighbourInfo.text = selectedNode.GetComponent<WayPointNode>().getNeighbourInfos();
        } else {
            selectedNeighbourInfo.text = "No node selected";
        }
    }

    private void handleMovement() {
        Vector3 newPos = transform.position;

        if (horizontal != 0) {
            newPos.x += horizontal * moveSpeed * Time.deltaTime;
        }

        if (altitude != 0) {
            newPos.y += altitude * moveSpeed * Time.deltaTime;
        }

        if (vertical != 0) {
            newPos.z += vertical * moveSpeed * Time.deltaTime;
        }

        transform.position = newPos;
    }

}

