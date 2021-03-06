using UnityEngine;
using System.Collections;
using System.Timers;

/*
    * AUTHOR: Trenton Pottruff
    * 
    * CONTRIBUTOR: Garrett Nicholas
    * (Logic for the different types of enemies and balancing)
*/

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class CPU : MonoBehaviour {
    public float minSpeed = 1;
    public float maxSpeed = 3;
    public float speed = 3;
    public bool shooter = false;
    public bool duplicator = false;
    public bool firing = false;
    public int ScoreOnDeath;
    public int minCoins;
    public int maxCoins;
    public GameObject FullHealthBar;
    public GameObject HealthBar;

    private Rigidbody2D rb;
    private GameObject bulletPrefab;
    private GameObject basicEnemy;
    private Vector2 td;
    private Health health;
    private GameObject player;
    private bool melee = false;
    private RaycastHit2D meleeHit;
    private Timer HealthBarTimer = new Timer();
    private bool hideHealth;
    private GameObject BronzeCoin;
    private GameObject SilverCoin;
    private GameObject GoldCoin;

    private void Start() {
        BronzeCoin = Resources.Load<GameObject>("Prefabs/BronzeCoin");
        SilverCoin = Resources.Load<GameObject>("Prefabs/SilverCoin");
        GoldCoin = Resources.Load<GameObject>("Prefabs/GoldCoin");
        bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
        basicEnemy = Resources.Load<GameObject>("Prefabs/Enemies/DupeMinion");
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        speed = Random.Range(minSpeed, maxSpeed);
        player = GameObject.FindGameObjectWithTag("Player");
        FullHealthBar.SetActive(false);
        StartCoroutine(FireBullet());
        StartCoroutine(MeleeAttack());
	}

    void Update() {
        player = GameObject.FindGameObjectWithTag("Player");
        if (hideHealth) {
            FullHealthBar.SetActive(false);
            hideHealth = false;
        }
        if (!Game.PAUSED) {
            Vector2 playerPosition = player.transform.position;
            Vector2 targetDirection = (playerPosition - (Vector2)transform.position).normalized;
            td = targetDirection;
            rb.velocity = targetDirection * speed;
            HealthBar.transform.localScale = new Vector3((float)(health.health / 100), 0.9081425f, 0.908152f);
            EnemyLogic();
        } else {
            rb.velocity = Vector2.zero;
        }
    }

    public void DropCoins() {
        int drop = Random.Range(minCoins, maxCoins+1);
        int goldCoins = drop / 10;
        int goldRemain = drop % 10;
        int silverCoins = goldRemain / 5;
        int bronzeCoins = goldRemain % 5;

        //Spawn Gold Coins
        for (int i = 0; i < goldCoins; i++) {
            GameObject gold = Instantiate(GoldCoin, this.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D rb = gold.GetComponent<Rigidbody2D>();
            Vector2 direction = Vector2.up;
            float angle = Random.Range(0f, 360f);
            direction = Quaternion.Euler(0, 0, angle) * direction;
            rb.AddForce(direction * 5, ForceMode2D.Impulse);
        }
        //Spawn Silver Coins
        for (int i = 0; i < silverCoins; i++) {
            GameObject silver = Instantiate(SilverCoin, this.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D rb = silver.GetComponent<Rigidbody2D>();
            Vector2 direction = Vector2.up;
            float angle = Random.Range(0f, 360f);
            direction = Quaternion.Euler(0, 0, angle) * direction;
            rb.AddForce(direction * 5, ForceMode2D.Impulse);
        }
        //Spawn Bronze Coins
        for (int i = 0; i < bronzeCoins; i++) {
            GameObject bronze = Instantiate(BronzeCoin, this.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D rb = bronze.GetComponent<Rigidbody2D>();
            Vector2 direction = Vector2.up;
            float angle = Random.Range(0f, 360f);
            direction = Quaternion.Euler(0, 0, angle) * direction;
            rb.AddForce(direction * 5, ForceMode2D.Impulse);
        }
    }

    private void EnemyLogic() {
        float duperate = Mathf.Round(Random.Range(0f, 100f));
        float firerate = Mathf.Round(Random.Range(0f, 1.5f));
        if (shooter && (firerate == 0f)) {
            firing = true;
        } else firing = false;
        if(duplicator && (duperate == 0f)) {
            Instantiate(basicEnemy, this.transform.position + new Vector3(0, 1), Quaternion.identity);
        }

        if (!shooter) {
            float distance = Vector2.Distance(player.transform.position, this.transform.position);
            if (distance <= 2) {
                Vector2 playerDirection = (player.transform.position - this.transform.position).normalized;
                meleeHit = Physics2D.Raycast(this.transform.position, playerDirection, 2f, 1 << LayerMask.NameToLayer("Player"));
                Debug.DrawRay(this.transform.position, playerDirection * 2, Color.red, 5000);
                melee = (bool) meleeHit;
            } else {
                melee = false;
            }            
        }
    }

    private IEnumerator FireBullet() {
        do {
            if (firing) {
                Vector2 direction = td.normalized;
                GameObject bulletObj = Instantiate(bulletPrefab, this.transform.position + (Vector3)direction, Quaternion.identity) as GameObject;
                Bullet bullet = bulletObj.GetComponent<Bullet>();
                bullet.owner = this.transform;
                bullet.damage = 5;
                bullet.SetVelocityOnAwake(rb.velocity + (direction * 10));
            }
            yield return new WaitForSeconds(0.1f);
        } while (true);
    }

    private IEnumerator MeleeAttack() {
        while (true) {
            if (melee) {
                Health playerHealth = meleeHit.collider.gameObject.GetComponent<Health>();
                playerHealth.DoDamage(1);    
            }
            yield return new WaitForSeconds(1);
        }
    }
    public void resetTimer() {
        FullHealthBar.SetActive(true);
        HealthBarTimer.Stop();
        HealthBarTimer.Dispose();
        HealthBarTimer = new Timer(5000);
        HealthBarTimer.Elapsed += (sender, e) => HandleTimer();
        HealthBarTimer.Start();
    }
    public void disposeTimer() {
        HealthBarTimer.Stop();
        HealthBarTimer.Dispose();
    }
    private void HandleTimer() {
        hideHealth = true;
        HealthBarTimer.Stop();
    }
}
