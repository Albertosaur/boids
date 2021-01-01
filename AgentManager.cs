using UnityEngine;

namespace Assets.Scripts
{
    public class AgentManager : MonoBehaviour
    {

        public GameObject[] Crowd;
        public GameObject AgentPrefab;
        public GameObject LeaderAgent;
        public GameObject[] targets;
        public int CrowdSize = 5;
        public Vector3 Range = new Vector3(5,5,5);
        public GameObject Target;
        [Range(0, 200)] public int NeighbourDistance = 50;
        [Range(0, 2)] public float MaxForce = 0.5f;
        [Range(0, 5)] public float MaxVelocity = 2.0f;

        public bool SeekGoal = false;
        public bool SeekLeader = true;
        public bool Flocking = true;
        public bool Willful = false;
        public int leader = 0;
        public int cohesionWeight = 1;
        public int seperationWeight = 1;
        public int alignWeigh = 1;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(this.transform.position, Range * 2);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(this.transform.position, 0.2f);
        }


        // Use this for initialization
        void Start ()
        {
            leader = Random.Range(0, CrowdSize);
            Crowd = new GameObject[CrowdSize];
            for (int i = 0; i < CrowdSize; i++)
            {
                if (leader == i)
                {
                    //assign leader
                    Vector3 leaderPosition = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), Random.Range(-Range.z, Range.z));
                    Crowd[i] = Instantiate(LeaderAgent, this.transform.position + leaderPosition, Quaternion.identity) as GameObject;
                    Crowd[i].GetComponent<Agent>().SetManager(this);
                    Crowd[i].transform.SetParent(transform);
                }
                else
                {
                    Vector3 agentPosition = new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), Random.Range(-Range.z, Range.z));
                    Crowd[i] = Instantiate(AgentPrefab, this.transform.position + agentPosition, Quaternion.identity) as GameObject;
                    Crowd[i].GetComponent<Agent>().SetManager(this);
                    Crowd[i].transform.SetParent(transform);
                }
            }
        }
	
        // Update is called once per frame
        void Update ()
        {
		    //if the leader dies set a new leader
        }
    }
}
