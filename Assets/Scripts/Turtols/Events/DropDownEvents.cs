using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class DropDownEvents : MonoBehaviour
{
    Dropdown dropdownItem;

    private void Start()
    {
        dropdownItem = GetComponent<Dropdown>();
    }

    public void InitialiseDropDown()
    {
        dropdownItem.options.Clear();
        Dropdown.OptionData data;

        if (Directory.Exists(Application.streamingAssetsPath))
        {
            DirectoryInfo direction = new DirectoryInfo(Application.streamingAssetsPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            data = new Dropdown.OptionData();
            data.text = "select";
            dropdownItem.options.Add(data);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }

                data = new Dropdown.OptionData();
                data.text = files[i].Name;
                dropdownItem.options.Add(data);
            }

            Debug.Log("initialise successfully");
        }
        else
            Debug.Log("the directory is not available!");
    }

    public void OnValueChanged()
    {
        Debug.Log(dropdownItem.captionText.text);
    }

}
