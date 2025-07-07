using UnityEngine;

public class ArrowController : MonoBehaviour
{
       [Header("Assign in Inspector")]
       public Transform foldTarget;       // The fold line or fold point
       public float followSpeed = 5f;     // How fast to rotate toward the target
       public bool lookAtTarget = true;   // Toggle to control rotation
       public bool animateBounce = true;  // Toggle for bobbing animation
   
       [Header("Bounce Animation")]
       public float bounceHeight = 0.2f;
       public float bounceSpeed = 2f;
   
       private Vector3 initialPosition;
   
       void Start()
       {
           initialPosition = transform.position;
       }
   
       void Update()
       {
           if (foldTarget == null) return;
   
           // 1. Point toward the fold line
           if (lookAtTarget)
           {
               Vector3 direction = (foldTarget.position - transform.position).normalized;
               Quaternion lookRotation = Quaternion.LookRotation(direction);
               transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, followSpeed * Time.deltaTime);
           }
   
           // 2. Animate bounce
           if (animateBounce)
           {
               float newX = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
               transform.position = initialPosition + new Vector3(newX,0,0);
           }
       }
   }
