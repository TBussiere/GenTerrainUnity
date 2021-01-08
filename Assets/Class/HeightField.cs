using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Class
{
    class HeightField : SF2
    {
        public enum Filter
        {
            NONE,
            HEIGHT,
            //GRADIENT,
            LAPLACIAN,
            SLOPE,
            AV_SLOPE,
            STREAM_POWER,
            WETNESS_INDEX,
            ACCESS,
            ORRIANTATION,
            ROUTE
        }

        public Filter currentFilter = Filter.NONE;

        public Texture2D img;
        public float bdeepth;
        public float wdeepth;
        public float powRender;

        float minlap = -1;
        float maxlap = -1;

        bool StreamAreaUptoDate = false;
        SF2 StreamArea = null;
        bool SlopeMapUptoDate = false;
        SF2 SlopeMap = null;

        SF2 Road = null;



        int[,] kerint;//unuse
        float[,] kernel;
        int kernSize;
        bool needCalculateRoad = false;

        //img box2d double double
        public HeightField(Texture2D img, Box2D b, float bdeepth, float wdeepth) : base(b, img.width, img.height)
        {
            this.img = img;
            this.bdeepth = bdeepth;
            this.wdeepth = wdeepth;

            notifyChanges();


            CalculateKernel(3);
            Smooth();
            CalculateKernel(5);
            Smooth();

            needCalculateRoad = true;
        }

        public void notifyChanges()
        {
            for (int i = 0; i < img.width; i++)
            {
                for (int j = 0; j < img.height; j++)
                {
                    float c = img.GetPixel(i, j).grayscale;//0to1
                    //c += bdeepth / wdeepth;
                    //float r = map(c, (bdeepth / wdeepth), 1 + (bdeepth / wdeepth), (bdeepth / wdeepth), 1);

                    float r = map(c, 0, 1, bdeepth, wdeepth);


                    setAt(i, j, r);
                }
            }
            MinMaxLapla();
            StreamAreaUptoDate = false;
            SlopeMapUptoDate = false;
        }

        private Color32[] getVertexesColor()
        {
            Color32[] res = new Color32[(nx + 1) * (ny + 1)];
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    //res.Add(new Color32(1,1,1,1));
                    res[index(i, j)] = new Color32(1, 1, 1, 1);
                }
            }
            return res;
        }

        public Color GetColor(int i,int j)
        {
            switch (currentFilter)
            {
                case Filter.NONE:
                    return new Color(0.5f, 0.5f, 0.5f, 1f);
                case Filter.HEIGHT:
                    float res = map(Height(i, j), bdeepth, wdeepth, 0, 1);
                    return Shade(res);
                case Filter.LAPLACIAN:
                    float res3 = Laplacien(i, j);//map(Laplacien(i, j), 0, 10000, 0, 1);
                    res3 = Mathf.Pow(Mathf.Abs(res3), 0.33f);
                    res3 = Mathf.InverseLerp(minlap, maxlap, res3);//float res31 = map(res3, 0, 0.0001f, 0, 1);
                    return Shade(res3);
                case Filter.SLOPE:
                    float res4 = slope(i, j);
                    res4 = Mathf.InverseLerp(0, 5, res4);
                    return Shade(res4);
                case Filter.AV_SLOPE:
                    float res5 = averageSlope(i, j);
                    res5 = Mathf.InverseLerp(0, 5, res5);
                    return Shade(res5);
                case Filter.STREAM_POWER:
                    if (StreamArea == null || !StreamAreaUptoDate)
                    {
                        StreamAreaUptoDate = true;
                        StreamArea = DrainageMonoDirr();
                    }
                    float slopeij = slope(i, j);
                    float res6 = Mathf.Pow(StreamArea.at(i, j), 0.5f) * slopeij;
                    //float res6 = StreamArea.at(i, j);
                    res6 = Mathf.InverseLerp(1, 10, res6);
                    float resok = Mathf.Clamp01(res6);
                    return Shade(resok);
                case Filter.WETNESS_INDEX:
                    if (StreamArea == null || !StreamAreaUptoDate)
                    {
                        StreamAreaUptoDate = true;
                        StreamArea = DrainageMonoDirr();
                    }
                    float slopeij2 = slope(i, j);
                    float res7 = Mathf.Log(StreamArea.at(i, j)) / (slopeij2 + 0.05f);
                    res7 = Mathf.InverseLerp(1, 30, res7);
                    float resokok = Mathf.Clamp01(res7);
                    return Shade(resokok);
                    break;
                case Filter.ACCESS:
                    break;
                case Filter.ORRIANTATION:
                    Vector3 norm = Normal(i, j);
                    return new Color(norm.x, norm.y, norm.z);
                case Filter.ROUTE:
                    if (needCalculateRoad)
                    {
                        SlopeMap = getSlopeMap();
                        Road = CalculateRoad();
                        needCalculateRoad = false;
                        Debug.Log("max :" + Road.max);
                    }
                    if (Road.at(i,j)>0)
                    {
                        return Shade(Mathf.InverseLerp(Road.min,Road.max,Road.at(i,j)));
                    }
                    else
                    {
                        return new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            return new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        internal void Smooth()
        {
            int offset = (kernSize - 1) / 2;
            for (int i = offset; i < nx - offset; i++)
            {
                for (int j = offset; j < ny - offset; j++)
                {
                    float val = 0f;
                    for (int k = -offset; k < offset; k++)
                    {
                        for (int l = -offset; l < offset; l++)
                        {
                            val += at(i + k, j + l) * kernel[k + offset, l + offset];
                        }
                    }
                    setAt(i, j, val);
                }
            }
        }

        internal void CalculateKernel(int kernSize)
        {
            if (kernel != null && this.kernSize == kernSize)
            {
                return;
            }
            kernel = new float[kernSize, kernSize];
            //kerint = new int[kernSize, kernSize];
            this.kernSize = kernSize;


            float sigma = 1f;
            int offset = (kernSize - 1) / 2;
            float s = 2f * sigma * sigma;
            float sum = 0f;
            for (int i = -offset; i < offset; i++)
            {
                for (int j = -offset / 2; j < offset; j++)
                {
                    float r = Mathf.Sqrt(i * i + j * j);
                    kernel[i + offset,j + offset] = (Mathf.Exp(-(r * r) / s)) / (Mathf.PI * s);
                    sum += kernel[i + offset, j + offset];
                }
            }

            for (int i = 0; i < kernSize; i++)
            {
                for (int j = 0; j < kernSize; j++)
                {
                    kernel[i, j] /= sum;

                    //kerint[i, j] = Mathf.FloorToInt(kernel[i, j] * 100);
                }
            }

        }

        internal Vector3[] GetVertexes()
        {
            Vector3[] res = new Vector3[(nx + 1) * (ny + 1)];
            int ind = 0;
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    //res.Add(Vertex(i, j));
                    res[ind] = Vertex(i, j);
                    ind++;
                }
            }
            return res;
        }

        private float map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public float Height(int i,int j)
        {
            return at(i,j);
        }
        public float[,] Heights()
        {
            float[,] heights = new float[nx,ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    float h = Height(i, j);
                    heights[i, j] = (float)h;
                }
            }
            return heights;

        }
        float slope(int i,int j)
        {
            Vector2 g = Gradient(i, j);
            return (float)Math.Sqrt(Vector2.Dot(g,g));
        }
        float averageSlope(int x, int y)//moy des pentes dans les 8 dir 
        {
            
            float res = 0;
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    if (x + i <= 0 || x + i >= nx - 1 || y + j <= 0 || y + j >= ny - 1)
                    {
                        res += 0;
                    }
                    else
                    {
                        res += slope(x + i, y + j);
                    }
                }
            }
            return res/9f;
        }

        Vector3 Vertex(int i,int j)
        {
            Vector2 baseVec = base.Vertex(i, j);
            return new Vector3(baseVec.x, 0, baseVec.y);//Height(i, j)
        }

        Vector3 Normal(int i,int j)
        {
            Vector2 baseVec = base.Gradient(i, j);
            return new Vector3(-baseVec.x,-baseVec.y,1f).normalized;
        }

        Color Shade(float t) 
        {
            if (t<=1)
            {
                return new Color(t, 0, 1f - t);
            }
            return new Color32(0, 0, 0, 120);// BUG PAS NORM
        }

        SF2 DrainageMonoDirr()
        {
            SF2 res = new SF2(this,1);

            List<SP2> fieldcopy = getFieldCopy();
            fieldcopy.Sort((x,y) => y.CompareTo(x));

            for (int i = 0; i < fieldcopy.Count(); i++)
            {

                ResultCheckSlope result = CheckSlope(fieldcopy[i]);

                if (result.nb > 0)
                {
                    float sp = res.at(fieldcopy[i].pos.x, fieldcopy[i].pos.y);
                    for (int ind = 0; ind < result.nb; ind++)
                    {
                        float curr = res.at(result.q[ind].pos.x, result.q[ind].pos.y);
                        float set = curr + (sp * result.avSlope[ind]);
                        set = Mathf.Clamp(set, 0, 1000);
                        res.setAt(result.q[ind].pos.x, result.q[ind].pos.y, set);
                        if (res.min > set) res.min = set;
                        if (res.max < set) res.max = set;
                    }
                }
            }

            return res;
        }

        SF2 Access()//tp HF c'est dure
        {
            return new SF2(new Box2D(a, b), nx, ny);
        }


        void MinMaxLapla()
        {
            minlap = Laplacien(0, 0);
            maxlap = minlap;
            for (int indi = 0; indi < img.width; indi++)
            {
                for (int indj = 0; indj < img.height; indj++)
                {
                    float lap = Laplacien(indi, indj);
                    lap = Mathf.Pow(Mathf.Abs(lap), 0.33f);
                    if (lap > maxlap) maxlap = lap;
                    if (lap < minlap) minlap = lap;
                }
            }
        }

        private ResultCheckSlope CheckSlope(SP2 p)//, List<SP2> listSP2
        {
            ResultCheckSlope res = new ResultCheckSlope();

            float slopesum = 0f;
            float zp = p.val;

            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (p.pos.x + i <= 0 || p.pos.x + i >= nx - 1 || p.pos.y + j <= 0 || p.pos.y + j >= ny - 1) continue;
                    float step = this.at(p.pos.x + i, p.pos.y + j) - zp;
                    if (step < 0f)
                    {
                        if ((i + j)%2 != 0)
                            res.slope[res.nb] = -step;
                        else
                            res.slope[res.nb] = -step / Mathf.Sqrt(2);

                        slopesum += res.slope[res.nb];
                        res.q[res.nb] = new SP2(this.at(p.pos.x + i, p.pos.y + j), new Vector2Int(p.pos.x + i, p.pos.y + j));
                        res.nb++;
                    }
                }
            }

            for (int i = 0; i < res.nb; i++)
            {
                res.avSlope[i] = res.slope[i] / slopesum;
            }

            return res;
        }

        internal class ResultCheckSlope
        {
            internal SP2[] q = new SP2[8];
            internal float[] avSlope = new float[8];
            internal float[] slope = new float[8];
            internal int nb = 0;
        }

        public SF2 getSlopeMap()
        {
            SF2 res = new SF2(this, 0);

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    float slopeij = slope(i, j);
                    res.setAt(i, j, slopeij);
                    if (res.min > slopeij) res.min = slopeij;
                    if (res.max < slopeij) res.max = slopeij;
                }
            }

            SlopeMapUptoDate = true;
            return res;
        }

        public SF2 CalculateRoad()
        {
            if (!needCalculateRoad)
            {
                return this.Road;
            }
            needCalculateRoad = false;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            SF2 res = new SF2(this, 0);

            Graph graph = new Graph(nx * ny);

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    float slopeij = SlopeMap.at(i, j);
                    for (int k = -1; k < 2; k++)
                    {
                        for (int l = -1; l < 2; l++)
                        {
                            int ind1 = index(i, j);
                            int ind2 = index(i + k, j + l);

                            if (k == 0 && l == 0) continue;
                            if ((i + k) <= 0 || (i + k) >= nx - 1 || (j + l) <= 0 || (j + l) >= ny - 1)
                            {
                                continue;
                            }
                            //cost = 1.2^DeltaSlope * dist
                            float deltaSlope = slopeij - SlopeMap.at(i + k, j + l);
                            deltaSlope *= 8;
                            float cost;

                            float baseLength;
                            if ((l + k) % 2 != 0)
                                baseLength = 1f;
                            else
                                baseLength = Mathf.Sqrt(2);

                            cost = baseLength * Mathf.Pow(10f, deltaSlope);

                            bool isok = graph.AddEdge(ind1, ind2, cost);
                        }
                    }
                }
            }

            int startP = index(1000,10);
            int dest = index(10, 1000);
            var path = graph.FindPath(startP);



            //Debug.Log(path[dest].distance);

            List<(int index,float linkcost)> resultPath = Path(startP, dest);


            List<(int index, float linkcost)> Path(int start, int destination)
            {

                List<(int index, float linkcost)> pathlist = new List<(int index, float linkcost)>();
                pathlist.Add( (destination, (float)(path[destination].distance - path[path[destination].prev].distance) ) );
                for (int i = destination; i != start; i = path[i].prev)
                {
                    float linkcost = (float)(path[path[i].prev].distance - path[path[path[i].prev].prev].distance);
                    pathlist.Add( (path[i].prev, linkcost) );
                }
                return pathlist;
            }

            res.max = resultPath[0].linkcost;
            res.min = resultPath[0].linkcost;

            for (int i = 0; i < resultPath.Count; i++)
            {
                int rx;
                int ry;
                var revind = reverseIndex(resultPath[i].index); 
                rx = revind.x;
                ry = revind.y;
                res.setAt(rx,ry, resultPath[i].linkcost);

                if (res.max < resultPath[i].linkcost) res.max = resultPath[i].linkcost;
                if (res.min > resultPath[i].linkcost) res.min = resultPath[i].linkcost;
            }

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log(elapsedMs);

            return res;
        }





    }//class
}//namespace
