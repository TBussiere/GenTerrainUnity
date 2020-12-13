using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Class
{
    public class Grid2D : Box2D
    {
        public int nx, ny;
        public Vector2 diag;
        public Vector2 cellDiag;
        public Vector2 invCellDiag;


        public Grid2D(Box2D box,int nxmax,int nymax) : base(box)
        {
            this.nx = nxmax;
            this.ny = nymax;
            diag = b - a;
            Vector2 scale = new Vector2(1 / (float)(nx - 1), 1 / (float)(ny - 1));
            cellDiag = diag;
            cellDiag.Scale(scale);

            invCellDiag = new Vector2(1.0f / cellDiag.x, 1.0f / cellDiag.y);

        }
        public Grid2D(Grid2D grid) : base(grid.a,grid.b)
        {
            this.nx = grid.nx;
            this.ny = grid.ny;
            diag = b - a;
            Vector2 scale = new Vector2(1 / (float)(nx - 1), 1 / (float)(ny - 1));
            cellDiag = diag;
            cellDiag.Scale(scale);

            invCellDiag = new Vector2(1 / cellDiag.x, 1 / cellDiag.y);

        }
        public bool inside(int x,int y)
        {
            return ((x >= 0) && (x < nx) && (y >= 0) && (y < ny));
        }
        public bool border(int x,int y)
        {
            return ((x == 0) || (x == nx-1) || (y == 0) || (y == ny-1));
        }

        public int index(int x, int y)
        {
            return x * nx + y ;
        }


        public Vector2 Vertex(int i,int j)
        {
            float u = i / (nx - 1f);
            float v = j / (ny - 1f);
            float x = (1f - u) * a.x + u * b.x;
            float y = (1f - v) * a.y + v * b.y;
            Vector2 temp = new Vector2(x, y);
            return temp;
        }
    }
}
