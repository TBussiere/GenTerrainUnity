using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createMesh : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Color[] color;

    public int xsize = 20;
    public int ysize = 20;



    // Start is called before the first frame update
    void create()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        vertices = new Vector3[(xsize + 1) * (ysize + 1)];
        triangles = new int[xsize * ysize * 6];
        color = new Color[(xsize + 1) * (ysize + 1)];
        int ind = 0;
        for (int i = 0; i <= ysize; i++)
        {
            for (int j = 0; j <= xsize; j++)
            {
                float h = 0;
                //float h = Mathf.PerlinNoise(i * .3f, j * .3f) * 2 + Mathf.PerlinNoise(i * .3f, j * .3f) * 5 + Mathf.PerlinNoise(i * 0.01f, j * 0.01f) * 50;
                vertices[ind] = new Vector3(i, h, j);
                color[ind] = new Color(0.5f, 0.5f, 0.5f,1f);
                ind++;
            }
        }

        int vert = 0;
        int tri = 0;
        for (int x = 0; x < xsize; x++)
        {
            for (int y = 0; y < ysize; y++)
            {
                triangles[tri + 0] = vert;
                triangles[tri + 1] = vert + 1;
                triangles[tri + 2] = vert + xsize + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + xsize + 2;
                triangles[tri + 5] = vert + xsize + 1;

                vert++;
                tri += 6;
            }
            vert++;
        }
    }

    public void createBaseTest()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        vertices = new Vector3[(xsize + 1) * (ysize + 1)];
        triangles = new int[xsize * ysize * 6];
        int ind = 0;
        for (int j = 0; j <= ysize; j++)
        {
            for (int i = 0; i <= xsize; i++)
            {
                vertices[ind] = new Vector3(i, 0, j);
                ind++;
            }
        }

        int vert = 0;
        int tri = 0;
        for (int y = 0; y < ysize; y++)
        {
            for (int x = 0; x < xsize; x++)
            {
                triangles[tri + 0] = vert;
                triangles[tri + 1] = vert + xsize + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + xsize + 1;
                triangles[tri + 5] = vert + xsize + 2;

                vert++;
                tri += 6;
            }
            vert++;
        }
    }

    public void CreateMeshFromsize(int xsize,int ysize)
    {
        this.xsize = xsize;
        this.ysize = ysize;

        create();
    }

    public void setColor(int i, int j, Color c)
    {
        int antibug = 1;
        color[i + (xsize + antibug) * j] = c;
    }

    public void setHeight(int i, int j, float hauteur)
    {
        int antibug = 1;
        Vector3 vert = vertices[i + j * (xsize + antibug)];
        //Debug.Log(i + j * xsize);
        vertices[i + (xsize + antibug) * j] = new Vector3(vert.x, hauteur, vert.z);
    }


    public void setHeightNoise(int i,int j)
    {
        float h = Mathf.PerlinNoise(i * .3f, j * .3f) * 2 + Mathf.PerlinNoise(i * .3f, j * .3f) * 5 + Mathf.PerlinNoise(i * 0.01f, j * 0.01f) * 50;
        Vector3 vert = vertices[i + j * xsize];
        vertices[i + j * xsize] = new Vector3(vert.x, h, vert.z);
    }
    //

    public void setHeights(float[,] hauteur)
    {
        int ind = 0;
        for (int i = 0; i < this.xsize; i++)
        {
            for (int j = 0; j < this.ysize; j++)
            {
                vertices[ind] = new Vector3(vertices[ind].y, hauteur[i,j], vertices[ind].z);
                ind++;
            }
        }
    }

    private void Update()
    {
        if (mesh != null)
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = color;

            mesh.RecalculateNormals();
        }
    }
}
