Shader "Custom/Portal/PortalMask"
{
    Properties
    {
        _StencilID("Stencil ID", Range(0, 255)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Front

        Pass
        {
            Blend Zero One // Don't draw this shader color
            ZWrite Off // Don't draw depth of this shader

            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
                Fail Keep
            }
        }
    }
}
