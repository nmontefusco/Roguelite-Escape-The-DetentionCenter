using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject
{
    public float restartLevelDelay = 1f;        
    public int pointsPerFood = 10;             
    public int pointsPerSoda = 20;              
    public int wallDamage = 1;
    public Text foodText;
    private Animator animator;                  
    private int food;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatsound1;
    public AudioClip eatsound2;
    public AudioClip exitsound1;
    public AudioClip exitsound2;
    public AudioClip blastersound1;
    public AudioClip blastersound2;
    public AudioClip gameOverSound;
    public AudioClip damage1, damage2;

    protected override void Start()
    {
        
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        base.Start();

        foodText.text = "Hope: " + food;
    }


    //This function is called when the behaviour becomes disabled or inactive.
    private void OnDisable()
    {
        //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
        GameManager.instance.playerFoodPoints = food;
    }


    private void Update()
    {
        //If it's not the player's turn, exit the function.
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;     
        int vertical = 0;

        horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        vertical = (int)(Input.GetAxisRaw("Vertical"));

         if (horizontal != 0)
        {
            vertical = 0;
        }

        if (horizontal != 0 || vertical != 0)
        {
            //Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
            //Pass in horizontal and vertical as parameters to specify the direction to move Player in.
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    //AttemptMove overrides the AttemptMove function in the base class MovingObject
    //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
       
        food--;
        foodText.text = "Hope: " + food;
        base.AttemptMove<T>(xDir, yDir);
        RaycastHit2D hit;

        //If Move returns true, meaning Player was able to move into an empty space.
        if (Move(xDir, yDir, out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }
         CheckIfGameOver();

        //Set the playersTurn boolean of GameManager to false now that players turn is over.
        GameManager.instance.playersTurn = false;
    }


    //OnCantMove overrides the abstract function OnCantMove in MovingObject.
    //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
    protected override void OnCantMove<T>(T component)
    {
       
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("playerShoot");
        SoundManager.instance.RandomizeSfx(blastersound1, blastersound2);
    }


    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check if the tag of the trigger collided with is Exit.
        if (other.tag == "Exit")
        {
            SoundManager.instance.RandomizeSfx(exitsound1,exitsound2);
            Invoke("Restart", restartLevelDelay);

            //Disable the player object since level is over.
            enabled = false;
        }

        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Hope: " + food;
            SoundManager.instance.RandomizeSfx(eatsound1, eatsound2);
            other.gameObject.SetActive(false);
        }

        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Hope: " + food;
            SoundManager.instance.RandomizeSfx(eatsound1, eatsound2);
            other.gameObject.SetActive(false);
        }
    }


    //Restart reloads the scene when called.
    private void Restart()
    {
       SceneManager.LoadScene(0);
    }


    //LoseFood is called when an enemy attacks the player.
    //It takes a parameter loss which specifies how many points to lose.
    public void LoseFood(int loss)
    {
        
        animator.SetTrigger("playerHit");
        food -= loss;
        foodText.text = "-" + loss + " Hope: " + food;
        SoundManager.instance.RandomizeSfx(damage1,damage2);

        CheckIfGameOver();
    }


    //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
    private void CheckIfGameOver()
    {
        
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }
}