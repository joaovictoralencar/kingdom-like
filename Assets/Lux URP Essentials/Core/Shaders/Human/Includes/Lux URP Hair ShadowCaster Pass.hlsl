#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct Attributes
{
    float3 positionOS                   : POSITION;
    float3 normalOS                     : NORMAL;
    float2 texcoord                     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                           : TEXCOORD0;
    float4 positionCS                   : SV_POSITION;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

//  Shadow caster specific input
float3 _LightDirection;
float3 _LightPosition;

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
        float3 lightDirectionWS = _LightDirection;
    #endif

    output.uv = input.texcoord;

    //  Two sided geometry: orient the normal towards the light source so the
    //  normal bias is applied away from the surface instead of into it.
    if (dot(normalWS, lightDirectionWS) < 0.0)
        normalWS = -normalWS;

    output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    #if UNITY_REVERSED_Z
        output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
        output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    
    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(input.positionCS);
    #endif

    half alpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a;
    alpha *= _BaseColor.a;
    clip(alpha - _Cutoff);
    
    return 0;
}