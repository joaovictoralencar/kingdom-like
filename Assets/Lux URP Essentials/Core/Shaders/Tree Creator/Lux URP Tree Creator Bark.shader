Shader "Lux URP/Nature/Tree Creator Bark"
{
    Properties
    {

        _Color                      ("Main Color", Color) = (1,1,1,1)
        [PowerSlider(5.0)]
        _Shininess                  ("Shininess", Range (0.01, 1)) = 0.078125
        _MainTex                    ("Base (RGB) Alpha (A)", 2D) = "white" {}
        [NoScaleOffset]
        _BumpMap                    ("Normalmap", 2D) = "bump" {}
        [NoScaleOffset]
        _GlossMap                   ("Gloss (A)", 2D) = "black" {}


        [HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
        [HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
        [HideInInspector] _SquashAmount ("Squash", Float) = 1


    //  Lightmapper and outline selection shader need _MainTex, _Color and _Cutoff
        
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            // #define _SPECULAR_SETUP 1

            //#pragma shader_feature _NORMALMAP

            #define DUMMYSHADER

            #define _NORMALMAP

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            // -------------------------------------
            // Unity defined keywords
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            // #pragma multi_compile _ LIGHTMAP_ON
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

        //  Include base inputs and all other needed "base" includes
            #include "Includes/Lux URP Tree Creator Inputs.hlsl"

            #include "Includes/Lux URP Creator Bark ForwardLit Pass.hlsl"

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            ENDHLSL
        }


    //  End Passes -----------------------------------------------------
    
    }
    FallBack "Hidden/InternalErrorShader"
    Dependency "OptimizedShader" = "Lux URP/Nature/Tree Creator Bark Optimized"
   
}
