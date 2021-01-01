using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Agent : MonoBehaviour
    {

        private AgentManager _agentManager;
        private Vector3 _velocity, _position;
        private Vector3 _goalPosition;
        private Vector3 _leaderPosition;
        private Vector3 _currentForce;
        Vector3 difference;


        Vector3 _avoidance = new Vector3(0, 0, 0);
        Vector3 SeekingLead = new Vector3(0,0,0);
        Vector3 SeekingGoal = new Vector3(0, 0, 0);

        // Use this for initialization
        void Start ()
        {
            _velocity = new Vector3(Random.Range(0.01f, 0.1f), Random.Range(0.01f, 0.1f), 0);
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(_avoidance);
            Vector3 ahead = _position + _velocity.normalized;

            RaycastHit2D hit = Physics2D.Raycast(_position, ahead, 10);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Fence"))
                {
                    _avoidance = ahead - hit.collider.transform.position;
                    _avoidance = _avoidance.normalized * 7;
                }
            }
            else
            {
                _avoidance *= 0;
            }

            _goalPosition = _agentManager.Target.transform.position;
            _leaderPosition = _agentManager.Crowd[_agentManager.leader].transform.position;

            SeekingLead = _leaderPosition;
            SeekingGoal = _goalPosition;

            if (_agentManager.SeekLeader)
            {
               
               difference = CalcDiff(SeekingLead);
            }
            else
            {
                difference = CalcDiff(SeekingGoal);
            }
          
            float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
            Flock();
        }

        Vector3 CalcDiff(Vector3 pos)
        {
            Vector3 difference = pos - transform.position;

            return difference;
        }

            /// <summary>
            ///Boid behaviour
            /// </summary>
            void Flock()
        {
            _velocity = GetComponent<Rigidbody2D>().velocity;
            _position = this.transform.position;

            //_agentManager.LeaderAgent;

            //Only use the flocking behaviour if the flocking bool is true
            if (_agentManager.Flocking)
            {
                //If seak goal is enabled try and find that goal
                if (_agentManager.SeekGoal)
                {
                    var target = Seek(_goalPosition);
                    _currentForce = target + Align() + Cohesion() + Seperation() + _avoidance;
                }
                else if(_agentManager.SeekLeader)
                {
                    foreach (var agent in _agentManager.Crowd)
                    {
                        if (agent.transform.position == _leaderPosition)
                        {
                            var target1 = Seek(_goalPosition);
                           ApplyForce((target1.normalized + _avoidance)+ (Align() * _agentManager.alignWeigh) + (Cohesion() * _agentManager.cohesionWeight) + (Seperation() * _agentManager.seperationWeight) );
                           
                        }
                        else
                        {
                            var target = Seek(_leaderPosition);

                            _currentForce = (target.normalized + _avoidance) + Align().normalized + Cohesion().normalized + Seperation().normalized;
                        }

                    }     
                }
                //Else just flock with neighbors
                else
                {
                  _currentForce = Align() + Cohesion() + Seperation() + _avoidance;
                }
                _currentForce = _currentForce.normalized;
            }
            else
            {
                //If no floking is enabled just go to the goal with no regard of other boids
                if (_agentManager.SeekGoal)
                {
                    var target = Seek(_goalPosition);
                    _currentForce = target + _avoidance;
                }
                _currentForce = _currentForce.normalized;
            }

            //Random behaviour - once every so often the boid will go a different way
            if (Random.Range(0, 25) <= 1 && _agentManager.Willful)
            {
                _currentForce = new Vector3(Random.Range(0.05f, 0.2f), Random.Range(0.05f, 0.2f), Random.Range(0.05f, 0.2f));
            }
            ApplyForce(_currentForce);
        }

        /// <summary>
        /// Returns a vector towards a target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Vector3 Seek(Vector3 target)
        {
            return target - transform.position;
        }

        /// <summary>
        /// Apply force to the agent and display a line showcasing it
        /// </summary>
        /// <param name="force"></param>
        void ApplyForce(Vector3 force)
        {
            Rigidbody2D rbody = GetComponent<Rigidbody2D>();

            // Caps the force using the max force variable from the manager
            if (force.magnitude > _agentManager.MaxForce)
            {
                force = force.normalized;
                force *= _agentManager.MaxForce;
            }
            
            rbody.AddForce(force);
            //Draw a line showcasing the current force
            Debug.DrawRay(transform.position, force, Color.white);

          


            // Caps the velocity using the max force variable from the manager
            if (rbody.velocity.magnitude > _agentManager.MaxVelocity)
            {
                rbody.velocity = rbody.velocity.normalized;
                rbody.velocity *= _agentManager.MaxVelocity;
            }
        }


        /// <summary>
        /// Average velocity
        /// </summary>
        /// <returns></returns>
        Vector3 Align()
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (var agent in _agentManager.Crowd)
            {
                //Don't include self in calculation
                if (agent == gameObject) continue;
                //if (agent.transform.position == _leaderPosition) continue;

                Agent other = agent.GetComponent<Agent>();
                float distance = Vector3.Distance(_position, other.GetPosition());
                //If the other agent is within distance then include their position for consideration when flocking
                if (!(distance < _agentManager.NeighbourDistance)) continue;
                sum += other.GetVelocity();
                count++;
            }

            // If there are no neighbors in the vicinity return a zero vector
            if (count <= 0) return Vector3.zero;

            // Get average velocity
            sum /= count;
           Vector3 steer = sum -_velocity;
            return steer;
        }

        /// <summary>
        /// Find the average in the crowds location 
        /// </summary>
        /// <returns></returns>
        Vector3 Cohesion()
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (var agent in _agentManager.Crowd)
            {
                //Don't include self in calculation
                if (agent == gameObject) continue;

               
                Agent other = agent.GetComponent<Agent>();
                float distance = Vector3.Distance(_position, other.GetPosition());
                //If the other agent is within distance then include their position for consideration when flocking
                if (!(distance < _agentManager.NeighbourDistance)) continue;
                sum += other.GetPosition();
                count++;
            }

            // If there are no neighbors in the vicinity return a zero vector
            if (count <= 0) return Vector3.zero;

            // Get average velocity
            sum /= count;
            return Seek(sum);
        }

        Vector3 Seperation()
        {
            Vector3 sum = Vector3.zero;
            int count = 0;

            foreach(var agent in _agentManager.Crowd)
            {
                if (agent == gameObject) continue;

                Agent other = agent.GetComponent<Agent>();
                float distance = Vector3.Distance(_position, other.GetPosition());

                if(distance < _agentManager.NeighbourDistance)
                {
                    Vector3 dVector = gameObject.GetComponent<Agent>().GetPosition() - other.GetPosition();
                    
                    if(dVector.magnitude > 0)
                    {
                        sum += dVector.normalized / dVector.magnitude;
                    }
                    count++;
                }
            }

            if (count <= 0) return Vector3.zero;
            
            return sum;
        }

        #region Gets & Sets
        /// <summary>
        /// Get a reference to it's manager
        /// </summary>
        /// <param name="agentManager"></param>
        public void SetManager(AgentManager agentManager)
        {
            _agentManager = agentManager;
        }

        public Vector3 GetVelocity()
        {
            return _velocity;
        }

        public Vector3 GetPosition()
        {
            return _position;
        }

        #endregion
    }
}
