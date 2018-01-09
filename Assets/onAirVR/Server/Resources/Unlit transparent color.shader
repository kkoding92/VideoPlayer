/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

Shader "onAirVR/Unlit transparent color" 
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
	}

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        Fog { Mode Off }
        Zwrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Color [_Color]

        Pass {}
    }
}
