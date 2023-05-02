using UnityEngine;

public class Player : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;

    public Sprite[] sprites;

    private int spriteIndexer;

    private Vector3 direction;
    public float gravity = -9.8f;

    public float strenght = 5f;

    private void Awake(){
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start(){
        InvokeRepeating(nameof(Animate), 0.15f, 0.15f);
    }

    private void OnEnable(){
        Vector3 position = transform.position;
        position.y = 0f;
        transform.position = position;

        direction = Vector3.zero;
    }

    private void Update(){

        if(Input.GetKeyDown(KeyCode.Space)  || Input.GetMouseButtonDown(0)){
            direction = Vector3.up * strenght;
        }

        if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began){
                 direction = Vector3.up * strenght;
            }
        }

        direction.y += gravity * Time.deltaTime;
        transform.position += direction * Time.deltaTime;

    }
    private void Animate(){
        spriteIndexer++;

        if(spriteIndexer >= sprites.Length){
            spriteIndexer = 0;
        }

        spriteRenderer.sprite = sprites[spriteIndexer];
    }

    private void OnTriggerEnter2D(Collider2D other){

        if (other.gameObject.tag == "Obstacle"){
            FindObjectOfType<GameManager>().GameOver();
        } else if (other.gameObject.tag == "Score") {
            FindObjectOfType<GameManager>().IncreaseScore();
        }

    }


}
