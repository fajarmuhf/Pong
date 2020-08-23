using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Pemain 1
    public PlayerControl player1; // skrip
    private Rigidbody2D player1Rigidbody;

    // Pemain 2
    public PlayerControl player2; // skrip
    private Rigidbody2D player2Rigidbody;

    // Bola
    public BallControl ball; // skrip
    private Rigidbody2D ballRigidbody;
    private CircleCollider2D ballCollider;

    // Skor maksimal
    public int maxScore;

    // Apakah debug window ditampilkan?
    private bool isDebugWindowShown = false;

    // Objek untuk menggambar prediksi lintasan bola
    public Trajectory trajectory;

    public GameObject powerUp;
    public int jumlahPowerUp;

    public GameObject fireBall;
    public int jumlahFireBall;

    public bool player1FireBall;
    public bool player2FireBall;

    // Start is called before the first frame update
    void Start()
    {
        player1Rigidbody = player1.GetComponent<Rigidbody2D>();
        player2Rigidbody = player2.GetComponent<Rigidbody2D>();
        ballRigidbody = ball.GetComponent<Rigidbody2D>();
        ballCollider = ball.GetComponent<CircleCollider2D>();

        jumlahPowerUp = 0;
        InvokeRepeating("MakePowerUp", 9.0f + (Random.Range(0, 30) / 10.0f), 4.0f + (Random.Range(0, 30) / 10.0f));

        jumlahFireBall = 0;
        InvokeRepeating("MakeFireBall", 9.0f+(Random.Range(0,30)/10.0f), 4.0f + (Random.Range(0, 30) / 10.0f));

        player1FireBall = false;
        player2FireBall = false;
    }

    public void MakePowerUp()
    {
        GameObject powerUpObject = Instantiate(powerUp);
        Physics2D.IgnoreCollision(GameObject.Find("Ball").GetComponent<Collider2D>(), powerUpObject.GetComponent<Collider2D>());
        for(int i = 0; i < jumlahFireBall; i++)
        {
            if (GameObject.Find("FireBall" + i) != null)
            Physics2D.IgnoreCollision(GameObject.Find("FireBall"+i).GetComponent<Collider2D>(), powerUpObject.GetComponent<Collider2D>());
        }
        powerUpObject.name = "PowerUp"+jumlahPowerUp;
        jumlahPowerUp++;
    }

    public void MakeFireBall()
    {
        GameObject fireBallObject = Instantiate(fireBall);
        Physics2D.IgnoreCollision(GameObject.Find("Ball").GetComponent<Collider2D>(), fireBallObject.GetComponent<Collider2D>());
        for (int i = 0; i < jumlahPowerUp; i++)
        {
            if(GameObject.Find("PowerUp" + i)!=null)
            Physics2D.IgnoreCollision(GameObject.Find("PowerUp" + i).GetComponent<Collider2D>(), fireBallObject.GetComponent<Collider2D>());
        }
        fireBallObject.name = "FireBall" + jumlahFireBall;
        jumlahFireBall++;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Untuk menampilkan GUI
    void OnGUI()
    {
        // Tampilkan skor pemain 1 di kiri atas dan pemain 2 di kanan atas
        GUI.Label(new Rect(Screen.width / 2 - 150 - 12, 20, 100, 100), "" + player1.Score);
        GUI.Label(new Rect(Screen.width / 2 + 150 + 12, 20, 100, 100), "" + player2.Score);

        // Tombol restart untuk memulai game dari awal
        if (GUI.Button(new Rect(Screen.width / 2 - 60, 35, 120, 53), "RESTART"))
        {
            // Ketika tombol restart ditekan, reset skor kedua pemain...
            player1.ResetScore();
            player2.ResetScore();

            // ...dan restart game.
            ball.SendMessage("RestartGame", 0.5f, SendMessageOptions.RequireReceiver);
            
            player1FireBall = false;
            player2FireBall = false;

            jumlahFireBall = 0;
            jumlahPowerUp = 0;
        }

        // Jika pemain 1 menang (mencapai skor maksimal), ...
        if (player1.Score == maxScore)
        {
            // ...tampilkan teks "PLAYER ONE WINS" di bagian kiri layar...
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 10, 2000, 1000), "PLAYER ONE WINS");

            // ...dan kembalikan bola ke tengah.
            ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            for (int i = 0; i < jumlahFireBall; i++)
            {
                Destroy(GameObject.Find("FireBall" + i));
            }
            for (int i = 0; i < jumlahPowerUp; i++)
            {
                Destroy(GameObject.Find("PowerUp" + i));
            }
        }
        // Sebaliknya, jika pemain 2 menang (mencapai skor maksimal), ...
        else if (player2.Score == maxScore)
        {
            // ...tampilkan teks "PLAYER TWO WINS" di bagian kanan layar... 
            GUI.Label(new Rect(Screen.width / 2 + 30, Screen.height / 2 - 10, 2000, 1000), "PLAYER TWO WINS");

            // ...dan kembalikan bola ke tengah.
            ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            for(int i = 0; i < jumlahFireBall; i++)
            {
                Destroy(GameObject.Find("FireBall" + i));
            }
            for (int i = 0; i < jumlahPowerUp; i++)
            {
                Destroy(GameObject.Find("PowerUp" + i));
            }
        }

        // Toggle nilai debug window ketika pemain mengeklik tombol ini.
        if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height - 73, 120, 53), "TOGGLE\nDEBUG INFO"))
        {
            isDebugWindowShown = !isDebugWindowShown;
        }

        // Jika isDebugWindowShown == true, tampilkan text area untuk debug window.
        if (isDebugWindowShown)
        {
            trajectory.enabled = !trajectory.enabled;
            // Simpan nilai warna lama GUI
            Color oldColor = GUI.backgroundColor;

            // Beri warna baru
            GUI.backgroundColor = Color.red;

            // Simpan variabel-variabel fisika yang akan ditampilkan. 
            float ballMass = ballRigidbody.mass;
            Vector2 ballVelocity = ballRigidbody.velocity;
            float ballSpeed = ballRigidbody.velocity.magnitude;
            Vector2 ballMomentum = ballMass * ballVelocity;
            float ballFriction = ballCollider.friction;

            float impulsePlayer1X = player1.LastContactPoint.normalImpulse;
            float impulsePlayer1Y = player1.LastContactPoint.tangentImpulse;
            float impulsePlayer2X = player2.LastContactPoint.normalImpulse;
            float impulsePlayer2Y = player2.LastContactPoint.tangentImpulse;

            // Tentukan debug text-nya
            string debugText =
                "Ball mass = " + ballMass + "\n" +
                "Ball velocity = " + ballVelocity + "\n" +
                "Ball speed = " + ballSpeed + "\n" +
                "Ball momentum = " + ballMomentum + "\n" +
                "Ball friction = " + ballFriction + "\n" +
                "Last impulse from player 1 = (" + impulsePlayer1X + ", " + impulsePlayer1Y + ")\n" +
                "Last impulse from player 2 = (" + impulsePlayer2X + ", " + impulsePlayer2Y + ")\n";

            // Tampilkan debug window
            GUIStyle guiStyle = new GUIStyle(GUI.skin.textArea);
            guiStyle.alignment = TextAnchor.UpperCenter;
            GUI.TextArea(new Rect(Screen.width / 2 - 200, Screen.height - 200, 400, 110), debugText, guiStyle);

            // Kembalikan warna lama GUI
            GUI.backgroundColor = oldColor;
        }

        if (player2FireBall)
        {
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 10, 2000, 1000), "PLAYER ONE WINS");

            // ...dan kembalikan bola ke tengah.
            ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            for(int i = 0; i < jumlahFireBall; i++)
            {
                Destroy(GameObject.Find("FireBall" + i));
            }
            for (int i = 0; i < jumlahPowerUp; i++)
            {
                Destroy(GameObject.Find("PowerUp" + i));
            }
        }
        else if (player1FireBall)
        {
            // ...tampilkan teks "PLAYER TWO WINS" di bagian kanan layar... 
            GUI.Label(new Rect(Screen.width / 2 + 30, Screen.height / 2 - 10, 2000, 1000), "PLAYER TWO WINS");

            // ...dan kembalikan bola ke tengah.
            ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            for (int i = 0; i < jumlahFireBall; i++)
            {
                Destroy(GameObject.Find("FireBall" + i));
            }
            for (int i = 0; i < jumlahPowerUp; i++)
            {
                Destroy(GameObject.Find("PowerUp" + i));
            }
        }
    }
}
