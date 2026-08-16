#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct Attributes
{
    float4 positionOS                   : POSITION;
    float2 texcoord                     : TEXCOORD0;
    float3 positionOld                  : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS                   : SV_POSITION;
    float4 positionCSNoJitter           : POSITION_CS_NO_JITTER;
    float4 previousPositionCSNoJitter   : PREV_POSITION_CS_NO_JITTER;
    float2 uv                           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings MotionVectorsVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float4x4 OtoW = GetObjectToWorldMatrix();

    float3 positionWS;

    #if !defined(_UPRIGHT)
        input.positionOS = float4(0,0,0,1);
        #if defined(_PIVOTTOBOTTOM)
            input.positionOS.xy = input.texcoord.xy - float2(0.5, 0.0);
        #else
            input.positionOS.xy = input.texcoord.xy - 0.5;
        #endif
        input.positionOS.x *= _Shrink;

        float2 scale;
        scale.x = length(float3(OtoW[0].x, OtoW[1].x, OtoW[2].x));
        scale.y = length(float3(OtoW[0].y, OtoW[1].y, OtoW[2].y));

        float4 positionVS = mul(GetWorldToViewMatrix(), float4(OtoW._m03_m13_m23, 1.0));
        positionVS.xyz += input.positionOS.xyz * float3(scale.xy, 1.0);
        //output.positionCS = mul(GetViewToHClipMatrix(), positionVS);

    //  MV
        positionWS = mul(GetViewToWorldMatrix(), positionVS).xyz;
    #else
    //  Instance world position
        positionWS = float3(OtoW[0].w, OtoW[1].w, OtoW[2].w);
        half3 viewDirWS = normalize(GetCameraPositionWS() - positionWS);
        half3 billboardTangentWS = normalize(float3(-viewDirWS.z, 0, viewDirWS.x));
    //  Expand Billboard
        float2 percent = input.texcoord.xy;
        float3 billboardPos = (percent.x - 0.5) * _Shrink * billboardTangentWS;
        #if defined(_PIVOTTOBOTTOM)
            billboardPos.y += percent.y;
        #else
            billboardPos.y += percent.y - 0.5;
        #endif
        //output.positionCS = TransformObjectToHClip(billboardPos);

    //  MV
        positionWS = mul(GetObjectToWorldMatrix(), float4(billboardPos, 1.0)).xyz;
    #endif

    output.uv = input.texcoord;
    output.uv.x = (output.uv.x - 0.5) * _Shrink + 0.5;


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
    output.previousPositionCSNoJitter = mul(_PrevViewProjMatrix, mul(UNITY_PREV_MATRIX_M, prevPos));
//  END: MV

    return output;
}

float4 MotionVectorsFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(input.positionCS);
    #endif
    
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a , _BaseColor, _Cutoff);

    #if defined(APPLICATION_SPACE_WARP_MOTION)
        return float4(CalcAswNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 1);
    #else
        return float4(CalcNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 0, 0);
    #endif
}