//-----------------------------------------------------------------------
// <copyright file = "MaskPayline.shader" company = "IGT">
//     Copyright (c) IGT 2014.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

Shader "Paylines/Mask"
{
    SubShader {
        Tags {"Queue" = "Transparent+10" "RenderType" = "Opaque" }
        
        // Turn off lighting, because it's expensive and the thing is supposed to be
        // invisible anyway.
        
        Lighting Off

        // Draw into the depth buffer in the usual way.  This is probably the default,
        // but it doesn't hurt to be explicit.

        ZTest LEqual
        ZWrite On

        // Don't draw anything into the RGBA channels. The ColorMask argument
        // lets us avoid writing to anything except the depth buffer.

        ColorMask 0

        // Do nothing specific in the pass:

        Pass {}
    }
}
