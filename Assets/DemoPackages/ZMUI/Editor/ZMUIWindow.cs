
# if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;

public class ZMUIWindow : OdinMenuEditorWindow
{
 

    [SerializeField]
    public ZMUIWindow uiSettingWindow ;

    [MenuItem("ZMFrame/ZMUI Setting",false,2)]
    public static void ShowAssetBundleWindow()
    {
        ZMUIWindow window = GetWindow<ZMUIWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(985,612);
        window.ForceMenuTreeRebuild();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
 
        OdinMenuTree menuTree = new OdinMenuTree(supportsMultiSelect: false)
        {
            { "ZMUI Setting",UISetting.Instance,EditorIcons.SettingsCog},
        };
        return menuTree;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UISetting.Instance.Save();
    }
}
#endif
