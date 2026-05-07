using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Security.Cryptography;

public class Snake : MonoBehaviour
{
    public BoxCollider gridArea;
    public float MoveSpeed = 1.00f;
    public GameObject BodyPrefab;
    public int bodyGap = 1;

    private Vector3 faceDir =  Vector3.right;
    private List<GameObject> BodySegments = new List<GameObject>();
    private List<Vector3> PrevPos = new List<Vector3>();

    public TextMeshProUGUI countText;

    private int count = 0;



    // Start is called before the first frame update
    void Start()
    {
        SetCountText();
        GrowSnake();

    }

    void FixedUpdate()
    {
        Bounds bounds = gridArea.bounds;
        transform.Translate(Vector3.forward * MoveSpeed , Space.Self);
        Vector3 position = transform.localPosition;
        /*if (position.x > bounds.max.x) position.x = bounds.min.x;
        if (position.x < bounds.min.x) position.x = bounds.max.x;
        if (position.z > bounds.max.z) position.z = bounds.min.z;
        if (position.z < bounds.min.z) position.z = bounds.max.z;*/
        if (position.x > 10) position.x = -10;
        if (position.x < -10) position.x = 10;
        if (position.z > 10) position.z = -10;
        if (position.z < -10) position.z = 10;

        position.y = 1.0f;

        transform.localPosition = position;

        PrevPos.Insert(0, transform.localPosition);
        //if (PrevPos.Count > 500) PrevPos.RemoveAt(PrevPos.Count - 1);

        int index = 1;
        foreach (var body in BodySegments)
        {
            Vector3 point = PrevPos[Mathf.Min(index * bodyGap, PrevPos.Count-1)];
            body.transform.localPosition = point;
            index++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fruit"))
        {
            count = count + 1;
            
            if (count >= 5)
            {
                ImageTrackingManager.deuteranopiaComplete = true;
            }

            SetCountText();
            GrowSnake();
        }

        if (other.gameObject.CompareTag("Poison"))
        {
            count = 0;
            SetCountText();

            foreach (var body in BodySegments) Destroy(body);
            BodySegments.Clear();


        }

    }

    void SetCountText()
    {
        string text = "Snake Minigame\nExploits Deuteranopia\nLength (5 to win): " + count.ToString();
        if (ImageTrackingManager.deuteranopiaComplete) text = text + "\n\nLEVEL COMPLETE\n\nHMD CODE: 20";
        countText.text = text;
    }

    public void GrowSnake()
    {
        GameObject bodyPart = Instantiate(BodyPrefab, transform.parent);
        BodySegments.Add(bodyPart);
    }

    public void turnLeft() { transform.Rotate(Vector3.up * -90); }
    public void turnRight() { transform.Rotate(Vector3.up * 90); }
}
