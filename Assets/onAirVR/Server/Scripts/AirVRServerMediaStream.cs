/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class AirVRServerMediaStream {
    private const int FramebufferCount = 3;

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_RegisterFramebufferTextures(int playerID, IntPtr[] textures, int textureCountPerFrame, int framebufferCount);

    public AirVRServerMediaStream(int playerID, AirVRClientConfig config, int cameraCount) {
        currentFramebufferIndex = 0;
        _cameraCount = cameraCount;

        _framebuffers = new RenderTexture[FramebufferCount * cameraCount];
        IntPtr[] framebuffers = new IntPtr[FramebufferCount * cameraCount];
        for (int f = 0; f < FramebufferCount; f++) {
            for (int t = 0; t < cameraCount; t++) {
                RenderTexture texture = new RenderTexture(config.videoWidth / cameraCount, config.videoHeight, 24);
                texture.antiAliasing = 1;
                texture.autoGenerateMips = false;
                texture.useMipMap = false;
                texture.filterMode = FilterMode.Bilinear;
                texture.anisoLevel = 0;
                texture.format = RenderTextureFormat.ARGB32;
                texture.Create();

                _framebuffers[f * cameraCount + t] = texture;
                framebuffers[f * cameraCount + t] = texture.GetNativeTexturePtr();
            }
        }

        onairvr_RegisterFramebufferTextures(playerID, framebuffers, cameraCount, FramebufferCount);
    }

    private RenderTexture[] _framebuffers;
    private int _cameraCount;
    public int currentFramebufferIndex { get; private set; }

    public void GetNextFramebufferTexturesAsRenderTargets(Camera[] cameras) {
        Assert.IsTrue(cameras.Length == _cameraCount);

        currentFramebufferIndex = (currentFramebufferIndex + 1) % FramebufferCount;
        for (int i = 0; i < cameras.Length; i++) {
            cameras[i].targetTexture = _framebuffers[currentFramebufferIndex * cameras.Length + i];
        }
    }

    public void Destroy() {
        foreach (RenderTexture framebuffer in _framebuffers) {
            framebuffer.Release();
        }
        _framebuffers = null;
    }
}
