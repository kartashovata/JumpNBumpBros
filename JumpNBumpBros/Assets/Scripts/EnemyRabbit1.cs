using UnityEngine;
using System.Collections;

public class EnemyRabbit1 : MonoBehaviour {
	[Range(1f,10F)]
	public float moveSpeed = 4f;  // enemy move speed when moving
	public int damageAmount = 10; // probably deal a lot of damage to kill player immediately
	public int enemyValue = 1;

	[Tooltip("Child gameObject for detecting stun")]
	public GameObject stunnedCheck; // what gameobject is the stunnedCheck
		
	public GameObject[] myWaypoints; // to define the movement waypoints
	
	public float waitAtWaypointTime = 1f;   // how long to wait at a waypoint
	
	public bool loopWaypoints = true; // should it loop through the waypoints
	
	// SFXs
	public AudioClip deathSFX;
	//animation for death
	public GameObject deathParticles;

	// private variables below
	
	// store references to components on the gameObject
	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;
	
	// movement tracking
	int _myWaypointIndex = 0; // used as index for My_Waypoints
	float _moveTime;
	[SerializeField]
	float _vx = 0f;
	[SerializeField]
	Vector3 localScale;
	[SerializeField]
	bool _moving = true;
	
	// store the layer number the enemy is on (setup in Awake)
	int _enemyLayer;

	void Awake() {
		// get a reference to the components we are going to be changing and store a reference for efficiency purposes
		_transform = GetComponent<Transform> ();
		
		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) // if Rigidbody is missing
			Debug.LogError("Rigidbody2D component missing from this gameobject");
		
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");
		
		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}

		if (stunnedCheck==null) {
			Debug.LogError("stunnedCheck child gameobject needs to be setup on the enemy");
		}
		
		// setup moving defaults
		_moveTime = 0f;
		_moving = true;
		
		// determine the enemies specified layer
		_enemyLayer = this.gameObject.layer;

	}
	
	// move the enemy when time is > _moveTime
	void Update () {
			if (Time.time >= _moveTime) {
				EnemyMovement();
			} else {
				_animator.SetBool("Moving", false);
			}
	}
	
	// Move the enemy through its rigidbody based on its waypoints
	void EnemyMovement() {
		// if there isn't anything in My_Waypoints
		if ((myWaypoints.Length != 0) && (_moving)) {
			
			// make sure the enemy is facing the waypoint (based on previous movement)
			Flip (_vx);
			
			// determine distance between waypoint and enemy
			_vx = myWaypoints[_myWaypointIndex].transform.position.x-_transform.position.x;
			
			// if the enemy is close enough to waypoint, make it's new target the next waypoint
			if (Mathf.Abs(_vx) <= 0.05f) {
				// At waypoint so stop moving
				_rigidbody.velocity = new Vector2(0, 0);
				
				// increment to next index in array
				_myWaypointIndex++;
				
				// reset waypoint back to 0 for looping
				if(_myWaypointIndex >= myWaypoints.Length) {
					if (loopWaypoints)
						_myWaypointIndex = 0;
					else
						_moving = false;
				}
				
				// setup wait time at current waypoint
				_moveTime = Time.time + waitAtWaypointTime;
			} else {
				// enemy is moving
				_animator.SetBool("Moving", true);
				
				// Set the enemy's velocity to moveSpeed in the x direction.
				_rigidbody.velocity = new Vector2(_transform.localScale.x * moveSpeed, _rigidbody.velocity.y);
			}
			
		}
	}
	
	// flip the enemy to face torward the direction he is moving in
	void Flip(float _vx) {
		
		// get the current scale
		Vector3 localScale = _transform.localScale;

		if ((_vx>0f)&&(localScale.x<0f))
			localScale.x*=-1;
		else if ((_vx<0f)&&(localScale.x>0f))
			localScale.x*=-1;

		// update the scale
		_transform.localScale = localScale;
	}
	
	// Attack player
	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player") 
		{
			CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
			if (player.playerCanMove) {
				// Make sure the enemy is facing the player on attack
				Flip(collision.transform.position.x-_transform.position.x);

				// stop moving
				_rigidbody.velocity = new Vector2(0, 0);
				
				// apply damage to the player
				player.ApplyDamage (damageAmount);
				
				// stop to enjoy killing the player
				_moveTime = Time.time + waitAtWaypointTime;
			}
		}
	}
	
	// if the Enemy collides with a MovingPlatform, then make it a child of that platform
	// so it will go for a ride on the MovingPlatform
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag=="MovingPlatform")
		{
			this.transform.parent = other.transform;
		}
	}
	
	// if the enemy exits a collision with a moving platform, then unchild it
	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.tag=="MovingPlatform")
		{
			this.transform.parent = null;
		}
	}
	
	// play sound through the audiosource on the gameobject
	void playSound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}
	
	// setup the enemy to be stunned
	public void KillEnemy()
	{
		// provide the player with feedback that enemy is dead
		playSound(deathSFX);
		// play the death animation
		_animator.SetTrigger("Death");

		// if explosion prefab is provide, then instantiate it
		if (deathParticles)
		{
			Instantiate(deathParticles,transform.position,transform.rotation);
		}
			
		// destroy the victory gameobject
		DestroyObject(this.gameObject);

			

	}
	

}
