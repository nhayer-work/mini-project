
#ifndef UNITY_COMMON_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#endif

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
//#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariables.hlsl"
#endif

// Graph Properties
//CBUFFER_START(UnityPerMaterial)
#define MAX_ORIGINS_COUNT 80 
float3 _ImpactPoints[MAX_ORIGINS_COUNT];
float3 _ImpactForces[MAX_ORIGINS_COUNT];
float3 _ImpactVelocities[MAX_ORIGINS_COUNT];
float _ImpactVolumes[MAX_ORIGINS_COUNT];
// X -> impact time
// Y -> impact mass
// Z -> impact radius
// W -> impact radius power
float4 _ImpactParameters[MAX_ORIGINS_COUNT]; 
int _ImpactsCount = 0;
float3 _Gravity;

uniform float _WaveTowardsNormal;
//CBUFFER_END

// Object and Global properties

// Graph Functions

void Unity_Maximum_float(float A, float B, out float Out)
{
    Out = max(A, B);
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_SquareRoot_float(float In, out float Out)
{
    Out = sqrt(In);
}

void Unity_Comparison_NotEqual_float(float A, float B, out float Out)
{
    Out = A != B ? 1 : 0;
}

void Multiply_float_float(float A, float B, out float Out)
{
    Out = A * B;
}

void Unity_Length_float3(float3 In, out float Out)
{
    Out = length(In);
}

void Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Subtract_float3(float3 A, float3 B, out float3 Out)
{
    Out = A - B;
}

void Unity_Absolute_float(float In, out float Out)
{
    Out = abs(In);
}

void Unity_Power_float(float A, float B, out float Out)
{
    Out = pow(abs(A), B);
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_Sine_float(float In, out float Out)
{
    Out = sin(In);
}

void Unity_Branch_float(float Predicate, float True, float False, out float Out)
{
    Out = Predicate ? True : False;
}

void Unity_InverseLerp_float(float A, float B, float T, out float Out)
{
    Out = (T - A) / (B - A);
}

void Unity_OneMinus_float(float In, out float Out)
{
    Out = 1 - In;
}

void Unity_Exponential_float(float In, out float Out)
{
    Out = exp(In);
}

void Unity_Comparison_GreaterOrEqual_float(float A, float B, out float Out)
{
    Out = A >= B ? 1 : 0;
}

void Unity_Comparison_Equal_float(float A, float B, out float Out)
{
    Out = A == B ? 1 : 0;
}

void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
{
    Out = Predicate ? True : False;
}

void Unity_Normalize_float3(float3 In, out float3 Out)
{
    Out = normalize(In);
}

void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
{
    Out = A * B;
}

void Unity_Cosine_float(float In, out float Out)
{
    Out = cos(In);
}

void Unity_Add_2_float3(float3 A, float3 B, out float3 Out)
{
    Out = A + B;
}

void Unity_MatrixConstruction_Row_float(float4 M0, float4 M1, float4 M2, float4 M3, out float4x4 Out4x4,
                                        out float3x3 Out3x3, out float2x2 Out2x2)
{
    Out4x4 = float4x4(M0.x, M0.y, M0.z, M0.w, M1.x, M1.y, M1.z, M1.w, M2.x, M2.y, M2.z, M2.w, M3.x, M3.y, M3.z, M3.w);
    Out3x3 = float3x3(M0.x, M0.y, M0.z, M1.x, M1.y, M1.z, M2.x, M2.y, M2.z);
    Out2x2 = float2x2(M0.x, M0.y, M1.x, M1.y);
}

void Unity_Multiply_float3x3_float3(float3x3 A, float3 B, out float3 Out)
{
    Out = mul(A, B);
}

void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
{
    Out = cross(A, B);
}

void Unity_Arcsine_float(float In, out float Out)
{
    Out = asin(In);
}

void Unity_Rotate_About_Axis_Radians_float(float3 In, float3 Axis, float Rotation, out float3 Out)
{
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);

    float3x3 rot_mat = {
        one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s,
        one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c,
        one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s,
        one_minus_c * Axis.z * Axis.z + c
    };

    Out = mul(rot_mat, In);
}

struct SurfaceInput
{
    float3 Position;
    float3 Normal;
    float3 TimeParameters;
};

void SampleWave(float _Density, float _Volume, float _Bulk_Modulus,
                float _Damping, float3 _Gravity,
                float3 _Impact_Point, float3 _Impact_Force, float3 _Impact_Velocity,
                float _Impact_Time, float _Impact_Mass, float _Impact_Radius, float _Radius_Power,              
                float _Use_Normal, SurfaceInput IN, 
                out float3 Translation_1, out float3 Normal_2)
{
    float _Property_c4d16da58c34442d9cd5b4df690918e4_Out_0 = _Bulk_Modulus;
    float _Property_29d87332c06a4b7b854a07e599cf4c1a_Out_0 = _Density;
    float _Maximum_28b1a5019dc94c458f6392b32941b94e_Out_2;
    Unity_Maximum_float(_Property_29d87332c06a4b7b854a07e599cf4c1a_Out_0, 9E-05,
                        _Maximum_28b1a5019dc94c458f6392b32941b94e_Out_2);
    float _Divide_db7ba3dd597b4a4eb8fe541b9b00cb55_Out_2;
    Unity_Divide_float(_Property_c4d16da58c34442d9cd5b4df690918e4_Out_0,
                       _Maximum_28b1a5019dc94c458f6392b32941b94e_Out_2, _Divide_db7ba3dd597b4a4eb8fe541b9b00cb55_Out_2);
    float _SquareRoot_539297d8b52749309d655101d9490058_Out_1;
    Unity_SquareRoot_float(_Divide_db7ba3dd597b4a4eb8fe541b9b00cb55_Out_2,
                           _SquareRoot_539297d8b52749309d655101d9490058_Out_1);
    float _Comparison_420dd12fda6243a5814e60f6af7cbed2_Out_2;
    Unity_Comparison_NotEqual_float(_SquareRoot_539297d8b52749309d655101d9490058_Out_1, 0,
                                    _Comparison_420dd12fda6243a5814e60f6af7cbed2_Out_2);
    float Constant_c1a43925f18943688f9427f3d6b907ff = 3.141593;
    float _Multiply_cbd0788ab0b4488fa87ff9811bacde44_Out_2;
    Multiply_float_float(2, Constant_c1a43925f18943688f9427f3d6b907ff,
                               _Multiply_cbd0788ab0b4488fa87ff9811bacde44_Out_2);
    float3 _Property_20c857f4a0544194acfdd112fab3e607_Out_0 = _Impact_Force;
    float _Length_a263112f6b04486fa4474fd989499a11_Out_1;
    Unity_Length_float3(_Property_20c857f4a0544194acfdd112fab3e607_Out_0,
                        _Length_a263112f6b04486fa4474fd989499a11_Out_1);
    float _Property_3e587a26f2d448eca2cf804da3e7bf58_Out_0 = _Density;
    float _Property_a2ee145cfdb446c5bd79c6c13c8f0765_Out_0 = _Volume;
    float _Multiply_9f34fcd600534cb2803ce4aca8cd3ea0_Out_2;
    Multiply_float_float(_Property_3e587a26f2d448eca2cf804da3e7bf58_Out_0,
                               _Property_a2ee145cfdb446c5bd79c6c13c8f0765_Out_0,
                               _Multiply_9f34fcd600534cb2803ce4aca8cd3ea0_Out_2);
    float3 _Property_97394991b7594e9bb01e99cf0e4d7f8d_Out_0 = _Gravity;
    float _Length_d1c1a62bf5f040d6bf8b574a21f92a05_Out_1;
    Unity_Length_float3(_Property_97394991b7594e9bb01e99cf0e4d7f8d_Out_0,
                        _Length_d1c1a62bf5f040d6bf8b574a21f92a05_Out_1);
    float _Multiply_d961267ea5b344a2b394199268c4d3d3_Out_2;
    Multiply_float_float(_Multiply_9f34fcd600534cb2803ce4aca8cd3ea0_Out_2,
                               _Length_d1c1a62bf5f040d6bf8b574a21f92a05_Out_1,
                               _Multiply_d961267ea5b344a2b394199268c4d3d3_Out_2);
    float _Subtract_7c3c1a1b7b6b4b53a2abb97e4e6013c3_Out_2;
    Subtract_float(_Length_a263112f6b04486fa4474fd989499a11_Out_1,
                         _Multiply_d961267ea5b344a2b394199268c4d3d3_Out_2,
                         _Subtract_7c3c1a1b7b6b4b53a2abb97e4e6013c3_Out_2);
    float _Property_54fbbbaa878a4deeaa630515c541047f_Out_0 = _Impact_Mass;
    float _Divide_eb067d2a437b4d4290aec2cbc23227e5_Out_2;
    Unity_Divide_float(_Subtract_7c3c1a1b7b6b4b53a2abb97e4e6013c3_Out_2,
                       _Property_54fbbbaa878a4deeaa630515c541047f_Out_0,
                       _Divide_eb067d2a437b4d4290aec2cbc23227e5_Out_2);
    float3 _Property_6e55875ede704380a4b993066e5e033c_Out_0 = _Impact_Velocity;
    float _Length_c4eac566ee4c4c899905946d293d7eba_Out_1;
    Unity_Length_float3(_Property_6e55875ede704380a4b993066e5e033c_Out_0,
                        _Length_c4eac566ee4c4c899905946d293d7eba_Out_1);
    float _Multiply_3d536e83265346e3bbce4043fec529c2_Out_2;
    Multiply_float_float(-4, _Length_c4eac566ee4c4c899905946d293d7eba_Out_1,
                               _Multiply_3d536e83265346e3bbce4043fec529c2_Out_2);
    float _Divide_e203ed37269942e2b3fe5aa746501719_Out_2;
    Unity_Divide_float(_Divide_eb067d2a437b4d4290aec2cbc23227e5_Out_2, _Multiply_3d536e83265346e3bbce4043fec529c2_Out_2,
                       _Divide_e203ed37269942e2b3fe5aa746501719_Out_2);
    float _Multiply_f6996e67c1d54134af076a5a8c7b13dd_Out_2;
    Multiply_float_float(_Multiply_cbd0788ab0b4488fa87ff9811bacde44_Out_2,
                               _Divide_e203ed37269942e2b3fe5aa746501719_Out_2,
                               _Multiply_f6996e67c1d54134af076a5a8c7b13dd_Out_2);
    float _Multiply_d44a0a1bd2d343da94f7d83e54f35480_Out_2;
    Multiply_float_float(_Multiply_f6996e67c1d54134af076a5a8c7b13dd_Out_2,
                               _Multiply_f6996e67c1d54134af076a5a8c7b13dd_Out_2,
                               _Multiply_d44a0a1bd2d343da94f7d83e54f35480_Out_2);
    float _Property_29afc424041a478db84af37a01f77e9e_Out_0 = _Damping;
    float _Property_13d177b778a44f819a1555b7ed31904f_Out_0 = _Impact_Mass;
    float _Multiply_d61a19cb06ec4fb796708fa6603518d4_Out_2;
    Multiply_float_float(_Property_13d177b778a44f819a1555b7ed31904f_Out_0, 2,
                               _Multiply_d61a19cb06ec4fb796708fa6603518d4_Out_2);
    float _Divide_7a1baddc74d24eaab284585434a90c54_Out_2;
    Unity_Divide_float(_Property_29afc424041a478db84af37a01f77e9e_Out_0,
                       _Multiply_d61a19cb06ec4fb796708fa6603518d4_Out_2,
                       _Divide_7a1baddc74d24eaab284585434a90c54_Out_2);
    float _Multiply_9b4ccdbe2f0b4ffcbda2f54f6e0f6765_Out_2;
    Multiply_float_float(_Divide_7a1baddc74d24eaab284585434a90c54_Out_2,
                               _Divide_7a1baddc74d24eaab284585434a90c54_Out_2,
                               _Multiply_9b4ccdbe2f0b4ffcbda2f54f6e0f6765_Out_2);
    float _Subtract_7f3b7661f969446fb41fb5be589d61ec_Out_2;
    Subtract_float(_Multiply_d44a0a1bd2d343da94f7d83e54f35480_Out_2,
                         _Multiply_9b4ccdbe2f0b4ffcbda2f54f6e0f6765_Out_2,
                         _Subtract_7f3b7661f969446fb41fb5be589d61ec_Out_2);
    float _Maximum_f4b63174f12e4bafba7d0661c9606984_Out_2;
    Unity_Maximum_float(_Subtract_7f3b7661f969446fb41fb5be589d61ec_Out_2, 0,
                        _Maximum_f4b63174f12e4bafba7d0661c9606984_Out_2);
    float _SquareRoot_4905b0c4468d4ae6ba04e013e48090de_Out_1;
    Unity_SquareRoot_float(_Maximum_f4b63174f12e4bafba7d0661c9606984_Out_2,
                           _SquareRoot_4905b0c4468d4ae6ba04e013e48090de_Out_1);
    float _Divide_39c3bdd772ec47ff9bdbddc15fcf3d01_Out_2;
    Unity_Divide_float(_SquareRoot_4905b0c4468d4ae6ba04e013e48090de_Out_1,
                       _SquareRoot_539297d8b52749309d655101d9490058_Out_1,
                       _Divide_39c3bdd772ec47ff9bdbddc15fcf3d01_Out_2);
    float3 _Property_bf30b7372a534c8da99f23e65dae51f8_Out_0 = IN.Position;
    float3 _Property_895579e695294a97a3e6f9be41231783_Out_0 = _Impact_Point;
    float3 _Subtract_dd1274e93f56410ab6b1c717397ddfcb_Out_2;
    Unity_Subtract_float3(_Property_bf30b7372a534c8da99f23e65dae51f8_Out_0,
                          _Property_895579e695294a97a3e6f9be41231783_Out_0,
                          _Subtract_dd1274e93f56410ab6b1c717397ddfcb_Out_2);
    float _Length_339f8d273ec344a3b62ecdcf68ab7af4_Out_1;
    Unity_Length_float3(_Subtract_dd1274e93f56410ab6b1c717397ddfcb_Out_2,
                        _Length_339f8d273ec344a3b62ecdcf68ab7af4_Out_1);
    float _Property_1771d5860c0249cfb919aa51d096f730_Out_0 = _Impact_Radius;
    float _Divide_94addbd92a26481aa3971462965e77a6_Out_2;
    Unity_Divide_float(_Length_339f8d273ec344a3b62ecdcf68ab7af4_Out_1, _Property_1771d5860c0249cfb919aa51d096f730_Out_0,
                       _Divide_94addbd92a26481aa3971462965e77a6_Out_2);
    float _Property_c2af3d08da1b422eb5b4a591747f7c1e_Out_0 = _Radius_Power;
    float _Absolute_f1f50160c92f49f3b95ba97b12bffcd5_Out_1;
    Unity_Absolute_float(_Property_c2af3d08da1b422eb5b4a591747f7c1e_Out_0,
                         _Absolute_f1f50160c92f49f3b95ba97b12bffcd5_Out_1);
    float _Power_3cc09de6231947688000411bba20a960_Out_2;
    Unity_Power_float(_Divide_94addbd92a26481aa3971462965e77a6_Out_2, _Absolute_f1f50160c92f49f3b95ba97b12bffcd5_Out_1,
                      _Power_3cc09de6231947688000411bba20a960_Out_2);
    float _Saturate_cbad5756eddf4adaadbf3c704df5ee1a_Out_1;
    Unity_Saturate_float(_Power_3cc09de6231947688000411bba20a960_Out_2,
                         _Saturate_cbad5756eddf4adaadbf3c704df5ee1a_Out_1);
    float _Lerp_61aaca6714fe4bce8de898099290f1de_Out_3;
    Unity_Lerp_float(0, _Length_339f8d273ec344a3b62ecdcf68ab7af4_Out_1,
                     _Saturate_cbad5756eddf4adaadbf3c704df5ee1a_Out_1, _Lerp_61aaca6714fe4bce8de898099290f1de_Out_3);
    float _Multiply_a6cdbd76afc54f0da0851f9124bfc398_Out_2;
    Multiply_float_float(_Divide_39c3bdd772ec47ff9bdbddc15fcf3d01_Out_2,
                               _Lerp_61aaca6714fe4bce8de898099290f1de_Out_3,
                               _Multiply_a6cdbd76afc54f0da0851f9124bfc398_Out_2);
    float _Property_5b0aa14c3e8a4f78ac0c3dd3d7b4fb5b_Out_0 = _Impact_Time;
    float _Subtract_c5c53fe683ec495ba975b6afb029d861_Out_2;
    Subtract_float(IN.TimeParameters.x, _Property_5b0aa14c3e8a4f78ac0c3dd3d7b4fb5b_Out_0,
                         _Subtract_c5c53fe683ec495ba975b6afb029d861_Out_2);
    float _Multiply_9a354ad488a94f16a639e64d86dca291_Out_2;
    Multiply_float_float(_SquareRoot_4905b0c4468d4ae6ba04e013e48090de_Out_1,
                               _Subtract_c5c53fe683ec495ba975b6afb029d861_Out_2,
                               _Multiply_9a354ad488a94f16a639e64d86dca291_Out_2);
    float _Subtract_f3081e4d031441338f95afcbcb44c928_Out_2;
    Subtract_float(_Multiply_a6cdbd76afc54f0da0851f9124bfc398_Out_2,
                         _Multiply_9a354ad488a94f16a639e64d86dca291_Out_2,
                         _Subtract_f3081e4d031441338f95afcbcb44c928_Out_2);
    float _Sine_ba706e57aebf4271ac835cbbd941992e_Out_1;
    Unity_Sine_float(_Subtract_f3081e4d031441338f95afcbcb44c928_Out_2, _Sine_ba706e57aebf4271ac835cbbd941992e_Out_1);
    float _Branch_0dae1897c51541c6a7a8bc4c050b144e_Out_3;
    Unity_Branch_float(_Comparison_420dd12fda6243a5814e60f6af7cbed2_Out_2, _Sine_ba706e57aebf4271ac835cbbd941992e_Out_1,
                       0, _Branch_0dae1897c51541c6a7a8bc4c050b144e_Out_3);
    float _Float_d2ca30f18422490c9b95dfcde8753a84_Out_0 = -1;
    float _Multiply_3dcd70c1ddda49489e86a446341e1893_Out_2;
    Multiply_float_float(_SquareRoot_539297d8b52749309d655101d9490058_Out_1,
                               _Subtract_c5c53fe683ec495ba975b6afb029d861_Out_2,
                               _Multiply_3dcd70c1ddda49489e86a446341e1893_Out_2);
    float _InverseLerp_ef70426348484c8c9adc840c71c863ab_Out_3;
    Unity_InverseLerp_float(0, _Multiply_3dcd70c1ddda49489e86a446341e1893_Out_2,
                            _Lerp_61aaca6714fe4bce8de898099290f1de_Out_3,
                            _InverseLerp_ef70426348484c8c9adc840c71c863ab_Out_3);
    float _Saturate_714892d00a39403686d98e25b610ea75_Out_1;
    Unity_Saturate_float(_InverseLerp_ef70426348484c8c9adc840c71c863ab_Out_3,
                         _Saturate_714892d00a39403686d98e25b610ea75_Out_1);
    float _OneMinus_f6c49abc86494251a95c14ebc1ab2558_Out_1;
    Unity_OneMinus_float(_Saturate_714892d00a39403686d98e25b610ea75_Out_1,
                         _OneMinus_f6c49abc86494251a95c14ebc1ab2558_Out_1);
    float _Multiply_7f6b15cde1974c55974bac98d3d7dfde_Out_2;
    Multiply_float_float(_OneMinus_f6c49abc86494251a95c14ebc1ab2558_Out_1,
                               _Divide_7a1baddc74d24eaab284585434a90c54_Out_2,
                               _Multiply_7f6b15cde1974c55974bac98d3d7dfde_Out_2);
    float _Multiply_85ed7c855e174a4496939a170e2f23ae_Out_2;
    Multiply_float_float(_Multiply_7f6b15cde1974c55974bac98d3d7dfde_Out_2,
                               _Subtract_c5c53fe683ec495ba975b6afb029d861_Out_2,
                               _Multiply_85ed7c855e174a4496939a170e2f23ae_Out_2);
    float _Multiply_c760a3028a8649d2a7fc58adbf1c6bf5_Out_2;
    Multiply_float_float(_Float_d2ca30f18422490c9b95dfcde8753a84_Out_0,
                               _Multiply_85ed7c855e174a4496939a170e2f23ae_Out_2,
                               _Multiply_c760a3028a8649d2a7fc58adbf1c6bf5_Out_2);
    float _Exponential_f87c2e1313294432be0b9f709fbb8ad0_Out_1;
    Unity_Exponential_float(_Multiply_c760a3028a8649d2a7fc58adbf1c6bf5_Out_2,
                            _Exponential_f87c2e1313294432be0b9f709fbb8ad0_Out_1);
    float _Comparison_0bb3c1f01538411599e9a172dc65ac80_Out_2;
    Unity_Comparison_GreaterOrEqual_float(_Multiply_3dcd70c1ddda49489e86a446341e1893_Out_2,
                                          _Lerp_61aaca6714fe4bce8de898099290f1de_Out_3,
                                          _Comparison_0bb3c1f01538411599e9a172dc65ac80_Out_2);
    float _Multiply_0d89a7990dbf48c794bed2c1ecdc5058_Out_2;
    Multiply_float_float(_Divide_eb067d2a437b4d4290aec2cbc23227e5_Out_2, 2,
                               _Multiply_0d89a7990dbf48c794bed2c1ecdc5058_Out_2);
    float _Comparison_9507c9b64aad476babb2ea0cf58f36ea_Out_2;
    Unity_Comparison_Equal_float(_Multiply_0d89a7990dbf48c794bed2c1ecdc5058_Out_2, 0,
                                 _Comparison_9507c9b64aad476babb2ea0cf58f36ea_Out_2);
    float _Multiply_9b863e86f283448cb2cf979a4a5fb53d_Out_2;
    Multiply_float_float(_Length_c4eac566ee4c4c899905946d293d7eba_Out_1,
                               _Length_c4eac566ee4c4c899905946d293d7eba_Out_1,
                               _Multiply_9b863e86f283448cb2cf979a4a5fb53d_Out_2);
    float _Divide_738023709f88412fa6a650228eb6db0e_Out_2;
    Unity_Divide_float(_Multiply_9b863e86f283448cb2cf979a4a5fb53d_Out_2,
                       _Multiply_0d89a7990dbf48c794bed2c1ecdc5058_Out_2,
                       _Divide_738023709f88412fa6a650228eb6db0e_Out_2);
    float _Branch_b771aa0ae14e4d6f831f190ed7fd5ca3_Out_3;
    Unity_Branch_float(_Comparison_9507c9b64aad476babb2ea0cf58f36ea_Out_2, 0,
                       _Divide_738023709f88412fa6a650228eb6db0e_Out_2, _Branch_b771aa0ae14e4d6f831f190ed7fd5ca3_Out_3);
    float _Branch_4f7fce0a915c4724a7578df657436d0a_Out_3;
    Unity_Branch_float(_Comparison_0bb3c1f01538411599e9a172dc65ac80_Out_2,
                       _Branch_b771aa0ae14e4d6f831f190ed7fd5ca3_Out_3, 0,
                       _Branch_4f7fce0a915c4724a7578df657436d0a_Out_3);
    float _Multiply_d994cb466ab641169688ce21653bec14_Out_2;
    Multiply_float_float(_Exponential_f87c2e1313294432be0b9f709fbb8ad0_Out_1,
                               _Branch_4f7fce0a915c4724a7578df657436d0a_Out_3,
                               _Multiply_d994cb466ab641169688ce21653bec14_Out_2);
    float _Multiply_67b9b628e02d49fcac1a58018de46171_Out_2;
    Multiply_float_float(_Branch_0dae1897c51541c6a7a8bc4c050b144e_Out_3,
                               _Multiply_d994cb466ab641169688ce21653bec14_Out_2,
                               _Multiply_67b9b628e02d49fcac1a58018de46171_Out_2);
    float3 _Property_84f114954d974bac8dc5eee0d8eda6cc_Out_0 = _Impact_Velocity;
    float _Length_1f3d63e680e441b28dff1d391856ced5_Out_1;
    Unity_Length_float3(_Property_84f114954d974bac8dc5eee0d8eda6cc_Out_0,
                        _Length_1f3d63e680e441b28dff1d391856ced5_Out_1);
    float _Comparison_dcd53bb213154d75b6007fffa5469d13_Out_2;
    Unity_Comparison_Equal_float(_Length_1f3d63e680e441b28dff1d391856ced5_Out_1, 0,
                                 _Comparison_dcd53bb213154d75b6007fffa5469d13_Out_2);
    float _Property_d160a6dd6afb48e991be7c0607a72bd8_Out_0 = _Use_Normal;
    float3 _Property_861e9539936a45b999c3b9fcda6559aa_Out_0 = IN.Normal;
    float3 _Branch_f3af098f99774055a6356acf2a93501d_Out_3;
    Unity_Branch_float3(_Property_d160a6dd6afb48e991be7c0607a72bd8_Out_0,
                        _Property_861e9539936a45b999c3b9fcda6559aa_Out_0,
                        _Property_84f114954d974bac8dc5eee0d8eda6cc_Out_0,
                        _Branch_f3af098f99774055a6356acf2a93501d_Out_3);
    float3 _Normalize_453d6e34dac940ceb4230d5ac620196c_Out_1;
    Unity_Normalize_float3(_Branch_f3af098f99774055a6356acf2a93501d_Out_3,
                           _Normalize_453d6e34dac940ceb4230d5ac620196c_Out_1);
    float3 _Branch_dbb933616e5c4c61aa6b8b17718fecae_Out_3;
    Unity_Branch_float3(_Comparison_dcd53bb213154d75b6007fffa5469d13_Out_2, float3(0, 0, 0),
                        _Normalize_453d6e34dac940ceb4230d5ac620196c_Out_1,
                        _Branch_dbb933616e5c4c61aa6b8b17718fecae_Out_3);
    float3 _Multiply_8e3c860f0566459e8c9ee5058792e329_Out_2;
    Unity_Multiply_float3_float3((_Multiply_67b9b628e02d49fcac1a58018de46171_Out_2.xxx),
                                 _Branch_dbb933616e5c4c61aa6b8b17718fecae_Out_3,
                                 _Multiply_8e3c860f0566459e8c9ee5058792e329_Out_2);
    float _Comparison_21c24a5822a84b68b83d70278eb16085_Out_2;
    Unity_Comparison_Equal_float(_Saturate_cbad5756eddf4adaadbf3c704df5ee1a_Out_1, 0,
                                 _Comparison_21c24a5822a84b68b83d70278eb16085_Out_2);
    float _Divide_701604318d3f4d4697eb05f82d914832_Out_2;
    Unity_Divide_float(1, _Lerp_61aaca6714fe4bce8de898099290f1de_Out_3, _Divide_701604318d3f4d4697eb05f82d914832_Out_2);
    float _Branch_9d205bed84c2451da683b30044f04139_Out_3;
    Unity_Branch_float(_Comparison_21c24a5822a84b68b83d70278eb16085_Out_2, 0,
                       _Divide_701604318d3f4d4697eb05f82d914832_Out_2, _Branch_9d205bed84c2451da683b30044f04139_Out_3);
    float _Comparison_4dc73c0bb26a48abad4ffeb6333ce387_Out_2;
    Unity_Comparison_Equal_float(_Branch_9d205bed84c2451da683b30044f04139_Out_3, 0,
                                 _Comparison_4dc73c0bb26a48abad4ffeb6333ce387_Out_2);
    float3 _Property_6fa5e88341dd43778648530815a6044f_Out_0 = IN.Normal;
    float3 _Normalize_52d356eac9b24756ab601f1ba3d874f9_Out_1;
    Unity_Normalize_float3(_Subtract_dd1274e93f56410ab6b1c717397ddfcb_Out_2,
                           _Normalize_52d356eac9b24756ab601f1ba3d874f9_Out_1);
    float _Split_9991116739b14ed0910f062d40dd3a29_R_1 = _Branch_dbb933616e5c4c61aa6b8b17718fecae_Out_3[0];
    float _Split_9991116739b14ed0910f062d40dd3a29_G_2 = _Branch_dbb933616e5c4c61aa6b8b17718fecae_Out_3[1];
    float _Split_9991116739b14ed0910f062d40dd3a29_B_3 = _Branch_dbb933616e5c4c61aa6b8b17718fecae_Out_3[2];
    float _Split_9991116739b14ed0910f062d40dd3a29_A_4 = 0;
    float _Cosine_2485444a6b8a4a1aa32e0b9ed886ea82_Out_1;
    Unity_Cosine_float(_Subtract_f3081e4d031441338f95afcbcb44c928_Out_2,
                       _Cosine_2485444a6b8a4a1aa32e0b9ed886ea82_Out_1);
    float _Multiply_e27631d6ec4c4a25b8f94ad9d65e3b8e_Out_2;
    Multiply_float_float(_Multiply_d994cb466ab641169688ce21653bec14_Out_2,
                               _Cosine_2485444a6b8a4a1aa32e0b9ed886ea82_Out_1,
                               _Multiply_e27631d6ec4c4a25b8f94ad9d65e3b8e_Out_2);
    float _Multiply_93d9a53d061e44eb8b4d79cb53be19d1_Out_2;
    Multiply_float_float(_Divide_39c3bdd772ec47ff9bdbddc15fcf3d01_Out_2, 1,
                               _Multiply_93d9a53d061e44eb8b4d79cb53be19d1_Out_2);
    float _Multiply_e216621b2f5c448d8d4cd4082af819ce_Out_2;
    Multiply_float_float(_Multiply_e27631d6ec4c4a25b8f94ad9d65e3b8e_Out_2,
                               _Multiply_93d9a53d061e44eb8b4d79cb53be19d1_Out_2,
                               _Multiply_e216621b2f5c448d8d4cd4082af819ce_Out_2);
    float3 _Multiply_9b32c763efe243bd95f881e9c5aa363d_Out_2;
    Unity_Multiply_float3_float3((_Multiply_e216621b2f5c448d8d4cd4082af819ce_Out_2.xxx),
                                 _Normalize_52d356eac9b24756ab601f1ba3d874f9_Out_1,
                                 _Multiply_9b32c763efe243bd95f881e9c5aa363d_Out_2);
    float3 _Multiply_4b67a188cb55440ab86c1e2a7c458290_Out_2;
    Unity_Multiply_float3_float3((_Split_9991116739b14ed0910f062d40dd3a29_R_1.xxx),
                                 _Multiply_9b32c763efe243bd95f881e9c5aa363d_Out_2,
                                 _Multiply_4b67a188cb55440ab86c1e2a7c458290_Out_2);
    float3 _Add_23e846b2e5c5407b95d25ed14f81801d_Out_2;
    Unity_Add_2_float3(_Multiply_4b67a188cb55440ab86c1e2a7c458290_Out_2, float3(1, 0, 0),
                     _Add_23e846b2e5c5407b95d25ed14f81801d_Out_2);
    float3 _Multiply_b44411d9c48843dfa2a337a18897a072_Out_2;
    Unity_Multiply_float3_float3((_Split_9991116739b14ed0910f062d40dd3a29_G_2.xxx),
                                 _Multiply_9b32c763efe243bd95f881e9c5aa363d_Out_2,
                                 _Multiply_b44411d9c48843dfa2a337a18897a072_Out_2);
    float3 _Add_353c5e1cfb4e43c89fc54dd0b82a088e_Out_2;
    Unity_Add_2_float3(_Multiply_b44411d9c48843dfa2a337a18897a072_Out_2, float3(0, 1, 0),
                     _Add_353c5e1cfb4e43c89fc54dd0b82a088e_Out_2);
    float3 _Multiply_4c91c660fc8243838c934492eb103d2b_Out_2;
    Unity_Multiply_float3_float3((_Split_9991116739b14ed0910f062d40dd3a29_B_3.xxx),
                                 _Multiply_9b32c763efe243bd95f881e9c5aa363d_Out_2,
                                 _Multiply_4c91c660fc8243838c934492eb103d2b_Out_2);
    float3 _Add_c0b6612259fb44c5b214f61b734de4fe_Out_2;
    Unity_Add_2_float3(_Multiply_4c91c660fc8243838c934492eb103d2b_Out_2, float3(0, 0, 1),
                     _Add_c0b6612259fb44c5b214f61b734de4fe_Out_2);
    float4x4 _MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var4x4_4;
    float3x3 _MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var3x3_5;
    float2x2 _MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var2x2_6;
    Unity_MatrixConstruction_Row_float((float4(_Add_23e846b2e5c5407b95d25ed14f81801d_Out_2, 1.0)),
                                       (float4(_Add_353c5e1cfb4e43c89fc54dd0b82a088e_Out_2, 1.0)),
                                       (float4(_Add_c0b6612259fb44c5b214f61b734de4fe_Out_2, 1.0)), float4(0, 0, 0, 0),
                                       _MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var4x4_4,
                                       _MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var3x3_5,
                                       _MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var2x2_6);
    float _Float_e065a43f7bdf46f2873a6c4a28616d89_Out_0 = 1E-15;
    float3 _Multiply_cd6e4c4a914c4028aa488042ece896cc_Out_2;
    Unity_Multiply_float3_float3((_Float_e065a43f7bdf46f2873a6c4a28616d89_Out_0.xxx),
                                 _Normalize_52d356eac9b24756ab601f1ba3d874f9_Out_1,
                                 _Multiply_cd6e4c4a914c4028aa488042ece896cc_Out_2);
    float3 _Multiply_86fd482251fc4006bff60ee0603a8fc6_Out_2;
    Unity_Multiply_float3x3_float3(_MatrixConstruction_a3fe5d6ee79d4c69b59da59f0f2adf41_var3x3_5,
                                   _Multiply_cd6e4c4a914c4028aa488042ece896cc_Out_2,
                                   _Multiply_86fd482251fc4006bff60ee0603a8fc6_Out_2);
    float3 _Normalize_e5460d1656614077bae70f6c644991f4_Out_1;
    Unity_Normalize_float3(_Multiply_86fd482251fc4006bff60ee0603a8fc6_Out_2,
                           _Normalize_e5460d1656614077bae70f6c644991f4_Out_1);
    float3 _CrossProduct_eb5231113b6041e9aff0fd240e29bcd1_Out_2;
    Unity_CrossProduct_float(_Normalize_52d356eac9b24756ab601f1ba3d874f9_Out_1,
                             _Normalize_e5460d1656614077bae70f6c644991f4_Out_1,
                             _CrossProduct_eb5231113b6041e9aff0fd240e29bcd1_Out_2);
    float _Length_25921d8717704739ad56712767c1c3d6_Out_1;
    Unity_Length_float3(_CrossProduct_eb5231113b6041e9aff0fd240e29bcd1_Out_2,
                        _Length_25921d8717704739ad56712767c1c3d6_Out_1);
    float _Comparison_3e6f21b8713247b396e7a62af4fdfe90_Out_2;
    Unity_Comparison_Equal_float(_Length_25921d8717704739ad56712767c1c3d6_Out_1, 0,
                                 _Comparison_3e6f21b8713247b396e7a62af4fdfe90_Out_2);
    float3 _Branch_4152a02e56b047ceaebaefaf15710c4a_Out_3;
    Unity_Branch_float3(_Comparison_3e6f21b8713247b396e7a62af4fdfe90_Out_2, float3(1, 1, 1),
                        _CrossProduct_eb5231113b6041e9aff0fd240e29bcd1_Out_2,
                        _Branch_4152a02e56b047ceaebaefaf15710c4a_Out_3);
    float _Arcsine_03ad1f43089147f69ace1ae168aa6c8a_Out_1;
    Unity_Arcsine_float(_Length_25921d8717704739ad56712767c1c3d6_Out_1,
                        _Arcsine_03ad1f43089147f69ace1ae168aa6c8a_Out_1);
    float3 _RotateAboutAxis_12cdd3e3e09240a1b36ce7594cdae852_Out_3;
    Unity_Rotate_About_Axis_Radians_float(_Property_6fa5e88341dd43778648530815a6044f_Out_0,
                                          _Branch_4152a02e56b047ceaebaefaf15710c4a_Out_3,
                                          _Arcsine_03ad1f43089147f69ace1ae168aa6c8a_Out_1,
                                          _RotateAboutAxis_12cdd3e3e09240a1b36ce7594cdae852_Out_3);
    float3 _Branch_84412c0c223f42cea4a2d4bc34cb0fe2_Out_3;
    Unity_Branch_float3(_Comparison_4dc73c0bb26a48abad4ffeb6333ce387_Out_2,
                        _Property_6fa5e88341dd43778648530815a6044f_Out_0,
                        _RotateAboutAxis_12cdd3e3e09240a1b36ce7594cdae852_Out_3,
                        _Branch_84412c0c223f42cea4a2d4bc34cb0fe2_Out_3);
    Translation_1 = _Multiply_8e3c860f0566459e8c9ee5058792e329_Out_2;
    Normal_2 = _Branch_84412c0c223f42cea4a2d4bc34cb0fe2_Out_3;
}

void SampleAllWaves_float(float3 WSPosition, float3 WSNormal, float Density, float BulkModulus, float Damping, out float3 WSWavyTranslation, out float3 WSWavyNormal)
{
    SurfaceInput surfInput;
    surfInput.Normal = WSNormal;
    surfInput.Position = WSPosition;
    surfInput.TimeParameters = _TimeParameters.xyz; 
    WSWavyTranslation = float3(0,0,0);
    WSWavyNormal = WSNormal;

    float3 impactTrans;
    float3 impactNorm;
    UNITY_UNROLLX(MAX_ORIGINS_COUNT) for (int i = 0; i < _ImpactsCount; i++)
    {
        SampleWave(Density, _ImpactVolumes[i], BulkModulus, Damping, _Gravity,
            _ImpactPoints[i], _ImpactForces[i], _ImpactVelocities[i],
            _ImpactParameters[i].x, _ImpactParameters[i].y, _ImpactParameters[i].z, _ImpactParameters[i].w,
            _WaveTowardsNormal, surfInput,
            impactTrans, impactNorm);
        
        WSWavyTranslation += impactTrans;
        if(i==0)
            WSWavyNormal = impactNorm;
        else
        {
            WSWavyNormal += impactNorm;
        } 
    }
}