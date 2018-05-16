
using UnityEngine;
using UnityEngine.UI;
using Obj;
using System.Globalization;
using System.Collections.Generic;

public class ObjParserExample : MonoBehaviour {

    public InputField pathInputField;
    public InputField scaleInputField;
    public Text status;

    private List<GameObject> loaded = new List<GameObject>();

    async public void Load()
    {
        status.text = "Loading new model...";
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        string path = pathInputField.text;
        float scale = float.Parse(scaleInputField.text, CultureInfo.InvariantCulture);

        // This line is all you need to load a model from file. Synchronous loading is also available with ObjParser.Parse()
        var model = await ObjParser.ParseAsync(path, scale);

        stopwatch.Stop();
        status.text = $"Model loaded in {stopwatch.Elapsed}";

        if (model != null)
        {
            loaded.Add(model);
            var combinedBounds = BoundsUtils.CalculateCombinedBounds(model);
            Camera.main.transform.position = combinedBounds.center + Vector3.back * combinedBounds.size.magnitude;
        }
    }

    public void Clear()
    {
        foreach (var model in loaded)
        {
            Destroy(model);
        }

        loaded.Clear();
    }

    public void Quit()
    {
        Application.Quit();
    }
}