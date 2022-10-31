using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateFromPixelMap : EditorWindow
{
    static EditorWindow ThisWindow;

    [MenuItem("Rubber Duck/Tool Windows/Create From Pixel Map")]
    public static void ShowWindow()
    {
        ThisWindow = GetWindow(typeof(CreateFromPixelMap));
    }
    
    ScriptableObject target;
    SerializedObject thisSerialised;

    public Texture2D mapImage;

    [System.Serializable]
    public struct Mappings
    {
        public GameObject spawnObject;
        public Color spawnColour;
    }
    public Mappings[] objectLegend;

    private Color currentPixelColour;
    public List<Color> tempColourList = new List<Color>();
    public GameObject parentObject;
    bool _useParentPosition;
    bool _zAxis;
    bool _pivotAtCentre;
    bool _lockLegend;


    public Node[][] finalMap;

    private void OnEnable()
    {
        target = this;
        thisSerialised = new SerializedObject(target);
    }

    private void OnGUI()
    {
        GUILayout.Label("Create a level from a pixel map");

        EditorGUI.BeginChangeCheck();
        mapImage = EditorGUILayout.ObjectField(new GUIContent("Map", "Select the pixel map to use as a base."), mapImage, typeof(Texture2D), false) as Texture2D;
        if (EditorGUI.EndChangeCheck())
        {
            if (mapImage != null)
            {
                if (!_lockLegend)
                {
                    ScanForColours();
                }
            }
            else
            {
                objectLegend = new Mappings[0];
                _lockLegend = false;
            }
            thisSerialised = new SerializedObject(target);
        }

        SerializedProperty mappingsProperty = thisSerialised.FindProperty("objectLegend");

        #region If there is a map in use

        EditorGUI.BeginDisabledGroup(mapImage == null);

        EditorGUILayout.PropertyField(mappingsProperty, true);

        _lockLegend = EditorGUILayout.Toggle(new GUIContent("Lock Legend Entries", "Prevents the legend from being reset between different maps when ticked. Useful if you have many maps with identical colours/keys."), _lockLegend);

        GUILayout.Space(16);

        parentObject = EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true) as GameObject;
        if (parentObject != null && !parentObject.scene.IsValid())
        {
            Debug.LogError("ERROR: Parent must be present in the scene");
            parentObject = null;
        }

        #region If there is a parent object

        EditorGUI.BeginDisabledGroup(parentObject == null);
        _useParentPosition = EditorGUILayout.Toggle(new GUIContent("Use Parent Position", "Determine whether to use global or local position based on the parent object's transform."), _useParentPosition);
        if (parentObject == null)
        {
            _useParentPosition = false;
        }
        EditorGUI.EndDisabledGroup();
        #endregion

        _zAxis = EditorGUILayout.Toggle(new GUIContent("Use Z-Axis", "Places objects along the Z axis instead of the Y axis when ticked."), _zAxis);

        _pivotAtCentre = EditorGUILayout.Toggle(new GUIContent("Pivot At Centre", "Changes pivot point from bottom-left to centre when ticked."), _pivotAtCentre);

        if (GUILayout.Button("Generate"))
        {
            GenerateLevel();
        }
        EditorGUI.EndDisabledGroup();
        #endregion

        thisSerialised.ApplyModifiedProperties();
    }

    void GenerateObject(int x, int y)
    {
        //read the pixel colour
        currentPixelColour = mapImage.GetPixel(x, y);
        if (currentPixelColour.a == 0)
        {
            //if no colour, do nothing
            Debug.Log("Skipped empty pixel");
            return;
        }

        foreach (Mappings map in objectLegend)
        {
            if (map.spawnColour == currentPixelColour && map.spawnObject != null)
            {
                Debug.Log("Colour Match Found");
                Vector3 pos = new Vector3(
                    x - (_pivotAtCentre ? mapImage.width / 2f : 0), //set X based on the x coordinate and adjust if pivoting from centre
                    _zAxis ? 0 : y - (_pivotAtCentre ? mapImage.height / 2f : 0), //set Y based on whether we use the Z axis, and if not, adjust if pivoting from centre
                    _zAxis ? y - (_pivotAtCentre ? mapImage.height / 2f : 0) : 0) //set Z based on whether we use the Z axis, and if so, adjust if pivoting from centre
                    + (_useParentPosition ? parentObject.transform.position : Vector3.zero); //add the parent's transform position to the final result to account for their location

                GameObject obj = PrefabUtility.InstantiatePrefab(map.spawnObject, parentObject != null ? parentObject.transform : null) as GameObject; //Instantiate(map.spawnObject, pos, Quaternion.identity, parentObject != null ? parentObject.transform : null);
                obj.transform.position = pos;

                if (obj.TryGetComponent<Node>(out Node n))
                    finalMap[x][y] = n;
                else
                    finalMap[x][y] = null;
            }
        }
    }

    void GenerateLevel()
    {
        //scan whole texture and get pixel positions
        for (int x = 0; x < mapImage.width; x++)
        {
            for (int y = 0; y < mapImage.height; y++)
            {
                GenerateObject(x, y);
            }
        }
        LinkNodes();
    }

    void LinkNodes()
    {
        for (int x = 0; x < mapImage.width; x++)
        {
            for (int y = 0; y < mapImage.height; y++)
            {
                CheckNeighbours(x, y);
            }
        }
    }

    void CheckNeighbours(int x, int y)
    {
        if (finalMap[x][y] != null)
        {
            for (int xOff = -1; xOff < 2; xOff+=2)
            {
                if (x + xOff < mapImage.width && x + xOff >= 0)
                {
                    if (finalMap[x + xOff][y] != null)
                    finalMap[x][y].AddNeighbourNode(finalMap[x + xOff][y]);
                }
            }

            for (int yOff = -1; yOff < 2; yOff+=2)
            {
                if (y + yOff < mapImage.height && y + yOff >= 0)
                {
                    if (finalMap[x][y+yOff] != null)
                        finalMap[x][y].AddNeighbourNode(finalMap[x][y + yOff]);
                }
            }
        }
    }

    void ScanForColours()
    {
        tempColourList.Clear();

        finalMap = new Node[mapImage.width][];
        for (int i = 0; i < finalMap.Length; i++)
        {
            finalMap[i] = new Node[mapImage.height];
        }

        //scan whole texture and get pixel positions
        for (int x = 0; x < mapImage.width; x++)
        {
            for (int y = 0; y < mapImage.height; y++)
            {
                SetColourFromPixel(x, y);
            }
        }

        objectLegend = new Mappings[tempColourList.Count];

        for (int i = 0; i < tempColourList.Count; i++)
        {
            objectLegend[i].spawnColour = tempColourList[i];
        }
    }

    void SetColourFromPixel(int x, int y)
    {
        //read the pixel colour
        currentPixelColour = mapImage.GetPixel(x, y);
        if (currentPixelColour.a == 0)
        {
            //if no colour, do nothing
            Debug.Log("Skipped empty pixel");
            return;
        }
        if (!tempColourList.Contains(currentPixelColour))
        {
            Debug.Log("Added a color");

            tempColourList.Add(currentPixelColour);
        }
    }
}