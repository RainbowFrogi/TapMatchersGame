using System.Collections;
using UnityEngine;

public class LocationHandler : MonoBehaviour
{
    GameManager gameManager;

    public int x, y;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    //Set the x and y positions of the newly created square
    public void SetXY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    void OnMouseDown()
    {
        gameManager.ClickMouse(x, y);
    }

    //Drop the square to the bottom (drop)
    public IEnumerator MoveToBottom(int drop)
    {
        int startY = y;
        int endY = drop;

        
        float currentY = startY;

        float time = 0f;
        
        while (currentY != endY)
        {
            time += Time.deltaTime;

            //Used to smoothly move the square down every frame as the it lerps down to the drop height
            currentY = Mathf.Lerp(startY, endY, (time / (startY - drop)) * gameManager.dropSpeedMultiplier);

            //Set square Y to currentY
            transform.position = new Vector2(transform.position.x, currentY);
            yield return null;
        }
    }

    public void WhereToDrop(int y)
    {
        StartCoroutine(MoveToBottom(y));
        this.y = y;
    }

    public void Respawned()
    {
        StartCoroutine(RespawnSequence());
    }

    //Respawns new squares over the grid on the same X pos where they got destroyed
    IEnumerator RespawnSequence()
    {
        float startY = transform.position.y;
        int endY = y;

        float time = 0f;
        
        float currentY = startY;

        Debug.Log($"Starting drop of {startY - y} units");

        while (currentY != endY)
        {
            time += Time.deltaTime;

            currentY = Mathf.Lerp(startY, endY, (time / (startY - y)) * GameManager.Instance.dropSpeedMultiplier);
            transform.position = new Vector2(transform.position.x, currentY);
            yield return null;
        }
    }
}