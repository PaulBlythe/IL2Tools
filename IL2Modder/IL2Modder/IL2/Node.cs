using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Skill.FbxSDK;

namespace IL2Modder.IL2
{
    public enum NodeType
    {
        Root,
        Holder,
        Mesh,
        CollisionObject,
        NodeTypes
    };
    public class Node
    {
        public String Name;
        public String Parent;
        public NodeType Type;
        public bool Hidden;
        public bool Seperable;
        public List<Node> children = new List<Node>();
        public List<CollisionNode> Colliders = new List<CollisionNode>();
        public Matrix world;
        public Matrix base_matrix;
        public Matrix previous_matrix;
        public FbxNode fbx_node = null;
        public int Damage;
        public bool originalHidden = false;

        public Node(String data)
        {
            Name = data.TrimStart('[');
            Name = Name.TrimEnd(']');
            Hidden = false;
            Seperable = false;
            originalHidden = false;
            world = Matrix.Identity;
            base_matrix = Matrix.Identity;
            Damage = 0;
        }
        
        public Node()
        {
            Hidden = false;
            Seperable = false;
            world = Matrix.Identity;
        }

        public static Node CopyNode(Node copy)
        {
            if (copy is MeshNode)
            {
                MeshNode mn = new MeshNode((MeshNode)copy);
                return mn;
            }
            Node n2 = new Node();         
            n2.Name = copy.Name;
            n2.Hidden = copy.Hidden;
            n2.Seperable = copy.Seperable;
            n2.world = Matrix.Identity * copy.world; 
            
            n2.Type = copy.Type;
            foreach (Node n in copy.children)
            {
                n2.children.Add(CopyNode(n));
            }
            foreach (CollisionNode b in copy.Colliders)
            {
                n2.Colliders.Add(b);
            }
            return n2;
        }

       
    }
}
