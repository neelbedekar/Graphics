using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.ShaderGraph
{
    static class CreateShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/Blank Shader Graph", priority = CoreUtils.Sections.k_Section1 +  CoreUtils.Priorities.k_AssetsCreateShaderMenuPriority)]
        public static void CreateBlankShaderGraph()
        {
            GraphUtil.CreateNewGraph();
        }
    }
}
