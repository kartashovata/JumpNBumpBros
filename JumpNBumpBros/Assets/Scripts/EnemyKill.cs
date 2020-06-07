using UnityEngine;
using System.Collections;

public class EnemyKill : MonoBehaviour {

	// if Player hits the stun point of the enemy, then call Stunned on the enemy
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			// tell the enemy to be stunned
			this.GetComponentInParent<EnemyRabbit1>().KillEnemy();

			//add score
			other.gameObject.GetComponent<CharacterController2D>().CollectCoin(this.GetComponentInParent<EnemyRabbit1>().enemyValue);

			//make player bounce
			other.gameObject.GetComponent<CharacterController2D>().EnemyBounce();
		}
	}
}
