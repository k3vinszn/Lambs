using System;
using UnityEngine;
using System.Collections.Generic;

namespace Steer2D
{
    public class Flock3D : SteeringBehaviour3D
    {
        public float NeighbourRadius = 1f;
        public float AlignmentWeight = .7f;
        public float CohesionWeigth = .5f;
        public float SeperationWeight = .2f;
        public bool DrawGizmos = false;

        List<SteeringAgent3D> neighbouringAgents = new List<SteeringAgent3D>();
        Vector3 currentPosition;

        public override Vector3 GetVelocity()
        {
            currentPosition = transform.position;

            UpdateNeighbouringAgents();

            return alignment() * AlignmentWeight + cohesion() * CohesionWeigth + seperation() * SeperationWeight;
        }

        Vector3 alignment()
        {
            Vector3 averageDirection = Vector3.zero;

            if (neighbouringAgents.Count == 0)
                return averageDirection;

            foreach (var agent in neighbouringAgents)
                averageDirection += agent.CurrentVelocity;

            averageDirection /= neighbouringAgents.Count;
            return averageDirection.normalized;
        }

        Vector3 cohesion()
        {
            Vector3 averagePosition = Vector3.zero;

            foreach (var agent in neighbouringAgents)
                averagePosition += agent.transform.position;

            averagePosition /= neighbouringAgents.Count;

            return (averagePosition - currentPosition).normalized;
        }

        Vector3 seperation()
        {
            Vector3 moveDirection = Vector3.zero;

            foreach (var agent in neighbouringAgents)
                moveDirection += agent.transform.position - currentPosition;

            return (moveDirection * -1);
        }

        void UpdateNeighbouringAgents()
        {
            neighbouringAgents.Clear();

            foreach (var agent in SteeringAgent3D.AgentList)
            {
                if (Vector3.Distance(agent.transform.position, currentPosition) < NeighbourRadius)
                    neighbouringAgents.Add(agent);
            }
        }

        void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, NeighbourRadius);
            }
        }
    }
}
