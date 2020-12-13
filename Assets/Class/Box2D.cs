using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Class
{
    public class Box2D
    {
        public Vector2 a, b;

        public Box2D(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }
        public Box2D(Box2D b)
        {
            this.a = b.a;
            this.b = b.b;
        }

        public Box2D(float x,float y)
        {
            this.a = new Vector2(-x / 2f, -y / 2f);
            this.b = new Vector2(x / 2f, y / 2f);
        }

        bool inside(Vector2 p)
        {
            return !(p.x < a.x || p.x > b.x || p.y < a.y || p.y > b.y);
        }

        bool intersect(Box2D b2)
        {
            return !(a.x >= b2.b.x) || (a.y >= b2.b.y) || (b.x <= b2.a.x) || (b.y <= b2.a.y);
        }
    }
}
