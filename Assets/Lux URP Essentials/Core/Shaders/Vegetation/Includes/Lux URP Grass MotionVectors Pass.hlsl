#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct Attributes
{
    float4 positionOS                   : POSITION;
    float3 normalOS                     : NORMAL;
    float2 texcoord                     : TEXCOORD0;
    half4 color                         : COLOR;

    float3 positionOld                  : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS                   : SV_POSITION;
    #if defined(_ALPHATEST_ON)
        float2 uv                       : TEXCOORD0;
        half2 fadeOcclusion             : TEXCOORD1;
    #endif

    float4 positionCSNoJitter           : POSITION_CS_NO_JITTER;
    float4 previousPositionCSNoJitter   : PREV_POSITION_CS_NO_JITTER;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings MotionVectorsVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionWS;
    half3 normalWS;
    half2 fadeOcclusion;

//  Cache vertex position as we need it in the 2nd call as well. 
    float4 positionOS = input.positionOS;

//  Wind for the current frame
    BendGrassReal(
        positionOS.xyz,
        input.normalOS,
        input.color,
        _TimeParameters,
        _LuxURPWindRT,
        sampler_LuxURPWindRT,
        positionWS,
        normalWS,
        fadeOcclusion
    );

    
//  MV
//  Here everything is tied to input.positionOS
    input.positionOS = mul(GetWorldToObjectMatrix(), float4(positionWS, 1.0));

    const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    #if defined(APPLICATION_SPACE_WARP_MOTION)
        // We do not need jittered position in ASW
        output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.positionOS));
        output.positionCS = output.positionCSNoJitter;
    #else
        // Jittered. Match the frame.
        output.positionCS = vertexInput.positionCS;
        output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.positionOS));
    #endif

    float4 prevPos = (unity_MotionVectorsParams.x == 1) ? float4(input.positionOld, 1) : input.positionOS;

//  Wind for the last frame    
    BendGrassReal(
        positionOS.xyz,
        input.normalOS,
        input.color,
        _LastTimeParameters,
        _LuxURPWindRTPrevious,
        sampler_LuxURPWindRTPrevious,
        positionWS,
        normalWS,
        fadeOcclusion
    );

//  Adjust prevPos according to wind
    prevPos = mul(UNITY_PREV_MATRIX_I_M, float4(positionWS, 1.0));
    output.previousPositionCSNoJitter = mul(_PrevViewProjMatrix, mul(UNITY_PREV_MATRIX_M, prevPos));

    return output;
}

float4 MotionVectorsFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(input.positionCS);
    #endif

    #if defined(_ALPHATEST_ON)
        Alpha(SampleAlbedoAlpha(input.uv.xy, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a * input.fadeOcclusion.x, /*_BaseColor*/ half4(1,1,1,1), _Cutoff);
    #endif
    
    #if defined(APLICATION_SPACE_WARP_MOTION)
        return float4(CalcAswNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 1);
    #else
        return float4(CalcNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 0, 0);
    #endif
}