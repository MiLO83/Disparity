using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Dispair : MonoBehaviour
{
    public float separationPx;
    public List<string> images = new List<string>();
    public float adj = 1f;
    public Texture2D depth;
    public Texture2D color;
    public Texture2D frame_output;
    public Texture2D depth_output;
    public Color lastColor;
    public RawImage rawImage;
    public int imgNum = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (!Directory.Exists(Application.dataPath + "/../Input/"))
            Directory.CreateDirectory(Application.dataPath + "/../Input/");

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/../Input/");

        if (!Directory.Exists(Application.dataPath + "/../Output/"))
            Directory.CreateDirectory(Application.dataPath + "/../Output/");

        Debug.Log("Looking for Images in : " + di.FullName);

        foreach (var fi in di.GetFiles("*.png"))
        {
            if (!fi.Name.Contains("_d") && !fi.Name.Contains("_p"))
                if (File.Exists(Application.dataPath + "/../Input/" + fi.Name.Remove(fi.Name.Length - 4, 4) + "_d.png"))
                {
                    images.Add(fi.Name);
                    Debug.Log(Application.dataPath + "/../Input/" + fi.Name);
                }
            
        }
        
        rawImage.texture = frame_output;
        Application.targetFrameRate = 30;
        StartCoroutine(Run());
        InvokeRepeating("Wiggle", 4f, 0.5f);
    }

    // Update is called once per frame
    public void Wiggle()
    {
        if (rawImage.texture != frame_output)
        {
            rawImage.texture = frame_output;
        } else
        {
            rawImage.texture = color;
        }
    }
    public IEnumerator Run()
    {
        foreach (string img in images)
        {
            depth = new Texture2D(32, 32);
            color = new Texture2D(32, 32);
            byte[] depth_bytes = File.ReadAllBytes(Application.dataPath + "/../Input/" + img.Remove(img.Length - 4, 4) + "_d.png");
            depth.LoadImage(depth_bytes);
            byte[] color_bytes = File.ReadAllBytes(Application.dataPath + "/../Input/" + img);
            color.LoadImage(color_bytes);
            separationPx = 0.125f;
            frame_output = new Texture2D(color.width, color.height);
            depth_output = new Texture2D(color.width, color.height);
            depth_output.filterMode = FilterMode.Point;
            frame_output.SetPixels(color.GetPixels());
            for (int y = 0; y < depth_output.height; y++)
            {
                for (int x = 0; x < depth_output.width; x++)
                {
                    depth_output.SetPixel(x, y, depth.GetPixel((int)(x + ((((depth.GetPixel(x, y).r) * (depth.GetPixel(x, y).r * 255)) * separationPx))), (int)(y)));
                }
                
            }
            depth_output.Apply();
            
            for (int y = 0; y < frame_output.height; y++)
            {
                for (int x = 0; x < frame_output.width; x++)
                {
                    frame_output.SetPixel(x, y, color.GetPixel((int)(x + ((((depth_output.GetPixel(x, y).r) * (depth.GetPixel(x, y).r * 255)) * separationPx))), (int)(y)));
                }
                
                frame_output.Apply();
            }
            
            byte[] output = frame_output.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/../Output/" + img.Remove(img.Length - 4, 4) + ".reye.png", output);
            yield return new WaitForSeconds(8f);

            if (File.Exists(Application.dataPath + "/../Input/" + img.Remove(img.Length - 4, 4) + "_p.png")) {
                depth = new Texture2D(32, 32);
                color = new Texture2D(32, 32);
                depth_bytes = File.ReadAllBytes(Application.dataPath + "/../Input/" + img.Remove(img.Length - 4, 4) + "_d.png");
                depth.LoadImage(depth_bytes);
                color_bytes = File.ReadAllBytes(Application.dataPath + "/../Input/" + img.Remove(img.Length - 4, 4) + "_p.png");
                color.LoadImage(color_bytes);
                separationPx = 0.125f;
                frame_output = new Texture2D(color.width, color.height);
                depth_output = new Texture2D(color.width, color.height);
                depth_output.filterMode = FilterMode.Point;
                frame_output.SetPixels(color.GetPixels());
                for (int y = 0; y < depth_output.height; y++)
                {
                    for (int x = 0; x < depth_output.width; x++)
                    {
                        depth_output.SetPixel(x, y, depth.GetPixel((int)(x + ((((depth.GetPixel(x, y).r) * (depth.GetPixel(x, y).r * 255)) * separationPx))), (int)(y)));
                    }
                    
                }
                depth_output.Apply();

                for (int y = 0; y < frame_output.height; y++)
                {
                    for (int x = 0; x < frame_output.width; x++)
                    {
                        frame_output.SetPixel(x, y, color.GetPixel((int)(x + ((((depth_output.GetPixel(x, y).r) * (depth.GetPixel(x, y).r * 255)) * separationPx))), (int)(y)));
                    }
                    
                    frame_output.Apply();
                }

                output = frame_output.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + "/../Output/" + img.Remove(img.Length - 4, 4) + ".reye_p.png", output);
                yield return new WaitForSeconds(8f);
            }
        }
        Application.Quit();
    }
}
