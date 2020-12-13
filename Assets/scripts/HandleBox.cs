using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Class;
using UnityEngine.UI;
using Min_Max_Slider;

using Filter = Assets.Class.HeightField.Filter;

public class HandleBox : MonoBehaviour
{
    public createMesh meshView;
    public Texture2D test;
    public float Hbas = 26;
    public float Hhaut = 324;

    //public Slider slider;
    public MinMaxSlider slider;
    public Dropdown dropdown;

    private HeightField hf;

    // Start is called before the first frame update
    void Start()
    {
        Filter[] tab = (Filter[])System.Enum.GetValues(typeof(Filter));

        dropdown.ClearOptions();
        List<Dropdown.OptionData> optionList = new List<Dropdown.OptionData>();
        foreach (var item in tab)
        {
            //string itemPrep = item.ToString().ToLowerInvariant().Replace('_',' ');
            optionList.Add(new Dropdown.OptionData(item.ToString()));
        }
        dropdown.AddOptions(optionList);

        slider.SetLimits(0, Hhaut * 2);
        slider.SetValues(Hbas, Hhaut);

        //slider.onValueChanged = ;

        //h saine (blackH) = 26
        //h tour eiffel whiteH = 324
        Box2D box = new Box2D(test.width, test.height);
        hf = new HeightField(test, box, Hbas, Hhaut);

        //meshView.CreateMeshFromsize(MAX_MESH_SIZE, MAX_MESH_SIZE);//255 max pour un mesh (yes merci les terrain qui marche mal)
        meshView.CreateMeshFromsize(test.width, test.height);


        //meshView.createBaseTest();        

        StartCoroutine("updateHeigth");


    }

    //IEnumerator pour step by step
    public void updateHeigth()
    {
        for (int j = 0; j < test.width; j++)
        {
            for (int i = 0; i < test.height; i++)
            {
                meshView.setHeight(i, j, hf.Height(i,j));
                //yield return new WaitForSeconds(0.0000001f); // pour le step by step
            }
        }
    }

    public void updateColor()
    {
        for (int j = 0; j < test.width; j++)
        {
            for (int i = 0; i < test.height; i++)
            {
                meshView.setColor(i, j, hf.GetColor(i,j) );
                //yield return new WaitForSeconds(1/100000000); // pour le step by step
            }
        }
    }

    private void Update()
    {
        
    }


    public void sliderChange(float a, float b)
    {
        if (hf != null)
        {
            hf.bdeepth = a;
            hf.wdeepth = b;

            hf.notifyChanges();
            StartCoroutine("updateHeigth");
        }
    }

    public void dropdownChange(int change)
    {
        if (hf != null)
        {
            
            Filter[] tab = (Filter[])System.Enum.GetValues(typeof(Filter));
            Debug.Log("change item to " + tab[change].ToString());
            hf.currentFilter = tab[change];

            StartCoroutine("updateColor");
        }
    }

    public void SmoothButton()
    {
        hf.CalculateKernel(5);
        hf.Smooth();

        StartCoroutine("updateHeigth");
        StartCoroutine("updateColor");
    }

    public void powText(string text)
    {
        text = text.Replace(".", ",");
        float pow;
        bool pass = float.TryParse(text, out pow);
        if (!pass)
        {
            Debug.Log("pass ici");
            return;
        }
        hf.powRender = pow;
        Debug.Log(pow);
    }
}


/*
        terrain.terrainData.heightmapResolution = test.width + 1;
        //Hhaut * (1+(Hbas / Hhaut))
        terrain.terrainData.size = new Vector3(test.width, 1000, test.height);
        float[,] heights = terrain.terrainData.GetHeights(0, 0, test.width, test.height);
        //heights[500, 500] = 1000f;
        //terrain.terrainData.SetHeights(0, 0, heights);

        heights = hf.Heights(heights);
        terrain.terrainData.SetHeights(0, 0, heights);

        //terrain.terrainData.SetAlphamaps()
*/


/*
 *  terrain.terrainData.heightmapResolution = test.width + 1;
        //Hhaut * (1+(Hbas / Hhaut))
        //terrain.terrainData.size = new Vector3(test.width, 1000, test.height);
        terrain.terrainData.size = new Vector3(8000, Hhaut * (1 + (Hbas / Hhaut)), 8000);
        float[,] heights = terrain.terrainData.GetHeights(0, 0, test.width, test.height);

        heights = hf.Heights(heights);
        terrain.terrainData.SetHeights(0, 0, heights);
 */
