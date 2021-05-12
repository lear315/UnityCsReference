// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine.Assertions;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using System.Runtime.InteropServices;

namespace UnityEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    [UsedByNativeCode]
    [NativeHeader("Runtime/Graphics/ShaderScriptBindings.h")]
    [NativeHeader("Runtime/Shaders/Keywords/KeywordSpaceScriptBindings.h")]
    public readonly struct LocalKeyword
    {
        [FreeFunction("ShaderScripting::GetKeywordCount")] extern private static uint GetShaderKeywordCount(Shader shader);
        [FreeFunction("ShaderScripting::GetKeywordIndex")] extern private static uint GetShaderKeywordIndex(Shader shader, string keyword);
        [FreeFunction("ShaderScripting::GetKeywordCount")] extern private static uint GetComputeShaderKeywordCount(ComputeShader shader);
        [FreeFunction("ShaderScripting::GetKeywordIndex")] extern private static uint GetComputeShaderKeywordIndex(ComputeShader shader, string keyword);

        public string name { get { return m_Name; } }

        public LocalKeyword(Shader shader, string name)
        {
            m_SpaceInfo = shader.keywordSpace;
            m_Name = name;
            m_Index = GetShaderKeywordIndex(shader, name);
            if (m_Index >= GetShaderKeywordCount(shader))
                Debug.LogErrorFormat("Local keyword {0} doesn't exist in the shader.", name);
        }

        public LocalKeyword(ComputeShader shader, string name)
        {
            m_SpaceInfo = shader.keywordSpace;
            m_Name = name;
            m_Index = GetComputeShaderKeywordIndex(shader, name);
            if (m_Index >= GetComputeShaderKeywordCount(shader))
                Debug.LogErrorFormat("Local keyword {0} doesn't exist in the compute shader.", name);
        }

        internal readonly LocalKeywordSpace m_SpaceInfo;
        internal readonly string m_Name;
        internal readonly uint m_Index;
    }
}