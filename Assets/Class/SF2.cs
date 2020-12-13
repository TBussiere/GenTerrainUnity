using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Class
{
    class SF2 : Grid2D
    {
        ///TODO faire en sorte qu'on puise trier les SF2
        List<float> field;

        public float min;
        public float max;

        public SF2(Grid2D grid) : base(grid)
        {
            field = new List<float>(nx*ny);
            for (int i = 0; i < grid.nx; i++)
            {
                for (int j = 0; j < grid.ny; j++)
                {
                    field.Add(0f);
                }
            }
        }

        public SF2(Grid2D grid, float initValue) : base(grid)
        {
            field = new List<float>(nx * ny);
            for (int i = 0; i < grid.nx; i++)
            {
                for (int j = 0; j < grid.ny; j++)
                {
                    field.Add(initValue);
                }
            }
        }

        public SF2(Box2D box, int nx,int ny) : base(box,nx,ny)
        {
            field = new List<float>(nx * ny);
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    field.Add(0);
                }
            }
        }

        public float at(int x, int y)
        {
            return field[this.index(x, y)];
        }

        public void setAt(int x, int y, float v)
        {
            int ind = this.index(x, y);
            //
            field[ind] = v;
        }

        protected List<SP2> getFieldCopy()
        {
            List<SP2> res = new List<SP2>();
            for (int i = 0; i < field.Count(); i++)
            {
                int y = i % nx;
                int x = (i - y) / nx;
                res.Add(new SP2(field[i], new Vector2Int(x, y)));
            }

            return res;
        }

        public Vector2 Gradient(int i, int j) // df/dx,df/dy ~  (  (f(x+e,y)-f(x-e,y))/2e  , ... )
        {
            Vector2 n;
            // Gradient along x axis
            if (i == 0)
            {
                n.x = (at(i + 1, j) - at(i, j)) * invCellDiag.x;
            }
            else if (i == this.nx - 1)
            {
                n.x = (at(i, j) - at(i - 1, j)) * invCellDiag.x;
            }
            else
            {
                n.x = (at(i + 1, j) - at(i - 1, j)) * 0.5f * invCellDiag.x;
            }

            // Gradient along y axis
            if (j == 0)
            {
                n.y = (at(i, j + 1) - at(i, j)) * invCellDiag.y;
            }
            else if (j == ny - 1)
            {
                n.y = (at(i, j) - at(i, j - 1)) * invCellDiag.y;
            }
            else
            {
                n.y = (at(i, j + 1) - at(i, j - 1)) * 0.5f * invCellDiag.y;
            }
            return n;
        }

        public float Laplacien(int i, int j)
        {
            float laplacian = 0f;
            // Divergence along x axis
            if (i == 0)
            {
                laplacian += (at(i, j) - 2f * at(i + 1, j) + at(i + 2, j)) / (invCellDiag.x * invCellDiag.x);
            }
            else if (i == nx - 1)
            {
                laplacian += (at(i, j) - 2f * at(i - 1, j) + at(i - 2, j)) / (invCellDiag.x * invCellDiag.x);
            }
            else
            {
                laplacian += (at(i + 1, j) - 2f * at(i, j) + at(i - 1, j)) / (invCellDiag.x * invCellDiag.x);
            }
            // Divergence along y axis
            if (j == 0)
            {
                laplacian += (at(i, j) - 2f * at(i, j + 1) + at(i, j + 2)) / (invCellDiag.y * invCellDiag.y);
            }
            else if (j == ny - 1)
            {
                laplacian += (at(i, j) - 2f * at(i, j - 1) + at(i, j - 2)) / (invCellDiag.y * invCellDiag.y);
            }
            else
            {
                laplacian += (at(i, j + 1) - 2f * at(i, j) + at(i, j - 1)) / (invCellDiag.y * invCellDiag.y);
            }
            return laplacian;
        }
    }


    public class SP2 : IComparable
    {
        public float val;
        public Vector2Int pos;
        //public float valEcoulement = 1;
        public SP2(float val, Vector2Int pos)
        {
            this.val = val;
            this.pos = pos;
        }

        public int CompareTo(object obj)
        {
            SP2 sp2 = obj as SP2;

            if (this.pos.x == sp2.pos.x && this.pos.y == sp2.pos.y)
            {
                return 0;
            }

            return this.val.CompareTo(sp2.val);
        }
    }

}
